# 如何檢查 Zeabur 服務日誌

## 🔍 檢查服務日誌步驟

### 1. 登入 Zeabur Dashboard
- 訪問: https://dash.zeabur.com
- 登入您的帳號

### 2. 選擇專案和服務
- 選擇您的專案 (rosca-system 或類似名稱)
- 點擊主應用程式服務 (通常名為 `app`)

### 3. 查看日誌
- 點擊 **"Logs"** 標籤
- 查看即時日誌輸出

## 🚨 常見的 502 錯誤日誌模式

### .NET 應用程式啟動失敗
```
fail: Microsoft.AspNetCore.Server.Kestrel[13]
      Unable to start Kestrel.
System.IO.IOException: Failed to bind to address
```

### 資料庫連接失敗
```
fail: Microsoft.EntityFrameworkCore.Database.Connection[20004]
      An error occurred using the connection to database
```

### 端口配置問題
```
crit: Microsoft.AspNetCore.Hosting.Diagnostics[6]
      Application startup exception
System.InvalidOperationException: Unable to configure HTTPS endpoint
```

### 環境變數缺失
```
System.ArgumentNullException: Value cannot be null. (Parameter 'connectionString')
```

## 🔧 根據日誌內容的修復建議

### 如果看到資料庫連接錯誤
檢查環境變數：
- `DB_HOST` (應該指向 MariaDB 服務)
- `DB_USER`
- `DB_PASSWORD`
- `DB_NAME`

### 如果看到端口綁定錯誤
檢查：
- `ASPNETCORE_URLS` 環境變數
- 應該設為 `http://+:5000` 和 `http://+:5001`

### 如果看到 JWT 配置錯誤
檢查：
- `JWT_SECRET_KEY` (至少 32 字符)
- `JWT_ISSUER`
- `JWT_AUDIENCE`

## 📋 環境變數檢查清單

在 Zeabur 服務設定中確認以下環境變數：

```bash
# 基本配置
ASPNETCORE_ENVIRONMENT=Production
TZ=Asia/Taipei

# 資料庫配置
DB_HOST=<mariadb-service-internal-url>
DB_USER=rosca_user
DB_PASSWORD=<your-password>
DB_NAME=rosca_db

# JWT 配置
JWT_SECRET_KEY=<32-character-secret>
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client

# CORS 配置
CORS_ALLOWED_ORIGINS=https://sf-test.zeabur.app
```

## 🛠️ 常見修復步驟

### 1. 重啟服務
在 Zeabur Dashboard 中：
- 停止服務
- 等待完全停止
- 重新啟動

### 2. 檢查服務依賴
確認 MariaDB 服務：
- 正在運行
- 可以連接
- 資料庫已初始化

### 3. 驗證環境變數
- 所有必要的環境變數都已設定
- 沒有拼寫錯誤
- 值格式正確

## 📞 獲取幫助

如果需要進一步協助，請提供：
1. Zeabur 服務日誌的完整輸出
2. 環境變數配置 (隱藏敏感資訊)
3. 服務啟動時間和錯誤時間

## 🔗 有用的連結

- [Zeabur 文檔](https://zeabur.com/docs)
- [ASP.NET Core 部署指南](https://docs.microsoft.com/aspnet/core/host-and-deploy/)
- [Docker 容器日誌](https://docs.docker.com/config/containers/logging/)