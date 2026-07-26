#!/usr/bin/env bash
#
# Triggers the daily price download. Intended for cron; the application deliberately has no
# internal scheduler, so this script (or the Task Scheduler equivalent) is what makes the
# nightly job happen.
#
# Install (weekdays at 21:30, comfortably after the 16:00 ET close plus provider lag):
#
#   30 21 * * 1-5 /opt/finance-analysis/scripts/daily-ingest.sh >> /var/log/finance-ingest.log 2>&1
#
# Configuration comes from the environment, or from a .env file next to the compose project.

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
env_file="${ENV_FILE:-${script_dir}/../.env}"

if [[ -f "${env_file}" ]]; then
    # shellcheck disable=SC1090
    set -a && source "${env_file}" && set +a
fi

api_base="${API_BASE_URL:-http://127.0.0.1:${API_LOOPBACK_PORT:-5080}}"
api_key="${INTERNAL_API_KEY:-}"

if [[ -z "${api_key}" ]]; then
    echo "INTERNAL_API_KEY is not set (looked in the environment and ${env_file})." >&2
    exit 78 # EX_CONFIG
fi

# Optional first argument pins a trade date (yyyy-MM-dd); otherwise the API resolves the most
# recent trading day itself.
query=""
if [[ $# -ge 1 ]]; then
    query="?tradeDate=$1"
fi

url="${api_base}/api/internal/ingestion/daily-prices${query}"

echo "[$(date --iso-8601=seconds)] POST ${url}"

response="$(curl --silent --show-error --fail-with-body \
    --max-time 30 \
    --request POST \
    --header "X-Internal-Api-Key: ${api_key}" \
    --header 'Content-Length: 0' \
    --write-out '\n%{http_code}' \
    "${url}")"

status="$(tail -n1 <<<"${response}")"
body="$(sed '$d' <<<"${response}")"

echo "${body}"

# 202 means the job was queued; 200 means the day was already ingested and was skipped. Both
# are successful outcomes for a job that must be safe to re-run.
if [[ "${status}" == "202" || "${status}" == "200" ]]; then
    echo "[$(date --iso-8601=seconds)] Ingestion accepted (HTTP ${status})."
    exit 0
fi

echo "[$(date --iso-8601=seconds)] Ingestion request failed with HTTP ${status}." >&2
exit 1
