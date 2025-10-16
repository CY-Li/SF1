# ROSCA 平安商會系統 Docker 部署指南

## 🚀 一鍵啟動

使用 Docker Compose 一鍵啟動整個 ROSCA 系統，包含所有服務和資料庫。

### 快速開始

```bash
# 1. 克隆專案 (如果還沒有)
git clone <your-repo-url>
cd rosca-system

# 2. 啟動系統
chmod +x docker-start.sh
./docker-start.sh start

# 或直接使用 Docker Compose
docker-compose up -d
```

### 系統要求

- Docker 20.10+
- Docker Compose 2.0+
- 至少 4GB RAM
- 至少 10GB 磁碟空間

## 📋 服務架構

### 容器服務

| 服務名稱 | 容器名稱 | 連接埠 | 說明 |
|---------|----------|--------|------|
| mariadb | rosca-mariadb | 3306 | MariaDB 11.3.2 資料庫 |
| backend-service | rosca-backend-service | 5001 | .NET Core 微服務 |
| backend | rosca-backend | 5000 | .NET Core API Gateway |
| frontend | rosca-frontend | 8080 | Vue.js 前台系統 |
| admin | rosca-admin | 4200 | Angular 後台系統 |
| nginx | rosca-nginx | 80, 443 | Nginx 反向代理 |

### 存儲卷

| 卷名稱 | 掛載路徑 | 用途 |
|--------|----------|------|
| mariadb_data | /var/lib/mysql | 資料庫資料 |
| uploads_volume | /app/uploads | 通用檔案上傳 |
| kyc_images_volume | /app/KycImages | KYC 身份認證圖片 |
| deposit_images_volume | /app/DepositImages | 存款憑證圖片 |
| withdraw_images_volume | /app/WithdrawImages | 提款憑證圖片 |
| ann_images_volume | /app/AnnImagessss | 公告圖片 |
| logs_volume | /app/logs | 應用程式日誌 |

## 🔧 配置說明

### 環境變數配置

複製並修改 `.env` 檔案：

```bash
cp .env .env.local
# 編輯 .env.local 檔案，修改密碼和其他設定
```

重要設定項目：

```bash
# 資料庫密碼 (請修改)
DB_PASSWORD=your_secure_password
DB_ROOT_PASSWORD=your_secure_root_password

# JWT 密鑰 (請修改，至少32字符)
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production

# CORS 設定
CORS_ALLOWED_ORIGINS=http://localhost:8080,http://localhost:4200
```

### Nginx 配置

如果需要自訂 Nginx 配置，編輯 `nginx/nginx.conf` 檔案。

## 📱 存取方式

### 直接存取

- **前台系統**: http://localhost:8080
- **後台系統**: http://localhost:4200
- **API Gateway**: http://localhost:5000
- **Backend Service**: http://localhost:5001
- **資料庫**: localhost:3306

### 透過 Nginx 代理

- **前台系統**: http://localhost
- **後台系統**: http://admin.localhost (需設定 hosts)
- **API 服務**: http://localhost/api/

#### 設定 hosts 檔案 (Windows)

編輯 `C:\Windows\System32\drivers\etc\hosts`：

```
127.0.0.1 localhost
127.0.0.1 admin.localhost
```

#### 設定 hosts 檔案 (macOS/Linux)

編輯 `/etc/hosts`：

```
127.0.0.1 localhost
127.0.0.1 admin.localhost
```

## 🛠️ 管理命令

### 使用啟動腳本

```bash
# 啟動所有服務
./docker-start.sh start

# 停止所有服務
./docker-start.sh stop

# 重啟服務
./docker-start.sh restart

# 查看服務狀態
./docker-start.sh status

# 查看日誌
./docker-start.sh logs

# 查看特定服務日誌
./docker-start.sh logs --service backend

# 重新建置映像檔
./docker-start.sh build

# 清理容器和映像檔
./docker-start.sh clean --clean-images
```

### 使用 Docker Compose

```bash
# 啟動服務
docker-compose up -d

# 停止服務
docker-compose down

# 查看服務狀態
docker-compose ps

# 查看日誌
docker-compose logs -f

# 查看特定服務日誌
docker-compose logs -f backend

# 重新建置並啟動
docker-compose up -d --build

# 重啟特定服務
docker-compose restart backend
```

## 🔍 健康檢查

### 自動健康檢查

所有服務都配置了健康檢查，可以透過以下方式查看：

```bash
# 查看所有服務健康狀態
docker-compose ps

# 查看特定服務健康狀態
docker inspect rosca-backend --format='{{.State.Health.Status}}'
```

### 手動健康檢查

```bash
# API Gateway 健康檢查
curl http://localhost:5000/health

# Backend Service 健康檢查
curl http://localhost:5001/health

# 前台系統檢查
curl http://localhost:8080

# 後台系統檢查
curl http://localhost:4200
```

## 📊 監控和日誌

### 查看即時日誌

```bash
# 所有服務日誌
docker-compose logs -f

# 特定服務日誌
docker-compose logs -f backend
docker-compose logs -f mariadb
docker-compose logs -f frontend
```

### 資源使用監控

```bash
# 查看容器資源使用情況
docker stats

# 查看磁碟使用情況
docker system df

# 查看存儲卷使用情況
docker volume ls
```

## 🗄️ 資料庫管理

### 連接資料庫

```bash
# 使用 Docker 連接
docker exec -it rosca-mariadb mysql -u rosca_user -p rosca_db

# 使用外部工具連接
# 主機: localhost
# 連接埠: 3306
# 用戶名: rosca_user
# 密碼: (在 .env 檔案中設定)
# 資料庫: rosca_db
```

### 資料庫備份

```bash
# 備份資料庫
docker exec rosca-mariadb mysqldump -u root -p rosca_db > backup.sql

# 恢復資料庫
docker exec -i rosca-mariadb mysql -u root -p rosca_db < backup.sql
```

## 🔒 安全設定

### 生產環境建議

1. **修改預設密碼**
   ```bash
   # 在 .env 檔案中設定強密碼
   DB_PASSWORD=your_very_secure_password_2024!
   DB_ROOT_PASSWORD=your_very_secure_root_password_2024!
   JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-32-characters-long
   ```

2. **限制網路存取**
   ```yaml
   # 在 docker-compose.yml 中移除不必要的 ports 映射
   # 只保留 nginx 的 80 和 443 連接埠
   ```

3. **啟用 HTTPS**
   ```bash
   # 在 nginx/ssl 目錄放置 SSL 憑證
   # 修改 nginx/nginx.conf 啟用 HTTPS
   ```

## 🚨 故障排除

### 常見問題

#### 1. 服務無法啟動

```bash
# 查看服務日誌
docker-compose logs service-name

# 檢查連接埠是否被佔用
netstat -tulpn | grep :5000

# 重新建置映像檔
docker-compose build --no-cache service-name
```

#### 2. 資料庫連接失敗

```bash
# 檢查資料庫服務狀態
docker-compose ps mariadb

# 查看資料庫日誌
docker-compose logs mariadb

# 測試資料庫連接
docker exec rosca-mariadb mysqladmin ping -h localhost
```

#### 3. 檔案上傳失敗

```bash
# 檢查存儲卷掛載
docker volume ls
docker volume inspect rosca-system_uploads_volume

# 檢查容器內目錄權限
docker exec rosca-backend ls -la /app/uploads
```

#### 4. 前端無法連接後端

```bash
# 檢查網路連接
docker network ls
docker network inspect rosca-system_rosca-network

# 檢查 CORS 設定
curl -H "Origin: http://localhost:8080" \
     -H "Access-Control-Request-Method: POST" \
     -X OPTIONS http://localhost:5000/api/auth/login
```

### 重置系統

如果遇到嚴重問題，可以完全重置系統：

```bash
# 停止並移除所有容器
docker-compose down -v

# 清理映像檔
docker system prune -a

# 重新啟動
./docker-start.sh start
```

## 📈 效能調優

### 資源限制

在 `docker-compose.yml` 中調整資源限制：

```yaml
services:
  backend:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
```

### 快取優化

```bash
# 啟用 Docker BuildKit 加速建置
export DOCKER_BUILDKIT=1

# 使用快取建置
docker-compose build --parallel
```

## 📞 支援

如遇到問題，請提供以下資訊：

1. 系統環境 (OS, Docker 版本)
2. 錯誤訊息和日誌
3. docker-compose.yml 和 .env 配置
4. 重現步驟

---

**🎉 現在您可以使用一個命令啟動整個 ROSCA 系統！**

```bash
./docker-start.sh start
```