# Zeabur 一鍵部署指南

## 概述

本指南提供三種方案在 Zeabur 上一次部署所有服務，就像本地 Docker Compose 一樣。

## 🚀 推薦方案：單一容器部署

### 方案特點
- ✅ 一個容器包含所有服務
- ✅ 使用 supervisor 管理多個進程
- ✅ nginx 作為反向代理
- ✅ 簡化的配置和部署

### 部署步驟

#### 1. 準備環境變數

在 Zeabur 專案中設定以下環境變數：

```env
# 資料庫配置 (使用 Zeabur MariaDB 服務)
DB_HOST=your-mariadb-host
DB_NAME=rosca_db
DB_USER=rosca_user
DB_PASSWORD=your_secure_password

# JWT 配置
JWT_SECRET_KEY=your-32-char-secret-key-here
JWT_ISSUER=ROSCA-API
JWT_AUDIENCE=ROSCA-Client

# CORS 配置
CORS_ALLOWED_ORIGINS=https://your-app.zeabur.app

# 其他配置
ASPNETCORE_ENVIRONMENT=Production
LOG_LEVEL=Information
```

#### 2. 部署到 Zeabur

1. **添加 MariaDB 服務**
   - 在 Zeabur 專案中點擊 "Add Service"
   - 選擇 "Marketplace" → "MariaDB"
   - 配置資料庫名稱、用戶和密碼

2. **部署主應用程式**
   - 點擊 "Add Service" → "Git Repository"
   - 選擇您的 GitHub 儲存庫
   - Zeabur 會自動使用根目錄的 `Dockerfile` 建置
   - 設定環境變數

3. **配置域名**
   - 為主應用程式設定域名
   - 前台：`https://your-app.zeabur.app/`
   - 後台：`https://your-app.zeabur.app/admin`

#### 3. 初始化資料庫

部署完成後，需要手動初始化資料庫：

1. 連接到 MariaDB 服務
2. 執行以下 SQL 檔案：
   ```sql
   -- 依序執行
   source database/init/01-schema.sql;
   source database/init/02-default-data.sql;
   source database/init/03-default-user.sql;
   ```

### 服務訪問

部署完成後：
- **前台系統**: `https://your-app.zeabur.app/`
- **後台管理**: `https://your-app.zeabur.app/admin`
- **API 端點**: `https://your-app.zeabur.app/api/`

## 🔧 替代方案：Docker Compose

如果 Zeabur 支援 Docker Compose，可以使用 `docker-compose.zeabur.yml`：

### 使用步驟

1. 將 `docker-compose.zeabur.yml` 重命名為 `docker-compose.yml`
2. 在 Zeabur 中選擇 "Docker Compose" 部署方式
3. 設定環境變數
4. 部署

## 📋 環境變數完整清單

### 必要變數

| 變數名稱 | 說明 | 範例值 |
|---------|------|--------|
| `DB_HOST` | MariaDB 主機 | `mariadb-service-host` |
| `DB_NAME` | 資料庫名稱 | `rosca_db` |
| `DB_USER` | 資料庫用戶 | `rosca_user` |
| `DB_PASSWORD` | 資料庫密碼 | `secure_password` |
| `JWT_SECRET_KEY` | JWT 密鑰 | `32-char-secret-key` |
| `CORS_ALLOWED_ORIGINS` | 允許的域名 | `https://your-app.zeabur.app` |

### 可選變數

| 變數名稱 | 預設值 | 說明 |
|---------|--------|------|
| `JWT_ISSUER` | `ROSCA-API` | JWT 發行者 |
| `JWT_AUDIENCE` | `ROSCA-Client` | JWT 受眾 |
| `JWT_EXPIRY_MINUTES` | `60` | JWT 過期時間 |
| `LOG_LEVEL` | `Information` | 日誌等級 |
| `FILE_UPLOAD_MAX_SIZE` | `10485760` | 檔案上傳大小限制 |

## 🔍 驗證部署

### 健康檢查

部署完成後，驗證以下端點：

1. **應用程式健康檢查**
   ```bash
   curl https://your-app.zeabur.app/api/HomePicture/GetAnnImages
   ```

2. **前台系統**
   ```bash
   curl https://your-app.zeabur.app/
   ```

3. **後台系統**
   ```bash
   curl https://your-app.zeabur.app/admin
   ```

### 功能測試

1. **前台功能**
   - 用戶註冊
   - 用戶登入
   - 瀏覽公告

2. **後台功能**
   - 管理員登入
   - 用戶管理
   - 內容管理

## 🚨 故障排除

### 常見問題

1. **資料庫連接失敗**
   - 檢查 `DB_HOST` 環境變數
   - 確認 MariaDB 服務正常運行
   - 驗證資料庫用戶權限

2. **API 調用失敗**
   - 檢查應用程式日誌
   - 驗證 JWT 配置
   - 確認 CORS 設定

3. **前端無法載入**
   - 檢查 nginx 配置
   - 驗證靜態檔案路徑
   - 確認域名配置

### 查看日誌

在 Zeabur 控制台中：
1. 進入服務詳情
2. 點擊 "Logs" 標籤
3. 查看即時日誌輸出

### 重新部署

如果需要重新部署：
1. 在 Zeabur 控制台中點擊 "Redeploy"
2. 或推送新的 commit 到 GitHub 觸發自動部署

## 📈 性能優化

### 資源配置

建議的資源配置：
- **CPU**: 1 vCPU
- **記憶體**: 1GB
- **存儲**: 10GB

### 擴展策略

1. **垂直擴展**: 增加 CPU 和記憶體
2. **水平擴展**: 部署多個實例（需要外部資料庫）

## 🔒 安全考量

1. **HTTPS**: Zeabur 自動提供 SSL 憑證
2. **環境變數**: 敏感資訊使用環境變數
3. **CORS**: 限制跨域訪問
4. **JWT**: 使用強密鑰

## 📞 支援

如果遇到問題：
1. 檢查 Zeabur 官方文檔
2. 查看應用程式日誌
3. 在 GitHub 儲存庫建立 Issue

---

**注意**: 這種單一容器部署方式適合中小型應用程式。對於大型應用程式，建議使用微服務架構分別部署各個服務。