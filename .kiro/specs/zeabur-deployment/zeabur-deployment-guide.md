# Zeabur 部署指南

## 重要說明

**Zeabur 會自動偵測 Dockerfile 進行部署，不需要 zeabur.json 配置檔案。**

## 部署步驟

### 1. 準備工作

專案使用根目錄的統一 Dockerfile 進行部署：
- 根目錄 `Dockerfile` - 整合所有服務的統一容器
- 包含 .NET Core API Gateway (Port 5000)
- 包含 .NET Core Backend Service (Port 5001)  
- 包含 Vue.js 前台系統 (Port 80)
- 包含 Angular 後台系統 (Port 8080)
- 使用 Supervisor 管理多個服務
- 使用 Nginx 作為反向代理

### 2. 在 Zeabur 控制台部署

1. 登入 Zeabur 控制台
2. 建立新專案 `rosca-system`
3. 連接 Git 儲存庫
4. Zeabur 會自動偵測根目錄的 Dockerfile
5. 建立單一服務包含所有功能
6. 設定環境變數和存儲卷

### 3. 環境變數配置

統一服務的環境變數配置：

```env
# .NET Core 應用程式配置
ASPNETCORE_ENVIRONMENT=Production
TZ=Asia/Taipei

# 資料庫連接配置
ConnectionStrings__BackEndDatabase=Server=43.167.174.222;Port=31500;User Id=root;Password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G;Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;
ConnectionStrings__DefaultConnection=Server=43.167.174.222;Port=31500;User Id=root;Password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G;Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;

# JWT 認證配置
JWT__SecretKey=your-super-secret-jwt-key-change-in-production-min-32-chars
JWT__Issuer=ROSCA-API
JWT__Audience=ROSCA-Client
JWT__ExpiryMinutes=60

# CORS 跨域配置
CORS__AllowedOrigins=https://your-domain.zeabur.app

# 檔案上傳配置
FileUpload__MaxFileSize=10485760
FileUpload__AllowedExtensions=.jpg,.jpeg,.png,.gif,.pdf

# 其他配置
Hangfire__DashboardEnabled=true
Serilog__MinimumLevel__Default=Information

# 內部服務通信 (容器內部)
APIUrl=http://localhost:5001/
```

### 4. 持久化存儲配置

在 Zeabur 控制台中為統一服務添加以下存儲卷：
- `/app/uploads` - 5GB (通用檔案上傳)
- `/app/KycImages` - 2GB (KYC 身份認證圖片)
- `/app/DepositImages` - 2GB (存款憑證圖片)
- `/app/WithdrawImages` - 2GB (提款憑證圖片)
- `/app/AnnImagessss` - 2GB (公告圖片)
- `/app/logs` - 1GB (應用程式日誌)

### 5. 資源配置建議

統一服務資源配置：
- **CPU**: 1.0 vCPU (包含所有服務)
- **記憶體**: 1GB RAM (包含所有服務)
- **存儲**: 13GB 持久化存儲

### 6. 域名配置

1. 在 Zeabur 控制台為統一服務配置域名
2. 主域名 (Port 80) - 前台系統
3. 主域名:8080 (Port 8080) - 後台管理系統  
4. 啟用 HTTPS (Zeabur 自動提供 SSL 憑證)
5. 可選：配置自定義域名

### 7. 服務間通信

統一容器內的服務通信：
- API Gateway (Port 5000) ↔ Backend Service (Port 5001) 透過 localhost 通信
- Frontend (Port 80) 透過 Nginx 代理訪問 API Gateway
- Admin (Port 8080) 透過 Nginx 代理訪問 API Gateway
- 所有服務在同一容器內，使用 Supervisor 管理

## 注意事項

1. **統一 Dockerfile**：使用根目錄的 Dockerfile 部署所有服務
2. **不要使用 zeabur.json**：Zeabur 會自動偵測 Dockerfile
3. **環境變數**：敏感資訊請在 Zeabur 控制台中設定
4. **外部資料庫**：使用提供的 MariaDB 連接資訊 (43.167.174.222:31500)
5. **持久化存儲**：確保配置所有必要的存儲卷
6. **多端口訪問**：前台 (Port 80)、後台 (Port 8080)
7. **健康檢查**：容器會自動檢查所有服務狀態