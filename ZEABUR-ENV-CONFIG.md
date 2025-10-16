# Zeabur 環境變數正確配置

## 🚨 當前配置問題

你的環境變數配置有以下問題：

1. **重複配置**: 同時設定了 `MARIADB_*` 和 `MYSQL_*` 變數
2. **變數引用錯誤**: 使用了 `${PASSWORD}` 但在 Zeabur 中不會正確解析
3. **不必要的變數**: Zeabur 會自動提供連接資訊
4. **資料庫名稱衝突**: `MARIADB_DATABASE=zeabur` 與 `MYSQL_DATABASE=rosca_db` 衝突

## ✅ 正確的環境變數配置

### 1. MariaDB 服務環境變數

在 Zeabur 控制台的 **mariadb 服務** 中設定：

```bash
# MariaDB 服務配置 (必需)
MYSQL_ROOT_PASSWORD=your_secure_root_password_2024!
MYSQL_DATABASE=rosca_db
MYSQL_USER=rosca_user
MYSQL_PASSWORD=your_secure_password_2024!
MYSQL_CHARACTER_SET_SERVER=utf8mb4
MYSQL_COLLATION_SERVER=utf8mb4_general_ci
TZ=Asia/Taipei
```

### 2. 應用服務環境變數

在 Zeabur 控制台的 **app 服務** 中設定：

```bash
# 資料庫連接配置 (用於應用連接字串)
DB_NAME=rosca_db
DB_USER=rosca_user
DB_PASSWORD=your_secure_password_2024!
DB_ROOT_PASSWORD=your_secure_root_password_2024!

# JWT 配置
JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-32-characters-long-2024
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client
JWT_EXPIRY_MINUTES=60

# CORS 配置
CORS_ALLOWED_ORIGINS=https://sf-test.zeabur.app

# 檔案上傳配置
FILE_UPLOAD_MAX_SIZE=52428800
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf

# 其他配置
HANGFIRE_DASHBOARD_ENABLED=true
LOG_LEVEL=Information

# 時區
TZ=Asia/Taipei
```

## 🔧 修復步驟

### 1. 清理錯誤的環境變數

在 Zeabur 控制台**刪除**以下變數：
```bash
❌ MARIADB_PORT
❌ MARIADB_ROOT_PASSWORD
❌ MARIADB_USERNAME
❌ MARIADB_DATABASE
❌ MARIADB_HOST
❌ PASSWORD
❌ MARIADB_PASSWORD
```

### 2. 設定 MariaDB 服務環境變數

1. 進入 Zeabur 控制台
2. 選擇 `rosca-system` 專案
3. 點擊 **mariadb 服務**
4. 進入 "Environment Variables" 標籤
5. 設定以下變數：

```bash
MYSQL_ROOT_PASSWORD=your_secure_root_password_2024!
MYSQL_DATABASE=rosca_db
MYSQL_USER=rosca_user
MYSQL_PASSWORD=your_secure_password_2024!
MYSQL_CHARACTER_SET_SERVER=utf8mb4
MYSQL_COLLATION_SERVER=utf8mb4_general_ci
TZ=Asia/Taipei
```

### 3. 設定應用服務環境變數

1. 點擊 **app 服務**
2. 進入 "Environment Variables" 標籤
3. 設定以下變數：

```bash
# 資料庫配置
DB_NAME=rosca_db
DB_USER=rosca_user
DB_PASSWORD=your_secure_password_2024!
DB_ROOT_PASSWORD=your_secure_root_password_2024!

# JWT 配置
JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-32-characters-long-2024
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client
JWT_EXPIRY_MINUTES=60

# CORS 配置
CORS_ALLOWED_ORIGINS=https://sf-test.zeabur.app

# 檔案上傳配置
FILE_UPLOAD_MAX_SIZE=52428800
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf

# 其他配置
HANGFIRE_DASHBOARD_ENABLED=true
LOG_LEVEL=Information
TZ=Asia/Taipei
```

## 🔍 Zeabur 自動提供的變數

Zeabur 會自動提供以下連接變數，**不需要手動設定**：

```bash
# 這些變數由 Zeabur 自動生成，不要手動設定
ZEABUR_MARIADB_CONNECTION_HOST    # MariaDB 主機名
ZEABUR_MARIADB_CONNECTION_PORT    # MariaDB 端口 (通常是 3306)
```

## 📋 完整的連接字串範例

基於正確的環境變數，連接字串會是：

```
Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_secure_password_2024!;Database=rosca_db;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;
```

## ⚠️ 安全注意事項

1. **密碼強度**: 使用至少 16 字符的強密碼
2. **JWT 密鑰**: 至少 32 字符，包含大小寫字母、數字、特殊字符
3. **不要在程式碼中硬編碼**: 所有敏感資訊都通過環境變數設定
4. **定期更換**: 建議定期更換密碼和密鑰

## 🚀 部署順序

1. **先設定 MariaDB 環境變數**
2. **重新啟動 MariaDB 服務**
3. **等待 MariaDB 完全啟動** (約 2-3 分鐘)
4. **設定應用環境變數**
5. **重新部署應用服務**

## 🔍 驗證配置

設定完成後，可以使用以下方式驗證：

```bash
# 測試資料庫連接
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'

# 回應時間應該在 2 秒內
# 不應該出現 timeout 錯誤
```

## 📞 如果仍有問題

請提供：
1. Zeabur 控制台的環境變數截圖
2. MariaDB 服務的日誌
3. 應用服務的日誌
4. API 測試的完整回應