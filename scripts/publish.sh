#!/usr/bin/env bash
# publish.ps1'in Linux/macOS karşılığı (test ve CI için).
# Kullanım: scripts/publish.sh ../sinav-takvimi-frontend [cikti-klasoru]
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
FRONTEND="$(cd "${1:?Frontend klasörünü verin}" && pwd)"
OUT="${2:-$ROOT/publish}"

echo "==> Frontend derleniyor: $FRONTEND"
(cd "$FRONTEND" && npm ci && VITE_API_BASE_URL= npm run build)

echo "==> dist -> API/wwwroot"
rm -rf "$ROOT/API/wwwroot"
cp -r "$FRONTEND/dist" "$ROOT/API/wwwroot"

echo "==> dotnet publish -> $OUT"
rm -rf "$OUT"
dotnet publish "$ROOT/API/API.csproj" -c Release -o "$OUT"
cp "$ROOT/deploy/web.config" "$OUT/web.config"
cp "$ROOT/db/schema.sql" "$OUT/schema.sql"
mkdir -p "$OUT/logs"
echo "Hazır: $OUT"
