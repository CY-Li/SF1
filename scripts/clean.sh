#!/bin/bash

# ROSCA 系統清理腳本
# 用於完全清理所有 Docker 資源

set -e

echo "🧹 清理 ROSCA 平安商會系統..."

# 檢查 docker-compose 是否可用
if ! command -v docker-compose > /dev/null 2>&1; then
    echo "❌ 錯誤: docker-compose 未安裝"
    exit 1
fi

# 詢問用戶確認
echo "⚠️  警告: 此操作將會："
echo "   - 停止並移除所有容器"
echo "   - 移除所有相關映像"
echo "   - 刪除所有資料 (包含資料庫)"
echo "   - 移除所有 volumes"
echo ""
read -p "確定要繼續嗎？(y/N): " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ 操作已取消"
    exit 0
fi

echo "🛑 停止所有服務..."
docker-compose down

echo "🗑️  移除容器和映像..."
docker-compose down --rmi all

echo "💾 移除 volumes (資料庫資料將被刪除)..."
docker-compose down -v --rmi all

echo "🧹 清理未使用的 Docker 資源..."
docker system prune -f

echo "🧹 清理未使用的 volumes..."
docker volume prune -f

echo ""
echo "✅ 清理完成！"
echo ""
echo "📋 重新開始:"
echo "   啟動系統: ./scripts/start.sh"
echo ""