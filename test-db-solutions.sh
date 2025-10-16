#!/bin/bash

# 測試不同的資料庫連接解決方案

echo "🔧 ROSCA 資料庫連接解決方案測試"
echo "=================================="

echo ""
echo "可用的解決方案："
echo "1. 使用 Root 使用者 (當前 zeabur.json)"
echo "2. 使用內建 MariaDB 服務"
echo "3. 使用簡化連接字串"
echo "4. 回復使用 appuser"
echo ""

read -p "選擇要測試的方案 (1-4): " choice

case $choice in
    1)
        echo "✅ 使用方案 1: Root 使用者"
        echo "當前 zeabur.json 已經配置為使用 root 使用者"
        echo "請在 Zeabur 控制台重新部署應用"
        ;;
    2)
        echo "✅ 使用方案 2: 內建 MariaDB 服務"
        cp zeabur-internal-db.json zeabur.json
        echo "已切換到內建 MariaDB 配置"
        echo "請在 Zeabur 控制台重新部署應用"
        ;;
    3)
        echo "✅ 使用方案 3: 簡化連接字串"
        cp zeabur-simple-connection.json zeabur.json
        echo "已切換到簡化連接字串配置"
        echo "請在 Zeabur 控制台重新部署應用"
        ;;
    4)
        echo "⚠️  回復方案 4: 使用 appuser"
        # 回復到 appuser 配置
        sed -i 's/User Id=root/User Id=appuser/g' zeabur.json
        echo "已回復到 appuser 配置"
        echo "請確保在 Zeabur MariaDB 中創建了 appuser"
        ;;
    *)
        echo "❌ 無效選擇"
        exit 1
        ;;
esac

echo ""
echo "🚀 部署後測試命令："
echo "curl -X POST https://sf-test.zeabur.app/api/Login \\"
echo "  -H \"Content-Type: application/json\" \\"
echo "  -d '{\"username\":\"admin\",\"password\":\"Admin123456\"}'"
echo ""
echo "📝 如果仍有問題，請檢查 Zeabur 日誌並嘗試其他方案"