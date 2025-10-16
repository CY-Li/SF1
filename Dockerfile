# ROSCA 平安商會系統 - 統一部署 Dockerfile (Zeabur 優化版)
# 多階段建置：整合所有服務到單一容器

# ===== Angular 後台建置階段 =====
FROM node:18-alpine AS angular-build
WORKDIR /app

# 安裝建置工具
RUN apk add --no-cache python3 make g++ && \
    rm -rf /var/cache/apk/*

# 複製 Angular 專案檔案
COPY backend/FontEnd/FontEnd/package*.json ./
RUN npm ci --no-audit --no-fund && \
    npm cache clean --force

# 複製 Angular 源代碼並建置
COPY backend/FontEnd/FontEnd/ ./

RUN npm run build:zeabur

# ===== .NET Core 建置階段 =====
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS dotnet-build
WORKDIR /src

# 複製 .NET 解決方案檔案
COPY backendAPI/DotNetBackEndCleanArchitecture/*.sln ./

# 複製專案檔案進行依賴項還原
COPY backendAPI/DotNetBackEndCleanArchitecture/Application/AppAbstraction/*.csproj ./Application/AppAbstraction/
COPY backendAPI/DotNetBackEndCleanArchitecture/Application/AppService/*.csproj ./Application/AppService/
COPY backendAPI/DotNetBackEndCleanArchitecture/Domain/DomainAbstraction/*.csproj ./Domain/DomainAbstraction/
COPY backendAPI/DotNetBackEndCleanArchitecture/Domain/DomainEntity/*.csproj ./Domain/DomainEntity/
COPY backendAPI/DotNetBackEndCleanArchitecture/Domain/DomainEntityDTO/*.csproj ./Domain/DomainEntityDTO/
COPY backendAPI/DotNetBackEndCleanArchitecture/Infrastructure/Persistence/*.csproj ./Infrastructure/Persistence/
COPY backendAPI/DotNetBackEndCleanArchitecture/Infrastructure/InfraCommon/*.csproj ./Infrastructure/InfraCommon/
COPY backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/*.csproj ./Presentation/DotNetBackEndService/
COPY backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/*.csproj ./DotNetBackEndApi/

# 還原 NuGet 套件
RUN dotnet restore DotNetBackEndApi/DotNetBackEndApi.csproj --runtime linux-musl-x64
RUN dotnet restore Presentation/DotNetBackEndService/DotNetBackEndService.csproj --runtime linux-musl-x64

# 複製所有 .NET 源代碼
COPY backendAPI/DotNetBackEndCleanArchitecture/Application/ ./Application/
COPY backendAPI/DotNetBackEndCleanArchitecture/Domain/ ./Domain/
COPY backendAPI/DotNetBackEndCleanArchitecture/Infrastructure/ ./Infrastructure/
COPY backendAPI/DotNetBackEndCleanArchitecture/Presentation/ ./Presentation/
COPY backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/ ./DotNetBackEndApi/

# 建置 API Gateway
WORKDIR /src/DotNetBackEndApi
RUN dotnet publish -c Release -o /app/publish/api-gateway \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

# 建置 Backend Service
WORKDIR /src/Presentation/DotNetBackEndService
RUN dotnet publish -c Release -o /app/publish/backend-service \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

# ===== 運行階段 =====
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS runtime
WORKDIR /app

# 安裝必要的系統套件
RUN apk add --no-cache \
    curl \
    tzdata \
    nginx \
    supervisor \
    && rm -rf /var/cache/apk/*

# 建立非 root 使用者
RUN addgroup -g 1001 -S appuser && \
    adduser -S appuser -G appuser -u 1001

# 建立應用程式目錄結構
RUN mkdir -p \
    /app/api-gateway \
    /app/backend-service \
    /app/frontend \
    /app/admin \
    /app/uploads \
    /app/KycImages \
    /app/DepositImages \
    /app/WithdrawImages \
    /app/AnnImagessss \
    /app/logs \
    /var/log/supervisor \
    /etc/supervisor/conf.d \
    && chown -R appuser:appuser /app \
    && chmod -R 755 /app

# 複製 .NET 應用程式
COPY --from=dotnet-build --chown=appuser:appuser /app/publish/api-gateway/ /app/api-gateway/
COPY --from=dotnet-build --chown=appuser:appuser /app/publish/backend-service/ /app/backend-service/

# 複製前台靜態檔案
COPY --chown=appuser:appuser frontend/*.html /app/frontend/
COPY --chown=appuser:appuser frontend/css/ /app/frontend/css/
COPY --chown=appuser:appuser frontend/js/ /app/frontend/js/
COPY --chown=appuser:appuser frontend/images/ /app/frontend/images/
COPY --chown=appuser:appuser frontend/component/ /app/frontend/component/

# 複製 Angular 建置檔案
COPY --from=angular-build --chown=appuser:appuser /app/dist/font-end/browser/ /app/admin/

# 建立 Supervisor 配置
RUN cat > /etc/supervisor/conf.d/supervisord.conf << 'EOF'
[supervisord]
nodaemon=true
user=root
logfile=/var/log/supervisor/supervisord.log
pidfile=/var/run/supervisord.pid

[program:nginx]
command=nginx -g "daemon off;"
autostart=true
autorestart=true
stderr_logfile=/var/log/supervisor/nginx.err.log
stdout_logfile=/var/log/supervisor/nginx.out.log
user=root

[program:api-gateway]
command=dotnet /app/api-gateway/DotNetBackEndApi.dll
directory=/app/api-gateway
autostart=true
autorestart=true
stderr_logfile=/var/log/supervisor/api-gateway.err.log
stdout_logfile=/var/log/supervisor/api-gateway.out.log
user=appuser
environment=ASPNETCORE_URLS="http://+:5000",ASPNETCORE_ENVIRONMENT="Production"

[program:backend-service]
command=dotnet /app/backend-service/DotNetBackEndService.dll
directory=/app/backend-service
autostart=true
autorestart=true
stderr_logfile=/var/log/supervisor/backend-service.err.log
stdout_logfile=/var/log/supervisor/backend-service.out.log
user=appuser
environment=ASPNETCORE_URLS="http://+:5001",ASPNETCORE_ENVIRONMENT="Production"
EOF

# 建立整合的 Nginx 配置
RUN cat > /etc/nginx/nginx.conf << 'EOF'
user nginx;
worker_processes auto;
error_log /var/log/nginx/error.log warn;
pid /var/run/nginx.pid;

events {
    worker_connections 1024;
    use epoll;
    multi_accept on;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;
    
    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                    '$status $body_bytes_sent "$http_referer" '
                    '"$http_user_agent" "$http_x_forwarded_for"';
    
    access_log /var/log/nginx/access.log main;
    
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;
    
    # Gzip 壓縮
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/json
        application/javascript
        application/xml+rss
        application/atom+xml
        image/svg+xml;
    
    # 前台系統 (主域名 Port 80)
    server {
        listen 80 default_server;
        server_name _;
        root /app/frontend;
        index index.html;
        
        # 靜態檔案快取設定
        location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
        
        # API 代理設定
        location /api/ {
            proxy_pass http://localhost:5000/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Host $host;
            
            # 處理 CORS 預檢請求
            if ($request_method = 'OPTIONS') {
                add_header 'Access-Control-Allow-Origin' '*' always;
                add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, OPTIONS' always;
                add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization' always;
                add_header 'Access-Control-Max-Age' 1728000 always;
                add_header 'Content-Type' 'text/plain; charset=utf-8';
                add_header 'Content-Length' 0;
                return 204;
            }
            
            # 優化的超時設定
            proxy_connect_timeout 10s;
            proxy_send_timeout 30s;
            proxy_read_timeout 30s;
            proxy_buffering off;
            proxy_request_buffering off;
            
            # 連接池優化
            proxy_http_version 1.1;
            proxy_set_header Connection "";
        }
        
        # SPA 路由支援
        location / {
            try_files $uri $uri/ /index.html;
        }
        
        # 錯誤頁面
        error_page 404 /index.html;
        error_page 500 502 503 504 /50x.html;
    }
    
    # 後台管理系統 (Port 8080)
    server {
        listen 8080;
        server_name _;
        root /app/admin;
        index index.html;
        
        # 靜態檔案快取設定
        location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
        
        # API 代理設定
        location /api/ {
            proxy_pass http://localhost:5000/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Host $host;
            
            # 處理 CORS 預檢請求
            if ($request_method = 'OPTIONS') {
                add_header 'Access-Control-Allow-Origin' '*' always;
                add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, OPTIONS' always;
                add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization' always;
                add_header 'Access-Control-Max-Age' 1728000 always;
                add_header 'Content-Type' 'text/plain; charset=utf-8';
                add_header 'Content-Length' 0;
                return 204;
            }
            
            # 優化的超時設定
            proxy_connect_timeout 10s;
            proxy_send_timeout 30s;
            proxy_read_timeout 30s;
            proxy_buffering off;
            proxy_request_buffering off;
            
            # 連接池優化
            proxy_http_version 1.1;
            proxy_set_header Connection "";
        }
        
        # Angular SPA 路由支援
        location / {
            try_files $uri $uri/ /index.html;
            
            # 防止快取 index.html
            location = /index.html {
                add_header Cache-Control "no-cache, no-store, must-revalidate";
                add_header Pragma "no-cache";
                add_header Expires "0";
            }
        }
        
        # 錯誤頁面
        error_page 404 /index.html;
        error_page 500 502 503 504 /50x.html;
        
        # 安全標頭
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;
        add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    }
}
EOF

# 暴露端口
EXPOSE 80 8080 5000 5001

# 設定環境變數
ENV ASPNETCORE_ENVIRONMENT=Production \
    TZ=Asia/Taipei \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# 健康檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost/ && curl -f http://localhost:8080/ && curl -f http://localhost:5000/api/HomePicture/GetAnnImages || exit 1

# 啟動 Supervisor
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]