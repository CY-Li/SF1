# 前後台路由問題修復指南

## 問題描述

訪問 https://sf-test.zeabur.app/ 時顯示的是後台管理系統而不是前台用戶系統。

## 問題原因

1. **nginx 路由配置問題**: 原始配置中前台和後台的路由優先級設定不當
2. **Angular base href 問題**: Angular 應用的 base href 設定可能影響路由
3. **檔案複製順序問題**: 可能存在檔案覆蓋的情況

## 修復方案

### 1. 修復的 nginx 配置

修復後的 nginx 配置調整了路由優先級：

```nginx
server {
    listen 80;
    server_name _;
    
    # 後台管理系統 (優先匹配，避免被根路徑攔截)
    location /admin {
        alias /var/www/admin/;
        try_files $uri $uri/ /admin/index.html;
    }
    
    # 處理 Angular 路由
    location ~ ^/admin/(.*)$ {
        alias /var/www/admin/;
        try_files $1 $1/ /admin/index.html;
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

    # 前台系統 (放在最後，作為預設路由)
    location / {
        root /var/www/frontend;
        try_files $uri $uri/ /index.html;
    }
}
```

### 2. 修復的重點

1. **路由優先級**: 將 `/admin` 路由放在根路徑 `/` 之前
2. **Angular base href**: 確保正確設定為 `/admin/`
3. **檔案結構**: 確保前台檔案在 `/var/www/frontend/`，後台檔案在 `/var/www/admin/`

## 部署步驟

### 1. 應用修復

修復已經應用到 `Dockerfile`，包含：
- ✅ 修正的 nginx 配置
- ✅ 正確的路由優先級
- ✅ Angular base href 修正

### 2. 重新部署

1. **提交變更到 GitHub**:
   ```bash
   git add Dockerfile
   git commit -m "fix: 修復前後台路由問題 - 確保根路徑顯示前台系統"
   git push origin main
   ```

2. **在 Zeabur 控制台觸發重新部署**:
   - 登入 Zeabur 控制台
   - 找到 `rosca-system` 專案
   - 點擊 `app` 服務
   - 點擊 "Redeploy" 按鈕

3. **等待部署完成** (通常需要 5-10 分鐘)

### 3. 驗證修復

部署完成後，測試以下 URL：

1. **前台系統** (根路徑):
   ```
   https://sf-test.zeabur.app/
   ```
   應該顯示：商會活動花絮、加入會員、近期公告等前台內容

2. **後台系統** (admin 路徑):
   ```
   https://sf-test.zeabur.app/admin
   ```
   應該顯示：Angular 管理後台界面

3. **API 端點**:
   ```
   https://sf-test.zeabur.app/api/health
   ```
   應該返回 API 健康狀態

4. **健康檢查**:
   ```
   https://sf-test.zeabur.app/health
   ```
   應該返回 "healthy"

## 預期結果

修復後的系統應該：

- ✅ **根路徑 `/`**: 顯示前台用戶系統 (Vue.js 應用)
- ✅ **Admin 路徑 `/admin`**: 顯示後台管理系統 (Angular 應用)
- ✅ **API 路徑 `/api/*`**: 正確代理到後端服務
- ✅ **健康檢查 `/health`**: 返回服務狀態

## 故障排除

如果修復後仍有問題：

### 1. 檢查部署狀態
```bash
# 在 Zeabur 控制台查看服務日誌
# 確認容器已成功重新啟動
```

### 2. 檢查 nginx 配置
```bash
# 進入容器檢查 nginx 配置
docker exec -it <container_id> cat /etc/nginx/sites-available/default
```

### 3. 檢查檔案結構
```bash
# 檢查前台檔案
docker exec -it <container_id> ls -la /var/www/frontend/

# 檢查後台檔案
docker exec -it <container_id> ls -la /var/www/admin/
```

### 4. 清除瀏覽器快取
- 按 `Ctrl+F5` 強制重新載入
- 或開啟無痕模式測試

## 技術細節

### nginx 路由匹配順序

nginx 按照以下順序匹配路由：
1. `location = /exact/path` (精確匹配)
2. `location ^~ /prefix` (前綴匹配，停止正則)
3. `location ~ regex` (正則匹配)
4. `location /prefix` (前綴匹配)

我們的配置利用這個順序，確保：
- `/admin` 優先匹配到後台
- `/` 作為最後的預設匹配到前台

### Angular 路由處理

Angular SPA 需要特殊的路由處理：
- 所有未匹配的路徑都應該返回 `index.html`
- `base href` 必須正確設定
- 靜態資源路徑需要正確解析

## 相關檔案

- `Dockerfile`: 主要的容器建置檔案
- `frontend/`: 前台 Vue.js 應用檔案
- `backend/FontEnd/FontEnd/`: 後台 Angular 應用檔案
- `zeabur.json`: Zeabur 部署配置

## 聯絡支援

如果問題持續存在，請提供：
1. 錯誤截圖
2. 瀏覽器開發者工具的 Network 標籤
3. Zeabur 服務日誌
4. 訪問的具體 URL 和預期行為