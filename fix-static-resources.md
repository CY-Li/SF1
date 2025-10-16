# 前台靜態資源 404 問題修復

## 問題描述

前台系統 (https://sf-test.zeabur.app/) 可以載入，但所有靜態資源（CSS、JS、圖片）都返回 404 錯誤：

```
GET https://sf-test.zeabur.app/scss/home.css 404 (Not Found)
GET https://sf-test.zeabur.app/css/library/bootstrap.min.css 404 (Not Found)
GET https://sf-test.zeabur.app/js/library/jquery.min.js 404 (Not Found)
GET https://sf-test.zeabur.app/images/home/member1.png 404 (Not Found)
```

## 問題原因

1. **nginx 路由配置問題**: 靜態檔案的路由被其他規則攔截
2. **root 路徑設定錯誤**: nginx 無法正確解析靜態資源的路徑
3. **路由優先級問題**: 靜態檔案匹配規則的優先級不正確

## 修復方案

### 1. 修正 nginx 配置

修復後的 nginx 配置：

```nginx
server {
    listen 80;
    server_name _;
    
    # 後台管理系統
    location /admin {
        alias /var/www/admin/;
        try_files $uri $uri/ /admin/index.html;
    }
    
    # API 代理
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        # ... proxy 設定
    }
    
    # 健康檢查
    location /health {
        return 200 "healthy\n";
    }

    # 前台靜態檔案 (優先匹配)
    location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        root /var/www/frontend;
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri =404;
    }

    # 前台系統 (預設路由)
    location / {
        root /var/www/frontend;
        index index.html;
        try_files $uri $uri/ /index.html;
    }
}
```

### 2. 修復重點

1. **移除全域 root 設定**: 避免路徑衝突
2. **靜態檔案優先匹配**: 確保靜態資源能正確載入
3. **正確的 root 路徑**: 為前台設定正確的根目錄

## 部署步驟

1. **提交修復**:
   ```bash
   git add Dockerfile
   git commit -m "fix: 修復前台靜態資源 404 問題"
   git push origin main
   ```

2. **重新部署**:
   - 在 Zeabur 控制台點擊 "Redeploy"
   - 等待部署完成

3. **驗證修復**:
   - 訪問 https://sf-test.zeabur.app/
   - 檢查瀏覽器開發者工具的 Network 標籤
   - 確認所有靜態資源都能正常載入 (200 狀態碼)

## 預期結果

修復後應該能正常載入：
- ✅ CSS 檔案: `/scss/home.css`, `/css/library/bootstrap.min.css`
- ✅ JS 檔案: `/js/library/jquery.min.js`, `/js/library/vue.min.js`
- ✅ 圖片檔案: `/images/home/member1.png`, `/images/home/member2.png`
- ✅ 前台功能: Vue.js 應用正常運作

## 故障排除

如果問題持續存在：

1. **檢查檔案是否存在**:
   ```bash
   # 進入容器檢查
   docker exec -it <container_id> ls -la /var/www/frontend/scss/
   docker exec -it <container_id> ls -la /var/www/frontend/css/library/
   docker exec -it <container_id> ls -la /var/www/frontend/js/library/
   ```

2. **檢查 nginx 配置**:
   ```bash
   docker exec -it <container_id> nginx -t
   docker exec -it <container_id> cat /etc/nginx/sites-available/default
   ```

3. **檢查 nginx 日誌**:
   ```bash
   docker exec -it <container_id> tail -f /var/log/nginx/error.log
   ```

## 技術細節

### nginx 路由匹配順序

nginx 按照以下順序匹配路由：
1. `location = /exact/path` (精確匹配)
2. `location ^~ /prefix` (前綴匹配，停止正則)
3. `location ~ regex` (正則匹配) ← 靜態檔案在這裡
4. `location /prefix` (前綴匹配) ← 前台路由在這裡

### 靜態資源處理

- 使用正則匹配 `~* \.(css|js|png|...)$` 確保靜態檔案優先處理
- 設定正確的 `root` 路徑指向 `/var/www/frontend`
- 使用 `try_files $uri =404` 直接返回 404 而不是 fallback

### 快取策略

- 靜態資源設定 1 年快取: `expires 1y`
- 添加 `Cache-Control` 標頭提升效能
- HTML 檔案不快取，確保更新能及時生效