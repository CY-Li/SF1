# ROSCA 系統服務說明

本文件詳細說明 ROSCA 系統各個服務的配置、端口和訪問方式。

## 服務架構概覽

```
┌─────────────────┐    ┌─────────────────┐
│   前台系統      │    │   後台管理      │
│   (Vue.js)      │    │   (Angular)     │
│   Port: 80      │    │   Port: 8080    │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └──────────┬───────────┘
                     │
          ┌─────────────────┐
          │   後端 API      │
          │   (.NET Core)   │
          │   Port: 5000    │
          └─────────┬───────┘
                    │
          ┌─────────────────┐
          │   MariaDB       │
          │   Port: 3306    │
          └─────────────────┘
```

## 服務詳細說明

### 1. MariaDB 資料庫服務

**容器名稱**: `rosca-mariadb`  
**映像**: `mariadb:11.3.2`  
**內部端口**: 3306  
**外部端口**: 3306 (開發環境)

#### 配置
- **資料庫名稱**: rosca2
- **字符集**: utf8mb4
- **時區**: Asia/Taipei
- **資料持久化**: Docker volume `db-data`

#### 環境變數
```bash
MYSQL_ROOT_PASSWORD=${DB_ROOT_PASSWORD}
MYSQL_DATABASE=${DB_NAME}
MYSQL_USER=${DB_USER}
MYSQL_PASSWORD=${DB_PASSWORD}
```

#### 連接資訊
```bash
# 從容器內連接
docker-compose exec mariadb mysql -u root -p

# 從外部連接 (開發環境)
mysql -h localhost -P 3306 -u ${DB_USER} -p${DB_PASSWORD} ${DB_NAME}
```

#### 健康檢查
```bash
# 檢查資料庫狀態
docker-compose exec mariadb mysqladmin ping -h localhost -u root -p

# 檢查資料庫連接
curl -f http://localhost:5000/health
```

#### 資料目錄
- **資料庫檔案**: `/var/lib/mysql` (掛載到 `db-data` volume)
- **初始化腳本**: `/docker-entrypoint-initdb.d` (掛載到 `./database/init`)
- **配置檔案**: `/etc/mysql/conf.d/custom.cnf` (掛載到 `./database/my.cnf`)
- **日誌檔案**: `/var/log/mysql` (掛載到 `db-logs` volume)

---

### 2. 後端 API 服務 (.NET Core)

**容器名稱**: `rosca-backend`  
**建置來源**: `./backendAPI/DotNetBackEndCleanArchitecture`  
**內部端口**: 5000  
**外部端口**: 無 (僅內部訪問)

#### 配置
- **框架**: ASP.NET Core 7.0
- **架構**: Clean Architecture
- **API 文件**: Swagger (開發環境)
- **背景任務**: Hangfire

#### 環境變數
```bash
ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
ASPNETCORE_URLS=http://+:5000
ConnectionStrings__BackEndDatabase="Server=mariadb;Port=3306;..."
JWT__SecretKey=${JWT_SECRET_KEY}
CORS__AllowedOrigins=${CORS_ALLOWED_ORIGINS}
```

#### API 端點
- **健康檢查**: `GET /health`
- **API 根路徑**: `/api/`
- **Swagger 文件**: `/swagger` (開發環境)
- **Hangfire 儀表板**: `/hangfire` (如果啟用)

#### 檔案上傳目錄
- **一般上傳**: `/app/uploads` (掛載到 `uploads` volume)
- **KYC 圖片**: `/app/KycImages` (掛載到 `backend-kycimages` volume)
- **存款圖片**: `/app/DepositImages` (掛載到 `backend-depositimages` volume)
- **提款圖片**: `/app/WithdrawImages` (掛載到 `backend-withdrawimages` volume)
- **公告圖片**: `/app/AnnImagessss` (掛載到 `backend-annimages` volume)

#### 健康檢查
```bash
# 內部健康檢查
curl -f http://backend:5000/health

# 透過前台代理檢查
curl -f http://localhost/api/health

# 透過後台代理檢查
curl -f http://localhost:8080/api/health
```

---

### 3. 前台使用者系統 (Vue.js + Nginx)

**容器名稱**: `rosca-frontend`  
**建置來源**: `./frontend`  
**內部端口**: 80  
**外部端口**: 80

#### 配置
- **Web 伺服器**: Nginx
- **前端框架**: Vue.js
- **靜態檔案**: 由 Nginx 直接提供
- **API 代理**: `/api/` → `http://backend:5000/api/`

#### 訪問方式
```bash
# 主要訪問地址
http://localhost

# 健康檢查
curl -f http://localhost
```

#### Nginx 配置重點
```nginx
# API 代理
location /api/ {
    proxy_pass http://backend:5000/api/;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    # ... 其他代理設定
}

# 靜態檔案
location / {
    root /usr/share/nginx/html;
    try_files $uri $uri/ /index.html;
}
```

#### 功能特色
- **響應式設計**: 支援手機和桌面瀏覽
- **SPA 路由**: 支援前端路由
- **API 整合**: 透過代理與後端通信
- **靜態資源快取**: 優化載入效能

---

### 4. 後台管理系統 (Angular + Nginx)

**容器名稱**: `rosca-admin`  
**建置來源**: `./backend/FontEnd`  
**內部端口**: 80  
**外部端口**: 8080

#### 配置
- **Web 伺服器**: Nginx
- **前端框架**: Angular 17
- **建置工具**: Node.js 18 + npm
- **API 代理**: `/api/` → `http://backend:5000/api/`

#### 訪問方式
```bash
# 主要訪問地址
http://localhost:8080

# 健康檢查
curl -f http://localhost:8080
```

#### 建置流程
```dockerfile
# 建置階段
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build --prod

# 運行階段
FROM nginx:alpine
COPY --from=build /app/dist/admin /usr/share/nginx/html
```

#### 功能特色
- **管理介面**: 完整的後台管理功能
- **權限控制**: 基於角色的訪問控制
- **資料視覺化**: 圖表和統計資訊
- **即時更新**: WebSocket 支援 (如果實作)

---

## 網路配置

### 內部網路
所有服務都在 `rosca-network` 橋接網路中運行：

```yaml
networks:
  rosca-network:
    driver: bridge
```

### 服務間通信
- **前台** → **後端**: `http://backend:5000/api/`
- **後台** → **後端**: `http://backend:5000/api/`
- **後端** → **資料庫**: `mariadb:3306`

### 外部訪問端口
- **80**: 前台使用者系統
- **8080**: 後台管理系統
- **3306**: MariaDB (僅開發環境)

---

## 資料持久化

### Docker Volumes
```yaml
volumes:
  db-data:           # 資料庫資料
  db-logs:           # 資料庫日誌
  uploads:           # 一般檔案上傳
  backend-kycimages: # KYC 驗證圖片
  backend-depositimages: # 存款憑證圖片
  backend-withdrawimages: # 提款申請圖片
  backend-annimages: # 公告圖片
```

### 備份建議
```bash
# 資料庫備份
docker-compose exec mariadb mysqldump -u root -p${DB_ROOT_PASSWORD} ${DB_NAME} > backup.sql

# 檔案備份
docker run --rm -v rosca-docker_uploads:/data -v $(pwd):/backup alpine tar czf /backup/uploads-backup.tar.gz -C /data .
```

---

## 監控和日誌

### 健康檢查端點
- **資料庫**: `mysqladmin ping`
- **後端 API**: `GET /health`
- **前台**: `GET /` (HTTP 200)
- **後台**: `GET /` (HTTP 200)

### 日誌查看
```bash
# 所有服務日誌
docker-compose logs -f

# 特定服務日誌
docker-compose logs -f mariadb
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f admin
```

### 效能監控
```bash
# 容器資源使用
docker stats

# 服務狀態
docker-compose ps

# 網路連接
docker-compose exec backend netstat -tuln
```

---

## 安全考量

### 開發環境
- 資料庫端口對外開放 (便於開發)
- 使用 HTTP (非 HTTPS)
- 預設密碼和金鑰

### 生產環境建議
- 關閉資料庫外部端口
- 啟用 HTTPS
- 更改所有預設密碼
- 使用強 JWT 金鑰
- 配置防火牆規則
- 定期更新容器映像

### 環境變數安全
```bash
# 敏感資訊應存放在 .env 文件中
DB_ROOT_PASSWORD=strong_password_here
JWT_SECRET_KEY=very-long-and-random-secret-key
```

---

## 故障排除快速參考

### 服務無法啟動
```bash
# 檢查服務狀態
docker-compose ps

# 檢查日誌
docker-compose logs [service_name]

# 重啟服務
docker-compose restart [service_name]
```

### 網路連接問題
```bash
# 測試容器間連接
docker-compose exec frontend ping backend
docker-compose exec backend ping mariadb

# 檢查端口
netstat -tulpn | grep -E ":(80|8080|3306|5000)"
```

### 資料庫問題
```bash
# 檢查資料庫連接
docker-compose exec mariadb mysql -u root -p -e "SELECT 1;"

# 重新初始化資料庫
docker-compose down -v
docker-compose up -d mariadb
```

更多詳細的故障排除資訊請參考 [TROUBLESHOOTING.md](TROUBLESHOOTING.md)。