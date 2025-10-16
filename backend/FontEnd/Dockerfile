# 多階段建置：第一階段 - 建置 Angular 應用程式 (Zeabur 優化版)
FROM node:18-alpine AS build

# 安裝必要的建置工具並清理快取
RUN apk add --no-cache python3 make g++ && \
    rm -rf /var/cache/apk/*

# 設定工作目錄
WORKDIR /app

# 複製 package.json 和 package-lock.json (利用 Docker 快取)
COPY FontEnd/package*.json ./

# 安裝依賴 (優化 npm 安裝)
RUN npm ci --only=production --no-audit --no-fund && \
    npm cache clean --force

# 複製源代碼 (排除不必要的檔案)
COPY FontEnd/src/ ./src/
COPY FontEnd/angular.json ./
COPY FontEnd/tsconfig*.json ./
COPY FontEnd/*.js ./

# 建置應用程式 (生產環境優化)
RUN npm run build -- --configuration=production --optimization=true --aot=true --build-optimizer=true

# 第二階段 - 生產環境 (Zeabur 優化版)
FROM nginx:alpine

# 安裝必要套件並清理快取 (減少映像檔案大小)
RUN apk add --no-cache curl tzdata && \
    rm -rf /var/cache/apk/*

# 建立必要的目錄並設定權限
RUN mkdir -p /var/cache/nginx /var/log/nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

# 複製自定義 nginx 配置
COPY FontEnd/admin-nginx.conf /etc/nginx/nginx.conf

# 複製啟動腳本
COPY FontEnd/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# 複製建置好的檔案到 nginx 目錄
COPY --from=build --chown=nginx:nginx /app/dist/font-end/browser/ /usr/share/nginx/html/

# 暴露端口 80
EXPOSE 80

# 設定環境變數 (針對 Zeabur 優化)
ENV TZ=Asia/Taipei \
    NGINX_WORKER_PROCESSES=auto \
    NGINX_WORKER_CONNECTIONS=1024

# 健康檢查 (優化檢查間隔)
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

# 使用自定義啟動腳本
ENTRYPOINT ["/docker-entrypoint.sh"]