#!/bin/bash

# ROSCA 檔案備份腳本
set -e

BACKUP_DIR="./backups/files"
DATE=$(date +%Y%m%d_%H%M%S)

echo "📁 開始 ROSCA 檔案備份..."

# 建立備份目錄
mkdir -p "$BACKUP_DIR"

# 檢查 Docker 是否運行
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker 未運行"
    exit 1
fi

echo "📦 備份上傳檔案..."

# 定義要備份的 volumes 和對應名稱
volumes=("uploads" "backend-kycimages" "backend-depositimages" "backend-withdrawimages" "backend-annimages")
names=("uploads" "kyc" "deposit" "withdraw" "ann")
success_count=0
total_count=${#volumes[@]}

for i in "${!volumes[@]}"; do
    volume="rosca-docker_${volumes[$i]}"
    name="${names[$i]}"
    backup_file="$BACKUP_DIR/${name}_${DATE}.tar.gz"
    
    echo -n "備份 $name 檔案: "
    
    # 檢查 volume 是否存在
    if ! docker volume ls | grep -q "$volume"; then
        echo "⚠️ Volume $volume 不存在，跳過"
        continue
    fi
    
    # 執行備份
    if docker run --rm \
        -v "$volume":/source:ro \
        -v "$(pwd)/$BACKUP_DIR":/backup \
        alpine tar czf "/backup/${name}_${DATE}.tar.gz" -C /source . >/dev/null 2>&1; then
        
        # 檢查備份檔案大小
        if [ -f "$backup_file" ]; then
            file_size=$(stat -f%z "$backup_file" 2>/dev/null || stat -c%s "$backup_file" 2>/dev/null)
            echo "✅ 成功 ($(( file_size / 1024 )) KB)"
            ((success_count++))
        else
            echo "❌ 失敗 (檔案未建立)"
        fi
    else
        echo "❌ 失敗 (備份錯誤)"
    fi
done

echo ""
echo "📊 備份統計:"
echo "   成功: $success_count/$total_count"
echo "   失敗: $(( total_count - success_count ))/$total_count"

# 清理 30 天前的檔案備份
echo "🧹 清理舊備份檔案..."
deleted_count=$(find "$BACKUP_DIR" -name "*.tar.gz" -mtime +30 -delete -print | wc -l)
echo "   已刪除 $deleted_count 個舊備份檔案"

# 顯示當前備份檔案
echo "📋 最新備份檔案:"
ls -lht "$BACKUP_DIR"/*.tar.gz 2>/dev/null | head -10 || echo "   無備份檔案"

if [ $success_count -eq $total_count ]; then
    echo "🎉 檔案備份完成！"
    exit 0
else
    echo "⚠️ 部分檔案備份失敗"
    exit 1
fi