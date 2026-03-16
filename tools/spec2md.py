#!/usr/bin/env python3
"""
spec2md.py  —  將 OpenAPI spec.json 轉成可讀的 API 文件 Markdown
用法：
    python3 tools/spec2md.py [spec.json] [output.md]
    python3 tools/spec2md.py               # 預設讀 spec.json，輸出 docs/api-spec.md
"""

import json
import re
import sys
from pathlib import Path
from collections import defaultdict

# ─── 設定 ────────────────────────────────────────────────────────────────────

DEFAULT_INPUT  = Path(__file__).parent.parent / "spec.json"
DEFAULT_OUTPUT = Path(__file__).parent.parent / "docs" / "api-spec.md"

METHOD_ORDER = ["get", "post", "put", "patch", "delete"]

# tag 顯示順序（其餘依 spec 順序）
TAG_ORDER = [
    "Auth", "Users", "Categories", "Sizes", "Tags",
    "Products", "Cart", "Orders", "Payments", "ECPay",
    "Admin - Categories", "Admin - Sizes", "Admin - Tags",
    "Admin - Products", "Admin",
]

# 需要 Authorization header 的 tag 或 endpoint（依 tag 判斷）
AUTHED_TAGS = {
    "Users", "Cart", "Orders",
    "Admin", "Admin - Categories", "Admin - Sizes",
    "Admin - Tags", "Admin - Products",
}

ADMIN_TAGS = {
    "Admin", "Admin - Categories", "Admin - Sizes",
    "Admin - Tags", "Admin - Products",
}

# ─── Schema 解析 ──────────────────────────────────────────────────────────────

class SchemaResolver:
    def __init__(self, components: dict):
        self.schemas = components.get("schemas", {})

    def resolve_ref(self, ref: str) -> dict:
        """#/components/schemas/Foo  →  schema dict"""
        name = ref.split("/")[-1]
        return self.schemas.get(name, {})

    def unwrap_api_response(self, schema: dict) -> tuple[str | None, dict | None]:
        """
        ApiResponseOfFoo  →  ("Foo", foo_schema)
        ApiResponse       →  (None, None)
        """
        ref = schema.get("$ref", "")
        if not ref:
            return None, None
        name = ref.split("/")[-1]
        if name == "ApiResponse":
            return None, None
        m = re.match(r"ApiResponseOf(.+)", name)
        if not m:
            return None, None
        inner_name = self._camel_inner(m.group(1))
        inner = self.schemas.get(inner_name)
        if inner is None:
            # try exact match
            inner = self.schemas.get(m.group(1))
        return inner_name, inner

    def _camel_inner(self, raw: str) -> str:
        """IReadOnlyListOfCategoryResponse → IReadOnlyListOfCategoryResponse (no change needed)"""
        return raw

    def to_example(self, schema: dict, depth: int = 0, visited: set = None) -> str:
        """將 schema 轉成 inline JSON example 字串"""
        if visited is None:
            visited = set()

        if "$ref" in schema:
            ref_name = schema["$ref"].split("/")[-1]
            if ref_name in visited:
                return f'"<{ref_name}>"'
            visited = visited | {ref_name}
            resolved = self.schemas.get(ref_name, {})
            return self.to_example(resolved, depth, visited)

        if not schema:
            return '"any"'

        typ_raw = schema.get("type", "object")
        # OpenAPI 3.1 allows type to be a list e.g. ["integer", "string"] or ["null", "string"]
        if isinstance(typ_raw, list):
            typ = next((t for t in typ_raw if t != "null"), typ_raw[0])
        else:
            typ = typ_raw

        # oneOf / anyOf must be checked before generic object fallback
        if "oneOf" in schema or "anyOf" in schema:
            options = schema.get("oneOf", schema.get("anyOf", []))
            # filter out pure-null options, pick first non-null to represent
            non_null = [o for o in options if not (o.get("type") == "null")]
            chosen = non_null[0] if non_null else (options[0] if options else {})
            return self.to_example(chosen, depth, visited)

        if "enum" in schema:
            non_null_vals = [v for v in schema["enum"] if v is not None]
            return " | ".join(f'"{v}"' for v in non_null_vals) if non_null_vals else '"string"'

        if typ == "object" or "properties" in schema or "allOf" in schema:
            props = schema.get("properties", {})
            # handle allOf
            for part in schema.get("allOf", []):
                part_resolved = part
                if "$ref" in part:
                    part_resolved = self.resolve_ref(part["$ref"])
                props = {**props, **part_resolved.get("properties", {})}

            if not props:
                return "{}"

            indent = "  " * (depth + 1)
            close  = "  " * depth
            lines = []
            for key, val in props.items():
                example_val = self.to_example(val, depth + 1, visited)
                lines.append(f'{indent}"{key}": {example_val}')
            return "{\n" + ",\n".join(lines) + "\n" + close + "}"

        if typ == "array":
            items = schema.get("items", {})
            inner = self.to_example(items, depth + 1, visited)
            indent = "  " * (depth + 1)
            close  = "  " * depth
            return f"[\n{indent}{inner}\n{close}]"

        type_map = {
            "string":  '"string"',
            "integer": "0",
            "number":  "0.0",
            "boolean": "true",
        }
        return type_map.get(typ, f'"({typ})"')

    def schema_display_name(self, ref: str) -> str:
        """ApiResponseOfIReadOnlyListOfCategoryResponse  →  ApiResponse<CategoryResponse[]>"""
        name = ref.split("/")[-1]
        # strip ApiResponseOf prefix
        m = re.match(r"ApiResponseOf(.+)", name)
        if not m:
            return name
        inner = m.group(1)
        # IReadOnlyListOfFoo → Foo[]
        list_m = re.match(r"IReadOnlyListOf(.+)", inner)
        if list_m:
            return f"ApiResponse<{list_m.group(1)}[]>"
        return f"ApiResponse<{inner}>"


# ─── Markdown 建構 ────────────────────────────────────────────────────────────

def format_params(params: list) -> str | None:
    if not params:
        return None
    lines = ["| 參數 | 位置 | 類型 | 必填 | 說明 |",
             "|------|------|------|:----:|------|"]
    for p in params:
        loc      = p.get("in", "")
        name     = p.get("name", "")
        required = "✓" if p.get("required") else ""
        schema   = p.get("schema", {})
        typ      = schema.get("type", "string")
        if "enum" in schema:
            typ = " / ".join(str(e) for e in schema["enum"])
        desc = p.get("description", "")
        lines.append(f"| `{name}` | {loc} | {typ} | {required} | {desc} |")
    return "\n".join(lines)


def format_request_body(body: dict, resolver: SchemaResolver) -> str | None:
    if not body:
        return None
    content = body.get("content", {})

    if "multipart/form-data" in content:
        schema = content["multipart/form-data"].get("schema", {})
        required_fields = schema.get("required", [])
        props = schema.get("properties", {})
        lines = ["```", "// multipart/form-data"]
        for field, fschema in props.items():
            req = " (required)" if field in required_fields else ""
            ftype = fschema.get("type", "file")
            lines.append(f"{field}: {ftype}{req}")
        lines.append("```")
        return "\n".join(lines)

    if "application/json" in content:
        schema = content["application/json"].get("schema", {})
        example = resolver.to_example(schema)
        return f"```json\n{example}\n```"

    return None


def format_responses(responses: dict, resolver: SchemaResolver) -> str:
    lines = []
    for status, resp in sorted(responses.items(), key=lambda x: int(x[0]) if x[0].isdigit() else 999):
        desc = resp.get("description", "")
        content = resp.get("content", {})

        if not content:
            lines.append(f"- **{status}** {desc}")
            continue

        json_content = content.get("application/json", {})
        schema = json_content.get("schema", {})
        ref = schema.get("$ref", "")

        display_name = resolver.schema_display_name(ref) if ref else ""
        _, inner = resolver.unwrap_api_response(schema)

        if inner is not None:
            example = resolver.to_example(inner)
            indented = "\n".join("  " + l for l in example.splitlines())
            lines.append(f"- **{status}** `{display_name}`")
            lines.append(f"  ```json\n{indented}\n  ```")
        else:
            # bare ApiResponse (error) or no schema
            label = f"`{display_name}`" if display_name else ""
            lines.append(f"- **{status}** {label} {desc}".strip())

    return "\n".join(lines)


def build_endpoint_section(path: str, method: str, op: dict, resolver: SchemaResolver) -> str:
    tag = (op.get("tags") or [""])[0]
    auth_required = tag in AUTHED_TAGS

    lines = []
    method_upper = method.upper()

    # ── 標題
    lines.append(f"### `{method_upper}` `{path}`")
    if auth_required:
        role = "Admin" if tag in ADMIN_TAGS else "Bearer"
        lines.append(f"> 需要 `{role}` Token")
    lines.append("")

    # ── Query / Path 參數
    params = op.get("parameters", [])
    if params:
        lines.append(format_params(params))
        lines.append("")

    # ── Request Body
    req_body = op.get("requestBody")
    if req_body:
        lines.append("**Request**")
        lines.append("")
        body_md = format_request_body(req_body, resolver)
        if body_md:
            lines.append(body_md)
        lines.append("")

    # ── Responses
    responses = op.get("responses", {})
    if responses:
        lines.append("**Responses**")
        lines.append("")
        lines.append(format_responses(responses, resolver))
        lines.append("")

    return "\n".join(lines)


def build_tag_section(tag: str, endpoints: list, resolver: SchemaResolver) -> str:
    lines = [f"## {tag}", ""]

    # 摘要表格
    lines.append("| Method | URL | Auth |")
    lines.append("|--------|-----|:----:|")
    for path, method, op in endpoints:
        auth_icon = "✓" if tag in AUTHED_TAGS else "✗"
        lines.append(f"| `{method.upper()}` | `{path}` | {auth_icon} |")
    lines.append("")
    lines.append("---")
    lines.append("")

    # 各 endpoint 詳細
    for path, method, op in endpoints:
        lines.append(build_endpoint_section(path, method, op, resolver))
        lines.append("---")
        lines.append("")

    return "\n".join(lines)


# ─── 主流程 ──────────────────────────────────────────────────────────────────

def convert(input_path: Path, output_path: Path):
    with open(input_path, encoding="utf-8") as f:
        spec = json.load(f)

    resolver = SchemaResolver(spec.get("components", {}))
    paths    = spec.get("paths", {})
    info     = spec.get("info", {})

    # 按 tag 分組
    tag_endpoints: dict[str, list] = defaultdict(list)
    for path, methods in paths.items():
        for method in METHOD_ORDER:
            if method not in methods:
                continue
            op  = methods[method]
            tag = (op.get("tags") or ["Other"])[0]
            tag_endpoints[tag].append((path, method, op))

    # 依 TAG_ORDER 排序，剩餘 tag 依 spec 出現順序附在後面
    ordered_tags = []
    for t in TAG_ORDER:
        if t in tag_endpoints:
            ordered_tags.append(t)
    for t in tag_endpoints:
        if t not in ordered_tags:
            ordered_tags.append(t)

    # 建 Markdown
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as out:
        title   = info.get("title", "API Spec")
        version = info.get("version", "")
        out.write(f"# {title}\n\n")
        out.write(f"> 版本：`{version}`　所有端點皆使用版本前綴 `/api/v1/`。\n")
        out.write("> 需要 JWT 的端點請在 Header 帶上 `Authorization: Bearer <token>`。\n\n")

        # Response Envelope 說明
        out.write("## Response Envelope\n\n")
        out.write("所有 JSON response 統一包在 `ApiResponse<T>` 中：\n\n")
        out.write("```json\n")
        out.write('// 成功\n{ "isSuccess": true, "code": "ok", "data": { ... } }\n\n')
        out.write('// 失敗\n{ "isSuccess": false, "code": "error_code",\n')
        out.write('  "problem": { "status": 400, "title": "error_code", "detail": "..." } }\n')
        out.write("```\n\n---\n\n")

        for tag in ordered_tags:
            out.write(build_tag_section(tag, tag_endpoints[tag], resolver))

    print(f"✓ {output_path}")


if __name__ == "__main__":
    inp = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_INPUT
    out = Path(sys.argv[2]) if len(sys.argv) > 2 else DEFAULT_OUTPUT
    convert(inp, out)
