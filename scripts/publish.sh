#!/usr/bin/env bash
# publish.ps1'in Linux/macOS karşılığı (test ve CI için).
# Kullanım: scripts/publish.sh ../sinav-takvimi-frontend [cikti-klasoru]
# Ortam değişkenleri: RUNTIME=win-x64 (sadece o platformun dosyaları), WEB_CONFIG=deploy/web.plesk.config
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
if [ -n "${RUNTIME:-}" ]; then
  # Taşınabilir paketten hedef platform dışındaki yerel kütüphaneleri çıkar (ör. win-x64 → sadece win, win-x64 kalır).
  # RID'li publish, Core'daki eski Microsoft.AspNetCore.Mvc.Core 2.x paket zinciri yüzünden NU1605 veriyor.
  for d in "$OUT"/runtimes/*/; do
    name=$(basename "$d")
    [ "$name" = "$RUNTIME" ] || [ "$name" = "${RUNTIME%%-*}" ] || rm -rf "$d"
  done
fi
cp "$ROOT/${WEB_CONFIG:-deploy/web.config}" "$OUT/web.config"
cp "$ROOT/db/schema.sql" "$OUT/schema.sql"
cp "$ROOT/db/schema.mysql.sql" "$OUT/schema.mysql.sql"
mkdir -p "$OUT/logs"
echo "Hazır: $OUT"
