#!/bin/bash

# ROSCA 平安商會系統 Zeabur 存儲備份腳本
# 用於備份 Zeabur 平台上的持久化存儲資料

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# 存儲路徑配置
UPLOADS_PATH="/app/uploads"
KYC_IMAGES_PATH="/app/KycImages"
DEPOSIT_IMAGES_PATH="/app/DepositImages"
WITHDRAW_IMAGES_PATH="/app/WithdrawImages"
ANN_IMAGES_PATH="/app/AnnImagessss"
LOGS_PATH="/app/logs"

# 備份配置
BACKUP_BASE_PATH="/app/backups"
BACKUP_DATE=$(date +%Y%m%d-%H%M%S)
BACKUP_PATH="$BACKUP_BASE_PATH/$BACKUP_DATE"
RETENTION_DAYS=30
COMPRESSION_LEVEL=6

echo -e "${BLUE}=== ROSCA Zeabur 存儲備份系統 ===${NC}"
echo "備份時間: $(date)"
echo "備份路徑: $BACKUP_PATH"
echo ""

# 函數：建立備份目錄
create_backup_directory() {
    echo -e "${YELLOW}建立備份目錄...${NC}"
    
    if [ ! -d "$BACKUP_BASE_PATH" ]; then
        mkdir -p "$BACKUP_BASE_PATH"
        echo -e "${GREEN}✓ 建立備份基礎目錄: $BACKUP_BASE_PATH${NC}"
    fi
    
    mkdir -p "$BACKUP_PATH"
    echo -e "${GREEN}✓ 建立備份目錄: $BACKUP_PATH${NC}"
    echo ""
}

# 函數：備份單個目錄
backup_directory() {
    local source_path="$1"
    local backup_name="$2"
    local description="$3"
    
    echo -e "${YELLOW}備份 $description...${NC}"
    
    if [ ! -d "$source_path" ]; then
        echo -e "${RED}✗ 來源目錄不存在: $source_path${NC}"
        return 1
    fi
    
    local file_count=$(find "$source_path" -type f 2>/dev/null | wc -l)
    if [ "$file_count" -eq 0 ]; then
        echo -e "${BLUE}ℹ 目錄為空，跳過備份: $source_path${NC}"
        return 0
    fi
    
    local backup_file="$BACKUP_PATH/${backup_name}-${BACKUP_DATE}.tar.gz"
    
    # 建立備份檔案
    tar -czf "$backup_file" -C "$(dirname "$source_path")" "$(basename "$source_path")" 2>/dev/null
    
    if [ $? -eq 0 ]; then
        local backup_size=$(du -h "$backup_file" | cut -f1)
        echo -e "${GREEN}✓ 備份完成: $backup_name ($file_count 個檔案, $backup_size)${NC}"
        
        # 建立備份資訊檔案
        cat > "$backup_file.info" << EOF
{
  "backupName": "$backup_name",
  "description": "$description",
  "sourcePath": "$source_path",
  "backupFile": "$backup_file",
  "backupDate": "$BACKUP_DATE",
  "fileCount": $file_count,
  "backupSize": "$backup_size",
  "compressionLevel": $COMPRESSION_LEVEL,
  "status": "completed"
}
EOF
        
        return 0
    else
        echo -e "${RED}✗ 備份失敗: $backup_name${NC}"
        return 1
    fi
}

# 函數：執行完整備份
perform_full_backup() {
    echo -e "${PURPLE}🗂️  執行完整存儲備份${NC}"
    echo ""
    
    local backup_count=0
    local success_count=0
    local failed_count=0
    
    # 備份各個存儲目錄
    local directories=(
        "$UPLOADS_PATH:uploads:通用檔案上傳"
        "$KYC_IMAGES_PATH:kyc-images:KYC身份認證圖片"
        "$DEPOSIT_IMAGES_PATH:deposit-images:存款憑證圖片"
        "$WITHDRAW_IMAGES_PATH:withdraw-images:提款憑證圖片"
        "$ANN_IMAGES_PATH:ann-images:公告圖片"
        "$LOGS_PATH:logs:應用程式日誌"
    )
    
    for dir_info in "${directories[@]}"; do
        local path="${dir_info%%:*}"
        local name="${dir_info#*:}"
        name="${name%:*}"
        local desc="${dir_info##*:}"
        
        backup_count=$((backup_count + 1))
        
        if backup_directory "$path" "$name" "$desc"; then
            success_count=$((success_count + 1))
        else
            failed_count=$((failed_count + 1))
        fi
        
        echo ""
    done
    
    # 建立備份摘要
    create_backup_summary "$backup_count" "$success_count" "$failed_count"
    
    echo -e "${BLUE}📊 備份摘要:${NC}"
    echo "   總計: $backup_count 個目錄"
    echo "   成功: $success_count 個"
    echo "   失敗: $failed_count 個"
    echo ""
    
    if [ "$failed_count" -eq 0 ]; then
        echo -e "${GREEN}🎉 完整備份成功完成！${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠️  備份完成但有 $failed_count 個失敗${NC}"
        return 1
    fi
}

# 函數：建立備份摘要
create_backup_summary() {
    local total="$1"
    local success="$2"
    local failed="$3"
    
    local summary_file="$BACKUP_PATH/backup-summary.json"
    
    cat > "$summary_file" << EOF
{
  "backupId": "$BACKUP_DATE",
  "timestamp": "$(date -Iseconds)",
  "type": "full-backup",
  "status": "$([ "$failed" -eq 0 ] && echo "success" || echo "partial")",
  "statistics": {
    "totalDirectories": $total,
    "successfulBackups": $success,
    "failedBackups": $failed
  },
  "backupPath": "$BACKUP_PATH",
  "retentionDays": $RETENTION_DAYS,
  "compressionLevel": $COMPRESSION_LEVEL,
  "system": {
    "hostname": "$(hostname)",
    "diskUsage": "$(df -h "$BACKUP_BASE_PATH" | tail -1 | awk '{print $5}')",
    "availableSpace": "$(df -h "$BACKUP_BASE_PATH" | tail -1 | awk '{print $4}')"
  }
}
EOF
    
    echo -e "${GREEN}✓ 備份摘要已建立: $summary_file${NC}"
}

# 函數：清理舊備份
cleanup_old_backups() {
    echo -e "${YELLOW}清理舊備份檔案...${NC}"
    
    if [ ! -d "$BACKUP_BASE_PATH" ]; then
        echo -e "${BLUE}ℹ 備份目錄不存在，跳過清理${NC}"
        return 0
    fi
    
    local old_backups=$(find "$BACKUP_BASE_PATH" -type d -name "20*" -mtime +$RETENTION_DAYS 2>/dev/null)
    local cleanup_count=0
    
    if [ -n "$old_backups" ]; then
        echo "$old_backups" | while read backup_dir; do
            if [ -d "$backup_dir" ]; then
                local backup_name=$(basename "$backup_dir")
                local backup_size=$(du -sh "$backup_dir" 2>/dev/null | cut -f1)
                
                rm -rf "$backup_dir"
                echo -e "${GREEN}✓ 已刪除舊備份: $backup_name ($backup_size)${NC}"
                cleanup_count=$((cleanup_count + 1))
            fi
        done
        
        echo -e "${GREEN}✓ 清理完成，刪除了 $cleanup_count 個舊備份${NC}"
    else
        echo -e "${BLUE}ℹ 沒有找到需要清理的舊備份${NC}"
    fi
    
    echo ""
}

# 函數：驗證備份完整性
verify_backup() {
    local backup_path="$1"
    
    echo -e "${YELLOW}驗證備份完整性...${NC}"
    
    if [ ! -d "$backup_path" ]; then
        echo -e "${RED}✗ 備份目錄不存在: $backup_path${NC}"
        return 1
    fi
    
    local backup_files=$(find "$backup_path" -name "*.tar.gz" -type f)
    local verified_count=0
    local failed_count=0
    
    if [ -z "$backup_files" ]; then
        echo -e "${RED}✗ 沒有找到備份檔案${NC}"
        return 1
    fi
    
    echo "$backup_files" | while read backup_file; do
        local backup_name=$(basename "$backup_file" .tar.gz)
        
        # 測試 tar 檔案完整性
        if tar -tzf "$backup_file" >/dev/null 2>&1; then
            echo -e "${GREEN}✓ 備份檔案完整: $backup_name${NC}"
            verified_count=$((verified_count + 1))
        else
            echo -e "${RED}✗ 備份檔案損壞: $backup_name${NC}"
            failed_count=$((failed_count + 1))
        fi
    done
    
    echo ""
    
    if [ "$failed_count" -eq 0 ]; then
        echo -e "${GREEN}✓ 所有備份檔案驗證通過${NC}"
        return 0
    else
        echo -e "${RED}✗ $failed_count 個備份檔案驗證失敗${NC}"
        return 1
    fi
}

# 函數：列出備份
list_backups() {
    echo -e "${PURPLE}📋 備份清單${NC}"
    echo ""
    
    if [ ! -d "$BACKUP_BASE_PATH" ]; then
        echo -e "${BLUE}ℹ 備份目錄不存在${NC}"
        return 0
    fi
    
    local backup_dirs=$(find "$BACKUP_BASE_PATH" -type d -name "20*" | sort -r)
    
    if [ -z "$backup_dirs" ]; then
        echo -e "${BLUE}ℹ 沒有找到備份${NC}"
        return 0
    fi
    
    printf "%-20s %-15s %-10s %-8s\n" "備份日期" "狀態" "大小" "檔案數"
    printf "%-20s %-15s %-10s %-8s\n" "----" "----" "----" "----"
    
    echo "$backup_dirs" | while read backup_dir; do
        local backup_name=$(basename "$backup_dir")
        local backup_size=$(du -sh "$backup_dir" 2>/dev/null | cut -f1)
        local file_count=$(find "$backup_dir" -name "*.tar.gz" -type f | wc -l)
        
        # 檢查備份狀態
        local status="未知"
        local summary_file="$backup_dir/backup-summary.json"
        if [ -f "$summary_file" ]; then
            status=$(grep '"status"' "$summary_file" | cut -d'"' -f4)
        fi
        
        local status_color=""
        case "$status" in
            "success") status_color="$GREEN" ;;
            "partial") status_color="$YELLOW" ;;
            *) status_color="$RED" ;;
        esac
        
        printf "${status_color}%-20s %-15s %-10s %-8s${NC}\n" \
            "$backup_name" "$status" "$backup_size" "$file_count"
    done
    
    echo ""
}

# 函數：恢復備份
restore_backup() {
    local backup_id="$1"
    local target_path="$2"
    
    if [ -z "$backup_id" ]; then
        echo -e "${RED}錯誤: 請指定備份 ID${NC}"
        return 1
    fi
    
    local backup_dir="$BACKUP_BASE_PATH/$backup_id"
    
    if [ ! -d "$backup_dir" ]; then
        echo -e "${RED}錯誤: 備份不存在: $backup_id${NC}"
        return 1
    fi
    
    echo -e "${YELLOW}恢復備份: $backup_id${NC}"
    echo -e "${RED}警告: 此操作將覆蓋現有檔案！${NC}"
    
    read -p "確定要繼續嗎？(y/N): " confirm
    if [ "$confirm" != "y" ] && [ "$confirm" != "Y" ]; then
        echo -e "${BLUE}操作已取消${NC}"
        return 0
    fi
    
    local backup_files=$(find "$backup_dir" -name "*.tar.gz" -type f)
    local restored_count=0
    
    echo "$backup_files" | while read backup_file; do
        local backup_name=$(basename "$backup_file" .tar.gz | sed "s/-$backup_id//")
        
        echo -e "${YELLOW}恢復: $backup_name${NC}"
        
        if [ -n "$target_path" ]; then
            tar -xzf "$backup_file" -C "$target_path"
        else
            tar -xzf "$backup_file" -C /app
        fi
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}✓ 恢復成功: $backup_name${NC}"
            restored_count=$((restored_count + 1))
        else
            echo -e "${RED}✗ 恢復失敗: $backup_name${NC}"
        fi
    done
    
    echo ""
    echo -e "${GREEN}恢復完成，共恢復 $restored_count 個備份${NC}"
}

# 函數：顯示使用說明
show_usage() {
    echo "使用方法: $0 [命令] [選項]"
    echo ""
    echo "命令:"
    echo "  backup      執行完整備份 (預設)"
    echo "  list        列出所有備份"
    echo "  verify      驗證備份完整性"
    echo "  cleanup     清理舊備份"
    echo "  restore     恢復備份"
    echo "  help        顯示此說明"
    echo ""
    echo "選項:"
    echo "  --backup-id ID    指定備份 ID (用於 restore)"
    echo "  --target PATH     指定恢復目標路徑"
    echo "  --retention DAYS  設定保留天數 (預設: $RETENTION_DAYS)"
    echo ""
    echo "範例:"
    echo "  $0 backup                    # 執行完整備份"
    echo "  $0 list                      # 列出所有備份"
    echo "  $0 verify                    # 驗證最新備份"
    echo "  $0 restore --backup-id 20250115-120000  # 恢復指定備份"
    echo ""
}

# 主函數
main() {
    local command="${1:-backup}"
    local backup_id=""
    local target_path=""
    
    # 解析參數
    while [[ $# -gt 0 ]]; do
        case $1 in
            --backup-id)
                backup_id="$2"
                shift 2
                ;;
            --target)
                target_path="$2"
                shift 2
                ;;
            --retention)
                RETENTION_DAYS="$2"
                shift 2
                ;;
            *)
                shift
                ;;
        esac
    done
    
    case "$command" in
        "backup")
            create_backup_directory
            perform_full_backup
            cleanup_old_backups
            ;;
        "list")
            list_backups
            ;;
        "verify")
            if [ -n "$backup_id" ]; then
                verify_backup "$BACKUP_BASE_PATH/$backup_id"
            else
                # 驗證最新備份
                local latest_backup=$(find "$BACKUP_BASE_PATH" -type d -name "20*" | sort -r | head -1)
                if [ -n "$latest_backup" ]; then
                    verify_backup "$latest_backup"
                else
                    echo -e "${RED}沒有找到備份${NC}"
                fi
            fi
            ;;
        "cleanup")
            cleanup_old_backups
            ;;
        "restore")
            restore_backup "$backup_id" "$target_path"
            ;;
        "help"|"-h"|"--help")
            show_usage
            ;;
        *)
            echo -e "${RED}未知命令: $command${NC}"
            show_usage
            exit 1
            ;;
    esac
}

# 執行主函數
main "$@"