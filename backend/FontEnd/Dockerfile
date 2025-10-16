# 多階段建置：第一階段 - 建置 Angular 應用程式
FROM node:18-alpine AS build

# 設定工作目錄
WORKDIR /app

# 複製 package.json 和 package-lock.json
COPY FontEnd/package*.json ./

# 安裝依賴
RUN npm ci --only=production

# 複製源代碼
COPY FontEnd/ ./

# 建置應用程式 (生產環境)
RUN npm run build -- --configuration=production

# 第二階段 - 生產環境
FROM nginx:alpine

# 安裝 curl 用於健康檢查
RUN apk add --no-cache curl

# 複製自定義 nginx 配置
COPY admin-nginx.conf /etc/nginx/nginx.conf

# 複製啟動腳本
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# 複製建置好的檔案到 nginx 目錄
COPY --from=build /app/dist/font-end/browser/ /usr/share/nginx/html/

# 建立必要的目錄並設定權限
RUN mkdir -p /var/cache/nginx /var/log/nginx && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

# 暴露端口 80
EXPOSE 80

# 健康檢查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

# 使用自定義啟動腳本
ENTRYPOINT ["/docker-entrypoint.sh"]