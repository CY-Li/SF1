# Zeabur 前端服務部署指南

## 概述

本指南說明如何在 Zeabur 平台部署 ROSCA 系統的前端服務，包括前台系統 (Vue.js + Nginx) 和後台管理系統 (Angular + Nginx)。

## 服務架構

```
┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Admin         │
│   (Vue.js)      │    │   (Angular)     │
│   Port: 80      │    │   Port: 80      │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └──────────┬───────────┘
                     │
          ┌─────────────────┐
          │   API Gateway   │
          │   (.NET Core)   │
          │   Port: 5000    │
          └─────────────────┘
```

## 部署步驟

### 步驟 1：準備前端服務部署

#### 1.1 驗證 Dockerfile 配置

確認以下 Dockerfile 已優化：

**前台系統 Dockerfile:**
- 路徑: `frontend/Dockerfile`
- 基礎映像: `nginx:alpine`
- 端口: 80
- 健康檢查: `http://localhost/`

**後台系統 Dockerfile:**
- 路徑: `backend/FontEnd/Dockerfile`
- 多階段建置: Node.js 建置 + Nginx 運行
- 端口: 80
- 健康檢查: `http://localhost/`

#### 1.2 確認 zeabur.json 配置

```json
{
  "services": {
    "frontend": {
      "build": {
        "dockerfile": "frontend/Dockerfile"
      },
      "name": "frontend",
      "env": {
        "TZ": "Asia/Taipei"
      },
      "resources": {
        "memory": "256Mi",
        "cpu": "0.25"
      }
    },
    "admin": {
      "build": {
        "dockerfile": "backend/FontEnd/Dockerfile"
      },
      "name": "admin",
      "env": {
        "TZ": "Asia/Taipei"
      },
      "resources": {
        "memory": "256Mi",
        "cpu": "0.25"
      }
    }
  },
  "domains": [
    {
      "name": "frontend",
      "service": "frontend"
    },
    {
      "name": "admin",
      "service": "admin"
    }
  ]
}
```

### 步驟 2：配置 API 代理連接

#### 2.1 前台系統 Nginx 配置

**檔案:** `frontend/nginx.conf`

關鍵配置：
```nginx
# API 代理設定
location /api/ {
    resolver 127.0.0.11 valid=30s;
    set $backend_upstream api-gateway:5000;
    
    proxy_pass http://$backend_upstream;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    
    # CORS 處理
    if ($request_method = 'OPTIONS') {
        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization' always;
        return 204;
    }
}

# SPA 路由支援
location / {
    try_files $uri $uri/ /index.html;
}
```

#### 2.2 後台系統 Nginx 配置

**檔案:** `backend/FontEnd/admin-nginx.conf`

關鍵配置：
```nginx
# API 代理設定
location /api/ {
    resolver 127.0.0.11 valid=30s;
    set $backend_upstream api-gateway:5000;
    
    proxy_pass http://$backend_upstream/api/;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}

# Angular SPA 路由支援
location / {
    try_files $uri $uri/ /index.html;
    
    # 防止快取 index.html
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
    }
}
```

### 步驟 3：在 Zeabur 控制台部署

#### 3.1 部署前台系統

1. 在 Zeabur 專案中，系統會自動偵測 `frontend` 配置
2. 點擊 **Deploy** 開始建置
3. 監控建置日誌，確保無錯誤
4. 等待服務狀態變為 **Running**

**預期建置時間:** 1-2 分鐘

#### 3.2 部署後台系統

1. 系統會自動偵測 `admin` 配置
2. 點擊 **Deploy** 開始建置
3. 監控建置日誌，確保 Angular 建置成功
4. 等待服務狀態變為 **Running**

**預期建置時間:** 3-5 分鐘 (包含 Angular 建置)

### 步驟 4：配置域名

#### 4.1 Zeabur 自動域名

Zeabur 會自動為每個服務分配域名：

- **前台系統**: `https://frontend-{project-id}.zeabur.app`
- **後台系統**: `https://admin-{project-id}.zeabur.app`

#### 4.2 自定義域名 (可選)

在 Zeabur 控制台配置自定義域名：

1. 前往專案設定 → 域名
2. 添加自定義域名
3. 配置 DNS 記錄
4. 啟用 HTTPS

**建議域名結構:**
- 前台: `https://yourdomain.com`
- 後台: `https://admin.yourdomain.com`

### 步驟 5：驗證部署

#### 5.1 檢查服務狀態

在 Zeabur 控制台確認：

- ✅ **frontend**: Status = Running, Port = 80
- ✅ **admin**: Status = Running, Port = 80

#### 5.2 測試前台系統

```bash
# 健康檢查
curl -f https://frontend-{project-id}.zeabur.app/

# 測試 API 代理
curl -f https://frontend-{project-id}.zeabur.app/api/HomePicture/GetAnnImages
```

#### 5.3 測試後台系統

```bash
# 健康檢查
curl -f https://admin-{project-id}.zeabur.app/

# 測試 API 代理
curl -f https://admin-{project-id}.zeabur.app/api/HomePicture/GetAnnImages
```

#### 5.4 功能測試

**前台系統測試:**
1. 訪問首頁
2. 測試用戶註冊/登入
3. 測試檔案上傳功能
4. 驗證 API 調用

**後台系統測試:**
1. 訪問管理後台
2. 測試管理員登入
3. 測試各管理功能
4. 驗證資料顯示

## 環境變數配置

### 前台系統環境變數

| 變數名稱 | 說明 | 範例值 |
|---------|------|--------|
| `TZ` | 時區設定 | `Asia/Taipei` |

### 後台系統環境變數

| 變數名稱 | 說明 | 範例值 |
|---------|------|--------|
| `TZ` | 時區設定 | `Asia/Taipei` |

### CORS 配置更新

部署完成後，需要更新後端服務的 CORS 設定：

```env
# 更新 API Gateway 的 CORS 設定
CORS_ALLOWED_ORIGINS=https://frontend-{project-id}.zeabur.app,https://admin-{project-id}.zeabur.app
```

## 靜態資源優化

### 快取策略

兩個前端服務都配置了優化的快取策略：

```nginx
# 靜態檔案長期快取
location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}

# HTML 檔案不快取
location = /index.html {
    add_header Cache-Control "no-cache, no-store, must-revalidate";
}
```

### Gzip 壓縮

啟用 Gzip 壓縮以減少傳輸大小：

```nginx
gzip on;
gzip_vary on;
gzip_min_length 1024;
gzip_comp_level 6;
gzip_types
    text/plain
    text/css
    text/javascript
    application/json
    application/javascript
    image/svg+xml;
```

## 監控和日誌

### 日誌查看

在 Zeabur 控制台：
1. 選擇對應的前端服務
2. 點擊 **Logs** 標籤
3. 查看 Nginx 存取和錯誤日誌

### 關鍵日誌指標

**成功啟動日誌:**
```
nginx: [notice] start worker processes
nginx: [notice] start worker process 1
```

**API 代理日誌:**
```
GET /api/HomePicture/GetAnnImages HTTP/1.1" 200
POST /api/Auth/Login HTTP/1.1" 200
```

**錯誤日誌範例:**
```
[error] connect() failed (111: Connection refused) while connecting to upstream
[warn] no resolver defined to resolve api-gateway
```

## 故障排除

### 常見問題

#### 1. API 代理連接失敗

**症狀:** 前端無法調用後端 API

**解決方案:**
1. 檢查 API Gateway 服務狀態
2. 確認 nginx 配置中的 upstream 設定
3. 檢查服務間網路連通性

**測試命令:**
```bash
# 在前端容器中測試
curl -f http://api-gateway:5000/api/HomePicture/GetAnnImages
```

#### 2. CORS 錯誤

**症狀:** 瀏覽器控制台顯示 CORS 錯誤

**解決方案:**
1. 更新後端 `CORS_ALLOWED_ORIGINS` 環境變數
2. 檢查 nginx CORS 配置
3. 確認域名配置正確

#### 3. SPA 路由不工作

**症狀:** 直接訪問子路由返回 404

**解決方案:**
1. 確認 nginx 配置包含 `try_files $uri $uri/ /index.html;`
2. 檢查前端路由配置
3. 驗證 base href 設定

#### 4. 靜態資源載入失敗

**症狀:** CSS/JS 檔案 404 錯誤

**解決方案:**
1. 檢查檔案複製路徑
2. 確認 nginx 靜態資源配置
3. 驗證檔案權限設定

### 效能調優

#### 資源配置建議

**生產環境:**
- Frontend: 0.5 vCPU, 512MB RAM
- Admin: 0.5 vCPU, 512MB RAM

**開發/測試環境:**
- Frontend: 0.25 vCPU, 256MB RAM
- Admin: 0.25 vCPU, 256MB RAM

#### Nginx 優化

```nginx
# 工作進程優化
worker_processes auto;
worker_connections 1024;

# 連接優化
keepalive_timeout 65;
tcp_nopush on;
tcp_nodelay on;

# 緩衝區優化
client_max_body_size 10M;
client_body_buffer_size 128k;
```

## 安全考量

### 安全標頭

後台系統已配置安全標頭：

```nginx
add_header X-Frame-Options "SAMEORIGIN" always;
add_header X-Content-Type-Options "nosniff" always;
add_header X-XSS-Protection "1; mode=block" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
```

### HTTPS 強制

Zeabur 自動提供 HTTPS：
- 自動 SSL 憑證
- HTTP 到 HTTPS 重定向
- HSTS 標頭

### 存取控制

1. **前台系統**: 公開訪問
2. **後台系統**: 建議限制管理員 IP 存取
3. **API 端點**: 透過 JWT 認證保護

## 下一步

完成前端服務部署後，繼續執行：
- **任務 5**: 配置和測試
- 更新 CORS 設定
- 進行端到端功能測試