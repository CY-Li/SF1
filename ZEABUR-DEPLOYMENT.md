# Zeabur 部署指南

## 概述

本指南說明如何將 ROSCA 平安商會系統部署到 Zeabur 雲端平台，使用現有的外部 MariaDB 資料庫服務。

## 前置條件

1. Zeabur 帳戶
2. 現有的 MariaDB 資料庫服務 (43.167.174.222:31500)
3. Git 存儲庫已推送到 GitHub/GitLab

## 部署步驟

### 1. 建立 Zeabur 專案

1. 登入 Zeabur 控制台
2. 點擊「Create Project」
3. 輸入專案名稱：`rosca-system`
4. 選擇 Git 存儲庫

### 2. 配置環境變數

在 Zeabur 控制台中設定以下環境變數（參考 `.env.zeabur` 檔案）：

```env
# 基本配置
ASPNETCORE_ENVIRONMENT=Production

# 外部資料庫配置
DB_HOST=43.167.174.222
DB_PORT=31500
DB_NAME=zeabur
DB_USER=root
DB_PASSWORD=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G

# JWT 配置
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production-min-32-chars-rosca-2024
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client
JWT_EXPIRY_MINUTES=60

# CORS 配置 (更新為實際域名)
CORS_ALLOWED_ORIGINS=https://your-app.zeabur.app,https://admin.your-app.zeabur.app

# 檔案上傳配置
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx

# 其他配置
HANGFIRE_DASHBOARD_ENABLED=true
LOG_LEVEL=Information
```

### 3. 部署服務

Zeabur 會根據 `zeabur.json` 配置自動部署以下服務：

1. **backend-service** - .NET Core 微服務 (Port: 5001)
2. **api-gateway** - API Gateway (Port: 5000)
3. **frontend** - 前台系統 (Vue.js + Nginx)
4. **admin** - 後台管理系統 (Angular + Nginx)

### 4. 配置域名

1. 在 Zeabur 控制台中配置域名
2. 前台系統：主域名
3. 後台系統：子域名或路徑
4. 啟用 HTTPS

### 5. 驗證部署

1. 檢查所有服務狀態為「Running」
2. 測試前台系統訪問
3. 測試後台系統訪問
4. 驗證 API 功能
5. 檢查資料庫連接

## 服務架構

```
┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Admin         │
│   (Vue.js)      │    │   (Angular)     │
│   Port: 80      │    │   Port: 8080    │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └──────────┬───────────┘
                     │
          ┌─────────────────┐
          │   API Gateway   │
          │   (.NET Core)   │
          │   Port: 5000    │
          └─────────┬───────┘
                    │
          ┌─────────────────┐
          │ Backend Service │
          │   (.NET Core)   │
          │   Port: 5001    │
          └─────────┬───────┘
                    │
          ┌─────────────────┐
          │   MariaDB       │
          │   (External)    │
          │ 43.167.174.222  │
          │   Port: 31500   │
          └─────────────────┘
```

## 重要配置變更

### 資料庫連接

- 已更新為外部 MariaDB 服務
- 移除本地資料庫依賴
- 連接字串已更新

### 服務間通信

- API Gateway 透過內部網路連接 Backend Service
- 前端服務透過 nginx 代理連接 API Gateway

### 檔案存儲

- 配置持久化存儲卷
- 支援檔案上傳功能

## 故障排除

### 常見問題

1. **資料庫連接失敗**
   - 檢查外部資料庫服務狀態
   - 驗證連接字串和認證資訊

2. **服務啟動失敗**
   - 檢查環境變數配置
   - 查看服務日誌

3. **CORS 錯誤**
   - 更新 CORS_ALLOWED_ORIGINS 環境變數
   - 確保域名配置正確

### 日誌查看

在 Zeabur 控制台中：
1. 選擇對應的服務
2. 點擊「Logs」標籤
3. 查看實時日誌輸出

## 安全注意事項

1. 定期更換 JWT 密鑰
2. 使用強密碼
3. 限制 CORS 來源
4. 定期備份資料庫
5. 監控服務狀態

## 聯絡支援

如遇到部署問題，請聯絡：
- Zeabur 官方支援
- 系統開發團隊