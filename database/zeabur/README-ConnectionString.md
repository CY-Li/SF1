# Zeabur 資料庫連接字串配置指南

## 概述

本文檔說明如何在 Zeabur 環境中配置 .NET Core 應用程式的 MariaDB 資料庫連接字串。

## 環境變數

### Zeabur 服務發現變數
Zeabur 會自動提供以下環境變數：
- `ZEABUR_MARIADB_CONNECTION_HOST` - MariaDB 服務主機
- `ZEABUR_MARIADB_CONNECTION_PORT` - MariaDB 服務端口

### 應用程式環境變數
需要手動設定的環境變數：
- `DB_NAME` - 資料庫名稱 (預設: rosca_db)
- `DB_USER` - 資料庫用戶 (預設: rosca_user)
- `DB_PASSWORD` - 資料庫密碼 (必須設定)

## 連接字串格式

### 標準格式
```
Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;
```

### 參數說明
- `CharSet=utf8mb4` - 支援完整的 UTF-8 字符集
- `AllowUserVariables=True` - 允許用戶變數
- `UseAffectedRows=False` - 使用匹配行數而非受影響行數
- `ConnectionTimeout=30` - 連接超時 30 秒
- `CommandTimeout=60` - 命令超時 60 秒
- `Pooling=true` - 啟用連接池
- `MinimumPoolSize=5` - 最小連接池大小
- `MaximumPoolSize=100` - 最大連接池大小
- `ConnectionLifeTime=300` - 連接生命週期 300 秒

## 配置檔案

### API Gateway (appsettings.zeabur.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;"
  }
}
```

### Backend Service (appsettings.zeabur.json)
```json
{
  "ConnectionStrings": {
    "BackEndDatabase": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;"
  }
}
```

## .NET Core 程式碼配置

### 使用 ZeaburDatabaseConfiguration 類別
```csharp
// 在 Program.cs 或 Startup.cs 中
services.AddZeaburDatabase<YourDbContext>(configuration);
services.AddZeaburDatabaseHealthCheck(configuration);
```

### 手動配置 Entity Framework Core
```csharp
services.AddDbContext<YourDbContext>(options =>
{
    var connectionString = ZeaburDatabaseConfiguration.GetZeaburConnectionString(configuration);
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
    {
        mysqlOptions.CharSet(CharSet.Utf8Mb4);
        mysqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        mysqlOptions.CommandTimeout(60);
    });
});
```

## 測試和驗證

### 使用測試腳本
```bash
# 執行連接測試
./database/zeabur/test-connection.sh

# 執行 SQL 驗證
mysql -h$DB_HOST -P$DB_PORT -u$DB_USER -p$DB_PASSWORD $DB_NAME < database/zeabur/verify-connection-string.sql
```

### 使用 C# 測試程式
```bash
# 編譯並執行測試程式
dotnet run --project database/zeabur/test-connection-string.cs
```

## 健康檢查

### 配置健康檢查端點
```csharp
// 在 Program.cs 中
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 健康檢查 URL
- API Gateway: `http://your-api-gateway-domain/health`
- Backend Service: `http://your-backend-service-domain/health`

## 故障排除

### 常見問題

1. **連接超時**
   - 檢查 `ZEABUR_MARIADB_CONNECTION_HOST` 和 `ZEABUR_MARIADB_CONNECTION_PORT` 環境變數
   - 確認 MariaDB 服務已啟動

2. **認證失敗**
   - 檢查 `DB_USER` 和 `DB_PASSWORD` 環境變數
   - 確認用戶權限設定正確

3. **字符集問題**
   - 確認連接字串包含 `CharSet=utf8mb4`
   - 檢查資料庫字符集設定

4. **連接池問題**
   - 調整 `MinimumPoolSize` 和 `MaximumPoolSize` 參數
   - 監控連接池使用情況

### 日誌檢查
```csharp
// 啟用 Entity Framework Core 日誌
options.LogTo(Console.WriteLine, LogLevel.Information);
options.EnableSensitiveDataLogging(); // 僅在開發環境
```

## 最佳實踐

1. **安全性**
   - 使用環境變數存儲敏感資訊
   - 不要在程式碼中硬編碼密碼
   - 定期更換資料庫密碼

2. **性能**
   - 適當設定連接池大小
   - 使用連接超時和命令超時
   - 監控資料庫連接使用情況

3. **可靠性**
   - 啟用重試機制
   - 配置健康檢查
   - 實施適當的錯誤處理

4. **監控**
   - 記錄資料庫操作日誌
   - 監控連接池狀態
   - 設定告警機制

## 相關檔案

- `appsettings.zeabur.json` - Zeabur 環境配置
- `ZeaburDatabaseConfiguration.cs` - 資料庫配置類別
- `test-connection.sh` - 連接測試腳本
- `verify-connection-string.sql` - SQL 驗證腳本
- `connection-strings.json` - 連接字串範本