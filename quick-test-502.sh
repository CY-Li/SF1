#!/bin/bash

# 快速 502 錯誤測試腳本
# 簡化版本，快速診斷問題

DOMAIN="https://sf-test.zeabur.app"

echo "=== 快速 502 診斷 ==="
echo "域名: $DOMAIN"
echo ""

# 測試主要端點
echo "1. 主頁面:"
curl -s -I "$DOMAIN/" | head -1

echo "2. 健康檢查:"
curl -s -I "$DOMAIN/health" | head -1

echo "3. API 端點:"
curl -s -I "$DOMAIN/api/" | head -1

echo "4. 後台:"
curl -s -I "$DOMAIN/admin/" | head -1

echo ""
echo "=== 詳細健康檢查 ==="
echo "嘗試獲取健康檢查回應:"
curl -s --max-time 10 "$DOMAIN/health" || echo "健康檢查失敗"

echo ""
echo "=== 連接測試 ==="
echo "測試 HTTPS 連接:"
curl -s -o /dev/null -w "HTTP狀態: %{http_code}, 回應時間: %{time_total}s" "$DOMAIN/"
echo ""

echo ""
echo "=== 可能的問題 ==="
echo "如果都是 502 錯誤，可能原因："
echo "1. .NET 應用程式未啟動"
echo "2. 端口配置錯誤"
echo "3. 環境變數問題"
echo "4. 資料庫連接失敗"
echo ""
echo "請檢查 Zeabur 服務日誌獲取更多資訊。"