# 使用 nginx:alpine 作為基礎映像
FROM nginx:alpine

# 複製自定義 nginx 配置
COPY admin-nginx.conf /etc/nginx/nginx.conf

# 複製建置好的檔案到 nginx 目錄
COPY FontEnd/dist/font-end/browser/ /usr/share/nginx/html/

# 建立必要的目錄並設定權限
RUN mkdir -p /var/cache/nginx /var/log/nginx && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

# 暴露端口 80
EXPOSE 80

# 健康檢查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

# 啟動 nginx
CMD ["nginx", "-g", "daemon off;"]