# Zeabur 手動部署指南

由於 Zeabur 自動檢測可能只識別到 Angular 專案，我們需要手動配置所有服務。

## 手動部署步驟

### 步驟 1：刪除現有的自動部署

1. 在 Zeabur 控制台中刪除自動創建的 Angular 服務
2. 確保專案是空的

### 步驟 2：手動添加 MariaDB 服務

1. 在 Zeabur 專案中點擊 "Add Service"
2. 選擇 "Marketplace" → "MariaDB"
3. 配置以下環境變數：
   ```
   MYSQL_ROOT_PASSWORD=your_root_password
   MYSQL_DATABASE=rosca_db
   MYSQL_USER=rosca_user
   MYSQL_PASSWORD=your_password
   MYSQL_CHARACTER_SET_SERVER=utf8mb4
   MYSQL_COLLATION_SERVER=utf8mb4_general_ci
   ```

### 步驟 3：添加 Backend Service (.NET Core 微服務)

1. 點擊 "Add Service" → "Git Repository"
2. 選擇您的 GitHub 儲存庫
3. 設定建置配置：
   - **Root Directory**: `backendAPI/DotNetBackEndCleanArchitecture`
   - **Dockerfile Path**: `Presentation/DotNetBackEndService/Dockerfile`
   - **Service Name**: `backend-service`
4. 配置環境變數：
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:5001
   ConnectionStrings__BackEndDatabase=Server=${MARIADB_HOST};Port=3306;User Id=rosca_user;Password=your_password;Database=rosca_db;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;
   JWT__SecretKey=your-32-char-secret-key
   JWT__Issuer=ROSCA-API
   JWT__Audience=ROSCA-Client
   JWT__ExpiryMinutes=60
   LOG_LEVEL=Information
   ```

### 步驟 4：添加 API Gateway (.NET Core)

1. 點擊 "Add Service" → "Git Repository"
2. 選擇您的 GitHub 儲存庫
3. 設定建置配置：
   - **Root Directory**: `backendAPI/DotNetBackEndCleanArchitecture`
   - **Dockerfile Path**: `Dockerfile`
   - **Service Name**: `backend`
4. 配置環境變數：
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:5000
   ConnectionStrings__BackEndDatabase=Server=${MARIADB_HOST};Port=3306;User Id=rosca_user;Password=your_password;Database=rosca_db;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;
   APIUrl=http://${BACKEND_SERVICE_HOST}:5001/
   JWT__SecretKey=your-32-char-secret-key
   JWT__Issuer=ROSCA-API
   JWT__Audience=ROSCA-Client
   JWT__ExpiryMinutes=60
   CORS__AllowedOrigins=https://your-domain.zeabur.app
   FILE_UPLOAD_MAX_SIZE=10485760
   FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf
   HANGFIRE_DASHBOARD_ENABLED=true
   LOG_LEVEL=Information
   ```

### 步驟 5：添加前台系統 (Vue.js + Nginx)

1. 點擊 "Add Service" → "Git Repository"
2. 選擇您的 GitHub 儲存庫
3. 設定建置配置：
   - **Root Directory**: `frontend`
   - **Dockerfile Path**: `Dockerfile`
   - **Service Name**: `frontend`

### 步驟 6：添加後台系統 (Angular + Nginx)

1. 點擊 "Add Service" → "Git Repository"
2. 選擇您的 GitHub 儲存庫
3. 設定建置配置：
   - **Root Directory**: `backend/FontEnd`
   - **Dockerfile Path**: `Dockerfile`
   - **Service Name**: `admin`

### 步驟 7：配置域名

1. 為 `frontend` 服務設定主域名
2. 為 `admin` 服務設定子域名
3. 更新 `CORS_ALLOWED_ORIGINS` 環境變數

## 重要注意事項

### 服務間連接

在 Zeabur 中，服務間可以通過內部域名連接：

- MariaDB: `${MARIADB_HOST}` (Zeabur 自動提供)
- Backend Service: `${BACKEND_SERVICE_HOST}` (需要在環境變數中設定)
- API Gateway: `${BACKEND_HOST}` (需要在 nginx 配置中使用)

### 環境變數替換

在添加每個服務後，需要將環境變數中的佔位符替換為實際的服務域名：

1. 獲取 MariaDB 的內部域名
2. 獲取 Backend Service 的內部域名
3. 更新相關服務的環境變數

### 資料庫初始化

由於手動部署無法自動執行初始化腳本，您需要：

1. 連接到 MariaDB 服務
2. 手動執行以下 SQL 檔案：
   - `database/init/01-schema.sql`
   - `database/init/02-default-data.sql`
   - `database/init/03-default-user.sql`

## 驗證部署

部署完成後，按以下順序驗證：

1. **MariaDB**: 檢查資料庫服務狀態
2. **Backend Service**: 訪問健康檢查端點
3. **API Gateway**: 測試 API 調用
4. **Frontend**: 訪問前台網站
5. **Admin**: 訪問後台管理系統

## 故障排除

如果遇到問題：

1. 檢查服務日誌
2. 驗證環境變數設定
3. 確認服務間網路連接
4. 檢查 Dockerfile 建置過程