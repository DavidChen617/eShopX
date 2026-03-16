#!/usr/bin/env python3
"""
domain2uml.py  —  從 C# Domain 層原始碼產生 PlantUML class diagram
用法：
    python3 tools/domain2uml.py [domain_src_dir] [output_dir]
    python3 tools/domain2uml.py          # 預設讀 src/Domain，輸出 docs/domain/
輸出：
    docs/domain/{Aggregate}.puml   每個 Aggregate 的詳細圖
    docs/domain/overview.puml      全域 Aggregate 概覽圖
"""

import re
import sys
from pathlib import Path
from dataclasses import dataclass, field
from collections import defaultdict

# ─── 設定 ────────────────────────────────────────────────────────────────────

DEFAULT_SRC = Path(__file__).parent.parent / "src" / "Domain"
DEFAULT_OUT = Path(__file__).parent.parent / "docs" / "domain"

# 不展開進 aggregate diagram 的型別（基礎框架類別）
BASE_TYPES = {"AggregateRoot", "Entity", "INotification", "Guid",
              "string", "int", "decimal", "bool", "DateTime", "object"}

# 哪些型別算 ValueObject（用 record 判斷，或在這白名單）
VALUE_OBJECT_NAMES = {"Money", "ProductSnapshot", "ReceiverInfo", "Avatar"}

# ─── Data Model ──────────────────────────────────────────────────────────────

@dataclass
class CsProperty:
    name: str
    type_name: str         # 原始型別字串，含泛型
    is_collection: bool    # List<T> / IReadOnlyList<T>
    element_type: str      # 若 is_collection，則是 T

@dataclass
class CsMethod:
    name: str
    return_type: str
    params: str            # 原始參數字串

@dataclass
class CsType:
    name: str
    kind: str              # class | abstract_class | enum | record | event
    base: str | None       # 繼承的父類別
    namespace: str
    source_file: Path
    properties: list[CsProperty] = field(default_factory=list)
    methods: list[CsMethod] = field(default_factory=list)
    enum_values: list[str] = field(default_factory=list)
    record_params: list[tuple[str, str]] = field(default_factory=list)  # [(type, name)]
    raises_events: list[str] = field(default_factory=list)  # event type names

# ─── Parser ───────────────────────────────────────────────────────────────────

_COLLECTION_RE = re.compile(
    r'^(?:IReadOnlyList|List|IList|IEnumerable|ICollection)<(.+)>$'
)

def _strip_nullable(t: str) -> str:
    return t.rstrip('?')

def _unwrap_collection(t: str) -> tuple[bool, str]:
    m = _COLLECTION_RE.match(t)
    if m:
        return True, m.group(1).rstrip('?')
    return False, _strip_nullable(t)

def parse_cs_file(path: Path) -> list[CsType]:
    src = path.read_text(encoding="utf-8")
    types: list[CsType] = []

    # ── namespace
    ns_m = re.search(r'^namespace\s+([\w.]+)', src, re.MULTILINE)
    namespace = ns_m.group(1) if ns_m else ""

    # ── domain events (sealed record … : INotification)
    for m in re.finditer(
        r'public\s+sealed\s+record\s+(\w+)\s*\(([^)]*)\)\s*:\s*INotification',
        src
    ):
        name, raw_params = m.group(1), m.group(2)
        params = _parse_record_params(raw_params)
        t = CsType(name=name, kind="event", base=None,
                   namespace=namespace, source_file=path,
                   record_params=params)
        types.append(t)

    # ── plain record (ValueObject)
    for m in re.finditer(
        r'public\s+sealed\s+record\s+(\w+)\s*\(([^)]*)\)',
        src
    ):
        name, raw_params = m.group(1), m.group(2)
        if any(t.name == name for t in types):
            continue  # already captured as event
        params = _parse_record_params(raw_params)
        t = CsType(name=name, kind="record", base=None,
                   namespace=namespace, source_file=path,
                   record_params=params)
        types.append(t)

    # ── enum
    for m in re.finditer(
        r'public\s+enum\s+(\w+)\s*\{([^}]+)\}',
        src, re.DOTALL
    ):
        name, body = m.group(1), m.group(2)
        values = [v.strip().split('=')[0].strip()
                  for v in body.split(',') if v.strip()]
        t = CsType(name=name, kind="enum", base=None,
                   namespace=namespace, source_file=path,
                   enum_values=[v for v in values if v])
        types.append(t)

    # ── class / abstract class
    class_pattern = re.compile(
        r'public\s+(?:sealed\s+)?(?P<abs>abstract\s+)?class\s+(?P<name>\w+)'
        r'(?:\s*:\s*(?P<base>[\w<>, ]+))?',
    )
    for m in class_pattern.finditer(src):
        name = m.group('name')
        if name in ('Program', 'Startup') or any(t.name == name for t in types):
            continue
        is_abstract = bool(m.group('abs'))
        raw_base = m.group('base') or ''
        # take first token before comma as the actual base class
        base_parts = [p.strip() for p in raw_base.split(',')]
        base = base_parts[0] if base_parts[0] else None
        # strip generic / interface noise
        if base and '<' in base:
            base = base.split('<')[0]
        if base in ('', 'INotification'):
            base = None

        kind = "abstract_class" if is_abstract else "class"
        t = CsType(name=name, kind=kind, base=base,
                   namespace=namespace, source_file=path)

        # parse body (between the matching braces)
        body = _extract_class_body(src, m.end())
        if body:
            t.properties = _parse_properties(body)
            t.methods = _parse_methods(body)
            t.raises_events = _parse_raised_events(body)

        types.append(t)

    return types


def _parse_record_params(raw: str) -> list[tuple[str, str]]:
    result = []
    for part in raw.split(','):
        part = part.strip()
        if not part:
            continue
        tokens = part.split()
        if len(tokens) >= 2:
            result.append((tokens[-2].rstrip('?'), tokens[-1]))
    return result


def _extract_class_body(src: str, start: int) -> str:
    """Grab text between the first { and its matching } after start."""
    depth = 0
    i = start
    body_start = None
    while i < len(src):
        c = src[i]
        if c == '{':
            if body_start is None:
                body_start = i + 1
            depth += 1
        elif c == '}':
            depth -= 1
            if depth == 0 and body_start is not None:
                return src[body_start:i]
        i += 1
    return ""


def _parse_properties(body: str) -> list[CsProperty]:
    props = []
    # public properties: Type Name { get; ... }
    pub_pattern = re.compile(
        r'(?:public|internal|protected)\s+'
        r'(?:(?:static|readonly|override|virtual|new)\s+)*'
        r'([\w<>\[\]?,\s]+?)\s+(\w+)\s*\{'
        r'[^}]*get[^}]*\}',
        re.DOTALL
    )
    for m in pub_pattern.finditer(body):
        raw_type = m.group(1).strip()
        name = m.group(2)
        if name in ('get', 'set', 'init'):
            continue
        is_col, elem = _unwrap_collection(raw_type)
        props.append(CsProperty(name=name, type_name=raw_type,
                                is_collection=is_col, element_type=elem))

    # private readonly List<T> _field = ... (composition holders)
    priv_pattern = re.compile(
        r'private\s+readonly\s+(List<[\w<>, ]+>)\s+\w+\s*='
    )
    for m in priv_pattern.finditer(body):
        raw_type = m.group(1).strip()
        is_col, elem = _unwrap_collection(raw_type)
        # synthesise a virtual property name from the element type
        props.append(CsProperty(name=f'_{elem}s', type_name=raw_type,
                                is_collection=True, element_type=elem))
    return props


def _parse_methods(body: str) -> list[CsMethod]:
    methods = []
    # public [static] ReturnType MethodName(params)
    pattern = re.compile(
        r'public\s+(?:static\s+)?(?:async\s+)?'
        r'([\w<>\[\]?, ]+?)\s+(\w+)\s*\(([^)]*)\)\s*[{=>\n]'
    )
    skip = {'get', 'set', 'init', 'Create', 'Id', 'Guid'}
    for m in pattern.finditer(body):
        ret, name, params = m.group(1).strip(), m.group(2), m.group(3).strip()
        if name[0].islower() or name in skip:
            continue
        if name in ('if', 'for', 'foreach', 'while', 'switch', 'return'):
            continue
        # skip properties that accidentally matched
        methods.append(CsMethod(name=name, return_type=ret, params=params))
    # deduplicate
    seen = set()
    unique = []
    for mth in methods:
        if mth.name not in seen:
            seen.add(mth.name)
            unique.append(mth)
    return unique


def _parse_raised_events(body: str) -> list[str]:
    return re.findall(r'RaiseDomainEvent\(\s*new\s+(\w+)', body)


# ─── Aggregate grouping ───────────────────────────────────────────────────────

def group_by_aggregate(all_types: list[CsType]) -> dict[str, list[CsType]]:
    groups: dict[str, list[CsType]] = defaultdict(list)
    for t in all_types:
        # determine aggregate from namespace segment after "Aggregates."
        m = re.search(r'Aggregates\.(\w+)', t.namespace)
        if m:
            groups[m.group(1)].append(t)
        elif 'ValueObjects' in t.namespace:
            groups['_ValueObjects'].append(t)
        elif 'Outbox' in t.namespace:
            groups['_Outbox'].append(t)
        else:
            groups['_Base'].append(t)
    return groups


# ─── PlantUML rendering ───────────────────────────────────────────────────────

COLORS = {
    "AggregateRoot": "#D4E6F1",
    "Entity":        "#D5F5E3",
    "enum":          "#FEF9E7",
    "record":        "#F9EBEA",
    "event":         "#FDEDEC",
    "abstract":      "#EBF5FB",
}

def _puml_type_label(t: CsType) -> str:
    if t.kind == "enum":
        return "enum"
    if t.kind in ("record", "event"):
        return "class"
    if t.kind == "abstract_class":
        return "abstract class"
    return "class"

def _puml_stereotype(t: CsType, base_map: dict[str, str]) -> str:
    if t.kind == "event":
        return " <<DomainEvent>>"
    if t.kind == "record":
        return " <<ValueObject>>"
    if t.kind == "abstract_class":
        return " <<abstract>>"
    root = _resolve_root(t.name, base_map)
    if root == "AggregateRoot":
        return " <<AggregateRoot>>"
    if root == "Entity":
        return " <<Entity>>"
    return ""

def _puml_color(t: CsType, base_map: dict[str, str]) -> str:
    if t.kind == "event":
        return COLORS["event"]
    if t.kind == "record":
        return COLORS["record"]
    if t.kind == "abstract_class":
        return COLORS["abstract"]
    if t.kind == "enum":
        return COLORS["enum"]
    root = _resolve_root(t.name, base_map)
    return COLORS.get(root, "")

def _resolve_root(name: str, base_map: dict[str, str], _seen=None) -> str:
    if _seen is None:
        _seen = set()
    if name in _seen:
        return name
    _seen.add(name)
    parent = base_map.get(name)
    if not parent or parent == name:
        return name
    return _resolve_root(parent, base_map, _seen)


def render_aggregate_puml(
    aggregate: str,
    types: list[CsType],
    all_type_names: set[str],
    base_map: dict[str, str],
) -> str:
    lines = [
        "@startuml",
        f"' Aggregate: {aggregate}",
        "skinparam classAttributeIconSize 0",
        "skinparam classFontStyle Bold",
        "skinparam ArrowColor #555555",
        "skinparam ClassBorderColor #888888",
        "hide empty members",
        "",
    ]

    type_map = {t.name: t for t in types}

    for t in types:
        color = _puml_color(t, base_map)
        color_str = f" {color}" if color else ""
        stereo = _puml_stereotype(t, base_map)
        label = _puml_type_label(t)

        lines.append(f'{label} {t.name}{stereo}{color_str} {{')

        if t.kind == "enum":
            for v in t.enum_values:
                lines.append(f'  {v}')

        elif t.kind in ("record", "event"):
            for typ, pname in t.record_params:
                lines.append(f'  +{pname}: {typ}')

        else:
            # skip inherited Id/DomainEvents and synthesised _ composition holders
            shown_props = [
                p for p in t.properties
                if p.name not in ('Id', 'DomainEvents') and not p.name.startswith('_')
            ]
            for p in shown_props:
                lines.append(f'  +{p.name}: {p.type_name}')

            # public methods (skip static Create — shown separately, keep domain ops)
            domain_methods = [
                m for m in t.methods
                if m.name not in ('Create', 'ClearDomainEvents', 'Of')
            ]
            if domain_methods:
                lines.append("  ..")
                for m in domain_methods:
                    lines.append(f'  +{m.name}()')

        lines.append("}")
        lines.append("")

    # ── Relationships
    lines.append("' --- Inheritance ---")
    for t in types:
        if not t.base:
            continue
        if t.base in all_type_names:
            lines.append(f'{t.name} --|> {t.base}')

    lines.append("")
    lines.append("' --- Composition ---")
    seen_rel: set[str] = set()
    for t in types:
        if t.kind not in ("class", "abstract_class"):
            continue
        for p in t.properties:
            elem = p.element_type if p.is_collection else _strip_nullable(p.type_name)
            if elem not in all_type_names or elem in BASE_TYPES or elem == t.name:
                continue
            if p.is_collection:
                rel = f'{t.name} "1" *-- "many" {elem}'
            else:
                rel = f'{t.name} --> {elem}'
            if rel not in seen_rel:
                seen_rel.add(rel)
                lines.append(rel)

    lines.append("")
    lines.append("' --- Domain Events ---")
    seen_ev: set[str] = set()
    for t in types:
        for ev in t.raises_events:
            if ev in type_map:
                rel = f'{t.name} ..> {ev} : raises'
                if rel not in seen_ev:
                    seen_ev.add(rel)
                    lines.append(rel)

    lines.append("")
    lines.append("@enduml")
    return "\n".join(lines)


def render_overview_puml(
    groups: dict[str, list[CsType]],
    base_map: dict[str, str],
) -> str:
    lines = [
        "@startuml",
        "' Domain Overview — Aggregate Roots",
        "skinparam classAttributeIconSize 0",
        "skinparam packageStyle Rectangle",
        "skinparam ArrowColor #555555",
        "skinparam ClassBorderColor #888888",
        "hide empty members",
        "",
    ]

    # collect aggregate roots + their FK references
    agg_roots: dict[str, CsType] = {}
    for agg, types in groups.items():
        if agg.startswith('_'):
            continue
        for t in types:
            root = _resolve_root(t.name, base_map)
            if root == "AggregateRoot":
                agg_roots[t.name] = t
                break

    # render each aggregate as a package with just the root
    for agg, types in groups.items():
        if agg.startswith('_'):
            continue
        lines.append(f'package "{agg}" {{')
        for t in types:
            root = _resolve_root(t.name, base_map)
            if root == "AggregateRoot":
                lines.append(f'  class {t.name} <<AggregateRoot>> {COLORS["AggregateRoot"]}')
        lines.append("}")
        lines.append("")

    # cross-aggregate FK relationships (Guid properties pointing to other aggregate roots)
    lines.append("' --- Cross-Aggregate References ---")
    agg_root_names = set(agg_roots.keys())
    for t in agg_roots.values():
        for p in t.properties:
            # e.g. UserId, OrderId, CategoryId → User, Order, Category
            if p.type_name in ('Guid', 'Guid?'):
                # heuristic: strip 'Id' suffix
                candidate = p.name.replace('Id', '')
                if candidate in agg_root_names and candidate != t.name:
                    lines.append(f'{t.name} --> {candidate} : {p.name}')

    lines.append("")
    lines.append("@enduml")
    return "\n".join(lines)


# ─── Main ─────────────────────────────────────────────────────────────────────

def main(src_dir: Path, out_dir: Path):
    out_dir.mkdir(parents=True, exist_ok=True)

    # parse all .cs files
    all_types: list[CsType] = []
    for cs_file in src_dir.rglob("*.cs"):
        if "obj" in cs_file.parts or "bin" in cs_file.parts:
            continue
        all_types.extend(parse_cs_file(cs_file))

    all_type_names = {t.name for t in all_types}
    base_map = {t.name: t.base for t in all_types if t.base}

    groups = group_by_aggregate(all_types)

    # per-aggregate diagrams
    for agg, types in groups.items():
        if agg.startswith('_'):
            continue
        puml = render_aggregate_puml(agg, types, all_type_names, base_map)
        out_path = out_dir / f"{agg.lower()}.puml"
        out_path.write_text(puml, encoding="utf-8")
        print(f"  ✓ {out_path.relative_to(out_dir.parent.parent)}")

    # overview
    overview = render_overview_puml(groups, base_map)
    overview_path = out_dir / "overview.puml"
    overview_path.write_text(overview, encoding="utf-8")
    print(f"  ✓ {overview_path.relative_to(out_dir.parent.parent)}")

    print(f"\n共產生 {len(groups) - sum(1 for k in groups if k.startswith('_'))} 個 Aggregate 圖 + 1 個 Overview")


if __name__ == "__main__":
    src = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_SRC
    out = Path(sys.argv[2]) if len(sys.argv) > 2 else DEFAULT_OUT
    main(src, out)
