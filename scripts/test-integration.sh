#!/bin/bash

# ROSCA 系統整合測試腳本
# 測試 Docker Compose 部署的完整性

set -e

echo "🧪 開始 ROSCA 系統整合測試..."
echo "=================================="

# 測試結果統計
PASSED=0
FAILED=0
TOTAL=0

# 測試函數
run_test() {
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

# 檢查前置條件
echo "🔍 檢查前置條件..."
echo "--------------------------------"

run_test "Docker 是否運行" "docker info"
run_test "Docker Compose 是否可用" "docker-compose --version"
run_test ".env 檔案是否存在" "[ -f .env ]"
run_test "docker-compose.yml 是否有效" "docker-compose config"

echo ""

# 測試 Docker Compose 啟動
echo "🚀 測試 Docker Compose 啟動..."
echo "--------------------------------"

echo "啟動所有服務..."
if docker-compose up -d; then
    echo "✅ 服務啟動成功"
    ((PASSED++))
else
    echo "❌ 服務啟動失敗"
    ((FAILED++))
fi
((TOTAL++))

# 等待服務就緒
echo "⏳ 等待服務就緒 (60秒)..."
sleep 60

echo ""

# 測試容器狀態
echo "📦 測試容器狀態..."
echo "--------------------------------"

# 檢查所有容器是否運行
containers=("rosca-mariadb" "rosca-backend" "rosca-frontend" "rosca-admin")
for container in "${containers[@]}"; do
    run_test "$container 容器運行" "docker ps --format '{{.Names}}' | grep -q '^$container$'"
done

echo ""

# 測試健康檢查
echo "🏥 測試服務健康檢查..."
echo "--------------------------------"

# 等待健康檢查穩定
echo "等待健康檢查穩定..."
sleep 30

run_test "MariaDB 健康檢查" "docker-compose ps mariadb | grep -q 'healthy'"
run_test "後端 API 健康檢查" "docker-compose ps backend | grep -q 'healthy'"
run_test "前台健康檢查" "docker-compose ps frontend | grep -q 'healthy'"
run_test "後台健康檢查" "docker-compose ps admin | grep -q 'healthy'"

echo ""

# 測試網路連通性
echo "🌐 測試容器間網路連通性..."
echo "--------------------------------"

run_test "前台到後端網路" "docker-compose exec -T frontend ping -c 1 backend"
run_test "後台到後端網路" "docker-compose exec -T admin ping -c 1 backend"
run_test "後端到資料庫網路" "docker-compose exec -T backend ping -c 1 mariadb"

echo ""

# 測試資料庫連接
echo "🗄️ 測試資料庫連接..."
echo "--------------------------------"

run_test "資料庫 ping 測試" "docker-compose exec -T mariadb mysqladmin ping -h localhost -u root -p\$DB_ROOT_PASSWORD"
run_test "資料庫查詢測試" "docker-compose exec -T mariadb mysql -u root -p\$DB_ROOT_PASSWORD -e 'SELECT 1;'"
run_test "應用資料庫存在" "docker-compose exec -T mariadb mysql -u root -p\$DB_ROOT_PASSWORD -e 'USE rosca2; SELECT 1;'"

echo ""

# 測試 HTTP 服務
echo "🌐 測試 HTTP 服務回應..."
echo "--------------------------------"

# 等待 HTTP 服務完全就緒
sleep 10

run_test "前台 HTTP 回應" "curl -f -s http://localhost >/dev/null"
run_test "後台 HTTP 回應" "curl -f -s http://localhost:8080 >/dev/null"

# 測試 API 代理 (可能返回 404 但不應該是連接錯誤)
echo -n "測試前台 API 代理: "
((TOTAL++))
response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost/api/health 2>/dev/null || echo "000")
if [ "$response" != "000" ] && [ "$response" != "502" ] && [ "$response" != "503" ]; then
    echo "✅ 通過 (HTTP $response)"
    ((PASSED++))
else
    echo "❌ 失敗 (HTTP $response)"
    ((FAILED++))
fi

echo -n "測試後台 API 代理: "
((TOTAL++))
response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/health 2>/dev/null || echo "000")
if [ "$response" != "000" ] && [ "$response" != "502" ] && [ "$response" != "503" ]; then
    echo "✅ 通過 (HTTP $response)"
    ((PASSED++))
else
    echo "❌ 失敗 (HTTP $response)"
    ((FAILED++))
fi

echo ""

# 測試資料持久化
echo "💾 測試資料持久化..."
echo "--------------------------------"

run_test "資料庫 volume 存在" "docker volume ls | grep -q 'db-data'"
run_test "上傳 volume 存在" "docker volume ls | grep -q 'uploads'"
run_test "資料庫資料目錄非空" "[ -n \"\$(docker-compose exec -T mariadb ls -A /var/lib/mysql 2>/dev/null)\" ]"

echo ""

# 測試預設資料載入
echo "📋 測試預設資料載入..."
echo "--------------------------------"

# 檢查是否有預設使用者
echo -n "測試預設使用者存在: "
((TOTAL++))
if docker-compose exec -T mariadb mysql -u root -p$DB_ROOT_PASSWORD -e "USE rosca2; SELECT COUNT(*) FROM Users WHERE Phone='0938766349';" 2>/dev/null | grep -q "1"; then
    echo "✅ 通過"
    ((PASSED++))
else
    echo "❌ 失敗"
    ((FAILED++))
fi

echo ""

# 顯示服務狀態
echo "📊 當前服務狀態..."
echo "--------------------------------"
docker-compose ps

echo ""

# 顯示資源使用情況
echo "💻 資源使用情況..."
echo "--------------------------------"
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}"

echo ""

# 測試結果統計
echo "📈 測試結果統計"
echo "=================================="
echo "✅ 通過: $PASSED"
echo "❌ 失敗: $FAILED"
echo "📊 總計: $TOTAL"
echo "📈 成功率: $(( PASSED * 100 / TOTAL ))%"

if [ $FAILED -eq 0 ]; then
    echo ""
    echo "🎉 所有整合測試通過！"
    echo "系統已成功部署並運行正常。"
    echo ""
    echo "🌐 訪問地址:"
    echo "   前台系統: http://localhost"
    echo "   後台管理: http://localhost:8080"
    echo ""
    echo "👤 預設測試帳號:"
    echo "   帳號: 0938766349"
    echo "   密碼: 123456"
    echo ""
    exit 0
else
    echo ""
    echo "⚠️ 有 $FAILED 個測試失敗！"
    echo ""
    echo "🔧 故障排除建議:"
    echo "1. 檢查服務日誌: docker-compose logs [service_name]"
    echo "2. 檢查服務狀態: docker-compose ps"
    echo "3. 檢查系統資源: docker stats"
    echo "4. 重啟服務: docker-compose restart [service_name]"
    echo "5. 完全重啟: docker-compose down && docker-compose up -d"
    echo ""
    echo "📋 詳細故障排除請參考: TROUBLESHOOTING.md"
    echo ""
    exit 1
fi