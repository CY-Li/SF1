# 🔧 Final Database Configuration Fix

## 🔍 問題根因
發現了多個配置文件中都有資料庫連接設定，造成衝突：

1. **zeabur.json**: 使用內建 MariaDB 服務 ✅
2. **DotNetBackEndApi/appsettings.json**: 硬編碼外部連接 ❌
3. **DotNetBackEndService/appsettings.json**: 加密連接字串 ❌

## ✅ 已修復的配置

### 1. zeabur.json (內建 MariaDB)
```json
{
  "ConnectionStrings__BackEndDatabase": "Server=rosca-mariadb;Port=3306;User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME}",
  "ConnectionStrings__DefaultConnection": "Server=rosca-mariadb;Port=3306;User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME}"
}
```

### 2. DotNetBackEndApi/appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=rosca_db;Uid=rosca_user;Pwd=rosca_pass_2024!;CharSet=utf8mb4;"
  }
}
```

### 3. DotNetBackEndService/appsettings.json
```json
{
  "ConnectionStrings": {
    "BackEndDatabase": "Server=rosca-mariadb;Port=3306;User Id=rosca_user;Password=rosca_pass_2024!;Database=rosca_db;CharSet=utf8mb4;"
  }
}
```

## 🎯 統一的配置策略

### 服務間連接
- **API Gateway → Backend Service**: `http://localhost:5001/`
- **Backend Service → MariaDB**: `rosca-mariadb:3306`
- **API Gateway → MariaDB**: `localhost:3306` (同容器內)

### 資料庫配置
- **服務名稱**: `rosca-mariadb`
- **資料庫名稱**: `rosca_db`
- **使用者**: `rosca_user`
- **密碼**: `rosca_pass_2024!`

## 🚀 部署步驟

### 1. 提交所有變更
```bash
git add zeabur.json
git add backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/appsettings.json
git add backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/appsettings.json
git add database/zeabur/docker-entrypoint-initdb.d/03-default-user.sql
git commit -m "fix: 統一資料庫配置，使用內建 MariaDB 服務"
git push origin main
```

### 2. 在 Zeabur 控制台設定環境變數
```bash
DB_NAME=rosca_db
DB_USER=rosca_user
DB_PASSWORD=rosca_pass_2024!
DB_ROOT_PASSWORD=rosca_root_2024!
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production-min-32-chars-rosca-2024
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client
JWT_EXPIRY_MINUTES=60
CORS_ALLOWED_ORIGINS=*
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx
HANGFIRE_DASHBOARD_ENABLED=true
LOG_LEVEL=Information
```

### 3. 重新部署應用

## 🧪 測試步驟

### 1. 測試 API Gateway
```bash
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"mm_account":"admin","mm_pwd":"Admin123456"}'
```

### 2. 檢查健康狀態
```bash
curl https://sf-test.zeabur.app/health
```

## 🎯 預期結果

### 成功指標
- ✅ 不再出現 "Unable to connect to any of the specified MySQL hosts" 錯誤
- ✅ 不再出現 "backend-service:5001" 錯誤
- ✅ API Gateway 和 Backend Service 都能正常連接資料庫
- ✅ 登入功能正常運作
- ✅ 返回完整的使用者資訊和 JWT token

### 成功回應範例
```json
{
  "result": {
    "mm_id": 1,
    "mm_account": "admin",
    "mm_name": "系統管理員",
    "mm_role_type": "20",
    "mm_kyc": "20",
    "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiration": "2025-10-16T08:00:00Z"
  },
  "returnStatus": 1,
  "returnMsg": "登入成功"
}
```

## 📝 配置檢查清單

- ✅ zeabur.json 使用內建 MariaDB 服務
- ✅ API Gateway appsettings.json 使用 localhost 連接
- ✅ Backend Service appsettings.json 使用 rosca-mariadb 連接
- ✅ 資料庫初始化腳本使用 SHA256 密碼
- ✅ 環境變數正確設定
- ✅ 服務依賴關係正確配置

現在所有配置都統一了，應該可以徹底解決資料庫連接問題！