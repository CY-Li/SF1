# Zeabur 快速設定指南

## 🚀 快速開始

### 1. 執行部署前檢查

```powershell
# Windows PowerShell
.\deployment\zeabur\verify-setup.ps1

# 或者手動檢查關鍵檔案
ls zeabur.json, .env.zeabur, ZEABUR-DEPLOYMENT.md
```

### 2. 登入 Zeabur 並建立專案

1. 前往 https://dash.zeabur.com
2. 使用 GitHub/GitLab 登入
3. 點擊 **Create Project**
4. 專案名稱：`rosca-system`
5. 連接此 Git 存儲庫

### 3. 設定環境變數

複製 `.env.zeabur` 內容到 Zeabur 專案環境變數：

```env
# 關鍵變數 (必須設定)
DB_HOST=43.167.174.222
DB_PORT=31500
DB_NAME=zeabur
DB_USER=root
DB_PASSWORD=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production-min-32-chars-rosca-2024

# 其他變數請參考 .env.zeabur 完整列表
```

### 4. 確認服務配置

Zeabur 將自動偵測 `zeabur.json` 並部署以下服務：

| 服務 | 類型 | 端口 | 說明 |
|------|------|------|------|
| backend-service | .NET Core | 5001 | 微服務 |
| api-gateway | .NET Core | 5000 | API 閘道 |
| frontend | Vue.js + Nginx | 80 | 前台系統 |
| admin | Angular + Nginx | 8080 | 後台系統 |

## ⚡ 重要提醒

### 資料庫連接
- ✅ 使用外部 MariaDB (43.167.174.222:31500)
- ✅ 已移除本地資料庫依賴
- ✅ 連接字串已配置

### 域名設定
部署完成後更新 CORS 設定：
```env
CORS_ALLOWED_ORIGINS=https://your-actual-domain.zeabur.app,https://admin-your-actual-domain.zeabur.app
```

### 檔案存儲
API Gateway 已配置持久化存儲卷：
- uploads: 5GB
- kyc-images: 2GB  
- deposit-images: 2GB
- withdraw-images: 2GB
- ann-images: 2GB

## 🔧 故障排除

### 常見問題

**部署失敗**
1. 檢查 Dockerfile 路徑是否正確
2. 確認環境變數已設定
3. 查看 Zeabur 服務日誌

**資料庫連接失敗**
1. 確認外部資料庫服務狀態
2. 檢查連接字串格式
3. 驗證網路連通性

**CORS 錯誤**
1. 更新 CORS_ALLOWED_ORIGINS
2. 確認域名配置正確
3. 檢查前端 API 請求 URL

## 📞 支援

- Zeabur 文檔: https://zeabur.com/docs
- Discord: https://discord.gg/zeabur
- 專案文檔: `ZEABUR-DEPLOYMENT.md`