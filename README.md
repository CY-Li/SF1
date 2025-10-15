# ROSCA 平安商會系統 - Docker 部署

這是一個完整的全端應用程式，包含前台使用者系統、後台管理系統、後端 API 和 MariaDB 資料庫的 Docker 部署配置。

## 系統架構

- **前台系統**: HTML + Vue.js (端口 80)
- **後台管理系統**: Angular 17 (端口 8080)  
- **後端 API**: .NET Core 7 Clean Architecture (內部端口 5000)
- **資料庫**: MariaDB 11.3.2 (內部端口 3306)

## 快速開始

### 前置需求

- Docker Engine 20.10+
- Docker Compose 2.0+
- 至少 4GB 可用記憶體
- 至少 10GB 可用磁碟空間

### 部署步驟

1. **複製環境變數文件**
   ```bash
   cp .env.example .env
   ```

2. **修改環境變數** (可選)
   ```bash
   # 編輯 .env 文件，修改資料庫密碼等設定
   nano .env
   ```

3. **啟動所有服務**
   ```bash
   docker-compose up -d
   ```

4. **檢查服務狀態**
   ```bash
   docker-compose ps
   ```

5. **查看日誌**
   ```bash
   # 查看所有服務日誌
   docker-compose logs -f
   
   # 查看特定服務日誌
   docker-compose logs -f backend
   ```

### 訪問應用程式

- **前台使用者系統**: http://localhost
- **後台管理系統**: http://localhost:8080

### 預設測試帳號

**一般使用者:**
- **帳號**: 0938766349
- **密碼**: 123456

**管理員:**
- **帳號**: admin
- **密碼**: 123456

### 資料庫連接資訊

**開發環境直接連接:**
- **主機**: localhost
- **端口**: 3306
- **資料庫**: rosca2
- **使用者**: appuser
- **密碼**: apppassword (可在 .env 文件中修改)

## 管理命令

### 啟動服務
```bash
# 啟動所有服務
docker-compose up -d

# 啟動特定服務
docker-compose up -d mariadb backend
```

### 停止服務
```bash
# 停止所有服務
docker-compose down

# 停止並移除 volumes (會刪除資料庫資料)
docker-compose down -v
```

### 重建服務
```bash
# 重建所有服務
docker-compose build --no-cache

# 重建特定服務
docker-compose build --no-cache backend
```

### 資料庫管理
```bash
# 連接到資料庫容器
docker-compose exec mariadb mysql -u root -p

# 備份資料庫
docker-compose exec mariadb mysqldump -u root -p rosca2 > backup.sql

# 恢復資料庫
docker-compose exec -T mariadb mysql -u root -p rosca2 < backup.sql

# 查看資料庫日誌
docker-compose logs mariadb

# 重新初始化資料庫 (會刪除所有資料)
docker-compose down -v
docker-compose up -d
```

### 查看狀態
```bash
# 查看服務狀態
docker-compose ps

# 查看資源使用情況
docker stats
```

## 開發指南

### 檔案結構
```
project/
├── docker-compose.yml          # Docker Compose 主配置
├── .env                        # 環境變數 (不要提交到版本控制)
├── .env.example               # 環境變數範例
├── README.md                  # 專案說明
├── frontend/                  # 前台 Vue.js 應用
│   ├── Dockerfile
│   ├── nginx.conf
│   └── [前台檔案]
├── backend/FontEnd/           # 後台 Angular 應用
│   ├── Dockerfile
│   ├── admin-nginx.conf
│   └── FontEnd/
├── backendAPI/                # 後端 .NET Core API
│   ├── Dockerfile
│   └── DotNetBackEndCleanArchitecture/
├── database/                  # 資料庫初始化腳本
│   ├── init/
│   └── my.cnf
└── scripts/                   # 管理腳本
    ├── start.sh
    ├── stop.sh
    └── clean.sh
```

### 資料持久化

- **資料庫資料**: 儲存在 `db-data` Docker volume
- **上傳檔案**: 儲存在 `uploads` Docker volume 和對應的目錄掛載

### 網路配置

所有服務都在 `rosca-network` 內部網路中運行，服務間可以使用服務名稱進行通信：
- `mariadb`: 資料庫服務
- `backend`: 後端 API 服務
- `frontend`: 前台服務
- `admin`: 後台管理服務

## 故障排除

### 常見問題

1. **資料庫連接失敗**
   ```bash
   # 檢查資料庫是否就緒
   docker-compose logs mariadb
   
   # 重啟資料庫服務
   docker-compose restart mariadb
   ```

2. **後端 API 無法啟動**
   ```bash
   # 檢查後端日誌
   docker-compose logs backend
   
   # 重建後端映像
   docker-compose build --no-cache backend
   ```

3. **前端無法訪問**
   ```bash
   # 檢查 Nginx 配置
   docker-compose exec frontend nginx -t
   
   # 重啟前端服務
   docker-compose restart frontend
   ```

4. **端口衝突**
   ```bash
   # 檢查端口使用情況
   netstat -tulpn | grep :80
   netstat -tulpn | grep :8080
   
   # 修改 docker-compose.yml 中的端口映射
   ```

### 清理環境

```bash
# 停止並移除所有容器
docker-compose down

# 移除所有相關映像
docker-compose down --rmi all

# 完全清理 (包含 volumes)
docker-compose down -v --rmi all
docker system prune -f
```

## 開發模式

如需在開發模式下運行，可以：

1. 修改 `.env` 中的 `ASPNETCORE_ENVIRONMENT=Development`
2. 掛載原始碼目錄以支援熱重載
3. 使用 `docker-compose.override.yml` 覆蓋開發配置

## 生產部署注意事項

1. **安全性**
   - 更改所有預設密碼
   - 使用 HTTPS
   - 配置防火牆規則
   - 定期更新容器映像

2. **效能**
   - 調整資料庫配置
   - 配置負載均衡
   - 監控資源使用情況

3. **備份**
   - 定期備份資料庫
   - 備份上傳檔案
   - 測試恢復流程

## 進階管理

### 管理腳本

系統提供了多個管理腳本來簡化操作：

```bash
# 系統啟動和停止
./scripts/start.sh              # 啟動開發環境
./scripts/start-prod.sh         # 啟動生產環境
./scripts/stop.sh               # 停止開發環境
./scripts/stop-prod.sh          # 停止生產環境
./scripts/clean.sh              # 清理系統

# 健康檢查和測試
./scripts/health-check.sh       # 健康檢查
./scripts/test.sh               # 系統測試
./scripts/integration-test.sh   # 整合測試

# 監控和日誌
./scripts/monitor.sh quick      # 快速監控
./scripts/monitor.sh full       # 完整監控
./scripts/logs-manager.sh -f backend  # 日誌管理

# 備份和恢復
./scripts/backup-database.sh    # 資料庫備份
./scripts/backup-files.sh       # 檔案備份
./scripts/backup-full.sh        # 完整備份
./scripts/restore-full.sh       # 完整恢復
```

### 環境配置

#### 開發環境
- 使用 `.env` 配置檔案
- 啟用詳細日誌記錄
- 開啟 Hangfire Dashboard
- 允許所有 CORS 來源

#### 生產環境
- 使用 `.env.prod` 配置檔案
- 最小化日誌記錄
- 關閉 Hangfire Dashboard
- 嚴格的 CORS 設定
- 啟用資源限制

### 效能優化

#### Docker 映像優化
1. **多階段建置**: 減少最終映像大小
2. **基礎映像**: 使用 Alpine Linux
3. **快取策略**: 合理安排 Dockerfile 指令順序
4. **排除檔案**: 使用 .dockerignore 排除不必要檔案

#### 資源配置
```yaml
# 在 docker-compose.prod.yml 中設定資源限制
services:
  backend:
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
          cpus: '1.0'
          memory: 1G
```

### 安全最佳實踐

#### 容器安全
- 使用非 root 使用者運行容器
- 定期更新基礎映像
- 掃描安全漏洞
- 最小權限原則

#### 網路安全
- 使用內部網路隔離服務
- 僅暴露必要端口
- 生產環境使用 HTTPS
- 配置防火牆規則

#### 資料安全
- 使用強密碼
- 加密敏感資料傳輸
- 定期備份
- 實施訪問控制

### 監控和告警

#### 健康檢查
- 容器層級健康檢查
- 應用層級健康端點
- 資料庫連線檢查
- 服務間連通性檢查

#### 效能監控
```bash
# 檢查資源使用
docker stats

# 系統監控
./scripts/monitor.sh full

# 應用程式健康檢查
curl http://localhost:5000/health
```

### 備份策略

#### 自動備份
- 資料庫：每日自動備份
- 檔案：每日增量備份
- 配置：每次修改後備份

#### 備份驗證
```bash
# 驗證備份完整性
./scripts/verify-backup.sh backup_file.sql

# 測試恢復流程
./scripts/restore-full.sh backup_date
```

## 故障排除進階

### 效能問題

1. **高 CPU 使用率**
   ```bash
   docker stats
   ./scripts/monitor.sh full
   ```

2. **記憶體不足**
   ```bash
   docker-compose restart backend
   docker system prune -f
   ```

3. **磁碟空間不足**
   ```bash
   ./scripts/logs-manager.sh -c
   docker system prune -a
   ```

### 網路問題

1. **容器間連線失敗**
   ```bash
   docker network inspect rosca-docker_rosca-network
   docker-compose exec frontend ping backend
   ```

2. **外部訪問失敗**
   ```bash
   netstat -tlnp | grep -E ":80|:8080|:5000"
   ```

### 資料庫問題

1. **連線失敗**
   ```bash
   docker-compose exec mariadb mysqladmin ping -u root -p
   ```

2. **效能問題**
   ```bash
   docker-compose exec mariadb mysql -u root -p -e "SHOW PROCESSLIST;"
   ```

## 維護計劃

### 日常維護
- 檢查系統狀態
- 監控資源使用
- 檢查日誌錯誤
- 驗證備份完整性

### 週期性維護
- 更新系統和依賴
- 清理舊日誌和備份
- 效能調優
- 安全掃描

### 緊急維護
- 故障恢復程序
- 資料恢復流程
- 服務降級策略

## 相關文件

- [部署指南](DEPLOYMENT-GUIDE.md) - 詳細的部署配置說明
- [備份恢復指南](BACKUP-RESTORE.md) - 完整的備份和恢復操作
- [故障排除指南](TROUBLESHOOTING.md) - 常見問題和解決方案
- [服務說明](SERVICES.md) - 各服務的詳細說明

## 支援

如有問題，請檢查：
1. Docker 和 Docker Compose 版本
2. 系統資源是否充足
3. 網路連接是否正常
4. 日誌檔案中的錯誤訊息
5. 相關文件中的故障排除指南