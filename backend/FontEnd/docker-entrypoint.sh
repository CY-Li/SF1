#!/bin/sh

# Zeabur 優化的啟動腳本
echo "Starting ROSCA Admin Frontend..."

# 替換 nginx 配置中的環境變數
# API_GATEWAY_HOST 是 Zeabur 提供的服務發現變數
if [ -n "$API_GATEWAY_HOST" ]; then
    echo "Using API Gateway Host: $API_GATEWAY_HOST"
    sed -i "s/api-gateway:5000/${API_GATEWAY_HOST}:5000/g" /etc/nginx/nginx.conf
else
    echo "Using default API Gateway Host: api-gateway:5000"
fi

# 設定時區
if [ -n "$TZ" ]; then
    echo "Setting timezone to: $TZ"
    ln -snf /usr/share/zoneinfo/$TZ /etc/localtime
    echo $TZ > /etc/timezone
fi

# 驗證 nginx 配置
echo "Validating nginx configuration..."
nginx -t

if [ $? -eq 0 ]; then
    echo "Nginx configuration is valid. Starting nginx..."
    # 啟動 nginx
    exec nginx -g "daemon off;"
else
    echo "Nginx configuration is invalid. Exiting..."
    exit 1
fi