# Zeabur 後端服務部署指南

## 概述

本指南說明如何在 Zeabur 平台部署 ROSCA 系統的後端服務，包括 Backend Service (.NET Core 微服務) 和 API Gateway (.NET Core)。

## 服務架構

```
┌─────────────────┐    ┌─────────────────┐
│   API Gateway   │    │ Backend Service │
│   (.NET Core)   │    │   (.NET Core)   │
│   Port: 5000    │────│   Port: 5001    │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └──────────┬───────────┘
                     │
          ┌─────────────────┐
          │   MariaDB       │
          │   (External)    │
          │ 43.167.174.222  │
          │   Port: 31500   │
          └─────────────────┘
```

## 部署步驟

### 步驟 1：準備後端服務部署

#### 1.1 驗證 Dockerfile 配置

確認以下 Dockerfile 已優化：

**Backend Service Dockerfile:**
- 路徑: `backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile`
- 端口: 5001
- 健康檢查: `/health`

**API Gateway Dockerfile:**
- 路徑: `backendAPI/DotNetBackEndCleanArchitecture/Dockerfile`
- 端口: 5000
- 健康檢查: `/api/HomePicture/GetAnnImages`

#### 1.2 確認 zeabur.json 配置

```json
{
  "services": {
    "backend-service": {
      "build": {
        "dockerfile": "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile"
      },
      "name": "backend-service",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "ASPNETCORE_URLS": "http://+:5001",
        "ConnectionStrings__BackEndDatabase": "Server=${DB_HOST};Port=${DB_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;",
        "JWT__SecretKey": "${JWT_SECRET_KEY}",
        "JWT__Issuer": "${JWT_ISSUER}",
        "JWT__Audience": "${JWT_AUDIENCE}",
        "JWT__ExpiryMinutes": "${JWT_EXPIRY_MINUTES}",
        "Serilog__MinimumLevel__Default": "${LOG_LEVEL}",
        "TZ": "Asia/Taipei"
      },
      "resources": {
        "memory": "512Mi",
        "cpu": "0.5"
      }
    },
    "api-gateway": {
      "build": {
        "dockerfile": "backendAPI/DotNetBackEndCleanArchitecture/Dockerfile"
      },
      "name": "api-gateway",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "ASPNETCORE_URLS": "http://+:5000",
        "ConnectionStrings__BackEndDatabase": "Server=${DB_HOST};Port=${DB_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;",
        "ConnectionStrings__DefaultConnection": "Server=${DB_HOST};Port=${DB_PORT};User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;",
        "APIUrl": "http://backend-service:5001/",
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
      "volumes": [
        {
          "name": "uploads",
          "dir": "/app/uploads",
          "size": "5GB"
        },
        {
          "name": "kyc-images",
          "dir": "/app/KycImages",
          "size": "2GB"
        },
        {
          "name": "deposit-images",
          "dir": "/app/DepositImages",
          "size": "2GB"
        },
        {
          "name": "withdraw-images",
          "dir": "/app/WithdrawImages",
          "size": "2GB"
        },
        {
          "name": "ann-images",
          "dir": "/app/AnnImagessss",
          "size": "2GB"
        }
      ],
      "resources": {
        "memory": "512Mi",
        "cpu": "0.5"
      }
    }
  }
}
```

### 步驟 2：配置外部資料庫連接

#### 2.1 資料庫連接字串

確保環境變數正確設定：

```env
# 外部 MariaDB 配置
DB_HOST=43.167.174.222
DB_PORT=31500
DB_NAME=zeabur
DB_USER=root
DB_PASSWORD=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G
```

#### 2.2 連接字串格式

系統將自動組合連接字串：
```
Server=43.167.174.222;Port=31500;User Id=root;Password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G;Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;
```

### 步驟 3：在 Zeabur 控制台部署

#### 3.1 部署 Backend Service

1. 在 Zeabur 專案中，系統會自動偵測 `backend-service` 配置
2. 點擊 **Deploy** 開始建置
3. 監控建置日誌，確保無錯誤
4. 等待服務狀態變為 **Running**

**預期建置時間:** 3-5 分鐘

#### 3.2 部署 API Gateway

1. 系統會自動偵測 `api-gateway` 配置
2. 點擊 **Deploy** 開始建置
3. 監控建置日誌，確保無錯誤
4. 等待服務狀態變為 **Running**

**預期建置時間:** 3-5 分鐘

### 步驟 4：驗證部署

#### 4.1 檢查服務狀態

在 Zeabur 控制台確認：

- ✅ **backend-service**: Status = Running, Port = 5001
- ✅ **api-gateway**: Status = Running, Port = 5000

#### 4.2 測試健康檢查

**Backend Service:**
```bash
curl -f http://backend-service:5001/health
# 預期回應: 200 OK
```

**API Gateway:**
```bash
curl -f http://api-gateway:5000/api/HomePicture/GetAnnImages
# 預期回應: 200 OK (可能是空陣列)
```

#### 4.3 檢查資料庫連接

查看服務日誌，確認：
- 資料庫連接成功
- Entity Framework 初始化正常
- 無連接錯誤

### 步驟 5：配置服務間通信

#### 5.1 內部網路通信

Zeabur 自動配置內部網路：

- **API Gateway → Backend Service**: `http://backend-service:5001/`
- **服務發現**: 自動透過服務名稱解析

#### 5.2 驗證服務間通信

在 API Gateway 日誌中確認：
```
[INFO] Successfully connected to Backend Service at http://backend-service:5001/
```

## 環境變數詳細說明

### Backend Service 環境變數

| 變數名稱 | 說明 | 範例值 |
|---------|------|--------|
| `ASPNETCORE_ENVIRONMENT` | 執行環境 | `Production` |
| `ASPNETCORE_URLS` | 監聽 URL | `http://+:5001` |
| `ConnectionStrings__BackEndDatabase` | 資料庫連接字串 | 自動組合 |
| `JWT__SecretKey` | JWT 密鑰 | 至少 32 字符 |
| `JWT__Issuer` | JWT 發行者 | `ROSCA-API` |
| `JWT__Audience` | JWT 受眾 | `ROSCA-Client` |
| `JWT__ExpiryMinutes` | JWT 過期時間 | `60` |
| `Serilog__MinimumLevel__Default` | 日誌等級 | `Information` |
| `TZ` | 時區 | `Asia/Taipei` |

### API Gateway 環境變數

除了 Backend Service 的變數外，還包括：

| 變數名稱 | 說明 | 範例值 |
|---------|------|--------|
| `APIUrl` | Backend Service URL | `http://backend-service:5001/` |
| `CORS__AllowedOrigins` | CORS 允許來源 | 前端域名列表 |
| `FileUpload__MaxFileSize` | 檔案上傳大小限制 | `10485760` (10MB) |
| `FileUpload__AllowedExtensions` | 允許的檔案副檔名 | `.jpg,.png,.pdf` |
| `Hangfire__DashboardEnabled` | 啟用 Hangfire 儀表板 | `true` |

## 持久化存儲配置

API Gateway 配置了以下存儲卷：

| 卷名稱 | 掛載路徑 | 大小 | 用途 |
|--------|----------|------|------|
| `uploads` | `/app/uploads` | 5GB | 一般檔案上傳 |
| `kyc-images` | `/app/KycImages` | 2GB | KYC 身份驗證圖片 |
| `deposit-images` | `/app/DepositImages` | 2GB | 存款憑證圖片 |
| `withdraw-images` | `/app/WithdrawImages` | 2GB | 提款憑證圖片 |
| `ann-images` | `/app/AnnImagessss` | 2GB | 公告圖片 |

## 監控和日誌

### 日誌查看

在 Zeabur 控制台：
1. 選擇對應服務
2. 點擊 **Logs** 標籤
3. 查看實時日誌

### 關鍵日誌指標

**成功啟動日誌:**
```
[INFO] Application started. Press Ctrl+C to shut down.
[INFO] Hosting environment: Production
[INFO] Content root path: /app
[INFO] Database connection established successfully
```

**錯誤日誌範例:**
```
[ERROR] Unable to connect to database: Connection timeout
[ERROR] JWT configuration missing or invalid
[WARN] CORS origin not configured properly
```

## 故障排除

### 常見問題

#### 1. 資料庫連接失敗

**症狀:** 服務啟動失敗，日誌顯示資料庫連接錯誤

**解決方案:**
1. 檢查環境變數 `DB_HOST`, `DB_PORT`, `DB_USER`, `DB_PASSWORD`
2. 確認外部資料庫服務狀態
3. 測試網路連通性

#### 2. 服務間通信失敗

**症狀:** API Gateway 無法連接到 Backend Service

**解決方案:**
1. 確認 `APIUrl` 環境變數設定為 `http://backend-service:5001/`
2. 檢查 Backend Service 是否正常運行
3. 查看網路配置

#### 3. 檔案上傳失敗

**症狀:** 檔案上傳功能不工作

**解決方案:**
1. 檢查存儲卷是否正確掛載
2. 確認檔案權限設定
3. 檢查檔案大小和類型限制

#### 4. JWT 認證失敗

**症狀:** 用戶無法登入或 token 驗證失敗

**解決方案:**
1. 確認 `JWT__SecretKey` 至少 32 字符
2. 檢查 JWT 配置的一致性
3. 驗證時間同步

### 效能調優

#### 資源配置建議

**生產環境:**
- Backend Service: 1 vCPU, 1GB RAM
- API Gateway: 1 vCPU, 1GB RAM

**開發/測試環境:**
- Backend Service: 0.5 vCPU, 512MB RAM
- API Gateway: 0.5 vCPU, 512MB RAM

#### 資料庫連接池

在 `appsettings.json` 中優化：
```json
{
  "ConnectionStrings": {
    "BackEndDatabase": "...;Pooling=true;MinPoolSize=5;MaxPoolSize=100;ConnectionTimeout=30;"
  }
}
```

## 安全考量

### 網路安全

1. **內部通信:** 服務間使用內部網路，不暴露到外部
2. **HTTPS 強制:** 所有外部 API 強制使用 HTTPS
3. **CORS 限制:** 嚴格限制跨域請求來源

### 資料安全

1. **敏感資料加密:** JWT 密鑰、資料庫密碼等加密存儲
2. **最小權限原則:** 資料庫用戶僅授予必要權限
3. **日誌安全:** 避免在日誌中記錄敏感資訊

## 下一步

完成後端服務部署後，繼續執行：
- **任務 4.3**: 部署前端服務
- **任務 5**: 配置和測試