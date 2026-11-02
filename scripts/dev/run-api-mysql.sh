#!/usr/bin/env bash
# Yerel geliştirme/test: Docker'da MySQL 8'i (yoksa) başlatır, test veritabanını SIFIRDAN kurar ve API'yi
# http://localhost:8081 adresinde arka planda çalıştırır. Gerçek veritabanına karşı KULLANMAYIN.
#
# Kullanım: scripts/dev/run-api-mysql.sh [--keep] [--demo]
#   --keep  veritabanını silmeden başlat (mevcut test verisi kalır)
#   --demo  sunum demo verisini kur (parola: DemoParola2026)
# Log: /tmp/sinav-api.log · Yönetici: admin@duzce.edu.tr / IlkParola2026 (ilk girişte değiştirilir)
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
KEEP=0; DEMO=0
for arg in "$@"; do
  case "$arg" in
    --keep) KEEP=1 ;;
    --demo) DEMO=1 ;;
  esac
done

DB=sinav_takvimi
ROOT_PW=Test_Sifre_2026
APP_PW=Sinav_App_2026

if ! docker info >/dev/null 2>&1; then
  (nohup dockerd >/tmp/dockerd.log 2>&1 &)
  for _ in $(seq 1 30); do docker info >/dev/null 2>&1 && break; sleep 1; done
fi
if ! docker ps -a --format '{{.Names}}' | grep -qx mysql8; then
  docker run -d --name mysql8 -e MYSQL_ROOT_PASSWORD=$ROOT_PW -p 3306:3306 mysql:8.0 >/dev/null
fi
docker start mysql8 >/dev/null
for _ in $(seq 1 60); do docker exec mysql8 mysqladmin -uroot -p$ROOT_PW ping 2>/dev/null | grep -q alive && break; sleep 2; done

# Önceki API sürecini durdur
for p in $(ps -eo pid,comm | awk '$2=="API"{print $1}'); do kill "$p"; done
for p in $(ps -eo pid,args | awk '/dotnet API.dll/ && !/awk/ {print $1}'); do kill "$p"; done
sleep 2

SQL="CREATE USER IF NOT EXISTS 'sinav'@'%' IDENTIFIED BY '$APP_PW'; GRANT ALL ON $DB.* TO 'sinav'@'%';"
if [ "$KEEP" = 0 ]; then SQL="DROP DATABASE IF EXISTS $DB; CREATE DATABASE $DB; $SQL"; else SQL="CREATE DATABASE IF NOT EXISTS $DB; $SQL"; fi
docker exec mysql8 mysql -uroot -p$ROOT_PW -e "$SQL" 2>&1 | grep -v Warning || true

cd "$ROOT/API"
export ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=$DB;User=sinav;Password=$APP_PW;"
export Database__Provider=MySql Database__MigrateOnStartup=true
export TokenOptions__SecurityKey='test-anahtari-0123456789-0123456789-0123456789-0123456789-abcdef'
export Seed__AdminEmail='admin@duzce.edu.tr' Seed__AdminPassword='IlkParola2026'
export ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:8081
if [ "$DEMO" = 1 ]; then export Seed__DemoData=true Seed__DemoPassword='DemoParola2026'; fi
nohup dotnet run --no-launch-profile >/tmp/sinav-api.log 2>&1 &
for _ in $(seq 1 90); do curl -sf localhost:8081/health >/dev/null && echo "API hazır: http://localhost:8081" && exit 0; sleep 2; done
echo "API açılmadı:"; grep -E "ERR|FTL|Exception" /tmp/sinav-api.log | head -20; exit 1
