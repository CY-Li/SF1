# ROSCA 系統備份與恢復指南

本文件提供 ROSCA Docker 部署的完整備份和恢復操作指南。

## 備份策略

### 備份內容
1. **資料庫資料** - 所有業務資料
2. **上傳檔案** - 使用者上傳的圖片和文件
3. **配置檔案** - 環境變數和配置
4. **Docker 映像** - 自建的應用程式映像

### 備份頻率建議
- **資料庫**: 每日自動備份
- **檔案**: 每日增量備份
- **配置**: 每次修改後備份
- **映像**: 每次更新後備份

## 資料庫備份

### 手動備份

#### 完整備份
```bash
# 備份整個資料庫
docker-compose exec mariadb mysqldump \
  -u root -p${DB_ROOT_PASSWORD} \
  --single-transaction \
  --routines \
  --triggers \
  ${DB_NAME} > backup_$(date +%Y%m%d_%H%M%S).sql

# 備份所有資料庫（包含系統資料庫）
docker-compose exec mariadb mysqldump \
  -u root -p${DB_ROOT_PASSWORD} \
  --all-databases \
  --single-transaction \
  --routines \
  --triggers > full_backup_$(date +%Y%m%d_%H%M%S).sql
```

#### 結構備份（僅表結構）
```bash
# 僅備份資料庫結構
docker-compose exec mariadb mysqldump \
  -u root -p${DB_ROOT_PASSWORD} \
  --no-data \
  --routines \
  --triggers \
  ${DB_NAME} > schema_backup_$(date +%Y%m%d_%H%M%S).sql
```

#### 資料備份（僅資料）
```bash
# 僅備份資料
docker-compose exec mariadb mysqldump \
  -u root -p${DB_ROOT_PASSWORD} \
  --no-create-info \
  --single-transaction \
  ${DB_NAME} > data_backup_$(date +%Y%m%d_%H%M%S).sql
```

### 自動備份腳本

建立自動備份腳本 `scripts/backup-database.sh`：

```bash
#!/bin/bash

# 資料庫自動備份腳本
BACKUP_DIR="./backups/database"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/rosca_backup_$DATE.sql"

# 建立備份目錄
mkdir -p "$BACKUP_DIR"

# 執行備份
echo "開始資料庫備份..."
docker-compose exec -T mariadb mysqldump \
  -u root -p${DB_ROOT_PASSWORD} \
  --single-transaction \
  --routines \
  --triggers \
  ${DB_NAME} > "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    echo "備份成功: $BACKUP_FILE"
    
    # 壓縮備份檔案
    gzip "$BACKUP_FILE"
    echo "備份已壓縮: $BACKUP_FILE.gz"
    
    # 清理 7 天前的備份
    find "$BACKUP_DIR" -name "*.sql.gz" -mtime +7 -delete
    echo "已清理舊備份檔案"
else
    echo "備份失敗！"
    exit 1
fi
```

### 定時備份設定

#### Linux/macOS (使用 crontab)
```bash
# 編輯 crontab
crontab -e

# 添加每日凌晨 2 點備份
0 2 * * * /path/to/rosca-docker/scripts/backup-database.sh >> /var/log/rosca-backup.log 2>&1
```

#### Windows (使用工作排程器)
```batch
# 建立 Windows 批次檔 scripts/backup-database.bat
@echo off
set BACKUP_DIR=.\backups\database
set DATE=%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set BACKUP_FILE=%BACKUP_DIR%\rosca_backup_%DATE%.sql

mkdir "%BACKUP_DIR%" 2>nul

echo 開始資料庫備份...
docker-compose exec -T mariadb mysqldump -u root -p%DB_ROOT_PASSWORD% --single-transaction --routines --triggers %DB_NAME% > "%BACKUP_FILE%"

if %errorlevel% equ 0 (
    echo 備份成功: %BACKUP_FILE%
) else (
    echo 備份失敗！
    exit /b 1
)
```

## 檔案備份

### 上傳檔案備份

#### 手動備份
```bash
# 建立備份目錄
mkdir -p ./backups/files

# 備份所有上傳檔案
docker run --rm \
  -v rosca-docker_uploads:/source:ro \
  -v rosca-docker_backend-kycimages:/kyc:ro \
  -v rosca-docker_backend-depositimages:/deposit:ro \
  -v rosca-docker_backend-withdrawimages:/withdraw:ro \
  -v rosca-docker_backend-annimages:/ann:ro \
  -v $(pwd)/backups/files:/backup \
  alpine sh -c "
    cd /backup && \
    tar czf uploads_$(date +%Y%m%d_%H%M%S).tar.gz -C /source . && \
    tar czf kyc_$(date +%Y%m%d_%H%M%S).tar.gz -C /kyc . && \
    tar czf deposit_$(date +%Y%m%d_%H%M%S).tar.gz -C /deposit . && \
    tar czf withdraw_$(date +%Y%m%d_%H%M%S).tar.gz -C /withdraw . && \
    tar czf ann_$(date +%Y%m%d_%H%M%S).tar.gz -C /ann .
  "
```

#### 自動備份腳本
建立 `scripts/backup-files.sh`：

```bash
#!/bin/bash

BACKUP_DIR="./backups/files"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p "$BACKUP_DIR"

echo "開始檔案備份..."

# 備份各類檔案
volumes=("uploads" "backend-kycimages" "backend-depositimages" "backend-withdrawimages" "backend-annimages")
names=("uploads" "kyc" "deposit" "withdraw" "ann")

for i in "${!volumes[@]}"; do
    volume="rosca-docker_${volumes[$i]}"
    name="${names[$i]}"
    
    echo "備份 $name 檔案..."
    docker run --rm \
      -v "$volume":/source:ro \
      -v "$(pwd)/$BACKUP_DIR":/backup \
      alpine tar czf "/backup/${name}_${DATE}.tar.gz" -C /source .
    
    if [ $? -eq 0 ]; then
        echo "$name 備份成功"
    else
        echo "$name 備份失敗"
    fi
done

# 清理 30 天前的檔案備份
find "$BACKUP_DIR" -name "*.tar.gz" -mtime +30 -delete
echo "檔案備份完成"
```

### 配置檔案備份

```bash
# 備份配置檔案
mkdir -p ./backups/config
cp .env ./backups/config/.env_$(date +%Y%m%d_%H%M%S)
cp docker-compose.yml ./backups/config/docker-compose_$(date +%Y%m%d_%H%M%S).yml
cp -r database/init ./backups/config/database_init_$(date +%Y%m%d_%H%M%S)
```

## 完整系統備份

### 一鍵備份腳本

建立 `scripts/backup-full.sh`：

```bash
#!/bin/bash

# ROSCA 系統完整備份腳本
BACKUP_ROOT="./backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="$BACKUP_ROOT/full_$DATE"

echo "🗄️ 開始 ROSCA 系統完整備份..."
echo "備份目錄: $BACKUP_DIR"

# 建立備份目錄
mkdir -p "$BACKUP_DIR"/{database,files,config,images}

# 1. 備份資料庫
echo "📊 備份資料庫..."
docker-compose exec -T mariadb mysqldump \
  -u root -p${DB_ROOT_PASSWORD} \
  --single-transaction \
  --routines \
  --triggers \
  ${DB_NAME} > "$BACKUP_DIR/database/rosca_$DATE.sql"

# 2. 備份檔案
echo "📁 備份上傳檔案..."
volumes=("uploads" "backend-kycimages" "backend-depositimages" "backend-withdrawimages" "backend-annimages")
names=("uploads" "kyc" "deposit" "withdraw" "ann")

for i in "${!volumes[@]}"; do
    volume="rosca-docker_${volumes[$i]}"
    name="${names[$i]}"
    
    docker run --rm \
      -v "$volume":/source:ro \
      -v "$(pwd)/$BACKUP_DIR/files":/backup \
      alpine tar czf "/backup/${name}_${DATE}.tar.gz" -C /source .
done

# 3. 備份配置
echo "⚙️ 備份配置檔案..."
cp .env "$BACKUP_DIR/config/"
cp docker-compose.yml "$BACKUP_DIR/config/"
cp docker-compose.prod.yml "$BACKUP_DIR/config/"
cp -r database/init "$BACKUP_DIR/config/"

# 4. 備份 Docker 映像
echo "🐳 備份 Docker 映像..."
docker save rosca-docker_backend > "$BACKUP_DIR/images/backend_$DATE.tar"
docker save rosca-docker_frontend > "$BACKUP_DIR/images/frontend_$DATE.tar"
docker save rosca-docker_admin > "$BACKUP_DIR/images/admin_$DATE.tar"

# 5. 建立備份資訊檔案
echo "📋 建立備份資訊..."
cat > "$BACKUP_DIR/backup_info.txt" << EOF
ROSCA 系統備份資訊
==================
備份時間: $(date)
備份版本: $DATE
系統狀態: $(docker-compose ps --format "table {{.Name}}\t{{.Status}}")

包含內容:
- 資料庫: rosca_$DATE.sql
- 上傳檔案: files/*.tar.gz
- 配置檔案: config/*
- Docker 映像: images/*.tar

恢復指令:
./scripts/restore-full.sh $DATE
EOF

# 6. 壓縮整個備份
echo "🗜️ 壓縮備份..."
cd "$BACKUP_ROOT"
tar czf "full_backup_$DATE.tar.gz" "full_$DATE"
rm -rf "full_$DATE"

echo "✅ 完整備份完成: $BACKUP_ROOT/full_backup_$DATE.tar.gz"
```

## 資料恢復

### 資料庫恢復

#### 完整恢復
```bash
# 停止服務
docker-compose down

# 清除現有資料
docker volume rm rosca-docker_db-data

# 重新啟動資料庫
docker-compose up -d mariadb

# 等待資料庫就緒
sleep 30

# 恢復資料
docker-compose exec -T mariadb mysql -u root -p${DB_ROOT_PASSWORD} ${DB_NAME} < backup_file.sql

echo "資料庫恢復完成"
```

#### 選擇性恢復
```bash
# 恢復特定資料表
docker-compose exec -T mariadb mysql -u root -p${DB_ROOT_PASSWORD} -e "
USE ${DB_NAME};
DROP TABLE IF EXISTS Users;
"
docker-compose exec -T mariadb mysql -u root -p${DB_ROOT_PASSWORD} ${DB_NAME} < users_backup.sql
```

### 檔案恢復

```bash
# 恢復上傳檔案
docker run --rm \
  -v rosca-docker_uploads:/target \
  -v $(pwd)/backups/files:/backup:ro \
  alpine sh -c "cd /target && tar xzf /backup/uploads_20240315_120000.tar.gz"

# 恢復 KYC 圖片
docker run --rm \
  -v rosca-docker_backend-kycimages:/target \
  -v $(pwd)/backups/files:/backup:ro \
  alpine sh -c "cd /target && tar xzf /backup/kyc_20240315_120000.tar.gz"
```

### 完整系統恢復

建立 `scripts/restore-full.sh`：

```bash
#!/bin/bash

# ROSCA 系統完整恢復腳本
BACKUP_DATE="$1"
BACKUP_ROOT="./backups"
BACKUP_FILE="$BACKUP_ROOT/full_backup_$BACKUP_DATE.tar.gz"

if [ -z "$BACKUP_DATE" ]; then
    echo "使用方式: $0 <backup_date>"
    echo "範例: $0 20240315_120000"
    exit 1
fi

if [ ! -f "$BACKUP_FILE" ]; then
    echo "備份檔案不存在: $BACKUP_FILE"
    exit 1
fi

echo "🔄 開始 ROSCA 系統完整恢復..."
echo "備份檔案: $BACKUP_FILE"

# 確認恢復操作
read -p "⚠️ 這將覆蓋現有的所有資料，確定要繼續嗎？(y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ 恢復操作已取消"
    exit 0
fi

# 1. 停止所有服務
echo "🛑 停止所有服務..."
docker-compose down

# 2. 解壓備份檔案
echo "📦 解壓備份檔案..."
cd "$BACKUP_ROOT"
tar xzf "full_backup_$BACKUP_DATE.tar.gz"
RESTORE_DIR="$BACKUP_ROOT/full_$BACKUP_DATE"

# 3. 恢復配置檔案
echo "⚙️ 恢復配置檔案..."
cp "$RESTORE_DIR/config/.env" ../
cp "$RESTORE_DIR/config/docker-compose.yml" ../
cp -r "$RESTORE_DIR/config/init" ../database/

# 4. 清除現有 volumes
echo "🗑️ 清除現有資料..."
docker volume rm -f $(docker volume ls -q | grep rosca-docker)

# 5. 載入 Docker 映像
echo "🐳 載入 Docker 映像..."
docker load < "$RESTORE_DIR/images/backend_$BACKUP_DATE.tar"
docker load < "$RESTORE_DIR/images/frontend_$BACKUP_DATE.tar"
docker load < "$RESTORE_DIR/images/admin_$BACKUP_DATE.tar"

# 6. 啟動資料庫
echo "🗄️ 啟動資料庫..."
docker-compose up -d mariadb
sleep 60

# 7. 恢復資料庫
echo "📊 恢復資料庫..."
docker-compose exec -T mariadb mysql -u root -p${DB_ROOT_PASSWORD} ${DB_NAME} < "$RESTORE_DIR/database/rosca_$BACKUP_DATE.sql"

# 8. 啟動所有服務
echo "🚀 啟動所有服務..."
docker-compose up -d

# 9. 恢復檔案
echo "📁 恢復上傳檔案..."
sleep 30

files=("uploads" "kyc" "deposit" "withdraw" "ann")
volumes=("uploads" "backend-kycimages" "backend-depositimages" "backend-withdrawimages" "backend-annimages")

for i in "${!files[@]}"; do
    file="${files[$i]}"
    volume="rosca-docker_${volumes[$i]}"
    
    if [ -f "$RESTORE_DIR/files/${file}_${BACKUP_DATE}.tar.gz" ]; then
        docker run --rm \
          -v "$volume":/target \
          -v "$(pwd)/$RESTORE_DIR/files":/backup:ro \
          alpine sh -c "cd /target && tar xzf /backup/${file}_${BACKUP_DATE}.tar.gz"
    fi
done

# 10. 清理臨時檔案
rm -rf "$RESTORE_DIR"

echo "✅ 系統恢復完成！"
echo ""
echo "🌐 訪問地址:"
echo "   前台: http://localhost"
echo "   後台: http://localhost:8080"
echo ""
echo "請檢查系統狀態: docker-compose ps"
```

## 災難恢復計劃

### 恢復優先順序
1. **資料庫恢復** - 最高優先級
2. **核心服務恢復** - 後端 API
3. **前端服務恢復** - 前台和後台
4. **檔案恢復** - 上傳的圖片和文件

### 恢復時間目標 (RTO)
- **資料庫**: 30 分鐘內
- **核心功能**: 1 小時內
- **完整功能**: 2 小時內

### 恢復點目標 (RPO)
- **資料庫**: 最多丟失 1 小時資料
- **檔案**: 最多丟失 24 小時資料

### 緊急恢復步驟
1. 評估損壞程度
2. 準備恢復環境
3. 恢復最新可用備份
4. 驗證資料完整性
5. 重啟服務
6. 通知使用者

## 備份驗證

### 定期驗證腳本

建立 `scripts/verify-backup.sh`：

```bash
#!/bin/bash

# 備份驗證腳本
BACKUP_FILE="$1"

if [ -z "$BACKUP_FILE" ]; then
    echo "使用方式: $0 <backup_file.sql>"
    exit 1
fi

echo "🔍 驗證備份檔案: $BACKUP_FILE"

# 1. 檢查檔案完整性
if [ ! -f "$BACKUP_FILE" ]; then
    echo "❌ 備份檔案不存在"
    exit 1
fi

# 2. 檢查檔案大小
file_size=$(stat -f%z "$BACKUP_FILE" 2>/dev/null || stat -c%s "$BACKUP_FILE" 2>/dev/null)
if [ "$file_size" -lt 1000 ]; then
    echo "❌ 備份檔案過小，可能損壞"
    exit 1
fi

# 3. 檢查 SQL 語法
if ! grep -q "CREATE TABLE" "$BACKUP_FILE"; then
    echo "❌ 備份檔案不包含表結構"
    exit 1
fi

# 4. 檢查重要資料表
tables=("Users" "Deposits" "Withdraws")
for table in "${tables[@]}"; do
    if ! grep -q "CREATE TABLE.*$table" "$BACKUP_FILE"; then
        echo "⚠️ 警告: 未找到資料表 $table"
    else
        echo "✅ 資料表 $table 存在"
    fi
done

echo "✅ 備份驗證完成"
```

## 監控和告警

### 備份狀態監控
```bash
# 檢查最新備份時間
find ./backups -name "*.sql" -mtime -1 | wc -l

# 檢查備份檔案大小
ls -lh ./backups/database/ | tail -5
```

### 自動告警設定
```bash
# 備份失敗告警 (可整合到備份腳本中)
if [ $backup_status -ne 0 ]; then
    echo "備份失敗！" | mail -s "ROSCA 備份失敗告警" admin@example.com
fi
```

## 最佳實踐

### 備份策略
1. **3-2-1 原則**: 3 份備份，2 種媒體，1 份異地
2. **定期測試**: 每月測試恢復流程
3. **版本控制**: 保留多個版本的備份
4. **加密存儲**: 敏感資料加密備份

### 安全考量
1. 備份檔案權限控制
2. 傳輸過程加密
3. 存儲位置安全
4. 訪問日誌記錄

### 效能優化
1. 增量備份減少時間
2. 壓縮減少存儲空間
3. 並行備份提高效率
4. 離峰時間執行

更多詳細資訊請參考 [TROUBLESHOOTING.md](TROUBLESHOOTING.md) 和 [SERVICES.md](SERVICES.md)。