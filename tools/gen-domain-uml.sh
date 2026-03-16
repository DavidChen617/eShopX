#!/usr/bin/env bash
# 從 C# Domain 層產生 PlantUML class diagrams
# 需先安裝：dotnet tool install --global PlantUmlClassDiagramGenerator
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$SCRIPT_DIR/.."

puml-gen \
  "$ROOT/src/Domain/Aggregates" \
  "$ROOT/docs/domain" \
  -dir \
  -excludePaths obj,bin \
  -createAssociation

echo "✓ docs/domain/ 已更新"
