#!/bin/bash

# ROSCA 系統前後台路由修復腳本
# 解決 https://sf-test.zeabur.app/ 顯示後台而非前台的問題

echo "🔍 診斷前後台路由問題..."

# 1. 檢查當前部署狀態
echo "📋 檢查當前 Dockerfile 配置..."
if [ -f "Dockerfile" ]; then
    echo "✅ 找到 Dockerfile"
    
    # 檢查 nginx 配置中的路由設定
    echo "🔍 檢查 nginx 路由配置..."
    grep -A 20 "location /" Dockerfile
else
    echo "❌ 未找到 Dockerfile"
    exit 1
fi

# 2. 檢查前台檔案
echo "📁 檢查前台檔案..."
if [ -f "frontend/index.html" ]; then
    echo "✅ 前台 index.html 存在"
    echo "📄 前台內容預覽:"
    head -10 frontend/index.html
else
    echo "❌ 前台 index.html 不存在"
fi

# 3. 檢查後台檔案
echo "📁 檢查後台檔案..."
if [ -f "backend/FontEnd/FontEnd/src/index.html" ]; then
    echo "✅ 後台 index.html 存在"
    echo "📄 後台內容預覽:"
    head -10 backend/FontEnd/FontEnd/src/index.html
else
    echo "❌ 後台 index.html 不存在"
fi

echo "🔧 問題診斷完成！"
echo ""
echo "🎯 可能的問題："
echo "1. nginx 配置中前台和後台的路由順序有問題"
echo "2. 前台檔案沒有正確複製到 /var/www/frontend/"
echo "3. Angular 建置後的檔案覆蓋了前台檔案"
echo ""
echo "💡 建議的解決方案："
echo "1. 修正 Dockerfile 中的 nginx 配置"
echo "2. 確保前台檔案正確複製"
echo "3. 調整路由優先級"