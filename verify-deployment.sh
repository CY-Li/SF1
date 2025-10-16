#!/bin/bash

# ROSCA 系統部署驗證腳本
# 用於驗證 Zeabur 部署是否成功

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}=== ROSCA 系統部署驗證 ===${NC}"
echo "驗證時間: $(date)"
echo ""

# 預設 URL (可透過參數覆蓋)
BASE_URL="${1:-https://your-app.zeabur.app}"

echo -e "${YELLOW}測試 URL: $BASE_URL${NC}"
echo ""

# 函數：HTTP 測試
test_endpoint() {
    local url="$1"
    local description="$2"
    local expected_status="${3:-200}"
    
    echo -n "測試 $description... "
    
    local status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$url" 2>/dev/null || echo "000")
    
    if [ "$status" = "$expected_status" ]; then
        echo -e "${GREEN}✓ 通過 ($status)${NC}"
        return 0
    else
        echo -e "${RED}✗ 失敗 ($status)${NC}"
        return 1
    fi
}

# 基本功能測試
echo -e "${BLUE}=== 基本功能測試 ===${NC}"
test_endpoint "$BASE_URL/" "前台系統"
test_endpoint "$BASE_URL/admin/" "後台系統"
test_endpoint "$BASE_URL/health" "健康檢查"
test_endpoint "$BASE_URL/api/system/info" "系統資訊" "401"

echo ""

# API 測試
echo -e "${BLUE}=== API 測試 ===${NC}"
test_endpoint "$BASE_URL/api/fileupload/config" "檔案上傳配置" "401"
test_endpoint "$BASE_URL/swagger/" "API 文檔" "200"

echo ""

# 靜態資源測試
echo -e "${BLUE}=== 靜態資源測試 ===${NC}"
test_endpoint "$BASE_URL/favicon.ico" "網站圖示"

echo ""

# 總結
echo -e "${GREEN}🎉 部署驗證完成！${NC}"
echo ""
echo -e "${BLUE}存取資訊:${NC}"
echo "前台系統: $BASE_URL/"
echo "後台系統: $BASE_URL/admin/"
echo "API 文檔: $BASE_URL/swagger/"
echo "健康檢查: $BASE_URL/health"
echo ""
echo -e "${YELLOW}注意: 某些 API 端點需要認證，返回 401 狀態碼是正常的。${NC}"