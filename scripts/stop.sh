#!/bin/bash

# ROSCA 系統停止腳本
# 用於停止所有 Docker 服務

set -e

echo "🛑 停止 ROSCA 平安商會系統..."

# 檢查 docker-compose 是否可用
if ! command -v docker-compose > /dev/null 2>&1; then
    echo "❌ 錯誤: docker-compose 未安裝"
    exit 1
fi

# 檢查是否有運行中的服務
if [ "$(docker-compose ps -q)" ]; then
    echo "📋 當前運行的服務:"
    docker-compose ps
    
    echo ""
    echo "🛑 停止所有服務..."
    docker-compose down
    
    echo "✅ 所有服務已停止"
else
    echo "ℹ️  沒有運行中的服務"
fi

echo ""
echo "📋 管理命令:"
echo "   重新啟動: ./scripts/start.sh"
echo "   完全清理: ./scripts/clean.sh"
echo "   查看狀態: docker-compose ps"
echo ""