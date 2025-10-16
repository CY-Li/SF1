# Zeabur 部署指南

## 概述

本指南將協助您將 ROSCA 系統部署到 Zeabur 雲端平台。Zeabur 是一個現代化的應用程式部署平台，支援 Docker 容器和多種程式語言框架。

## 系統架構

部署後的系統將包含以下服務：

- **MariaDB 資料庫** - 資料存儲
- **Backend Service** - .NET Core 微服務 (Port 5001)
- **API Gateway** - .NET Core API 閘道 (Port 5000)
- **Frontend** - Vue.js 前台系統 (Port 80)
- **Admin** - Angular 後台管理系統 (Port 8080)

## 部署前準備

### 1. 檢查檔案結構

確保您的專案包含以下檔案：

```
├── zeabur.json                    # Zeabur 配置檔案
├── .env.zeabur.example           # 環境變數範例
├── database/
│   └── init/
│       ├── 01-schema.sql         # 資料庫結構
│       ├── 02-default-data.sql   # 預設資料
│       └── 03-default-user.sql   # 預設用戶
├── backendAPI/
│   └── DotNetBackEndCleanArchitecture/
├── frontend/
└── backend/FontEnd/
```

### 2. 準備環境變數

複製 `.env.zeabur.example` 為 `.env.zeabur` 並填入實際值：

```bash
cp .env.zeabur.example .env.zeabur
```

編輯 `.env.zeabur` 檔案，設定以下重要變數：

```env
# 資料庫配置
DB_NAME=rosca_db
DB_USER=rosca_user
DB_PASSWORD=your_secure_password_here
DB_ROOT_PASSWORD=your_root_password_here

# JWT 配置
JWT_SECRET_KEY=your-32-char-secret-key-here
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client

# CORS 配置 (部署後更新)
CORS_ALLOWED_ORIGINS=https://your-app.zeabur.app,https://your-app-admin.zeabur.app
```

## 部署步驟

### 步驟 1：建立 Zeabur 專案

1. 登入 [Zeabur 控制台](https://dash.zeabur.com)
2. 點擊 "New Project" 建立新專案
3. 選擇 "Deploy from GitHub" 或上傳專案檔案
4. 選擇您的 Git 儲存庫

### 步驟 2：配置服務

Zeabur 會自動讀取 `zeabur.json` 配置檔案並建立以下服務：

1. **mariadb** - 資料庫服務
2. **backend-service** - .NET Core 微服務
3. **backend** - API Gateway
4. **frontend** - 前台系統
5. **admin** - 後台管理系統

### 步驟 3：設定環境變數

在 Zeabur 控制台中為每個服務設定環境變數：

1. 進入專案設定
2. 選擇 "Environment Variables"
3. 添加 `.env.zeabur` 中的所有變數

### 步驟 4：配置域名

1. 在 Zeabur 控制台中進入 "Domains" 設定
2. 為 frontend 服務設定主域名
3. 為 admin 服務設定子域名或路徑
4. 啟用 HTTPS (Zeabur 自動提供 SSL 憑證)

### 步驟 5：部署驗證

部署完成後，驗證以下功能：

1. **資料庫連接**
   ```bash
   curl https://your-app.zeabur.app/api/HomePicture/GetAnnImages
   ```

2. **前台系統**
   - 訪問 `https://your-app.zeabur.app`
   - 測試用戶註冊登入功能

3. **後台系統**
   - 訪問 `https://your-app-admin.zeabur.app`
   - 使用預設管理員帳戶登入

## 環境變數詳細說明

### 必要變數

| 變數名稱 | 說明 | 範例值 |
|---------|------|--------|
| `DB_NAME` | 資料庫名稱 | `rosca_db` |
| `DB_USER` | 資料庫用戶 | `rosca_user` |
| `DB_PASSWORD` | 資料庫密碼 | `secure_password` |
| `JWT_SECRET_KEY` | JWT 密鑰 (至少32字符) | `your-secret-key-here` |
| `CORS_ALLOWED_ORIGINS` | 允許的跨域來源 | `https://your-app.zeabur.app` |

### 可選變數

| 變數名稱 | 預設值 | 說明 |
|---------|--------|------|
| `JWT_EXPIRY_MINUTES` | `60` | JWT 過期時間 |
| `FILE_UPLOAD_MAX_SIZE` | `10485760` | 檔案上傳大小限制 |
| `LOG_LEVEL` | `Information` | 日誌等級 |
| `HANGFIRE_DASHBOARD_ENABLED` | `true` | 是否啟用 Hangfire 儀表板 |

## 服務間通信

在 Zeabur 環境中，服務間通信使用以下方式：

1. **前台 → API Gateway**
   ```
   https://your-app.zeabur.app/api/* → API Gateway
   ```

2. **後台 → API Gateway**
   ```
   https://your-app-admin.zeabur.app/api/* → API Gateway
   ```

3. **API Gateway → Backend Service**
   ```
   內部網路通信，使用 Zeabur 服務發現
   ```

## 檔案存儲

系統使用 Zeabur 的持久化存儲卷：

- `/app/uploads` - 一般上傳檔案
- `/app/KycImages` - KYC 驗證圖片
- `/app/DepositImages` - 存款憑證圖片
- `/app/WithdrawImages` - 提款憑證圖片
- `/app/AnnImagessss` - 公告圖片

## 監控和日誌

### 查看日誌

在 Zeabur 控制台中：

1. 進入專案
2. 選擇服務
3. 點擊 "Logs" 標籤查看即時日誌

### 健康檢查

系統提供以下健康檢查端點：

- API Gateway: `GET /api/HomePicture/GetAnnImages`
- Frontend: `GET /`
- Admin: `GET /`

## 故障排除

### 常見問題

1. **資料庫連接失敗**
   - 檢查資料庫服務是否正常運行
   - 驗證連接字串中的主機名和端口
   - 確認資料庫用戶權限

2. **API 調用 404 錯誤**
   - 檢查 nginx 代理配置
   - 驗證服務發現變數
   - 確認 API Gateway 服務狀態

3. **CORS 錯誤**
   - 更新 `CORS_ALLOWED_ORIGINS` 環境變數
   - 確保包含正確的域名
   - 重新部署 API Gateway 服務

4. **檔案上傳失敗**
   - 檢查存儲卷掛載
   - 驗證檔案權限設定
   - 確認檔案大小限制

### 日誌分析

查看服務日誌以診斷問題：

```bash
# 在 Zeabur 控制台中查看各服務日誌
# 或使用 Zeabur CLI (如果可用)
zeabur logs <service-name>
```

## 備份和恢復

### 資料庫備份

Zeabur 提供自動備份功能：

1. 在資料庫服務設定中啟用自動備份
2. 設定備份頻率和保留期限
3. 定期驗證備份完整性

### 檔案備份

建議定期備份上傳的檔案：

1. 使用 Zeabur 的存儲快照功能
2. 或設定定期同步到外部存儲服務

## 性能優化

### 資源配置

根據使用情況調整服務資源：

- **資料庫**: 512MB - 1GB 記憶體
- **API 服務**: 512MB 記憶體, 0.5 vCPU
- **前端服務**: 256MB 記憶體, 0.25 vCPU

### 快取策略

1. 啟用 nginx 靜態檔案快取
2. 配置適當的 HTTP 快取標頭
3. 考慮使用 CDN 加速靜態資源

## 安全考量

1. **HTTPS 強制**: Zeabur 自動提供 SSL 憑證
2. **環境變數安全**: 敏感資訊使用環境變數存儲
3. **CORS 限制**: 只允許信任的域名跨域訪問
4. **定期更新**: 定期更新 JWT 密鑰和資料庫密碼

## 支援和聯繫

如果在部署過程中遇到問題：

1. 查看 [Zeabur 官方文檔](https://zeabur.com/docs)
2. 檢查本專案的 `TROUBLESHOOTING.md` 檔案
3. 在專案 GitHub 儲存庫中建立 Issue

---

**注意**: 請確保在生產環境中使用強密碼和安全的配置設定。定期檢查和更新系統以確保安全性。