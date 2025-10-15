#!/bin/bash

# ROSCA Docker 生產環境停止腳本
echo "🛑 停止 ROSCA 生產環境..."

# 確認停止操作
read -p "⚠️ 確定要停止生產環境嗎？這將影響線上服務 (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ 取消停止操作"
    exit 0
fi

# 執行備份（可選）
read -p "💾 是否要在停止前執行備份？ (Y/n): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Nn]$ ]]; then
    echo "📦 執行快速備份..."
    ./scripts/backup-database.sh
fi

# 優雅停止服務
echo "🔄 優雅停止服務..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml stop

# 等待服務完全停止
echo "⏳ 等待服務完全停止..."
sleep 10

# 移除容器
echo "🗑️ 移除容器..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

echo "✅ ROSCA 生產環境已停止"
echo ""
echo "💡 提示:"
echo "   - 資料庫資料已保存在 database/data 目錄"
echo "   - 上傳檔案已保存在 Docker volumes 中"
echo "   - 重新啟動: ./scripts/start-prod.sh"
echo "   - 查看備份: ls -la backups/"