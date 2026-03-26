#!/bin/bash
# ═══════════════════════════════════════════════════════════
# Smoke Test — Website_QLPT
# Dùng cho cả Staging và Production
# Usage: bash scripts/smoke-test.sh <BASE_URL>
# Example: bash scripts/smoke-test.sh https://staging.qlpt.vn
# ═══════════════════════════════════════════════════════════

BASE="${1:-https://localhost:5001}"
FAILED=0

check() {
    local URL="$1"
    local LABEL="$2"
    if curl -sf --max-time 10 "$URL" > /dev/null; then
        echo "✅ PASS — $LABEL ($URL)"
    else
        echo "❌ FAIL — $LABEL ($URL)"
        FAILED=$((FAILED + 1))
    fi
}

echo "═══════════════════════════════════════"
echo "  Smoke Test: $BASE"
echo "═══════════════════════════════════════"

check "$BASE/health"                        "Health Check"
check "$BASE/health/live"                   "Liveness Probe"
check "$BASE/health/ready"                  "Readiness Probe"
check "$BASE/Identity/Account/Login"        "Login Page"
check "$BASE/api/v1/properties"             "API v1 Properties"
check "$BASE/api/v1/rooms"                  "API v1 Rooms"

echo "═══════════════════════════════════════"
if [ $FAILED -eq 0 ]; then
    echo "✅ All smoke tests PASSED on $BASE"
    exit 0
else
    echo "❌ $FAILED test(s) FAILED on $BASE"
    exit 1
fi
