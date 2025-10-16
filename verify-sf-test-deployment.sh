#!/bin/bash

# ROSCA 系統部署驗證腳本 - sf-test.zeabur.app
# 驗證 https://sf-test.zeabur.app/ 的部署狀態

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

DOMAIN="https://sf-test.zeabur.app"

echo -e "${BLUE}=== ROSCA 系統部署驗證 ===${NC}"
echo -e "${BLUE}域名: ${DOMAIN}${NC}"
echo -e "${BLUE}時間: $(date)${NC}"
echo ""

# 檢查函數
check_endpoint() {
    local url=$1
    local name=$2
    local expected_status=${3:-200}
    
    echo -n "檢查 ${name}... "
    
    if curl -s -o /dev/null -w "%{http_code}" --max-time 30 "$url" | grep -q "$expected_status"; then
        echo -e "${GREEN}✓ 正常${NC}"
        return 0
    else
        echo -e "${RED}✗ 失敗${NC}"
        return 1
    fi
}

# 檢查 JSON 回應
check_json_endpoint() {
    local url=$1
    local name=$2
    
    echo -n "檢查 ${name}... "
    
    local response=$(curl -s --max-time 30 "$url")
    if echo "$response" | jq . >/dev/null 2>&1; then
        echo -e "${GREEN}✓ 正常 (JSON)${NC}"
        return 0
    else
        echo -e "${RED}✗ 失敗 (非 JSON 或無回應)${NC}"
        echo "回應: $response"
        return 1
    fi
}

echo -e "${YELLOW}=== 基本連接測試 ===${NC}"

# 1. 前台系統
check_endpoint "${DOMAIN}/" "前台系統"

# 2. 後台系統
check_endpoint "${DOMAIN}/admin/" "後台系統"

# 3. API 健康檢查
check_json_endpoint "${DOMAIN}/health" "API 健康檢查"

# 4. Swagger 文檔
check_endpoint "${DOMAIN}/swagger/" "Swagger 文檔"

echo ""
echo -e "${YELLOW}=== API 功能測試 ===${NC}"

# 5. 系統資訊 API
check_json_endpoint "${DOMAIN}/api/system/info" "系統資訊 API"

# 6. 健康檢查詳細資訊
echo -n "獲取健康檢查詳細資訊... "
health_response=$(curl -s --max-time 30 "${DOMAIN}/health")
if echo "$health_response" | jq . >/dev/null 2>&1; then
    echo -e "${GREEN}✓ 成功${NC}"
    echo "健康狀態:"
    echo "$health_response" | jq .
else
    echo -e "${RED}✗ 失敗${NC}"
    echo "回應: $health_response"
fi

echo ""
echo -e "${YELLOW}=== 靜態資源測試 ===${NC}"

# 7. 檢查靜態資源
check_endpoint "${DOMAIN}/favicon.ico" "Favicon"

echo ""
echo -e "${YELLOW}=== 效能測試 ===${NC}"

# 8. 回應時間測試
echo -n "測試回應時間... "
response_time=$(curl -s -o /dev/null -w "%{time_total}" --max-time 30 "${DOMAIN}/health")
if (( $(echo "$response_time < 2.0" | bc -l) )); then
    echo -e "${GREEN}✓ ${response_time}s (良好)${NC}"
else
    echo -e "${YELLOW}⚠ ${response_time}s (較慢)${NC}"
fi

echo ""
echo -e "${BLUE}=== 部署驗證完成 ===${NC}"

# 總結
echo ""
echo -e "${BLUE}快速存取連結:${NC}"
echo -e "前台系統: ${DOMAIN}/"
echo -e "後台系統: ${DOMAIN}/admin/"
echo -e "API 文檔: ${DOMAIN}/swagger/"
echo -e "健康檢查: ${DOMAIN}/health"

echo ""
echo -e "${GREEN}驗證完成！${NC}"
echo "如有問題，請檢查 Zeabur 服務日誌。"