#!/usr/bin/env bash
# Fresh VPS bootstrap for MyFreelance (standalone SQL + web).
# Run as root on Debian 12:
#   curl -fsSL https://raw.githubusercontent.com/minasghazaryan/myfreelance/main/deploy/setup-standalone.sh | bash
#
# Or after git clone:
#   bash deploy/setup-standalone.sh

set -euo pipefail

REPO_URL="${REPO_URL:-https://github.com/minasghazaryan/myfreelance.git}"
APP_DIR="${APP_DIR:-/opt/myfreelance}"
HOST_PORT="${MYFREELANCE_HOST_PORT:-8082}"

if [[ "${EUID:-$(id -u)}" -ne 0 ]]; then
  echo "Run as root (e.g. sudo bash deploy/setup-standalone.sh)" >&2
  exit 1
fi

echo "==> Installing dependencies..."
export DEBIAN_FRONTEND=noninteractive
apt-get update -qq
apt-get install -y -qq git curl ca-certificates ufw

if ! command -v docker >/dev/null 2>&1; then
  echo "==> Installing Docker..."
  curl -fsSL https://get.docker.com | sh
fi

echo "==> Cloning/updating app in ${APP_DIR}..."
if [[ -d "${APP_DIR}/.git" ]]; then
  git -C "${APP_DIR}" pull --ff-only
else
  git clone "${REPO_URL}" "${APP_DIR}"
fi

cd "${APP_DIR}/deploy"

if [[ ! -f .env ]]; then
  echo "==> Creating deploy/.env with generated secrets..."
  sql_pw="$(openssl rand -base64 24 | tr -d '/+=' | head -c 24)Aa1!"
  jwt_key="$(openssl rand -base64 48 | tr -d '/+=' | head -c 48)"

  cp .env.standalone.example .env
  sed -i "s|MSSQL_SA_PASSWORD=.*|MSSQL_SA_PASSWORD=${sql_pw}|" .env
  sed -i "s|Password=ChangeMe_Strong_Sql_123!|Password=${sql_pw}|" .env
  sed -i "s|Jwt__Key=.*|Jwt__Key=${jwt_key}|" .env
  chmod 600 .env
  echo "    Saved secrets to ${APP_DIR}/deploy/.env"
else
  echo "==> Using existing ${APP_DIR}/deploy/.env"
fi

echo "==> Building and starting containers (first run may take several minutes)..."
docker compose -f docker-compose.standalone.yml --env-file .env up -d --build

echo "==> Configuring firewall..."
ufw allow OpenSSH >/dev/null 2>&1 || true
ufw allow "${HOST_PORT}/tcp" >/dev/null 2>&1 || true
echo "y" | ufw enable >/dev/null 2>&1 || true

public_ip="$(curl -fsS --max-time 5 ifconfig.me 2>/dev/null || hostname -I | awk '{print $1}')"

echo
echo "=============================================="
echo " MyFreelance is starting on port ${HOST_PORT}"
echo " URL: http://${public_ip}:${HOST_PORT}"
echo " Admin: admin@aurumwealth.gh / Admin@123!"
echo " Change admin password after first login."
echo " Env file: ${APP_DIR}/deploy/.env"
echo "=============================================="
echo
docker compose -f docker-compose.standalone.yml ps
