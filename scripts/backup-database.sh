#!/bin/bash

# ROSCA 資料庫自動備份腳本
set -e

BACKUP_DIR="./backups/database"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/rosca_backup_$DATE.sql"

echo "🗄️ 開始 ROSCA 資料庫備份..."

# 檢查 Docker Compose 是否運行
if ! docker-compose ps | grep -q "rosca-mariadb.*Up"; then
    echo "❌ MariaDB 容器未運行，請先啟動服務"
    exit 1
fi

# 建立備份目錄
mkdir -p "$BACKUP_DIR"

# 載入環境變數
if [ -f .env ]; then
    source .env
else
    echo "❌ .env 檔案不存在"
    exit 1
fi

# 執行備份
echo "📊 備份資料庫到: $BACKUP_FILE"
docker-compose exec -T mariadb mysqldump \
  -u root -p${DB_ROOT_PASSWORD} \
  --single-transaction \
  --routines \
  --triggers \
  --add-drop-table \
  --create-options \
  --disable-keys \
  --extended-insert \
  --quick \
  --lock-tables=false \
  ${DB_NAME} > "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    echo "✅ 備份成功: $BACKUP_FILE"
    
    # 檢查備份檔案大小
    file_size=$(stat -f%z "$BACKUP_FILE" 2>/dev/null || stat -c%s "$BACKUP_FILE" 2>/dev/null)
    echo "📏 備份檔案大小: $(( file_size / 1024 )) KB"
    
    # 壓縮備份檔案
    echo "🗜️ 壓縮備份檔案..."
    gzip "$BACKUP_FILE"
    echo "✅ 備份已壓縮: $BACKUP_FILE.gz"
    
    # 清理 7 天前的備份
    echo "🧹 清理舊備份檔案..."
    find "$BACKUP_DIR" -name "*.sql.gz" -mtime +7 -delete
    
    # 顯示當前備份檔案
    echo "📋 當前備份檔案:"
    ls -lh "$BACKUP_DIR"/*.sql.gz 2>/dev/null | tail -5 || echo "無備份檔案"
    
    echo "🎉 資料庫備份完成！"
else
    echo "❌ 備份失敗！"
    rm -f "$BACKUP_FILE"
    exit 1
fi