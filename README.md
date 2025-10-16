# ROSCA 平安商會系統

一個完整的互助會管理系統，支援會員管理、標會運作、財務管理等功能。

## 🚀 快速部署

### Zeabur 一鍵部署 (推薦)

[![Deploy on Zeabur](https://zeabur.com/button.svg)](https://zeabur.com/templates)

1. 點擊上方按鈕或訪問 [Zeabur Dashboard](https://dash.zeabur.com)
2. 建立新專案並連接此 GitHub 儲存庫
3. 按照 [ZEABUR-DEPLOY.md](./ZEABUR-DEPLOY.md) 指南完成部署

### 本地 Docker 部署

```bash
# 克隆專案
git clone <your-repo-url>
cd rosca-system

# 啟動系統
chmod +x docker-start.sh
./docker-start.sh start
```

詳細說明請參考 [README-Docker.md](./README-Docker.md)

## 📋 系統功能

### 核心功能
- 👥 **會員管理** - 會員註冊、資料管理、KYC 認證
- 💰 **標會管理** - 標會建立、投標、得標、繳費管理
- 📊 **財務管理** - 收支記錄、帳務對帳、財務報表
- 🔐 **權限管理** - 角色權限、操作日誌、安全控制
- 📱 **多端支援** - 響應式設計、手機 APP 支援

### 技術特色
- 🏗️ **微服務架構** - .NET Core + Vue.js + Angular
- 🗄️ **資料庫** - MariaDB 11.3.2 with UTF8MB4
- 📁 **檔案管理** - 完整的檔案上傳、存取、快取系統
- 🔒 **安全機制** - JWT 認證、CORS 配置、檔案驗證
- 📈 **監控日誌** - 結構化日誌、健康檢查、效能監控

## 🏗️ 系統架構

```
┌─────────────────┐    ┌─────────────────┐
│   前台系統      │    │   後台系統      │
│  (Vue.js)       │    │  (Angular)      │
│  Port: 8080     │    │  Port: 4200     │
└─────────────────┘    └─────────────────┘
         │                       │
         └───────────┬───────────┘
                     │
         ┌─────────────────┐
         │  API Gateway    │
         │  (.NET Core)    │
         │  Port: 5000     │
         └─────────────────┘
                     │
         ┌─────────────────┐
         │ Backend Service │
         │  (.NET Core)    │
         │  Port: 5001     │
         └─────────────────┘
                     │
         ┌─────────────────┐
         │    MariaDB      │
         │   11.3.2        │
         │  Port: 3306     │
         └─────────────────┘
```

## 📁 專案結構

```
rosca-system/
├── backendAPI/DotNetBackEndCleanArchitecture/  # .NET Core 後端
│   ├── DotNetBackEndApi/                       # API Gateway
│   └── Presentation/DotNetBackEndService/      # 微服務
├── frontend/                                   # Vue.js 前台
├── backend/FontEnd/                           # Angular 後台
├── database/zeabur/                           # 資料庫配置
├── storage/zeabur/                            # 存儲管理
├── Dockerfile                                 # 容器建置檔案
├── zeabur.json                               # Zeabur 部署配置
├── docker-compose.yml                        # Docker Compose 配置
└── README.md                                 # 本檔案
```

## 🔧 開發環境設定

### 必要工具
- .NET 8.0 SDK
- Node.js 18+
- Docker & Docker Compose
- MariaDB 11.3.2

### 本地開發

1. **後端開發**
   ```bash
   cd backendAPI/DotNetBackEndCleanArchitecture
   dotnet restore
   dotnet run --project DotNetBackEndApi
   ```

2. **前端開發**
   ```bash
   cd frontend
   # 使用 Live Server 或其他靜態伺服器
   ```

3. **後台開發**
   ```bash
   cd backend/FontEnd
   npm install
   ng serve
   ```

## 📚 文檔

- [Zeabur 部署指南](./ZEABUR-DEPLOY.md) - 一鍵部署到 Zeabur 平台
- [Docker 部署指南](./README-Docker.md) - 本地 Docker 部署
- [部署檢查清單](./DEPLOYMENT-CHECKLIST.md) - 完整的部署驗證清單
- [檔案上傳系統](./backendAPI/DotNetBackEndCleanArchitecture/README-FileUpload.md) - 檔案管理功能說明

## 🌐 存取方式

### 生產環境 (Zeabur)
- **前台**: https://your-app.zeabur.app/
- **後台**: https://your-app.zeabur.app/admin/
- **API**: https://your-app.zeabur.app/api/
- **健康檢查**: https://your-app.zeabur.app/health

### 本地環境 (Docker)
- **前台**: http://localhost:8080
- **後台**: http://localhost:4200
- **API Gateway**: http://localhost:5000
- **Backend Service**: http://localhost:5001

## 🔒 安全配置

### 環境變數設定

複製 `.env.zeabur.example` 並修改以下重要設定：

```bash
# JWT 密鑰 (至少 32 字符)
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production

# 資料庫密碼
DB_PASSWORD=your_secure_password
DB_ROOT_PASSWORD=your_secure_root_password

# CORS 設定
CORS_ALLOWED_ORIGINS=https://your-domain.com
```

### 安全建議
- 使用強密碼 (至少 16 字符，包含大小寫字母、數字、特殊字符)
- 定期更換 JWT 密鑰和資料庫密碼
- 啟用 HTTPS (Zeabur 自動提供)
- 設定適當的 CORS 政策

## 📊 監控和維護

### 健康檢查
```bash
# API 健康檢查
curl https://your-app.zeabur.app/health

# 資料庫健康檢查
curl https://your-app.zeabur.app/health/database
```

### 日誌查看
- **Zeabur**: Dashboard → 服務 → Logs
- **Docker**: `docker-compose logs -f`

### 效能監控
- CPU 使用率: < 70%
- 記憶體使用率: < 80%
- 磁碟使用率: < 85%
- API 回應時間: < 500ms

## 🚨 故障排除

### 常見問題

1. **服務無法啟動**
   - 檢查環境變數配置
   - 查看服務日誌
   - 確認資料庫連接

2. **檔案上傳失敗**
   - 檢查存儲卷掛載
   - 驗證檔案大小和類型
   - 確認目錄權限

3. **前端無法連接後端**
   - 檢查 CORS 設定
   - 確認 API 基礎 URL
   - 驗證網路連接

詳細故障排除請參考各文檔中的相關章節。

## 🤝 貢獻

歡迎提交 Issue 和 Pull Request 來改善這個專案。

### 開發流程
1. Fork 專案
2. 建立功能分支
3. 提交變更
4. 建立 Pull Request

## 📄 授權

本專案採用 MIT 授權條款。

## 📞 支援

如有問題或需要技術支援，請：

1. 查看相關文檔
2. 搜尋現有 Issues
3. 建立新的 Issue
4. 聯絡開發團隊

---

**🎉 感謝使用 ROSCA 平安商會系統！**