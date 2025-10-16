#!/bin/bash

# 測試資料庫連接修復腳本

echo "🔍 測試資料庫連接修復..."

DOMAIN="https://sf-test.zeabur.app"

echo "📋 測試項目："
echo "1. 基本健康檢查"
echo "2. 資料庫連接測試 (登入 API)"
echo "3. 錯誤分析"
echo ""

# 1. 健康檢查
echo "❤️ 測試基本健康檢查..."
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN/health")
echo "健康檢查狀態: $HEALTH_STATUS"

if [ "$HEALTH_STATUS" = "200" ]; then
    echo "✅ 基本服務正常"
else
    echo "❌ 基本服務異常"
fi
echo ""

# 2. 測試資料庫連接
echo "🔐 測試資料庫連接 (登入 API)..."

# 測試管理員帳號
echo "測試管理員帳號: admin / Admin123456"
LOGIN_START=$(date +%s%N)

LOGIN_RESPONSE=$(curl -s -w "\n%{http_code}\n%{time_total}" -X POST "$DOMAIN/api/Login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123456"}')

LOGIN_END=$(date +%s%N)
LOGIN_TIME=$(( (LOGIN_END - LOGIN_START) / 1000000 ))

# 解析回應
LOGIN_BODY=$(echo "$LOGIN_RESPONSE" | head -n -2)
LOGIN_STATUS=$(echo "$LOGIN_RESPONSE" | tail -n 2 | head -n 1)
LOGIN_CURL_TIME=$(echo "$LOGIN_RESPONSE" | tail -n 1)

echo "登入狀態碼: $LOGIN_STATUS"
echo "回應時間: ${LOGIN_TIME}ms"
echo "回應內容: $LOGIN_BODY"
echo ""

# 3. 錯誤分析
echo "🔍 錯誤分析..."

if echo "$LOGIN_BODY" | grep -q "appuser"; then
    echo "❌ 仍然出現 appuser 錯誤"
    echo "   建議: 檢查 Zeabur 是否有自動覆蓋變數"
elif echo "$LOGIN_BODY" | grep -q "rosca_user"; then
    echo "❌ rosca_user 連接錯誤"
    echo "   建議: 檢查 MariaDB 初始化是否成功"
elif echo "$LOGIN_BODY" | grep -q "Access denied"; then
    echo "❌ 資料庫存取被拒絕"
    echo "   建議: 檢查使用者權限設定"
elif echo "$LOGIN_BODY" | grep -q "timeout\|Timeout"; then
    echo "❌ 連接超時"
    echo "   建議: 檢查 MariaDB 服務狀態"
elif [ "$LOGIN_STATUS" = "200" ]; then
    echo "✅ 登入成功！資料庫連接正常"
elif [ "$LOGIN_STATUS" = "401" ]; then
    echo "✅ 資料庫連接正常，但帳號密碼錯誤 (這是預期的)"
elif [ "$LOGIN_STATUS" = "500" ]; then
    echo "❌ 伺服器內部錯誤"
else
    echo "⚠️ 未知狀態: $LOGIN_STATUS"
fi
echo ""

# 4. 測試測試帳號
echo "🔐 測試測試帳號: 0938766349 / 123456"
TEST_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$DOMAIN/api/Login" \
  -H "Content-Type: application/json" \
  -d '{"username":"0938766349","password":"123456"}')

TEST_BODY=$(echo "$TEST_RESPONSE" | head -n -1)
TEST_STATUS=$(echo "$TEST_RESPONSE" | tail -n 1)

echo "測試帳號狀態: $TEST_STATUS"
echo "測試帳號回應: $TEST_BODY"
echo ""

# 5. 總結
echo "📊 診斷總結"
echo "===================="

if [ "$HEALTH_STATUS" = "200" ] && [ "$LOGIN_STATUS" = "200" ]; then
    echo "🎉 完全修復成功！"
    echo "✅ 基本服務: 正常"
    echo "✅ 資料庫連接: 正常"
    echo "✅ 登入功能: 正常"
elif [ "$HEALTH_STATUS" = "200" ] && [ "$LOGIN_STATUS" = "401" ]; then
    echo "🎉 資料庫連接修復成功！"
    echo "✅ 基本服務: 正常"
    echo "✅ 資料庫連接: 正常"
    echo "ℹ️ 登入失敗: 帳號密碼問題 (資料庫連接正常)"
elif [ "$HEALTH_STATUS" = "200" ] && echo "$LOGIN_BODY" | grep -q "appuser"; then
    echo "⚠️ 部分修復"
    echo "✅ 基本服務: 正常"
    echo "❌ 資料庫連接: 仍有 appuser 問題"
    echo ""
    echo "🔧 下一步建議:"
    echo "1. 配合 Zeabur 使用 appuser"
    echo "2. 或使用不同的變數名稱"
else
    echo "❌ 需要進一步修復"
    echo "基本服務: $([ "$HEALTH_STATUS" = "200" ] && echo "正常" || echo "異常")"
    echo "資料庫連接: 異常"
fi

echo ""
echo "🔍 如需進一步協助，請提供："
echo "- 完整的錯誤訊息"
echo "- Zeabur MariaDB 服務日誌"
echo "- Zeabur 應用服務日誌"