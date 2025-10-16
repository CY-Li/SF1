#!/bin/bash

# ROSCA 系統前後台路由修復驗證腳本

echo "🔍 驗證前後台路由修復..."

DOMAIN="https://sf-test.zeabur.app"

echo "📋 測試計劃："
echo "1. 測試根路徑 / (應顯示前台系統)"
echo "2. 測試 /admin 路徑 (應顯示後台系統)"
echo "3. 測試 API 端點"
echo "4. 測試健康檢查"
echo ""

# 1. 測試根路徑
echo "🏠 測試前台系統 (根路徑)..."
echo "URL: $DOMAIN/"
curl -s -I "$DOMAIN/" | head -5
echo ""

# 檢查是否包含前台特徵
echo "🔍 檢查前台內容特徵..."
FRONTEND_CONTENT=$(curl -s "$DOMAIN/" | grep -i "商會活動花絮\|加入會員\|近期公告" | wc -l)
if [ "$FRONTEND_CONTENT" -gt 0 ]; then
    echo "✅ 前台內容檢測成功 (找到 $FRONTEND_CONTENT 個特徵)"
else
    echo "❌ 前台內容檢測失敗"
fi
echo ""

# 2. 測試後台路徑
echo "🏢 測試後台系統 (/admin)..."
echo "URL: $DOMAIN/admin"
curl -s -I "$DOMAIN/admin" | head -5
echo ""

# 檢查是否包含後台特徵
echo "🔍 檢查後台內容特徵..."
BACKEND_CONTENT=$(curl -s "$DOMAIN/admin" | grep -i "app-root\|angular\|FontEnd" | wc -l)
if [ "$BACKEND_CONTENT" -gt 0 ]; then
    echo "✅ 後台內容檢測成功 (找到 $BACKEND_CONTENT 個特徵)"
else
    echo "❌ 後台內容檢測失敗"
fi
echo ""

# 3. 測試 API 端點
echo "🔌 測試 API 端點..."
echo "URL: $DOMAIN/api/health"
API_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN/api/health")
echo "API 狀態碼: $API_STATUS"
if [ "$API_STATUS" = "200" ]; then
    echo "✅ API 端點正常"
else
    echo "❌ API 端點異常"
fi
echo ""

# 4. 測試健康檢查
echo "❤️ 測試健康檢查..."
echo "URL: $DOMAIN/health"
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$DOMAIN/health")
echo "健康檢查狀態碼: $HEALTH_STATUS"
if [ "$HEALTH_STATUS" = "200" ]; then
    echo "✅ 健康檢查正常"
else
    echo "❌ 健康檢查異常"
fi
echo ""

# 總結
echo "📊 測試總結："
echo "===================="
if [ "$FRONTEND_CONTENT" -gt 0 ]; then
    echo "✅ 前台系統: 正常"
else
    echo "❌ 前台系統: 異常"
fi

if [ "$BACKEND_CONTENT" -gt 0 ]; then
    echo "✅ 後台系統: 正常"
else
    echo "❌ 後台系統: 異常"
fi

if [ "$API_STATUS" = "200" ]; then
    echo "✅ API 服務: 正常"
else
    echo "❌ API 服務: 異常"
fi

if [ "$HEALTH_STATUS" = "200" ]; then
    echo "✅ 健康檢查: 正常"
else
    echo "❌ 健康檢查: 異常"
fi

echo ""
echo "🎯 如果前台系統仍顯示異常，請檢查："
echo "1. Zeabur 部署是否已完成"
echo "2. 容器是否已重新啟動"
echo "3. nginx 配置是否正確載入"
echo "4. 前台檔案是否正確複製"