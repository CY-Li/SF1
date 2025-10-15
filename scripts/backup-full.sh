#!/bin/bash

# ROSCA 系統完整備份腳本
set -e

BACKUP_ROOT="./backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="$BACKUP_ROOT/full_$DATE"

echo "🗄️ 開始 ROSCA 系統完整備份..."
echo "備份時間: $(date)"
echo "備份目錄: $BACKUP_DIR"
echo ""

# 檢查前置條件
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker 未運行"
    exit 1
fi

if ! docker-compose ps | grep -q "Up"; then
    echo "❌ 系統未運行，請先啟動服務"
    exit 1
fi

# 載入環境變數
if [ -f .env ]; then
    source .env
else
    echo "❌ .env 檔案不存在"
    exit 1
fi

# 建立備份目錄結構
mkdir -p "$BACKUP_DIR"/{database,files,config,images,logs}

echo "📊 1/5 備份資料庫..."
# 備份資料庫
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
  ${DB_NAME} > "$BACKUP_DIR/database/rosca_$DATE.sql"

if [ $? -eq 0 ]; then
    echo "✅ 資料庫備份成功"
    gzip "$BACKUP_DIR/database/rosca_$DATE.sql"
else
    echo "❌ 資料庫備份失敗"
    exit 1
fi

echo "📁 2/5 備份上傳檔案..."
# 備份檔案
volumes=("uploads" "backend-kycimages" "backend-depositimages" "backend-withdrawimages" "backend-annimages")
names=("uploads" "kyc" "deposit" "withdraw" "ann")

for i in "${!volumes[@]}"; do
    volume="rosca-docker_${volumes[$i]}"
    name="${names[$i]}"
    
    if docker volume ls | grep -q "$volume"; then
        echo -n "  備份 $name: "
        if docker run --rm \
            -v "$volume":/source:ro \
            -v "$(pwd)/$BACKUP_DIR/files":/backup \
            alpine tar czf "/backup/${name}_${DATE}.tar.gz" -C /source . >/dev/null 2>&1; then
            echo "✅"
        else
            echo "❌"
        fi
    else
        echo "  跳過 $name (volume 不存在)"
    fi
done

echo "⚙️ 3/5 備份配置檔案..."
# 備份配置
cp .env "$BACKUP_DIR/config/" 2>/dev/null || echo "  警告: .env 複製失敗"
cp docker-compose.yml "$BACKUP_DIR/config/" 2>/dev/null || echo "  警告: docker-compose.yml 複製失敗"
cp docker-compose.prod.yml "$BACKUP_DIR/config/" 2>/dev/null || echo "  警告: docker-compose.prod.yml 複製失敗"
cp docker-compose.override.yml "$BACKUP_DIR/config/" 2>/dev/null || true
cp -r database/init "$BACKUP_DIR/config/" 2>/dev/null || echo "  警告: database/init 複製失敗"
cp -r scripts "$BACKUP_DIR/config/" 2>/dev/null || echo "  警告: scripts 複製失敗"
echo "✅ 配置檔案備份完成"

echo "🐳 4/5 備份 Docker 映像..."
# 備份 Docker 映像
images=("rosca-docker_backend" "rosca-docker_frontend" "rosca-docker_admin")
for image in "${images[@]}"; do
    if docker images | grep -q "$image"; then
        image_name=$(echo "$image" | sed 's/rosca-docker_//')"
        echo -n "  備份 $image_name 映像: "
        if docker save "$image" > "$BACKUP_DIR/images/${image_name}_${DATE}.tar" 2>/dev/null; then
            echo "✅"
        else
            echo "❌"
        fi
    else
        echo "  跳過 $image (映像不存在)"
    fi
done

echo "📋 5/5 建立備份資訊..."
# 建立備份資訊檔案
cat > "$BACKUP_DIR/backup_info.txt" << EOF
ROSCA 系統備份資訊
==================
備份時間: $(date)
備份版本: $DATE
系統版本: $(git rev-parse HEAD 2>/dev/null || echo "未知")

系統狀態:
$(docker-compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}")

資源使用:
$(docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}")

包含內容:
- 資料庫: database/rosca_$DATE.sql.gz
- 上傳檔案: files/*.tar.gz
- 配置檔案: config/*
- Docker 映像: images/*.tar
- 備份資訊: backup_info.txt

恢復指令:
./scripts/restore-full.sh $DATE

備份檔案清單:
$(find "$BACKUP_DIR" -type f -exec ls -lh {} \; | awk '{print $9 " (" $5 ")"}')
EOF

# 建立快速恢復腳本
cat > "$BACKUP_DIR/quick_restore.sh" << 'EOF'
#!/bin/bash
# 快速恢復腳本 (在備份目錄中執行)

echo "🔄 開始快速恢復..."

# 返回專案根目錄
cd ../../../

# 停止服務
echo "🛑 停止服務..."
docker-compose down

# 恢復配置
echo "⚙️ 恢復配置..."
cp backups/full_*/config/.env ./ 2>/dev/null || true
cp backups/full_*/config/docker-compose.yml ./ 2>/dev/null || true

# 清除 volumes
echo "🗑️ 清除現有資料..."
docker volume prune -f

# 恢復資料庫
echo "🗄️ 恢復資料庫..."
docker-compose up -d mariadb
sleep 60

# 載入資料庫備份
zcat backups/full_*/database/*.sql.gz | docker-compose exec -T mariadb mysql -u root -p${DB_ROOT_PASSWORD} ${DB_NAME}

# 啟動所有服務
echo "🚀 啟動服務..."
docker-compose up -d
sleep 30

# 恢復檔案
echo "📁 恢復檔案..."
for file in backups/full_*/files/*.tar.gz; do
    if [ -f "$file" ]; then
        name=$(basename "$file" | cut -d'_' -f1)
        case $name in
            "uploads") volume="rosca-docker_uploads" ;;
            "kyc") volume="rosca-docker_backend-kycimages" ;;
            "deposit") volume="rosca-docker_backend-depositimages" ;;
            "withdraw") volume="rosca-docker_backend-withdrawimages" ;;
            "ann") volume="rosca-docker_backend-annimages" ;;
            *) continue ;;
        esac
        
        docker run --rm \
            -v "$volume":/target \
            -v "$(pwd)/$(dirname "$file")":/backup:ro \
            alpine sh -c "cd /target && tar xzf /backup/$(basename "$file")"
    fi
done

echo "✅ 快速恢復完成！"
echo "🌐 訪問: http://localhost (前台) | http://localhost:8080 (後台)"
EOF

chmod +x "$BACKUP_DIR/quick_restore.sh"

echo "🗜️ 壓縮備份..."
# 壓縮整個備份
cd "$BACKUP_ROOT"
tar czf "full_backup_$DATE.tar.gz" "full_$DATE"

# 計算備份大小
backup_size=$(stat -f%z "full_backup_$DATE.tar.gz" 2>/dev/null || stat -c%s "full_backup_$DATE.tar.gz" 2>/dev/null)
backup_size_mb=$(( backup_size / 1024 / 1024 ))

echo ""
echo "✅ 完整備份完成！"
echo "📦 備份檔案: $BACKUP_ROOT/full_backup_$DATE.tar.gz"
echo "📏 備份大小: ${backup_size_mb} MB"
echo ""
echo "🔄 恢復指令:"
echo "   ./scripts/restore-full.sh $DATE"
echo ""
echo "📋 備份內容驗證:"
tar tzf "full_backup_$DATE.tar.gz" | head -10
echo "   ... (更多檔案)"

# 清理臨時目錄
rm -rf "full_$DATE"

# 清理 30 天前的完整備份
echo "🧹 清理舊備份..."
find "$BACKUP_ROOT" -name "full_backup_*.tar.gz" -mtime +30 -delete

echo "🎉 備份流程完成！"