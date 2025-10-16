# 🚀 Zeabur 內建 MariaDB 部署指南

## 🎯 方案 C: 使用內建 MariaDB 服務

已成功配置使用 Zeabur 內建 MariaDB 服務，避免外部連接問題。

## ✅ 已完成的配置

### 1. zeabur.json 配置
- ✅ 添加 MariaDB 服務定義
- ✅ 配置應用依賴 MariaDB
- ✅ 使用環境變數管理連接字串
- ✅ 配置資料庫初始化腳本

### 2. 資料庫初始化
- ✅ 更新為使用 SHA256 密碼格式
- ✅ 配置測試帳號
- ✅ 使用環境變數 `${DB_NAME}`

### 3. 連接字串
```
Server=rosca-mariadb;Port=3306;User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME}
```

## 🔧 環境變數配置

在 Zeabur 控制台設定以下環境變數：

```bash
# 資料庫配置
DB_NAME=rosca_db
DB_USER=rosca_user
DB_PASSWORD=rosca_pass_2024!
DB_ROOT_PASSWORD=rosca_root_2024!

# JWT 配置
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production-min-32-chars-rosca-2024
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client
JWT_EXPIRY_MINUTES=60

# CORS 配置
CORS_ALLOWED_ORIGINS=*

# 檔案上傳配置
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx

# 功能開關
HANGFIRE_DASHBOARD_ENABLED=true

# 日誌配置
LOG_LEVEL=Information
```

## 🚀 部署步驟

### 1. 提交變更
```bash
git add zeabur.json database/zeabur/docker-entrypoint-initdb.d/03-default-user.sql
git commit -m "feat: 使用 Zeabur 內建 MariaDB 服務"
git push origin main
```

### 2. 在 Zeabur 控制台部署
1. 進入專案頁面
2. 重新部署應用服務
3. 等待 MariaDB 和應用服務啟動

### 3. 設定環境變數
在 Zeabur 控制台為應用服務設定上述環境變數

## 🧪 測試步驟

### 1. 檢查服務狀態
確認 MariaDB 和應用服務都正常運行

### 2. 測試資料庫連接
```bash
# 測試登入 API
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"mm_account":"admin","mm_pwd":"Admin123456"}'
```

### 3. 測試使用者帳號
```bash
# 測試使用者登入
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"mm_account":"0938766349","mm_pwd":"Admin123456"}'
```

## 🎯 預期結果

### 成功指標
- ✅ MariaDB 服務正常啟動
- ✅ 應用服務成功連接到 MariaDB
- ✅ 資料庫初始化腳本執行成功
- ✅ 測試帳號可以正常登入
- ✅ 返回 JWT token 和使用者資訊

### 成功回應範例
```json
{
  "result": {
    "mm_id": 1,
    "mm_account": "admin",
    "mm_name": "系統管理員",
    "mm_role_type": "20",
    "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiration": "2025-10-16T08:00:00Z"
  },
  "returnStatus": 1,
  "returnMsg": "登入成功"
}
```

## 🔍 故障排除

### 如果 MariaDB 啟動失敗
1. 檢查環境變數設定
2. 確認初始化腳本語法正確
3. 檢查資源配置是否足夠

### 如果應用連接失敗
1. 確認 `depends_on` 配置正確
2. 檢查連接字串格式
3. 驗證環境變數值

### 如果登入失敗
1. 確認資料庫初始化成功
2. 檢查密碼 SHA256 雜湊正確
3. 驗證表格結構完整

## 📝 優點總結

- ✅ **完全控制**：資料庫配置和初始化
- ✅ **避免網路問題**：內部服務通信
- ✅ **自動初始化**：測試資料和帳號
- ✅ **環境隔離**：每個部署獨立的資料庫
- ✅ **易於維護**：統一的配置管理

現在可以開始部署了！