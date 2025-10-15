# ROSCA 部署指南

## 環境配置差異

### 開發環境 (Development)

開發環境適用於本地開發和測試，具有以下特點：

#### 配置檔案
- 使用 `docker-compose.yml` + `docker-compose.override.yml`
- 環境變數檔案：`.env`

#### 特點
- 啟用詳細日誌記錄
- 開啟 Hangfire Dashboard
- 允許所有 CORS 來源
- 使用較短的 JWT 過期時間
- 啟用熱重載（如果支援）

#### 啟動方式
```bash
# 開發環境啟動
docker-compose up -d

# 或使用腳本
./scripts/start.sh
```

#### 環境變數設定
```env
ASPNETCORE_ENVIRONMENT=Development
LOG_LEVEL=Debug
HANGFIRE_DASHBOARD_ENABLED=true
CORS_ALLOWED_ORIGINS=http://localhost,http://localhost:8080,http://localhost:3000
JWT_EXPIRY_MINUTES=60
```

### 生產環境 (Production)

生產環境適用於正式部署，注重安全性和效能：

#### 配置檔案
- 使用 `docker-compose.yml` + `docker-compose.prod.yml`
- 環境變數檔案：`.env.prod`

#### 特點
- 最小化日誌記錄
- 關閉 Hangfire Dashboard
- 嚴格的 CORS 設定
- 較長的 JWT 過期時間
- 啟用資源限制
- 使用 HTTPS

#### 啟動方式
```bash
# 生產環境啟動
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# 或使用腳本
./scripts/start-prod.sh
```

#### 環境變數設定
```env
ASPNETCORE_ENVIRONMENT=Production
LOG_LEVEL=Warning
HANGFIRE_DASHBOARD_ENABLED=false
CORS_ALLOWED_ORIGINS=https://yourdomain.com
JWT_EXPIRY_MINUTES=1440
```

## 安全性配置

### 開發環境安全注意事項
1. 不要在開發環境中使用生產資料
2. 定期更新開發環境的密碼
3. 限制開發環境的網路訪問

### 生產環境安全要求
1. **強密碼政策**
   - 資料庫密碼至少 16 字元
   - JWT 密鑰至少 32 字元
   - 定期更換密碼

2. **網路安全**
   - 使用 HTTPS
   - 配置防火牆
   - 限制資料庫訪問

3. **容器安全**
   - 使用非 root 使用者運行容器
   - 定期更新基礎映像
   - 掃描安全漏洞

## 效能優化

### Docker 映像優化
1. **多階段建置**
   - 分離建置和運行環境
   - 減少最終映像大小

2. **快取策略**
   - 合理安排 Dockerfile 指令順序
   - 使用 .dockerignore 排除不必要檔案

3. **基礎映像選擇**
   - 使用 Alpine Linux 減少映像大小
   - 選擇官方維護的映像

### 資源配置
```yaml
# 生產環境資源限制範例
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

### 資料庫優化
1. **連線池設定**
   - 適當的最大連線數
   - 連線超時設定

2. **索引優化**
   - 為常用查詢建立索引
   - 定期分析查詢效能

3. **備份策略**
   - 定期自動備份
   - 測試備份恢復流程

## 監控和日誌

### 日誌管理
1. **日誌等級**
   - 開發：Debug/Information
   - 生產：Warning/Error

2. **日誌輪轉**
   - 設定日誌檔案大小限制
   - 自動清理舊日誌

3. **集中化日誌**
   - 考慮使用 ELK Stack
   - 或 Grafana + Loki

### 健康檢查
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
  timeout: 10s
  retries: 5
  interval: 30s
  start_period: 60s
```

### 監控指標
- CPU 使用率
- 記憶體使用率
- 磁碟空間
- 網路流量
- 應用程式回應時間
- 資料庫連線數

## 部署檢查清單

### 部署前檢查
- [ ] 環境變數配置正確
- [ ] 密碼已更新為強密碼
- [ ] SSL 憑證已配置
- [ ] 防火牆規則已設定
- [ ] 備份策略已建立
- [ ] 監控系統已配置

### 部署後驗證
- [ ] 所有服務正常啟動
- [ ] 健康檢查通過
- [ ] 應用程式功能正常
- [ ] 資料庫連線正常
- [ ] 日誌記錄正常
- [ ] 效能指標正常

## 故障排除

### 常見問題
1. **容器啟動失敗**
   - 檢查日誌：`docker-compose logs [service]`
   - 檢查資源使用：`docker stats`

2. **資料庫連線失敗**
   - 檢查網路連通性
   - 驗證連線字串
   - 檢查資料庫服務狀態

3. **效能問題**
   - 檢查資源使用情況
   - 分析慢查詢日誌
   - 檢查網路延遲

### 緊急恢復程序
1. **服務中斷**
   ```bash
   # 快速重啟
   docker-compose restart
   
   # 完整重建
   docker-compose down && docker-compose up -d
   ```

2. **資料恢復**
   ```bash
   # 從備份恢復
   ./scripts/restore-database.sh backup-file.sql
   ```

3. **回滾部署**
   ```bash
   # 回滾到上一個版本
   docker-compose down
   git checkout previous-version
   docker-compose up -d
   ```