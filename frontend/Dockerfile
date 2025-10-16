# ROSCA 平安商會系統 - 前台系統 Dockerfile
# 使用 nginx:alpine 作為基礎映像（最小化映像大小）
FROM nginx:alpine

# 安裝 curl 用於健康檢查
RUN apk add --no-cache curl tzdata

# 複製靜態檔案到 nginx 預設目錄
COPY . /usr/share/nginx/html/

# 複製自定義 nginx 配置
COPY nginx.conf /etc/nginx/nginx.conf

# 建立必要的目錄並設定權限
RUN mkdir -p /var/cache/nginx /var/log/nginx && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

# 暴露端口 80
EXPOSE 80

# 設定環境變數
ENV TZ=Asia/Taipei

# 健康檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

# 啟動 nginx
CMD ["nginx", "-g", "daemon off;"]