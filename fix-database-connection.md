# 資料庫連接超時問題修復

## 問題描述

從日誌可以看到資料庫連接超時錯誤：

```
MySqlConnector.MySqlException (0x80004005): Connect Timeout expired.
```

API 請求需要 15 秒才回應，這表示資料庫連接有問題。

## 問題原因

1. **連接超時設定太短**: 當前設定 `ConnectionTimeout=30` 可能不足
2. **資料庫服務未就緒**: MariaDB 服務可能還在啟動中
3. **網路連接問題**: 應用無法連接到 MariaDB 服務
4. **環境變數配置錯誤**: Zeabur 的資料庫連接變數可能未正確設定

## 修復方案

### 1. 優化連接字串

修改 `zeabur.json` 中的連接字串，增加超時時間和重試機制：

```json
{
  "ConnectionStrings__BackEndDatabase": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;",
  "ConnectionStrings__DefaultConnection": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;"
}
```

### 2. 修正 zeabur.json 配置

當前的 `zeabur.json` 有一些問題需要修正：

```json
{
  "name": "rosca-system",
  "services": {
    "app": {
      "build": {
        "dockerfile": "Dockerfile"
      },
      "name": "rosca-app",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "ConnectionStrings__BackEndDatabase": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;",
        "ConnectionStrings__DefaultConnection": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;",
        "JWT__SecretKey": "${JWT_SECRET_KEY}",
        "JWT__Issuer": "${JWT_ISSUER}",
        "JWT__Audience": "${JWT_AUDIENCE}",
        "JWT__ExpiryMinutes": "${JWT_EXPIRY_MINUTES}",
        "CORS__AllowedOrigins": "${CORS_ALLOWED_ORIGINS}",
        "FileUpload__MaxFileSize": "${FILE_UPLOAD_MAX_SIZE}",
        "FileUpload__AllowedExtensions": "${FILE_UPLOAD_EXTENSIONS}",
        "Hangfire__DashboardEnabled": "${HANGFIRE_DASHBOARD_ENABLED}",
        "Serilog__MinimumLevel__Default": "${LOG_LEVEL}",
        "TZ": "Asia/Taipei"
      },
      "depends_on": ["mariadb"]
    },
    "mariadb": {
      "template": "mariadb:11.3.2",
      "name": "rosca-mariadb",
      "env": {
        "MYSQL_ROOT_PASSWORD": "${DB_ROOT_PASSWORD}",
        "MYSQL_DATABASE": "${DB_NAME}",
        "MYSQL_USER": "${DB_USER}",
        "MYSQL_PASSWORD": "${DB_PASSWORD}",
        "MYSQL_CHARACTER_SET_SERVER": "utf8mb4",
        "MYSQL_COLLATION_SERVER": "utf8mb4_general_ci",
        "TZ": "Asia/Taipei"
      }
    }
  }
}
```

### 3. 環境變數檢查清單

確保在 Zeabur 控制台設定以下環境變數：

```bash
# 資料庫配置
DB_NAME=rosca_db
DB_USER=rosca_user
DB_PASSWORD=your_secure_password
DB_ROOT_PASSWORD=your_root_password

# JWT 配置
JWT_SECRET_KEY=your-super-secret-jwt-key-min-32-chars
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
```

### 4. 資料庫初始化檢查

確保資料庫初始化腳本存在：

- `database/zeabur/my.cnf`
- `database/zeabur/docker-entrypoint-initdb.d/01-schema.sql`
- `database/zeabur/docker-entrypoint-initdb.d/02-default-data.sql`
- `database/zeabur/docker-entrypoint-initdb.d/03-default-user.sql`

## 修復步驟

### 1. 更新 zeabur.json

```bash
# 更新配置檔案
git add zeabur.json
git commit -m "fix: 修復資料庫連接超時問題 - 增加連接超時和重試機制"
```

### 2. 檢查 Zeabur 環境變數

1. 登入 Zeabur 控制台
2. 進入 `rosca-system` 專案
3. 檢查 `app` 服務的環境變數設定
4. 確認 `mariadb` 服務的環境變數設定

### 3. 重新部署

1. 推送程式碼變更
2. 在 Zeabur 控制台重新部署服務
3. 等待 MariaDB 服務完全啟動
4. 再啟動應用服務

### 4. 驗證修復

```bash
# 測試資料庫連接
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'

# 檢查回應時間應該在 1-2 秒內
```

## 故障排除

### 1. 檢查服務狀態

在 Zeabur 控制台檢查：
- MariaDB 服務是否正常運行
- 應用服務是否能正常啟動
- 服務間的網路連接是否正常

### 2. 檢查日誌

```bash
# 檢查應用日誌
# 在 Zeabur 控制台查看 app 服務日誌

# 檢查資料庫日誌
# 在 Zeabur 控制台查看 mariadb 服務日誌
```

### 3. 測試資料庫連接

```bash
# 進入應用容器測試連接
docker exec -it <app_container> bash

# 測試資料庫連接
mysql -h ${ZEABUR_MARIADB_CONNECTION_HOST} -P ${ZEABUR_MARIADB_CONNECTION_PORT} -u ${DB_USER} -p${DB_PASSWORD} ${DB_NAME}
```

### 4. 常見問題

1. **服務啟動順序**: 確保 MariaDB 先於應用啟動
2. **網路連接**: 檢查服務間的網路配置
3. **環境變數**: 確認所有必要的環境變數都已設定
4. **資料庫初始化**: 確認資料庫 schema 已正確建立

## 預期結果

修復後：
- ✅ 資料庫連接正常，無超時錯誤
- ✅ API 回應時間在 1-2 秒內
- ✅ 登入功能正常運作
- ✅ 應用日誌無連接錯誤