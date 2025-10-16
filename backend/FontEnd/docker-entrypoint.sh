#!/bin/sh

# 替換 nginx 配置中的環境變數
# API_GATEWAY_HOST 是 Zeabur 提供的服務發現變數
if [ -n "$API_GATEWAY_HOST" ]; then
    sed -i "s/localhost:5000/${API_GATEWAY_HOST}:5000/g" /etc/nginx/nginx.conf
fi

# 啟動 nginx
exec nginx -g "daemon off;"