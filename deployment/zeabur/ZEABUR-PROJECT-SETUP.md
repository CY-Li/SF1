# Zeabur 專案建立指南

## 概述

本指南詳細說明如何在 Zeabur 控制台建立新專案並準備服務部署。

## 步驟 1：建立 Zeabur 專案

### 1.1 登入 Zeabur 控制台

1. 前往 [Zeabur 控制台](https://dash.zeabur.com)
2. 使用 GitHub/GitLab 帳戶登入
3. 確保您的 Git 存儲庫已推送到遠端

### 1.2 建立新專案

1. 在控制台首頁點擊 **「Create Project」**
2. 輸入專案資訊：
   - **Project Name**: `rosca-system`
   - **Description**: `ROSCA 平安商會系統 - 完整的會員管理和金融服務平台`
   - **Region**: 選擇最接近用戶的區域（建議：Asia Pacific）

3. 點擊 **「Create」** 建立專案

### 1.3 連接 Git 存儲庫

1. 在專案頁面點擊 **「Add Service」**
2. 選擇 **「Git Repository」**
3. 選擇您的 Git 提供商（GitHub/GitLab）
4. 授權 Zeabur 存取您的存儲庫
5. 選擇包含 ROSCA 系統的存儲庫
6. 選擇部署分支（通常是 `main` 或 `master`）

## 步驟 2：配置專案設定

### 2.1 環境變數配置

在專案設定中添加全域環境變數：

```env
# 基本系統配置
ASPNETCORE_ENVIRONMENT=Production

# 外部資料庫配置
DB_HOST=43.167.174.222
DB_PORT=31500
DB_NAME=zeabur
DB_USER=root
DB_PASSWORD=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G

# JWT 認證配置
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production-min-32-chars-rosca-2024
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client
JWT_EXPIRY_MINUTES=60

# CORS 跨域配置 (將在部署後更新為實際域名)
CORS_ALLOWED_ORIGINS=https://rosca-system.zeabur.app,https://admin-rosca-system.zeabur.app

# 檔案上傳配置
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx

# 功能開關
HANGFIRE_DASHBOARD_ENABLED=true

# 日誌配置
LOG_LEVEL=Information
```

### 2.2 專案結構確認

確保您的專案根目錄包含以下檔案：

```
├── zeabur.json                 # Zeabur 部署配置
├── .env.zeabur                 # 環境變數模板
├── ZEABUR-DEPLOYMENT.md        # 部署文檔
├── backendAPI/                 # .NET Core 後端服務
├── frontend/                   # Vue.js 前台系統
├── backend/FontEnd/            # Angular 後台系統
└── deployment/zeabur/          # 部署相關檔案
```

## 步驟 3：準備服務部署

### 3.1 驗證 Dockerfile 配置

確保以下 Dockerfile 存在且配置正確：

1. **Backend Service**: `backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile`
2. **API Gateway**: `backendAPI/DotNetBackEndCleanArchitecture/Dockerfile`
3. **Frontend**: `frontend/Dockerfile`
4. **Admin**: `backend/FontEnd/Dockerfile`

### 3.2 驗證 zeabur.json 配置

確保 `zeabur.json` 包含所有必要的服務配置：

- ✅ backend-service (Port: 5001)
- ✅ api-gateway (Port: 5000)
- ✅ frontend (Port: 80)
- ✅ admin (Port: 8080)

### 3.3 資料庫連接測試

在部署前測試外部資料庫連接：

```bash
# 使用 MySQL 客戶端測試連接
mysql -h 43.167.174.222 -P 31500 -u root -p zeabur
```

預期結果：成功連接到資料庫

## 步驟 4：部署準備檢查清單

在開始部署前，請確認以下項目：

### 4.1 程式碼準備
- [ ] 所有程式碼已提交到 Git 存儲庫
- [ ] 移除了本地資料庫依賴
- [ ] 更新了資料庫連接字串
- [ ] 所有 Dockerfile 已優化

### 4.2 配置檔案
- [ ] zeabur.json 配置完整
- [ ] 環境變數已設定
- [ ] nginx 配置已更新
- [ ] CORS 設定已配置

### 4.3 外部依賴
- [ ] 外部 MariaDB 服務可正常連接
- [ ] 資料庫 schema 已初始化
- [ ] 必要的資料表已建立

### 4.4 安全設定
- [ ] JWT 密鑰已設定（至少 32 字符）
- [ ] 資料庫密碼已確認
- [ ] CORS 來源已限制
- [ ] 檔案上傳限制已設定

## 下一步

完成專案建立後，您可以繼續執行：

1. **任務 4.2**: 部署後端服務
2. **任務 4.3**: 部署前端服務
3. **任務 5**: 配置和測試

## 故障排除

### 常見問題

**Q: Git 存儲庫連接失敗**
A: 確保存儲庫是公開的，或已正確授權 Zeabur 存取私有存儲庫

**Q: 環境變數設定錯誤**
A: 檢查變數名稱是否正確，值是否包含特殊字符需要轉義

**Q: zeabur.json 配置錯誤**
A: 驗證 JSON 格式是否正確，路徑是否存在

### 支援資源

- [Zeabur 官方文檔](https://zeabur.com/docs)
- [Zeabur Discord 社群](https://discord.gg/zeabur)
- 專案 README.md 檔案