# ROSCA 系統故障排除指南

本文件提供 ROSCA Docker 部署的常見問題解決方案。

## 快速診斷

### 檢查系統狀態
```bash
# 檢查所有服務狀態
docker-compose ps

# 檢查服務健康狀態
docker-compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"

# 檢查 Docker 系統資源
docker stats --no-stream
```

### 檢查日誌
```bash
# 查看所有服務日誌
docker-compose logs

# 查看特定服務日誌
docker-compose logs mariadb
docker-compose logs backend
docker-compose logs frontend
docker-compose logs admin

# 即時查看日誌
docker-compose logs -f [service_name]
```

## 常見問題與解決方案

### 1. 資料庫相關問題

#### 問題：資料庫連接失敗
**症狀：**
- 後端 API 無法啟動
- 日誌顯示 "Connection refused" 或 "Access denied"

**解決方案：**
```bash
# 1. 檢查資料庫容器狀態
docker-compose ps mariadb

# 2. 檢查資料庫日誌
docker-compose logs mariadb

# 3. 檢查資料庫是否就緒
docker-compose exec mariadb mysqladmin ping -h localhost -u root -p

# 4. 重啟資料庫服務
docker-compose restart mariadb

# 5. 如果問題持續，重建資料庫
docker-compose down
docker volume rm $(docker volume ls -q | grep rosca)
docker-compose up -d mariadb
```

#### 問題：資料庫初始化失敗
**症狀：**
- 資料庫容器啟動但沒有建立表格
- 登入時提示 "Table doesn't exist"

**解決方案：**
```bash
# 1. 檢查初始化腳本
ls -la database/init/

# 2. 檢查腳本權限
chmod 644 database/init/*.sql

# 3. 重新初始化資料庫
docker-compose down
docker volume rm rosca-docker_db-data
docker-compose up -d mariadb

# 4. 監控初始化過程
docker-compose logs -f mariadb
```

### 2. 後端 API 問題

#### 問題：後端 API 無法啟動
**症狀：**
- 容器不斷重啟
- 健康檢查失敗

**解決方案：**
```bash
# 1. 檢查後端日誌
docker-compose logs backend

# 2. 檢查環境變數
docker-compose exec backend env | grep -E "(DB_|ASPNETCORE_)"

# 3. 檢查資料庫連接
docker-compose exec backend ping mariadb

# 4. 重建後端映像
docker-compose build --no-cache backend
docker-compose up -d backend

# 5. 手動測試 API
curl -f http://localhost:5000/health
```

#### 問題：API 回應 500 錯誤
**症狀：**
- 前端無法載入資料
- API 請求返回內部伺服器錯誤

**解決方案：**
```bash
# 1. 檢查詳細錯誤日誌
docker-compose logs backend | grep -i error

# 2. 檢查資料庫連接字串
docker-compose exec backend cat appsettings.json

# 3. 測試資料庫查詢
docker-compose exec mariadb mysql -u root -p -e "USE rosca2; SHOW TABLES;"

# 4. 重啟後端服務
docker-compose restart backend
```

### 3. 前端問題

#### 問題：前端無法訪問
**症狀：**
- 瀏覽器顯示 "This site can't be reached"
- 端口 80 無回應

**解決方案：**
```bash
# 1. 檢查前端容器狀態
docker-compose ps frontend

# 2. 檢查端口是否被佔用
netstat -tulpn | grep :80
# Windows: netstat -an | findstr :80

# 3. 檢查 Nginx 配置
docker-compose exec frontend nginx -t

# 4. 檢查前端日誌
docker-compose logs frontend

# 5. 重啟前端服務
docker-compose restart frontend
```

#### 問題：API 請求失敗 (CORS 錯誤)
**症狀：**
- 瀏覽器控制台顯示 CORS 錯誤
- 網路請求被阻擋

**解決方案：**
```bash
# 1. 檢查 Nginx 代理配置
docker-compose exec frontend cat /etc/nginx/nginx.conf

# 2. 檢查後端 CORS 設定
docker-compose logs backend | grep -i cors

# 3. 測試 API 代理
curl -H "Origin: http://localhost" http://localhost/api/health

# 4. 重建前端映像
docker-compose build --no-cache frontend
```

### 4. 後台管理系統問題

#### 問題：後台無法訪問 (端口 8080)
**症狀：**
- 無法開啟 http://localhost:8080
- 連接被拒絕

**解決方案：**
```bash
# 1. 檢查後台容器狀態
docker-compose ps admin

# 2. 檢查端口映射
docker port rosca-admin

# 3. 檢查端口衝突
netstat -tulpn | grep :8080

# 4. 檢查後台日誌
docker-compose logs admin

# 5. 重啟後台服務
docker-compose restart admin
```

### 5. 網路連接問題

#### 問題：容器間無法通信
**症狀：**
- 服務無法互相訪問
- DNS 解析失敗

**解決方案：**
```bash
# 1. 檢查網路配置
docker network ls
docker network inspect rosca-docker_rosca-network

# 2. 測試容器間連接
docker-compose exec frontend ping backend
docker-compose exec backend ping mariadb

# 3. 檢查服務名稱解析
docker-compose exec frontend nslookup backend

# 4. 重建網路
docker-compose down
docker network prune
docker-compose up -d
```

### 6. 效能問題

#### 問題：系統回應緩慢
**症狀：**
- 頁面載入時間過長
- API 請求超時

**解決方案：**
```bash
# 1. 檢查系統資源使用
docker stats

# 2. 檢查磁碟空間
df -h
docker system df

# 3. 檢查記憶體使用
free -h

# 4. 優化 Docker 資源
docker system prune -f
docker volume prune -f

# 5. 調整容器資源限制 (在 docker-compose.yml 中)
```

### 7. 檔案權限問題

#### 問題：檔案上傳失敗
**症狀：**
- 無法上傳圖片或文件
- 權限被拒絕錯誤

**解決方案：**
```bash
# 1. 檢查上傳目錄權限
docker-compose exec backend ls -la /app/uploads

# 2. 修正目錄權限
docker-compose exec backend chown -R www-data:www-data /app/uploads
docker-compose exec backend chmod -R 755 /app/uploads

# 3. 檢查 volume 掛載
docker volume inspect rosca-docker_uploads
```

## 完全重置系統

如果問題無法解決，可以完全重置系統：

```bash
# 1. 停止所有服務
docker-compose down

# 2. 移除所有相關資源
docker-compose down -v --rmi all

# 3. 清理 Docker 系統
docker system prune -a -f
docker volume prune -f

# 4. 重新啟動
./scripts/start.sh
```

## 日誌分析

### 重要日誌位置
- **MariaDB**: `docker-compose logs mariadb`
- **後端 API**: `docker-compose logs backend`
- **前端**: `docker-compose logs frontend`
- **後台**: `docker-compose logs admin`

### 常見錯誤模式
```bash
# 資料庫連接錯誤
grep -i "connection.*refused\|access.*denied" <(docker-compose logs)

# API 錯誤
grep -i "error\|exception\|failed" <(docker-compose logs backend)

# Nginx 錯誤
grep -i "error\|failed" <(docker-compose logs frontend admin)
```

## 效能監控

### 即時監控
```bash
# 監控容器資源使用
watch docker stats

# 監控服務狀態
watch docker-compose ps

# 監控網路連接
watch "docker-compose exec backend netstat -tuln"
```

### 健康檢查
```bash
# 檢查所有服務健康狀態
docker-compose ps --format "table {{.Name}}\t{{.Status}}"

# 手動健康檢查
curl -f http://localhost/health
curl -f http://localhost:8080/
curl -f http://localhost:5000/health
```

## 聯絡支援

如果問題仍然無法解決，請提供以下資訊：

1. **系統資訊**:
   ```bash
   docker --version
   docker-compose --version
   uname -a  # Linux/macOS
   systeminfo  # Windows
   ```

2. **錯誤日誌**:
   ```bash
   docker-compose logs > system-logs.txt
   ```

3. **系統狀態**:
   ```bash
   docker-compose ps
   docker stats --no-stream
   ```

4. **環境配置**:
   ```bash
   cat .env  # 請移除敏感資訊如密碼
   ```