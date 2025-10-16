# Zeabur 多階段建置 Dockerfile
# 這個 Dockerfile 將建置所有服務並使用 nginx 作為反向代理

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

# 階段 3: 建置 Angular Admin
FROM node:18-alpine AS admin-build
WORKDIR /app
COPY backend/FontEnd/FontEnd/package*.json ./
RUN npm ci --only=production
COPY backend/FontEnd/FontEnd/ .
RUN npm run build

# 階段 4: 準備前台檔案
FROM nginx:alpine AS frontend-files
COPY frontend/ /frontend/

# 階段 5: 最終運行階段
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

# 複製前端檔案
COPY --from=frontend-files /frontend /var/www/frontend/
COPY --from=admin-build /app/dist/font-end/browser /var/www/admin/

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

    # 後台
    location /admin {
        alias /var/www/admin;
        index index.html;
        try_files \$uri \$uri/ /admin/index.html;
    }

    # API 代理到 API Gateway
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
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
stdout_logfile=/var/log/nginx/access.log
stderr_logfile=/var/log/nginx/error.log

[program:backend]
command=dotnet DotNetBackEndApi.dll
directory=/app/backend
autostart=true
autorestart=true
environment=ASPNETCORE_URLS="http://+:5000",ASPNETCORE_ENVIRONMENT="Production"
stdout_logfile=/var/log/backend.log
stderr_logfile=/var/log/backend.error.log

[program:backend-service]
command=dotnet DotNetBackEndService.dll
directory=/app/backend-service
autostart=true
autorestart=true
environment=ASPNETCORE_URLS="http://+:5001",ASPNETCORE_ENVIRONMENT="Production"
stdout_logfile=/var/log/backend-service.log
stderr_logfile=/var/log/backend-service.error.log
EOF

# 創建必要的目錄
RUN mkdir -p /app/uploads /app/KycImages /app/DepositImages /app/WithdrawImages /app/AnnImagessss \
    && mkdir -p /var/log/nginx \
    && chown -R www-data:www-data /var/www \
    && chmod -R 755 /var/www

# 暴露端口
EXPOSE 80

# 健康檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost/api/HomePicture/GetAnnImages || exit 1

# 啟動 supervisor
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]