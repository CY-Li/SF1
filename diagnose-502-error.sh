#!/bin/bash

# ROSCA 系統 502 錯誤診斷腳本
# 用於診斷 https://sf-test.zeabur.app/ 的 502 錯誤

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

DOMAIN="https://sf-test.zeabur.app"

echo -e "${BLUE}=== ROSCA 系統 502 錯誤診斷 ===${NC}"
echo -e "${BLUE}域名: ${DOMAIN}${NC}"
echo -e "${BLUE}時間: $(date)${NC}"
echo ""

# 診斷函數
check_http_status() {
    local url=$1
    local name=$2
    
    echo -n "檢查 ${name}... "
    
    local status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 30 "$url" 2>/dev/null || echo "000")
    local response_time=$(curl -s -o /dev/null -w "%{time_total}" --max-time 30 "$url" 2>/dev/null || echo "timeout")
    
    case $status in
        200)
            echo -e "${GREEN}✓ 正常 (${status}) - ${response_time}s${NC}"
            return 0
            ;;
        502)
            echo -e "${RED}✗ 502 Bad Gateway - ${response_time}s${NC}"
            return 1
            ;;
        503)
            echo -e "${YELLOW}⚠ 503 Service Unavailable - ${response_time}s${NC}"
            return 1
            ;;
        504)
            echo -e "${YELLOW}⚠ 504 Gateway Timeout - ${response_time}s${NC}"
            return 1
            ;;
        000)
            echo -e "${RED}✗ 連接失敗 (無回應)${NC}"
            return 1
            ;;
        *)
            echo -e "${YELLOW}⚠ HTTP ${status} - ${response_time}s${NC}"
            return 1
            ;;
    esac
}

# 詳細診斷函數
detailed_check() {
    local url=$1
    local name=$2
    
    echo -e "${PURPLE}=== 詳細檢查: ${name} ===${NC}"
    
    echo "URL: $url"
    echo -n "HTTP 狀態: "
    curl -s -I --max-time 30 "$url" | head -1 || echo "連接失敗"
    
    echo -n "回應標頭: "
    curl -s -I --max-time 30 "$url" | grep -E "(Server|Content-Type|X-|Cache)" | head -5 || echo "無標頭資訊"
    
    echo -n "連接時間分析: "
    curl -s -o /dev/null -w "DNS:%{time_namelookup}s 連接:%{time_connect}s SSL:%{time_appconnect}s 總計:%{time_total}s" --max-time 30 "$url" 2>/dev/null || echo "連接失敗"
    echo ""
    echo ""
}

echo -e "${YELLOW}=== 基本連接測試 ===${NC}"

# 1. 主域名
check_http_status "${DOMAIN}/" "主域名"

# 2. 健康檢查端點
check_http_status "${DOMAIN}/health" "健康檢查"

# 3. 後台系統
check_http_status "${DOMAIN}/admin/" "後台系統"

# 4. API 端點
check_http_status "${DOMAIN}/api/" "API 根路徑"

# 5. Swagger 文檔
check_http_status "${DOMAIN}/swagger/" "Swagger 文檔"

echo ""
echo -e "${YELLOW}=== 詳細診斷 ===${NC}"

# 詳細檢查主要端點
detailed_check "${DOMAIN}/" "主域名"
detailed_check "${DOMAIN}/health" "健康檢查"

echo -e "${YELLOW}=== 網路層診斷 ===${NC}"

# DNS 解析
echo -n "DNS 解析: "
nslookup sf-test.zeabur.app 2>/dev/null | grep "Address:" | tail -1 || echo "DNS 解析失敗"

# Ping 測試
echo -n "Ping 測試: "
ping -c 3 sf-test.zeabur.app 2>/dev/null | tail -1 || echo "Ping 失敗"

# 端口檢查
echo -n "端口 443 (HTTPS): "
timeout 10 bash -c "</dev/tcp/sf-test.zeabur.app/443" 2>/dev/null && echo "開放" || echo "關閉或無回應"

echo -n "端口 80 (HTTP): "
timeout 10 bash -c "</dev/tcp/sf-test.zeabur.app/80" 2>/dev/null && echo "開放" || echo "關閉或無回應"

echo ""
echo -e "${YELLOW}=== 502 錯誤可能原因分析 ===${NC}"

# 檢查是否是應用程式啟動問題
echo "正在分析 502 錯誤原因..."

# 嘗試不同的端點來判斷問題
health_status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "${DOMAIN}/health" 2>/dev/null || echo "000")
api_status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "${DOMAIN}/api/" 2>/dev/null || echo "000")

if [ "$health_status" = "502" ] && [ "$api_status" = "502" ]; then
    echo -e "${RED}❌ 後端服務 (.NET) 可能未正常啟動${NC}"
    echo "   - 檢查 .NET 應用程式是否正常運行"
    echo "   - 檢查端口 5000 和 5001 是否正確監聽"
    echo "   - 檢查環境變數配置"
elif [ "$health_status" = "200" ]; then
    echo -e "${GREEN}✓ 後端服務正常，可能是前端路由問題${NC}"
    echo "   - Nginx 配置可能有問題"
    echo "   - 前端檔案可能未正確部署"
else
    echo -e "${YELLOW}⚠ 混合狀態，需要進一步檢查${NC}"
fi

echo ""
echo -e "${YELLOW}=== 建議的修復步驟 ===${NC}"

echo "1. 檢查 Zeabur 服務日誌:"
echo "   - 登入 Zeabur Dashboard"
echo "   - 選擇您的服務"
echo "   - 查看 'Logs' 標籤"

echo ""
echo "2. 檢查應用程式狀態:"
echo "   curl -v ${DOMAIN}/health"

echo ""
echo "3. 檢查 Nginx 配置:"
echo "   - 確認 Nginx 是否正確代理到後端"
echo "   - 檢查端口配置 (5000, 5001)"

echo ""
echo "4. 檢查環境變數:"
echo "   - ASPNETCORE_URLS"
echo "   - 資料庫連接字串"
echo "   - JWT 配置"

echo ""
echo "5. 重啟服務 (在 Zeabur Dashboard):"
echo "   - 停止服務"
echo "   - 重新啟動"

echo ""
echo -e "${BLUE}=== 診斷完成 ===${NC}"
echo "如需更多幫助，請提供 Zeabur 服務日誌內容。"