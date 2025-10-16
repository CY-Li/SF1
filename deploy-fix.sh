#!/bin/bash

# ROSCA 系統前後台路由修復部署腳本

echo "🚀 開始修復前後台路由問題..."

# 1. 備份原始 Dockerfile
if [ -f "Dockerfile" ]; then
    echo "📦 備份原始 Dockerfile..."
    cp Dockerfile Dockerfile.backup.$(date +%Y%m%d_%H%M%S)
    echo "✅ 備份完成"
fi

# 2. 應用修復的 Dockerfile
echo "🔧 應用修復的 Dockerfile..."
cp Dockerfile.fixed Dockerfile
echo "✅ Dockerfile 已更新"

# 3. 檢查前台檔案
echo "📁 檢查前台檔案結構..."
if [ -d "frontend" ]; then
    echo "✅ 前台目錄存在"
    if [ -f "frontend/index.html" ]; then
        echo "✅ 前台 index.html 存在"
    else
        echo "❌ 前台 index.html 不存在"
    fi
else
    echo "❌ 前台目錄不存在"
fi

# 4. 檢查後台檔案
echo "📁 檢查後台檔案結構..."
if [ -d "backend/FontEnd/FontEnd" ]; then
    echo "✅ 後台目錄存在"
    if [ -f "backend/FontEnd/FontEnd/src/index.html" ]; then
        echo "✅ 後台 index.html 存在"
    else
        echo "❌ 後台 index.html 不存在"
    fi
else
    echo "❌ 後台目錄不存在"
fi

echo ""
echo "🎯 修復重點："
echo "1. ✅ 調整了 nginx 路由優先級"
echo "2. ✅ 確保 /admin 路徑優先匹配後台"
echo "3. ✅ 根路徑 / 指向前台系統"
echo "4. ✅ 修正了 Angular base href"
echo ""
echo "📋 接下來的步驟："
echo "1. 提交程式碼變更到 GitHub"
echo "2. 在 Zeabur 控制台觸發重新部署"
echo "3. 等待部署完成"
echo "4. 測試 https://sf-test.zeabur.app/ (應顯示前台)"
echo "5. 測試 https://sf-test.zeabur.app/admin (應顯示後台)"
echo ""
echo "🔍 測試命令："
echo "curl -I https://sf-test.zeabur.app/"
echo "curl -I https://sf-test.zeabur.app/admin"