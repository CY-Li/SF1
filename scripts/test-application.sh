#!/bin/bash

# ROSCA 應用程式功能測試腳本
# 測試前台、後台和 API 的基本功能

set -e

echo "🧪 開始 ROSCA 應用程式功能測試..."
echo "===================================="

# 測試結果統計
PASSED=0
FAILED=0
TOTAL=0

# 測試函數
test_function() {
    local test_name="$1"
    local test_command="$2"
    local expected_result="${3:-0}"
    
    echo -n "測試 $test_name: "
    ((TOTAL++))
    
    if eval "$test_command" >/dev/null 2>&1; then
        local result=0
    else
        local result=1
    fi
    
    if [ "$result" -eq "$expected_result" ]; then
        echo "✅ 通過"
        ((PASSED++))
    else
        echo "❌ 失敗"
        ((FAILED++))
    fi
}

# HTTP 測試函數
test_http() {
    local test_name="$1"
    local url="$2"
    local expected_status="${3:-200}"
    
    echo -n "測試 $test_name: "
    ((TOTAL++))
    
    local status=$(curl -s -o /dev/null -w "%{http_code}" "$url" 2>/dev/null || echo "000")
    
    if [ "$status" = "$expected_status" ]; then
        echo "✅ 通過 (HTTP $status)"
        ((PASSED++))
    else
        echo "❌ 失敗 (HTTP $status, 預期 $expected_status)"
        ((FAILED++))
    fi
}

# API 測試函數
test_api() {
    local test_name="$1"
    local endpoint="$2"
    local method="${3:-GET}"
    local data="${4:-}"
    
    echo -n "測試 $test_name: "
    ((TOTAL++))
    
    local url="http://localhost/api/$endpoint"
    local curl_cmd="curl -s -o /dev/null -w '%{http_code}'"
    
    if [ "$method" = "POST" ] && [ -n "$data" ]; then
        curl_cmd="$curl_cmd -X POST -H 'Content-Type: application/json' -d '$data'"
    elif [ "$method" = "POST" ]; then
        curl_cmd="$curl_cmd -X POST -H 'Content-Type: application/json'"
    fi
    
    local status=$(eval "$curl_cmd '$url'" 2>/dev/null || echo "000")
    
    # API 測試通常接受 200, 400, 401, 404 等狀態碼（表示 API 有回應）
    if [ "$status" != "000" ] && [ "$status" != "502" ] && [ "$status" != "503" ] && [ "$status" != "504" ]; then
        echo "✅ 通過 (HTTP $status)"
        ((PASSED++))
    else
        echo "❌ 失敗 (HTTP $status - 服務無回應)"
        ((FAILED++))
    fi
}

# 等待服務完全啟動
echo "⏳ 等待服務完全啟動..."
sleep 30

echo ""

# 測試前台系統訪問
echo "📱 測試前台使用者系統..."
echo "--------------------------------"

test_http "前台首頁載入" "http://localhost"
test_http "前台靜態資源" "http://localhost/favicon.ico" "200"

# 測試前台 API 代理
echo -n "測試前台 API 代理連通性: "
((TOTAL++))
response=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost/api/" 2>/dev/null || echo "000")
# API 根路徑可能返回 404，但不應該是 502/503 (代理錯誤)
if [ "$response" != "000" ] && [ "$response" != "502" ] && [ "$response" != "503" ]; then
    echo "✅ 通過 (HTTP $response)"
    ((PASSED++))
else
    echo "❌ 失敗 (HTTP $response - 代理配置錯誤)"
    ((FAILED++))
fi

echo ""

# 測試後台管理系統訪問
echo "🖥️ 測試後台管理系統..."
echo "--------------------------------"

test_http "後台首頁載入" "http://localhost:8080"
test_http "後台靜態資源" "http://localhost:8080/favicon.ico" "200"

# 測試後台 API 代理
echo -n "測試後台 API 代理連通性: "
((TOTAL++))
response=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:8080/api/" 2>/dev/null || echo "000")
if [ "$response" != "000" ] && [ "$response" != "502" ] && [ "$response" != "503" ]; then
    echo "✅ 通過 (HTTP $response)"
    ((PASSED++))
else
    echo "❌ 失敗 (HTTP $response - 代理配置錯誤)"
    ((FAILED++))
fi

echo ""

# 測試後端 API 功能
echo "🔌 測試後端 API 功能..."
echo "--------------------------------"

# 基本 API 端點測試
test_api "健康檢查端點" "health"
test_api "登入端點存在" "Login" "POST"
test_api "註冊端點存在" "Register" "POST"

# 測試需要認證的端點（應該返回 401）
echo -n "測試需要認證的端點: "
((TOTAL++))
response=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost/api/User/Profile" 2>/dev/null || echo "000")
if [ "$response" = "401" ] || [ "$response" = "403" ]; then
    echo "✅ 通過 (HTTP $response - 正確要求認證)"
    ((PASSED++))
elif [ "$response" = "404" ]; then
    echo "⚠️ 警告 (HTTP $response - 端點可能不存在)"
    ((PASSED++))
else
    echo "❌ 失敗 (HTTP $response - 認證機制異常)"
    ((FAILED++))
fi

echo ""

# 測試資料庫連接和預設資料
echo "🗄️ 測試資料庫功能..."
echo "--------------------------------"

test_function "資料庫連接測試" "docker-compose exec -T mariadb mysql -u root -p\$DB_ROOT_PASSWORD -e 'SELECT 1;'"
test_function "應用資料庫存在" "docker-compose exec -T mariadb mysql -u root -p\$DB_ROOT_PASSWORD -e 'USE rosca2; SELECT 1;'"

# 檢查重要資料表是否存在
echo -n "測試 Users 資料表存在: "
((TOTAL++))
if docker-compose exec -T mariadb mysql -u root -p$DB_ROOT_PASSWORD -e "USE rosca2; DESCRIBE Users;" >/dev/null 2>&1; then
    echo "✅ 通過"
    ((PASSED++))
else
    echo "❌ 失敗"
    ((FAILED++))
fi

# 檢查預設使用者是否存在
echo -n "測試預設使用者帳號 (0938766349): "
((TOTAL++))
user_count=$(docker-compose exec -T mariadb mysql -u root -p$DB_ROOT_PASSWORD -e "USE rosca2; SELECT COUNT(*) FROM Users WHERE Phone='0938766349';" 2>/dev/null | tail -n 1 | tr -d '\r')
if [ "$user_count" = "1" ]; then
    echo "✅ 通過"
    ((PASSED++))
else
    echo "❌ 失敗 (找到 $user_count 個使用者)"
    ((FAILED++))
fi

echo ""

# 測試登入功能（模擬）
echo "🔐 測試登入功能..."
echo "--------------------------------"

# 測試登入 API 端點（預期會因為缺少參數而返回 400）
echo -n "測試登入 API 端點回應: "
((TOTAL++))
login_response=$(curl -s -o /dev/null -w "%{http_code}" -X POST \
    -H "Content-Type: application/json" \
    "http://localhost/api/Login" 2>/dev/null || echo "000")

if [ "$login_response" = "400" ] || [ "$login_response" = "422" ]; then
    echo "✅ 通過 (HTTP $login_response - 正確驗證輸入)"
    ((PASSED++))
elif [ "$login_response" = "200" ]; then
    echo "⚠️ 警告 (HTTP $login_response - 可能需要檢查驗證邏輯)"
    ((PASSED++))
else
    echo "❌ 失敗 (HTTP $login_response - API 無法正常回應)"
    ((FAILED++))
fi

# 測試帶參數的登入請求
echo -n "測試登入請求處理: "
((TOTAL++))
login_data='{"phone":"0938766349","password":"123456"}'
login_response=$(curl -s -o /dev/null -w "%{http_code}" -X POST \
    -H "Content-Type: application/json" \
    -d "$login_data" \
    "http://localhost/api/Login" 2>/dev/null || echo "000")

if [ "$login_response" = "200" ] || [ "$login_response" = "400" ] || [ "$login_response" = "401" ]; then
    echo "✅ 通過 (HTTP $login_response - API 正常處理請求)"
    ((PASSED++))
else
    echo "❌ 失敗 (HTTP $login_response - API 處理異常)"
    ((FAILED++))
fi

echo ""

# 測試檔案上傳目錄
echo "📁 測試檔案上傳功能..."
echo "--------------------------------"

test_function "上傳目錄可寫入" "docker-compose exec -T backend test -w /app/uploads"
test_function "KYC 圖片目錄存在" "docker-compose exec -T backend test -d /app/KycImages"
test_function "存款圖片目錄存在" "docker-compose exec -T backend test -d /app/DepositImages"
test_function "提款圖片目錄存在" "docker-compose exec -T backend test -d /app/WithdrawImages"

echo ""

# 測試 CORS 配置
echo "🌐 測試 CORS 配置..."
echo "--------------------------------"

echo -n "測試 CORS 標頭: "
((TOTAL++))
cors_header=$(curl -s -H "Origin: http://localhost" -I "http://localhost/api/health" 2>/dev/null | grep -i "access-control-allow-origin" || echo "")
if [ -n "$cors_header" ]; then
    echo "✅ 通過 (CORS 標頭存在)"
    ((PASSED++))
else
    echo "⚠️ 警告 (未檢測到 CORS 標頭)"
    ((FAILED++))
fi

echo ""

# 測試服務效能
echo "⚡ 測試基本效能..."
echo "--------------------------------"

echo -n "測試前台回應時間: "
((TOTAL++))
start_time=$(date +%s%N)
curl -s -f "http://localhost" >/dev/null 2>&1
end_time=$(date +%s%N)
response_time=$(( (end_time - start_time) / 1000000 ))  # 轉換為毫秒

if [ $response_time -lt 3000 ]; then
    echo "✅ 通過 (${response_time}ms)"
    ((PASSED++))
else
    echo "⚠️ 警告 (${response_time}ms - 回應較慢)"
    ((FAILED++))
fi

echo -n "測試後台回應時間: "
((TOTAL++))
start_time=$(date +%s%N)
curl -s -f "http://localhost:8080" >/dev/null 2>&1
end_time=$(date +%s%N)
response_time=$(( (end_time - start_time) / 1000000 ))

if [ $response_time -lt 3000 ]; then
    echo "✅ 通過 (${response_time}ms)"
    ((PASSED++))
else
    echo "⚠️ 警告 (${response_time}ms - 回應較慢)"
    ((FAILED++))
fi

echo ""

# 顯示當前系統狀態
echo "📊 系統狀態摘要..."
echo "--------------------------------"
echo "容器狀態:"
docker-compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"

echo ""
echo "資源使用:"
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"

echo ""

# 測試結果統計
echo "📈 應用程式功能測試結果"
echo "===================================="
echo "✅ 通過: $PASSED"
echo "❌ 失敗: $FAILED"
echo "📊 總計: $TOTAL"

if [ $TOTAL -gt 0 ]; then
    success_rate=$(( PASSED * 100 / TOTAL ))
    echo "📈 成功率: ${success_rate}%"
fi

if [ $FAILED -eq 0 ]; then
    echo ""
    echo "🎉 所有應用程式功能測試通過！"
    echo ""
    echo "✨ 系統功能驗證完成："
    echo "   📱 前台系統: 正常運行"
    echo "   🖥️ 後台管理: 正常運行"
    echo "   🔌 後端 API: 正常回應"
    echo "   🗄️ 資料庫: 連接正常"
    echo "   👤 預設帳號: 已建立"
    echo ""
    echo "🌐 可以開始使用系統："
    echo "   前台: http://localhost"
    echo "   後台: http://localhost:8080"
    echo "   帳號: 0938766349"
    echo "   密碼: 123456"
    echo ""
    exit 0
else
    echo ""
    echo "⚠️ 有 $FAILED 個功能測試失敗！"
    echo ""
    echo "🔧 建議檢查項目："
    echo "1. 服務是否完全啟動: docker-compose ps"
    echo "2. 檢查 API 日誌: docker-compose logs backend"
    echo "3. 檢查資料庫日誌: docker-compose logs mariadb"
    echo "4. 檢查網路連接: docker-compose exec frontend ping backend"
    echo "5. 重啟問題服務: docker-compose restart [service_name]"
    echo ""
    echo "📋 詳細故障排除: TROUBLESHOOTING.md"
    echo ""
    exit 1
fi