# 優化版 Zeabur Dockerfile - 修復 Angular 建置問題
# 跳過 Angular 建置，使用預建置檔案以避免 Node.js 相關問題

# 階段 1: 建置 .NET Core Backend Service
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS backend-service-build
WORKDIR /src
COPY backendAPI/DotNetBackEndCleanArchitecture/ .
WORKDIR /src/Presentation/DotNetBackEndService
RUN dotnet restore
RUN dotnet build -c Release -o /app/backend-service/build
RUN dotnet publish -c Release -o /app/backend-service/publish --no-restore

# 階段 2: 建置 .NET Core API Gateway
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS backend-build
WORKDIR /src
COPY backendAPI/DotNetBackEndCleanArchitecture/ .
WORKDIR /src/DotNetBackEndApi
RUN dotnet restore
RUN dotnet build -c Release -o /app/backend/build
RUN dotnet publish -c Release -o /app/backend/publish --no-restore

# 階段 3: 最終運行階段
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# 安裝 nginx 和 supervisor
RUN apt-get update && apt-get install -y \
    nginx \
    supervisor \
    curl \
    && rm -rf /var/lib/apt/lists/*

# 複製 .NET Core 應用程式
COPY --from=backend-service-build /app/backend-service/publish /app/backend-service/
COPY --from=backend-build /app/backend/publish /app/backend/

# 複製前端檔案 (使用預建置檔案)
COPY frontend/ /var/www/frontend/
COPY backend/FontEnd/FontEnd/dist/font-end/browser/ /var/www/admin/

# 修正 Angular base href
RUN if [ -f /var/www/admin/index.html ]; then \
        sed -i 's|<base href="/backend/">|<base href="/admin/">|g' /var/www/admin/index.html; \
    fi

# 創建 nginx 配置
COPY <<EOF /etc/nginx/sites-available/default
server {
    listen 80;
    server_name _;

    # 前台
    location / {
        root /var/www/frontend;
        index index.html;
        try_files \$uri \$uri/ /index.html;
    }

    # 後台 Angular SPA
    location /admin {
        alias /var/www/admin/;
        index index.html;
        try_files \$uri \$uri/ /admin/index.html;
        
        # 防止快取 index.html
        location = /admin/index.html {
            alias /var/www/admin/index.html;
            add_header Cache-Control "no-cache, no-store, must-revalidate";
            add_header Pragma "no-cache";
            add_header Expires "0";
        }
    }
    
    # 處理 Angular 路由 (所有 /admin/* 路徑)
    location ~ ^/admin/(.*)$ {
        alias /var/www/admin/;
        try_files \$1 \$1/ /admin/index.html;
    }

    # API 代理到 API Gateway
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_connect_timeout 30s;
        proxy_send_timeout 30s;
        proxy_read_timeout 30s;
        client_max_body_size 50M;
    }
    
    # 健康檢查端點
    location /health {
        access_log off;
        return 200 "healthy\\n";
        add_header Content-Type text/plain;
    }

    # 靜態檔案快取
    location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
EOF

# 創建 supervisor 配置
COPY <<EOF /etc/supervisor/conf.d/supervisord.conf
[supervisord]
nodaemon=true
user=root

[program:nginx]
command=nginx -g "daemon off;"
autostart=true
autorestart=true
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0

[program:backend]
command=dotnet DotNetBackEndApi.dll
directory=/app/backend
autostart=true
autorestart=true
environment=ASPNETCORE_URLS="http://+:5000",ASPNETCORE_ENVIRONMENT="Production"
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0

[program:backend-service]
command=dotnet DotNetBackEndService.dll
directory=/app/backend-service
autostart=true
autorestart=true
environment=ASPNETCORE_URLS="http://+:5001",ASPNETCORE_ENVIRONMENT="Production"
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0
EOF

# 創建必要的目錄和設定權限
RUN mkdir -p /app/uploads /app/KycImages /app/DepositImages /app/WithdrawImages /app/AnnImagessss \
    && mkdir -p /var/log/nginx /run/nginx \
    && chown -R www-data:www-data /var/www \
    && chmod -R 755 /var/www \
    && chown -R www-data:www-data /app/uploads /app/KycImages /app/DepositImages /app/WithdrawImages /app/AnnImagessss

# 暴露端口
EXPOSE 80

# 健康檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# 啟動 supervisor
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]