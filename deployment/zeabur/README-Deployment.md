# ROSCA 平安商會系統 Zeabur 部署指南

本指南將協助您將 ROSCA 平安商會系統部署到 Zeabur 雲端平台。

## 📋 部署前準備

### 1. 帳號準備
- ✅ Zeabur 帳號 (https://zeabur.com)
- ✅ GitHub 帳號 (用於程式碼託管)
- ✅ 域名 (可選，Zeabur 提供免費子域名)

### 2. 本地環境
- ✅ Git
- ✅ Docker (用於本地測試)
- ✅ Node.js 18+ (前端建置)
- ✅ .NET 8 SDK (後端建置)

### 3. 程式碼準備
- ✅ 確保所有程式碼已提交到 GitHub
- ✅ 檢查 Dockerfile 配置正確
- ✅ 驗證 zeabur.json 配置檔案

## 🚀 部署步驟

### 步驟 1: 建立 Zeabur 專案

1. **登入 Zeabur 控制台**
   ```
   https://dash.zeabur.com
   ```

2. **建立新專案**
   - 點擊 "Create Project"
   - 專案名稱: `rosca-system`
   - 選擇適當的區域 (建議選擇離用戶最近的區域)

3. **連接 GitHub 儲存庫**
   - 點擊 "Add Service" → "Git"
   - 選擇您的 GitHub 儲存庫
   - 授權 Zeabur 存取權限

### 步驟 2: 部署資料庫服務

1. **添加 MariaDB 服務**
   ```bash
   # 在 Zeabur 控制台中
   1. 點擊 "Add Service"
   2. 選擇 "Marketplace"
   3. 搜尋並選擇 "MariaDB"
   4. 版本選擇: 11.3.2
   ```

2. **配置資料庫環境變數**
   ```bash
   MYSQL_ROOT_PASSWORD=your_secure_root_password
   MYSQL_DATABASE=rosca_db
   MYSQL_USER=rosca_user
   MYSQL_PASSWORD=your_secure_password
   MYSQL_CHARACTER_SET_SERVER=utf8mb4
   MYSQL_COLLATION_SERVER=utf8mb4_general_ci
   TZ=Asia/Taipei
   ```

3. **配置資料庫初始化**
   - 上傳初始化腳本到 `/docker-entrypoint-initdb.d/`
   - 腳本執行順序: 01-schema.sql → 02-default-data.sql → 03-default-user.sql

### 步驟 3: 部署後端服務

1. **部署 Backend Service (.NET Core 微服務)**
   ```bash
   # 服務名稱: rosca-backend-service
   # 建置路徑: ./backendAPI/DotNetBackEndCleanArchitecture
   # Dockerfile: Presentation/DotNetBackEndService/Dockerfile
   # 連接埠: 5001
   ```

2. **配置環境變數**
   ```bash
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:5001
   ConnectionStrings__BackEndDatabase=Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_password;Database=rosca_db;CharSet=utf8mb4;
   JWT__SecretKey=your-super-secret-jwt-key-min-32-chars
   JWT__Issuer=ROSCA-API
   JWT__Audience=ROSCA-Client
   JWT__ExpiryMinutes=60
   TZ=Asia/Taipei
   ```

3. **配置存儲卷**
   ```bash
   # 在 Zeabur 控制台的 Storage 設定中添加
   uploads: /app/uploads (5GB)
   kyc-images: /app/KycImages (2GB)
   deposit-images: /app/DepositImages (2GB)
   withdraw-images: /app/WithdrawImages (2GB)
   ann-images: /app/AnnImagessss (2GB)
   logs: /app/logs (1GB)
   ```

4. **部署 API Gateway (.NET Core)**
   ```bash
   # 服務名稱: rosca-backend
   # 建置路徑: ./backendAPI/DotNetBackEndCleanArchitecture
   # Dockerfile: Dockerfile
   # 連接埠: 5000
   ```

5. **配置 API Gateway 環境變數**
   ```bash
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:5000
   ConnectionStrings__DefaultConnection=Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_password;Database=rosca_db;CharSet=utf8mb4;
   APIUrl=http://${ZEABUR_BACKEND_SERVICE_DOMAIN}:5001/
   JWT__SecretKey=your-super-secret-jwt-key-min-32-chars
   JWT__Issuer=ROSCA-API
   JWT__Audience=ROSCA-Client
   CORS__AllowedOrigins=https://your-frontend-domain.zeabur.app,https://your-admin-domain.zeabur.app
   ```

### 步驟 4: 部署前端服務

1. **部署前台系統 (Vue.js + Nginx)**
   ```bash
   # 服務名稱: rosca-frontend
   # 建置路徑: ./frontend
   # Dockerfile: Dockerfile
   # 連接埠: 80
   ```

2. **配置前台環境變數**
   ```bash
   VUE_APP_API_BASE_URL=https://your-backend-domain.zeabur.app
   VUE_APP_ENVIRONMENT=production
   ```

3. **部署後台系統 (Angular + Nginx)**
   ```bash
   # 服務名稱: rosca-admin
   # 建置路徑: ./backend/FontEnd
   # Dockerfile: Dockerfile
   # 連接埠: 80
   ```

4. **配置後台環境變數**
   ```bash
   NG_APP_API_BASE_URL=https://your-backend-domain.zeabur.app
   NG_APP_ENVIRONMENT=production
   ```

### 步驟 5: 配置域名和 SSL

1. **設定自訂域名 (可選)**
   ```bash
   # 在 Zeabur 控制台的 Domains 設定中
   前台: www.your-domain.com
   後台: admin.your-domain.com
   API: api.your-domain.com
   ```

2. **SSL 憑證**
   - Zeabur 自動提供 Let's Encrypt SSL 憑證
   - 自訂域名需要設定 DNS CNAME 記錄

3. **DNS 設定範例**
   ```bash
   # 在您的 DNS 提供商設定
   www.your-domain.com    CNAME    your-frontend.zeabur.app
   admin.your-domain.com  CNAME    your-admin.zeabur.app
   api.your-domain.com    CNAME    your-backend.zeabur.app
   ```

## 🔧 部署後配置

### 1. 資料庫初始化驗證

```bash
# 連接到 MariaDB 服務檢查
mysql -h ${ZEABUR_MARIADB_CONNECTION_HOST} -P ${ZEABUR_MARIADB_CONNECTION_PORT} -u rosca_user -p

# 檢查資料庫和表格
USE rosca_db;
SHOW TABLES;
SELECT COUNT(*) FROM Users;
```

### 2. 健康檢查驗證

```bash
# 檢查各服務健康狀態
curl https://your-backend-service.zeabur.app/health
curl https://your-backend.zeabur.app/health
curl https://your-frontend.zeabur.app/
curl https://your-admin.zeabur.app/
```

### 3. 功能測試

1. **用戶註冊登入**
   - 測試新用戶註冊
   - 測試用戶登入
   - 驗證 JWT 認證

2. **檔案上傳**
   - 測試各類型檔案上傳
   - 驗證檔案存儲路徑
   - 檢查檔案存取權限

3. **管理功能**
   - 測試管理後台登入
   - 驗證用戶管理功能
   - 檢查系統設定

## 📊 監控和維護

### 1. 日誌監控

```bash
# 在 Zeabur 控制台查看服務日誌
# 或使用 CLI 工具
zeabur logs rosca-backend-service
zeabur logs rosca-backend
zeabur logs rosca-frontend
zeabur logs rosca-admin
```

### 2. 效能監控

- **CPU 使用率**: 建議保持在 70% 以下
- **記憶體使用率**: 建議保持在 80% 以下
- **磁碟使用率**: 建議保持在 85% 以下
- **回應時間**: API 回應時間應在 500ms 以內

### 3. 自動擴展配置

```json
{
  "scaling": {
    "minReplicas": 1,
    "maxReplicas": 3,
    "targetCPUUtilization": 70,
    "targetMemoryUtilization": 80
  }
}
```

## 🔒 安全配置

### 1. 環境變數安全

- ✅ 使用強密碼 (至少 16 字符，包含大小寫字母、數字、特殊字符)
- ✅ JWT 密鑰至少 32 字符
- ✅ 定期更換敏感資訊
- ✅ 不在程式碼中硬編碼敏感資訊

### 2. 網路安全

- ✅ 啟用 HTTPS (Zeabur 自動配置)
- ✅ 配置適當的 CORS 政策
- ✅ 設定安全標頭
- ✅ 限制 API 存取頻率

### 3. 資料庫安全

- ✅ 使用專用資料庫用戶 (非 root)
- ✅ 限制資料庫連接來源
- ✅ 定期備份資料庫
- ✅ 啟用資料庫日誌記錄

## 🚨 故障排除

### 常見問題

#### 1. 服務無法啟動

```bash
# 檢查服務日誌
zeabur logs service-name

# 常見原因:
- 環境變數配置錯誤
- 資料庫連接失敗
- 連接埠衝突
- 記憶體不足
```

#### 2. 資料庫連接失敗

```bash
# 檢查連接字串格式
Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=user;Password=pass;Database=db;

# 檢查資料庫服務狀態
# 在 Zeabur 控制台查看 MariaDB 服務狀態
```

#### 3. 檔案上傳失敗

```bash
# 檢查存儲卷配置
# 在 Zeabur 控制台的 Storage 設定中確認卷已正確掛載

# 檢查檔案權限
ls -la /app/uploads/

# 檢查磁碟空間
df -h /app/
```

#### 4. 前端無法連接後端

```bash
# 檢查 CORS 設定
CORS__AllowedOrigins=https://frontend-domain.zeabur.app

# 檢查 API 基礎 URL
VUE_APP_API_BASE_URL=https://backend-domain.zeabur.app

# 檢查網路連接
curl https://backend-domain.zeabur.app/health
```

### 效能問題

#### 1. 回應時間過長

- 檢查資料庫查詢效能
- 優化 API 端點
- 啟用快取機制
- 增加服務實例數量

#### 2. 記憶體使用過高

- 檢查記憶體洩漏
- 優化圖片處理
- 調整快取設定
- 增加記憶體配額

#### 3. CPU 使用率過高

- 檢查無限迴圈或死鎖
- 優化演算法效能
- 啟用負載平衡
- 增加 CPU 配額

## 📞 支援資源

### 官方文檔
- [Zeabur 官方文檔](https://zeabur.com/docs)
- [Zeabur CLI 工具](https://zeabur.com/docs/cli)
- [Zeabur 範例專案](https://github.com/zeabur/zeabur)

### 社群支援
- [Zeabur Discord](https://discord.gg/zeabur)
- [GitHub Issues](https://github.com/zeabur/zeabur/issues)

### 緊急聯絡
如遇到緊急問題，請提供以下資訊：
1. 錯誤訊息和日誌
2. 服務配置截圖
3. 重現步驟
4. 影響範圍和緊急程度

---

**注意**: 本指南基於 Zeabur 平台的最新功能編寫，部分功能可能因平台更新而有所變化。建議定期查看官方文檔以獲取最新資訊。