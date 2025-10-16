# ROSCA 平安商會系統 Zeabur 一鍵部署指南

## 🚀 一鍵部署到 Zeabur

本專案已優化為單一 Docker 映像檔，包含所有服務，可直接部署到 Zeabur 平台。

### 🔧 Angular 建置問題解決方案

如果遇到 Angular 建置失敗 (`ng: not found`)，有兩種解決方案：

#### 方案 1: 修復 Angular 建置 (推薦)
- 已修復 `npm ci --only=production` 問題
- 現在使用 `npm ci` 安裝完整依賴包含 Angular CLI

#### 方案 2: 跳過 Angular 建置
如果仍有問題，可使用備用 Dockerfile：
```bash
# 重命名檔案
mv Dockerfile Dockerfile.with-angular
mv Dockerfile.no-angular Dockerfile
# 重新部署
```

### 📋 部署前準備

1. **Zeabur 帳號**: 註冊 [Zeabur](https://zeabur.com) 帳號
2. **GitHub 儲存庫**: 將程式碼推送到 GitHub
3. **環境變數**: 準備必要的環境變數

### 🎯 部署步驟

#### 1. 在 Zeabur 建立專案

1. 登入 [Zeabur Dashboard](https://dash.zeabur.com)
2. 點擊 "Create Project"
3. 專案名稱: `rosca-system`

#### 2. 添加 MariaDB 服務

1. 在專案中點擊 "Add Service"
2. 選擇 "Marketplace" → "MariaDB"
3. 版本選擇: `11.3.2`
4. 服務名稱: `mariadb`

#### 3. 配置資料庫環境變數

在 MariaDB 服務的環境變數中設定：

```bash
MYSQL_ROOT_PASSWORD=your_secure_root_password_2024!
MYSQL_DATABASE=rosca_db
MYSQL_USER=rosca_user
MYSQL_PASSWORD=your_secure_password_2024!
MYSQL_CHARACTER_SET_SERVER=utf8mb4
MYSQL_COLLATION_SERVER=utf8mb4_general_ci
TZ=Asia/Taipei
```

#### 4. 部署主應用程式

1. 點擊 "Add Service" → "Git"
2. 選擇您的 GitHub 儲存庫
3. 服務名稱: `app`
4. Zeabur 會自動偵測 `zeabur.json` 配置

#### 5. 配置應用程式環境變數

在主應用程式服務中設定以下環境變數：

```bash
# 基本配置
ASPNETCORE_ENVIRONMENT=Production
TZ=Asia/Taipei

# JWT 配置 (請修改為您的密鑰)
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production-min-32-chars
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client
JWT_EXPIRY_MINUTES=60

# 資料庫配置 (使用您在步驟3設定的值)
DB_USER=rosca_user
DB_PASSWORD=your_secure_password_2024!
DB_NAME=rosca_db

# CORS 配置 (部署後更新為實際域名)
CORS_ALLOWED_ORIGINS=https://your-app.zeabur.app

# 檔案上傳配置
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx

# 功能開關
HANGFIRE_DASHBOARD_ENABLED=true
LOG_LEVEL=Information
```

#### 6. 配置存儲卷

Zeabur 會根據 `zeabur.json` 自動配置以下存儲卷：

- `uploads` (5GB) - 通用檔案上傳
- `kyc-images` (2GB) - KYC 身份認證圖片
- `deposit-images` (2GB) - 存款憑證圖片
- `withdraw-images` (2GB) - 提款憑證圖片
- `ann-images` (2GB) - 公告圖片
- `logs` (1GB) - 應用程式日誌

#### 7. 設定域名 (可選)

1. 在服務設定中點擊 "Domains"
2. 添加自訂域名或使用 Zeabur 提供的免費域名
3. 更新 CORS 環境變數為實際域名

### 🌐 存取方式

部署完成後，您可以透過以下方式存取：

- **前台系統**: `https://your-app.zeabur.app/`
- **後台系統**: `https://your-app.zeabur.app/admin/`
- **API 文檔**: `https://your-app.zeabur.app/swagger/`
- **健康檢查**: `https://your-app.zeabur.app/health`

### 🔧 系統架構

```
┌─────────────────────────────────────┐
│         Zeabur 平台                 │
├─────────────────────────────────────┤
│  ┌─────────────────────────────┐    │
│  │      主應用程式 (app)        │    │
│  │  ┌─────────────────────┐    │    │
│  │  │      Nginx          │    │    │
│  │  │   (反向代理)        │    │    │
│  │  └─────────────────────┘    │    │
│  │  ┌─────────────────────┐    │    │
│  │  │   前台 (Vue.js)     │    │    │
│  │  └─────────────────────┘    │    │
│  │  ┌─────────────────────┐    │    │
│  │  │  後台 (Angular)     │    │    │
│  │  └─────────────────────┘    │    │
│  │  ┌─────────────────────┐    │    │
│  │  │  API Gateway        │    │    │
│  │  │  (.NET Core)        │    │    │
│  │  └─────────────────────┘    │    │
│  │  ┌─────────────────────┐    │    │
│  │  │ Backend Service     │    │    │
│  │  │  (.NET Core)        │    │    │
│  │  └─────────────────────┘    │    │
│  └─────────────────────────────┘    │
│  ┌─────────────────────────────┐    │
│  │     MariaDB 11.3.2          │    │
│  │      (資料庫)               │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

### 📊 資源配置

- **CPU**: 0.5 - 1.0 核心
- **記憶體**: 512MB - 1GB
- **存儲**: 15GB (應用程式) + 11GB (資料庫)
- **網路**: 自動配置

### 🔍 部署後驗證

#### 1. 檢查服務狀態

```bash
# 健康檢查
curl https://your-app.zeabur.app/health

# API 測試
curl https://your-app.zeabur.app/api/system/info
```

#### 2. 測試功能

1. **前台系統**: 訪問首頁，測試用戶註冊/登入
2. **後台系統**: 訪問 `/admin/`，測試管理員登入
3. **檔案上傳**: 測試各類型檔案上傳功能
4. **資料庫**: 檢查資料是否正確初始化

### 🚨 故障排除

#### 常見問題

1. **Angular 建置失敗 (`ng: not found`)**
   - 原因：Angular CLI 未安裝或不在 PATH 中
   - 解決方案：
     ```bash
     # 方案 1: 使用修復後的 Dockerfile (推薦)
     git pull  # 獲取最新修復
     
     # 方案 2: 使用無 Angular 建置版本
     mv Dockerfile Dockerfile.with-angular
     mv Dockerfile.no-angular Dockerfile
     git add . && git commit -m "Use no-angular Dockerfile"
     git push
     ```

2. **服務無法啟動**
   - 檢查環境變數是否正確設定
   - 查看服務日誌找出錯誤原因

3. **資料庫連接失敗**
   - 確認 MariaDB 服務正常運行
   - 檢查資料庫環境變數設定

4. **檔案上傳失敗**
   - 確認存儲卷已正確掛載
   - 檢查檔案大小和類型限制

5. **前端無法連接後端**
   - 檢查 CORS 設定
   - 確認域名配置正確

#### 查看日誌

在 Zeabur Dashboard 中：
1. 選擇對應的服務
2. 點擊 "Logs" 標籤
3. 查看即時日誌輸出

### 🔒 安全建議

1. **修改預設密碼**: 使用強密碼替換所有預設值
2. **定期更新**: 定期更新 JWT 密鑰和資料庫密碼
3. **HTTPS**: 確保所有連接都使用 HTTPS
4. **備份**: 定期備份資料庫和重要檔案

### 📈 效能優化

1. **資源監控**: 定期檢查 CPU 和記憶體使用情況
2. **資料庫優化**: 根據使用情況調整資料庫配置
3. **快取策略**: 啟用適當的快取機制
4. **CDN**: 考慮使用 CDN 加速靜態資源

---

## 🎉 部署完成！

恭喜！您的 ROSCA 平安商會系統已成功部署到 Zeabur 平台。

**下一步**:
1. 建立管理員帳號
2. 測試所有功能
3. 配置監控和備份
4. 進行用戶培訓

如有問題，請參考 Zeabur 官方文檔或聯絡技術支援。