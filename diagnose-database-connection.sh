#!/bin/bash

# ROSCA 系統資料庫連接診斷腳本

echo "🔍 診斷資料庫連接問題..."

DOMAIN="https://sf-test.zeabur.app"

echo "📋 診斷項目："
echo "1. 測試 API 健康檢查"
echo "2. 測試登入 API (檢查資料庫連接)"
echo "3. 檢查 API 回應時間"
echo "4. 分析錯誤模式"
echo ""

# 1. 測試健康檢查
echo "❤️ 測試 API 健康檢查..."
HEALTH_START=$(date +%s%N)
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN/health")
HEALTH_END=$(date +%s%N)
HEALTH_TIME=$(( (HEALTH_END - HEALTH_START) / 1000000 ))

echo "健康檢查狀態: $HEALTH_STATUS"
echo "健康檢查時間: ${HEALTH_TIME}ms"

if [ "$HEALTH_STATUS" = "200" ]; then
    echo "✅ 基礎服務正常"
else
    echo "❌ 基礎服務異常"
fi
echo ""

# 2. 測試登入 API
echo "🔐 測試登入 API (資料庫連接測試)..."
LOGIN_START=$(date +%s%N)

# 使用測試帳號嘗試登入
LOGIN_RESPONSE=$(curl -s -w "\n%{http_code}\n%{time_total}" -X POST "$DOMAIN/api/Login" \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}')

LOGIN_END=$(date +%s%N)
LOGIN_TIME=$(( (LOGIN_END - LOGIN_START) / 1000000 ))

# 解析回應
LOGIN_BODY=$(echo "$LOGIN_RESPONSE" | head -n -2)
LOGIN_STATUS=$(echo "$LOGIN_RESPONSE" | tail -n 2 | head -n 1)
LOGIN_CURL_TIME=$(echo "$LOGIN_RESPONSE" | tail -n 1)

echo "登入 API 狀態: $LOGIN_STATUS"
echo "登入 API 時間: ${LOGIN_TIME}ms"
echo "cURL 回報時間: ${LOGIN_CURL_TIME}s"

# 分析回應時間
if (( LOGIN_TIME > 10000 )); then
    echo "⚠️ 回應時間過長 (>10秒) - 可能是資料庫連接問題"
elif (( LOGIN_TIME > 5000 )); then
    echo "⚠️ 回應時間較長 (>5秒) - 資料庫可能有延遲"
elif (( LOGIN_TIME > 2000 )); then
    echo "⚠️ 回應時間正常但偏慢 (>2秒)"
else
    echo "✅ 回應時間正常 (<2秒)"
fi
echo ""

# 3. 檢查錯誤模式
echo "🔍 分析 API 回應內容..."
if echo "$LOGIN_BODY" | grep -q "timeout\|Timeout\|TIMEOUT"; then
    echo "❌ 發現超時錯誤"
elif echo "$LOGIN_BODY" | grep -q "connection\|Connection\|CONNECTION"; then
    echo "❌ 發現連接錯誤"
elif echo "$LOGIN_BODY" | grep -q "database\|Database\|DATABASE"; then
    echo "❌ 發現資料庫錯誤"
elif [ "$LOGIN_STATUS" = "200" ]; then
    echo "✅ API 回應正常"
elif [ "$LOGIN_STATUS" = "401" ]; then
    echo "✅ API 功能正常 (認證失敗是預期的)"
elif [ "$LOGIN_STATUS" = "500" ]; then
    echo "❌ 伺服器內部錯誤"
else
    echo "⚠️ 未知狀態: $LOGIN_STATUS"
fi
echo ""

# 4. 測試其他 API 端點
echo "🔌 測試其他 API 端點..."

# 測試 API 根路徑
API_ROOT_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN/api/")
echo "API 根路徑 (/api/): $API_ROOT_STATUS"

# 測試可能的健康檢查端點
API_HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN/api/health")
echo "API 健康檢查 (/api/health): $API_HEALTH_STATUS"
echo ""

# 5. 總結和建議
echo "📊 診斷總結"
echo "===================="

if [ "$HEALTH_STATUS" = "200" ] && (( LOGIN_TIME < 5000 )); then
    echo "✅ 系統狀態: 良好"
    echo "💡 建議: 系統運作正常"
elif [ "$HEALTH_STATUS" = "200" ] && (( LOGIN_TIME > 10000 )); then
    echo "⚠️ 系統狀態: 資料庫連接問題"
    echo "💡 建議:"
    echo "   1. 檢查 MariaDB 服務狀態"
    echo "   2. 檢查資料庫連接字串"
    echo "   3. 檢查網路連接"
    echo "   4. 增加連接超時時間"
elif [ "$HEALTH_STATUS" != "200" ]; then
    echo "❌ 系統狀態: 服務異常"
    echo "💡 建議:"
    echo "   1. 檢查應用服務狀態"
    echo "   2. 檢查容器日誌"
    echo "   3. 重新部署服務"
else
    echo "⚠️ 系統狀態: 需要進一步檢查"
fi

echo ""
echo "🔧 修復步驟建議："
echo "1. 更新 zeabur.json 中的資料庫連接配置"
echo "2. 檢查 Zeabur 控制台的環境變數設定"
echo "3. 重新部署應用"
echo "4. 等待 MariaDB 服務完全啟動"
echo "5. 再次測試 API 功能"

echo ""
echo "📞 如需進一步協助，請提供："
echo "- Zeabur 控制台的服務日誌"
echo "- 環境變數配置截圖"
echo "- 本診斷腳本的完整輸出"