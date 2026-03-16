#!/usr/bin/env bash
# 將 spec.json 轉成 docs/api-spec.md
# 用法：
#   tools/specmd.sh                          # 預設
#   tools/specmd.sh path/to/spec.json        # 指定輸入
#   tools/specmd.sh spec.json output.md      # 指定輸入+輸出
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$SCRIPT_DIR/.."

INPUT="${1:-$ROOT/spec.json}"
OUTPUT="${2:-$ROOT/docs/api-spec.md}"

python3 "$SCRIPT_DIR/spec2md.py" "$INPUT" "$OUTPUT"
