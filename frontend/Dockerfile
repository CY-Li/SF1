# ROSCA 平安商會系統 - 前台系統 Dockerfile (Zeabur 優化版)
# 使用 nginx:alpine 作為基礎映像（最小化映像大小）
FROM nginx:alpine

# 安裝必要套件並清理快取 (減少映像檔案大小)
RUN apk add --no-cache curl tzdata && \
    rm -rf /var/cache/apk/*

# 建立必要的目錄並設定權限
RUN mkdir -p /var/cache/nginx /var/log/nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

# 複製自定義 nginx 配置 (優化順序以利用 Docker 快取)
COPY nginx.conf /etc/nginx/nginx.conf.template

# 複製靜態檔案到 nginx 預設目錄 (排除不必要的檔案)
COPY --chown=nginx:nginx *.html /usr/share/nginx/html/
COPY --chown=nginx:nginx css/ /usr/share/nginx/html/css/
COPY --chown=nginx:nginx js/ /usr/share/nginx/html/js/
COPY --chown=nginx:nginx images/ /usr/share/nginx/html/images/
COPY --chown=nginx:nginx component/ /usr/share/nginx/html/component/

# 暴露端口 80
EXPOSE 80

# 設定環境變數 (針對 Zeabur 優化)
ENV TZ=Asia/Taipei \
    NGINX_WORKER_PROCESSES=auto \
    NGINX_WORKER_CONNECTIONS=1024

# 健康檢查 (優化檢查間隔)
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

# 使用 envsubst 啟動 nginx
CMD envsubst '$BACKEND_API_URL' < /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf && nginx -g 'daemon off;'