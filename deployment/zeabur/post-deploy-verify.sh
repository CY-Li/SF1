#!/bin/bash

# ROSCA 平安商會系統 Zeabur 部署後驗證腳本
# 用於驗證部署後的系統功能是否正常

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 配置變數
PROJECT_NAME="rosca-system"
TIMEOUT=30
RETRY_COUNT=3

echo -e "${BLUE}=== ROSCA Zeabur 部署後驗證工具 ===${NC}"
echo "驗證時間: $(date)"
echo "專案名稱: $PROJECT_NAME"
echo ""

# 驗證結果統計
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# 函數：執行測試
run_test() {
    local test_name="$1"
    local test_function="$2"
    
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    echo -e "${YELLOW}測試: $test_name${NC}"
    
    if $test_function; then
        echo -e "${GREEN}✓ 通過: $test_name${NC}"
        PASSED_TESTS=$((PASSED_TESTS + 1))
        return 0
    else
        echo -e "${RED}✗ 失敗: $test_name${NC}"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        return 1
    fi
}

# 函數：HTTP 請求測試
http_test() {
    local url="$1"
    local expected_status="$2"
    local description="$3"
    
    echo "  測試 URL: $url"
    
    local response=$(curl -s -w "%{http_code}" -o /dev/null --max-time $TIMEOUT "$url" 2>/dev/null || echo "000")
    
    if [ "$response" = "$expected_status" ]; then
        echo "  回應狀態: $response (預期: $expected_status) ✓"
        return 0
    else
        echo "  回應狀態: $response (預期: $expected_status) ✗"
        return 1
    fi
}

# 函數：JSON API 測試
json_api_test() {
    local url="$1"
    local expected_field="$2"
    local description="$3"
    
    echo "  測試 API: $url"
    
    local response=$(curl -s --max-time $TIMEOUT "$url" 2>/dev/null)
    
    if [ -z "$response" ]; then
        echo "  錯誤: 無回應"
        return 1
    fi
    
    if echo "$response" | jq -e "$expected_field" >/dev/null 2>&1; then
        echo "  JSON 格式: 有效 ✓"
        echo "  預期欄位: $expected_field ✓"
        return 0
    else
        echo "  JSON 格式: 無效或缺少欄位 ✗"
        echo "  回應內容: $response"
        return 1
    fi
}

# 函數：取得服務 URL
get_service_url() {
    local service_name="$1"
    
    # 這裡應該使用 Zeabur CLI 或 API 取得實際的服務 URL
    # 暫時使用環境變數或預設格式
    case "$service_name" in
        "frontend")
            echo "${FRONTEND_URL:-https://$PROJECT_NAME-frontend.zeabur.app}"
            ;;
        "admin")
            echo "${ADMIN_URL:-https://$PROJECT_NAME-admin.zeabur.app}"
            ;;
        "backend")
            echo "${BACKEND_URL:-https://$PROJECT_NAME-backend.zeabur.app}"
            ;;
        "backend-service")
            echo "${BACKEND_SERVICE_URL:-https://$PROJECT_NAME-backend-service.zeabur.app}"
            ;;
        *)
            echo ""
            ;;
    esac
}

# 測試：前台系統可用性
test_frontend_availability() {
    local url=$(get_service_url "frontend")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得前台 URL"
        return 1
    fi
    
    http_test "$url" "200" "前台首頁"
}

# 測試：後台系統可用性
test_admin_availability() {
    local url=$(get_service_url "admin")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得後台 URL"
        return 1
    fi
    
    http_test "$url" "200" "後台首頁"
}

# 測試：API Gateway 健康檢查
test_api_gateway_health() {
    local url=$(get_service_url "backend")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得 API Gateway URL"
        return 1
    fi
    
    json_api_test "$url/health" ".status" "API Gateway 健康檢查"
}

# 測試：Backend Service 健康檢查
test_backend_service_health() {
    local url=$(get_service_url "backend-service")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得 Backend Service URL"
        return 1
    fi
    
    json_api_test "$url/health" ".status" "Backend Service 健康檢查"
}

# 測試：資料庫連接
test_database_connection() {
    local url=$(get_service_url "backend")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得 API URL"
        return 1
    fi
    
    # 測試資料庫健康檢查端點
    json_api_test "$url/health/database" ".status" "資料庫連接"
}

# 測試：JWT 認證端點
test_jwt_authentication() {
    local url=$(get_service_url "backend")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得 API URL"
        return 1
    fi
    
    # 測試登入端點 (應該返回 400 或 401，表示端點存在)
    local response=$(curl -s -w "%{http_code}" -o /dev/null --max-time $TIMEOUT \
        -X POST "$url/api/auth/login" \
        -H "Content-Type: application/json" \
        -d '{}' 2>/dev/null || echo "000")
    
    if [ "$response" = "400" ] || [ "$response" = "401" ]; then
        echo "  認證端點: 可用 ✓"
        return 0
    else
        echo "  認證端點: 不可用 (狀態碼: $response) ✗"
        return 1
    fi
}

# 測試：檔案上傳配置
test_file_upload_config() {
    local url=$(get_service_url "backend")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得 API URL"
        return 1
    fi
    
    # 測試檔案上傳配置端點
    json_api_test "$url/api/fileupload/config" ".maxFileSize" "檔案上傳配置"
}

# 測試：CORS 配置
test_cors_configuration() {
    local backend_url=$(get_service_url "backend")
    local frontend_url=$(get_service_url "frontend")
    
    if [ -z "$backend_url" ] || [ -z "$frontend_url" ]; then
        echo "  錯誤: 無法取得服務 URL"
        return 1
    fi
    
    # 測試 CORS 預檢請求
    local response=$(curl -s -w "%{http_code}" -o /dev/null --max-time $TIMEOUT \
        -X OPTIONS "$backend_url/api/auth/login" \
        -H "Origin: $frontend_url" \
        -H "Access-Control-Request-Method: POST" \
        -H "Access-Control-Request-Headers: Content-Type" 2>/dev/null || echo "000")
    
    if [ "$response" = "200" ] || [ "$response" = "204" ]; then
        echo "  CORS 配置: 正常 ✓"
        return 0
    else
        echo "  CORS 配置: 異常 (狀態碼: $response) ✗"
        return 1
    fi
}

# 測試：SSL 憑證
test_ssl_certificates() {
    local services=("frontend" "admin" "backend" "backend-service")
    
    for service in "${services[@]}"; do
        local url=$(get_service_url "$service")
        
        if [ -z "$url" ]; then
            continue
        fi
        
        # 檢查 SSL 憑證
        if echo "$url" | grep -q "https://"; then
            local ssl_check=$(curl -s --max-time $TIMEOUT -I "$url" 2>&1 | grep -i "HTTP" || echo "FAILED")
            
            if echo "$ssl_check" | grep -q "200\|301\|302"; then
                echo "  $service SSL: 有效 ✓"
            else
                echo "  $service SSL: 無效 ✗"
                return 1
            fi
        else
            echo "  $service: 未使用 HTTPS ⚠"
        fi
    done
    
    return 0
}

# 測試：存儲卷掛載
test_storage_volumes() {
    local url=$(get_service_url "backend")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得 API URL"
        return 1
    fi
    
    # 測試存儲統計端點 (需要管理員權限，可能返回 401)
    local response=$(curl -s -w "%{http_code}" -o /dev/null --max-time $TIMEOUT \
        "$url/api/fileupload/storage-stats" 2>/dev/null || echo "000")
    
    if [ "$response" = "401" ] || [ "$response" = "403" ]; then
        echo "  存儲端點: 可用 (需要認證) ✓"
        return 0
    elif [ "$response" = "200" ]; then
        echo "  存儲端點: 可用 ✓"
        return 0
    else
        echo "  存儲端點: 不可用 (狀態碼: $response) ✗"
        return 1
    fi
}

# 測試：服務間通信
test_service_communication() {
    local gateway_url=$(get_service_url "backend")
    
    if [ -z "$gateway_url" ]; then
        echo "  錯誤: 無法取得 API Gateway URL"
        return 1
    fi
    
    # 測試 API Gateway 是否能正常代理到 Backend Service
    # 這裡測試一個需要 Backend Service 的端點
    local response=$(curl -s -w "%{http_code}" -o /dev/null --max-time $TIMEOUT \
        "$gateway_url/api/system/info" 2>/dev/null || echo "000")
    
    if [ "$response" = "200" ] || [ "$response" = "401" ] || [ "$response" = "403" ]; then
        echo "  服務間通信: 正常 ✓"
        return 0
    else
        echo "  服務間通信: 異常 (狀態碼: $response) ✗"
        return 1
    fi
}

# 測試：回應時間
test_response_time() {
    local url=$(get_service_url "backend")
    
    if [ -z "$url" ]; then
        echo "  錯誤: 無法取得 API URL"
        return 1
    fi
    
    # 測試健康檢查端點的回應時間
    local start_time=$(date +%s%N)
    local response=$(curl -s --max-time $TIMEOUT "$url/health" 2>/dev/null || echo "")
    local end_time=$(date +%s%N)
    
    if [ -z "$response" ]; then
        echo "  回應時間測試: 失敗 (無回應) ✗"
        return 1
    fi
    
    local duration=$(( (end_time - start_time) / 1000000 )) # 轉換為毫秒
    
    echo "  回應時間: ${duration}ms"
    
    if [ $duration -lt 2000 ]; then # 2秒內
        echo "  回應時間: 良好 ✓"
        return 0
    elif [ $duration -lt 5000 ]; then # 5秒內
        echo "  回應時間: 可接受 ⚠"
        return 0
    else
        echo "  回應時間: 過慢 ✗"
        return 1
    fi
}

# 函數：載入環境變數
load_environment() {
    if [ -f ".env.zeabur" ]; then
        echo -e "${YELLOW}載入環境變數...${NC}"
        set -a
        source .env.zeabur
        set +a
    fi
    
    # 也可以從命令列參數載入 URL
    while [[ $# -gt 0 ]]; do
        case $1 in
            --frontend-url)
                FRONTEND_URL="$2"
                shift 2
                ;;
            --admin-url)
                ADMIN_URL="$2"
                shift 2
                ;;
            --backend-url)
                BACKEND_URL="$2"
                shift 2
                ;;
            --backend-service-url)
                BACKEND_SERVICE_URL="$2"
                shift 2
                ;;
            --project)
                PROJECT_NAME="$2"
                shift 2
                ;;
            *)
                shift
                ;;
        esac
    done
}

# 函數：顯示服務 URL
show_service_urls() {
    echo -e "${CYAN}📋 服務 URL${NC}"
    echo "前台系統: $(get_service_url "frontend")"
    echo "後台系統: $(get_service_url "admin")"
    echo "API Gateway: $(get_service_url "backend")"
    echo "Backend Service: $(get_service_url "backend-service")"
    echo ""
}

# 函數：顯示測試結果摘要
show_test_summary() {
    echo -e "${PURPLE}📊 測試結果摘要${NC}"
    echo "總測試項目: $TOTAL_TESTS"
    echo -e "${GREEN}通過: $PASSED_TESTS${NC}"
    echo -e "${RED}失敗: $FAILED_TESTS${NC}"
    
    local success_rate=0
    if [ $TOTAL_TESTS -gt 0 ]; then
        success_rate=$((PASSED_TESTS * 100 / TOTAL_TESTS))
    fi
    
    echo "成功率: ${success_rate}%"
    echo ""
    
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "${GREEN}🎉 所有測試通過！系統部署成功。${NC}"
        
        echo ""
        echo -e "${BLUE}建議的後續步驟:${NC}"
        echo "1. 建立管理員帳號"
        echo "2. 測試用戶註冊登入功能"
        echo "3. 驗證檔案上傳功能"
        echo "4. 配置監控和告警"
        echo "5. 設定定期備份"
        
        return 0
    else
        echo -e "${RED}❌ 有 $FAILED_TESTS 個測試失敗，請檢查系統配置。${NC}"
        
        echo ""
        echo -e "${BLUE}故障排除建議:${NC}"
        echo "1. 檢查服務日誌: zeabur logs --project $PROJECT_NAME --service [service-name]"
        echo "2. 驗證環境變數配置"
        echo "3. 檢查服務間網路連接"
        echo "4. 確認資料庫初始化完成"
        
        return 1
    fi
}

# 主函數
main() {
    load_environment "$@"
    
    echo -e "${PURPLE}🧪 開始部署後驗證${NC}"
    echo ""
    
    show_service_urls
    
    # 基礎可用性測試
    echo -e "${BLUE}=== 基礎可用性測試 ===${NC}"
    run_test "前台系統可用性" test_frontend_availability
    run_test "後台系統可用性" test_admin_availability
    run_test "API Gateway 健康檢查" test_api_gateway_health
    run_test "Backend Service 健康檢查" test_backend_service_health
    
    echo ""
    
    # 功能測試
    echo -e "${BLUE}=== 功能測試 ===${NC}"
    run_test "資料庫連接" test_database_connection
    run_test "JWT 認證端點" test_jwt_authentication
    run_test "檔案上傳配置" test_file_upload_config
    run_test "服務間通信" test_service_communication
    
    echo ""
    
    # 安全和配置測試
    echo -e "${BLUE}=== 安全和配置測試 ===${NC}"
    run_test "CORS 配置" test_cors_configuration
    run_test "SSL 憑證" test_ssl_certificates
    run_test "存儲卷掛載" test_storage_volumes
    
    echo ""
    
    # 效能測試
    echo -e "${BLUE}=== 效能測試 ===${NC}"
    run_test "回應時間" test_response_time
    
    echo ""
    
    show_test_summary
}

# 執行主函數
main "$@"