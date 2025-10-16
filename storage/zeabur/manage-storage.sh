#!/bin/bash

# ROSCA 平安商會系統存儲管理腳本 (Zeabur 版本)
# 用於管理持久化存儲卷和檔案操作

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 存儲路徑配置
UPLOADS_PATH="/app/uploads"
KYC_IMAGES_PATH="/app/KycImages"
DEPOSIT_IMAGES_PATH="/app/DepositImages"
WITHDRAW_IMAGES_PATH="/app/WithdrawImages"
ANN_IMAGES_PATH="/app/AnnImagessss"
LOGS_PATH="/app/logs"

# 存儲大小限制 (MB)
UPLOADS_LIMIT=5120      # 5GB
KYC_IMAGES_LIMIT=2048   # 2GB
DEPOSIT_IMAGES_LIMIT=2048 # 2GB
WITHDRAW_IMAGES_LIMIT=2048 # 2GB
ANN_IMAGES_LIMIT=2048   # 2GB
LOGS_LIMIT=1024         # 1GB

echo -e "${BLUE}=== ROSCA 存儲管理工具 (Zeabur 版本) ===${NC}"

# 函數：顯示使用說明
show_usage() {
    echo "使用方法: $0 [命令] [選項]"
    echo ""
    echo "命令:"
    echo "  init          初始化存儲目錄"
    echo "  status        顯示存儲狀態"
    echo "  cleanup       清理過期檔案"
    echo "  backup        備份檔案"
    echo "  restore       恢復檔案"
    echo "  check         檢查存儲健康狀態"
    echo "  optimize      優化存儲空間"
    echo "  monitor       監控存儲使用情況"
    echo ""
    echo "選項:"
    echo "  -v, --verbose    詳細輸出"
    echo "  -h, --help       顯示此說明"
}

# 函數：初始化存儲目錄
init_storage() {
    echo -e "${YELLOW}初始化存儲目錄...${NC}"
    
    local directories=(
        "$UPLOADS_PATH"
        "$KYC_IMAGES_PATH"
        "$DEPOSIT_IMAGES_PATH"
        "$WITHDRAW_IMAGES_PATH"
        "$ANN_IMAGES_PATH"
        "$LOGS_PATH"
    )
    
    for dir in "${directories[@]}"; do
        if [ ! -d "$dir" ]; then
            mkdir -p "$dir"
            echo -e "${GREEN}✓ 建立目錄: $dir${NC}"
        else
            echo -e "${BLUE}ℹ 目錄已存在: $dir${NC}"
        fi
        
        # 設定適當的權限
        chmod 755 "$dir"
        
        # 建立 .gitkeep 檔案以確保目錄在版本控制中被保留
        touch "$dir/.gitkeep"
    done
    
    echo -e "${GREEN}✓ 存儲目錄初始化完成${NC}"
}

# 函數：取得目錄大小 (MB)
get_directory_size() {
    local dir="$1"
    if [ -d "$dir" ]; then
        du -sm "$dir" 2>/dev/null | cut -f1 || echo "0"
    else
        echo "0"
    fi
}

# 函數：取得檔案數量
get_file_count() {
    local dir="$1"
    if [ -d "$dir" ]; then
        find "$dir" -type f 2>/dev/null | wc -l || echo "0"
    else
        echo "0"
    fi
}

# 函數：顯示存儲狀態
show_storage_status() {
    echo -e "${YELLOW}檢查存儲狀態...${NC}"
    echo ""
    
    printf "%-25s %-10s %-10s %-10s %-10s\n" "目錄" "大小(MB)" "限制(MB)" "檔案數" "使用率"
    printf "%-25s %-10s %-10s %-10s %-10s\n" "----" "------" "------" "----" "----"
    
    local directories=(
        "$UPLOADS_PATH:$UPLOADS_LIMIT"
        "$KYC_IMAGES_PATH:$KYC_IMAGES_LIMIT"
        "$DEPOSIT_IMAGES_PATH:$DEPOSIT_IMAGES_LIMIT"
        "$WITHDRAW_IMAGES_PATH:$WITHDRAW_IMAGES_LIMIT"
        "$ANN_IMAGES_PATH:$ANN_IMAGES_LIMIT"
        "$LOGS_PATH:$LOGS_LIMIT"
    )
    
    for dir_info in "${directories[@]}"; do
        local dir="${dir_info%:*}"
        local limit="${dir_info#*:}"
        local size=$(get_directory_size "$dir")
        local count=$(get_file_count "$dir")
        local usage_percent=$((size * 100 / limit))
        
        local dir_name=$(basename "$dir")
        
        if [ $usage_percent -gt 90 ]; then
            printf "${RED}%-25s %-10s %-10s %-10s %-10s%%${NC}\n" "$dir_name" "$size" "$limit" "$count" "$usage_percent"
        elif [ $usage_percent -gt 70 ]; then
            printf "${YELLOW}%-25s %-10s %-10s %-10s %-10s%%${NC}\n" "$dir_name" "$size" "$limit" "$count" "$usage_percent"
        else
            printf "${GREEN}%-25s %-10s %-10s %-10s %-10s%%${NC}\n" "$dir_name" "$size" "$limit" "$count" "$usage_percent"
        fi
    done
    
    echo ""
    
    # 顯示總使用量
    local total_size=0
    local total_limit=0
    
    for dir_info in "${directories[@]}"; do
        local dir="${dir_info%:*}"
        local limit="${dir_info#*:}"
        local size=$(get_directory_size "$dir")
        total_size=$((total_size + size))
        total_limit=$((total_limit + limit))
    done
    
    local total_usage_percent=$((total_size * 100 / total_limit))
    echo -e "${BLUE}總使用量: ${total_size}MB / ${total_limit}MB (${total_usage_percent}%)${NC}"
}

# 函數：清理過期檔案
cleanup_expired_files() {
    echo -e "${YELLOW}清理過期檔案...${NC}"
    
    local cleaned_count=0
    local cleaned_size=0
    
    # 清理超過 30 天的日誌檔案
    if [ -d "$LOGS_PATH" ]; then
        local old_logs=$(find "$LOGS_PATH" -name "*.log" -type f -mtime +30 2>/dev/null | wc -l)
        if [ "$old_logs" -gt 0 ]; then
            find "$LOGS_PATH" -name "*.log" -type f -mtime +30 -delete 2>/dev/null
            echo -e "${GREEN}✓ 清理了 $old_logs 個過期日誌檔案${NC}"
            cleaned_count=$((cleaned_count + old_logs))
        fi
    fi
    
    # 清理超過 365 天的一般上傳檔案
    if [ -d "$UPLOADS_PATH" ]; then
        local old_uploads=$(find "$UPLOADS_PATH" -type f -mtime +365 2>/dev/null | wc -l)
        if [ "$old_uploads" -gt 0 ]; then
            find "$UPLOADS_PATH" -type f -mtime +365 -delete 2>/dev/null
            echo -e "${GREEN}✓ 清理了 $old_uploads 個過期上傳檔案${NC}"
            cleaned_count=$((cleaned_count + old_uploads))
        fi
    fi
    
    # 清理超過 3 年的存款/提款憑證
    for path in "$DEPOSIT_IMAGES_PATH" "$WITHDRAW_IMAGES_PATH"; do
        if [ -d "$path" ]; then
            local old_files=$(find "$path" -type f -mtime +1095 2>/dev/null | wc -l)
            if [ "$old_files" -gt 0 ]; then
                find "$path" -type f -mtime +1095 -delete 2>/dev/null
                echo -e "${GREEN}✓ 清理了 $old_files 個過期憑證檔案 ($(basename "$path"))${NC}"
                cleaned_count=$((cleaned_count + old_files))
            fi
        fi
    done
    
    # 清理超過 2 年的公告圖片
    if [ -d "$ANN_IMAGES_PATH" ]; then
        local old_announcements=$(find "$ANN_IMAGES_PATH" -type f -mtime +730 2>/dev/null | wc -l)
        if [ "$old_announcements" -gt 0 ]; then
            find "$ANN_IMAGES_PATH" -type f -mtime +730 -delete 2>/dev/null
            echo -e "${GREEN}✓ 清理了 $old_announcements 個過期公告圖片${NC}"
            cleaned_count=$((cleaned_count + old_announcements))
        fi
    fi
    
    if [ "$cleaned_count" -eq 0 ]; then
        echo -e "${BLUE}ℹ 沒有找到需要清理的過期檔案${NC}"
    else
        echo -e "${GREEN}✓ 總共清理了 $cleaned_count 個檔案${NC}"
    fi
}

# 函數：檢查存儲健康狀態
check_storage_health() {
    echo -e "${YELLOW}檢查存儲健康狀態...${NC}"
    
    local issues=0
    
    # 檢查目錄是否存在且可寫
    local directories=(
        "$UPLOADS_PATH"
        "$KYC_IMAGES_PATH"
        "$DEPOSIT_IMAGES_PATH"
        "$WITHDRAW_IMAGES_PATH"
        "$ANN_IMAGES_PATH"
        "$LOGS_PATH"
    )
    
    for dir in "${directories[@]}"; do
        if [ ! -d "$dir" ]; then
            echo -e "${RED}✗ 目錄不存在: $dir${NC}"
            issues=$((issues + 1))
        elif [ ! -w "$dir" ]; then
            echo -e "${RED}✗ 目錄不可寫: $dir${NC}"
            issues=$((issues + 1))
        else
            echo -e "${GREEN}✓ 目錄正常: $(basename "$dir")${NC}"
        fi
    done
    
    # 檢查磁碟空間使用率
    local disk_usage=$(df "$UPLOADS_PATH" | tail -1 | awk '{print $5}' | sed 's/%//')
    if [ "$disk_usage" -gt 90 ]; then
        echo -e "${RED}✗ 磁碟空間不足: ${disk_usage}%${NC}"
        issues=$((issues + 1))
    elif [ "$disk_usage" -gt 80 ]; then
        echo -e "${YELLOW}⚠ 磁碟空間警告: ${disk_usage}%${NC}"
    else
        echo -e "${GREEN}✓ 磁碟空間正常: ${disk_usage}%${NC}"
    fi
    
    # 檢查檔案權限
    for dir in "${directories[@]}"; do
        if [ -d "$dir" ]; then
            local perm=$(stat -c "%a" "$dir" 2>/dev/null || echo "000")
            if [ "$perm" != "755" ]; then
                echo -e "${YELLOW}⚠ 權限異常: $dir ($perm)${NC}"
            fi
        fi
    done
    
    echo ""
    if [ "$issues" -eq 0 ]; then
        echo -e "${GREEN}✓ 存儲健康狀態良好${NC}"
        return 0
    else
        echo -e "${RED}✗ 發現 $issues 個問題${NC}"
        return 1
    fi
}

# 函數：優化存儲空間
optimize_storage() {
    echo -e "${YELLOW}優化存儲空間...${NC}"
    
    # 壓縮舊日誌檔案
    if [ -d "$LOGS_PATH" ]; then
        find "$LOGS_PATH" -name "*.log" -type f -mtime +7 ! -name "*.gz" -exec gzip {} \; 2>/dev/null
        local compressed=$(find "$LOGS_PATH" -name "*.gz" -type f -mtime -1 2>/dev/null | wc -l)
        if [ "$compressed" -gt 0 ]; then
            echo -e "${GREEN}✓ 壓縮了 $compressed 個日誌檔案${NC}"
        fi
    fi
    
    # 移除空目錄
    local empty_dirs=0
    for dir in "${directories[@]}"; do
        if [ -d "$dir" ]; then
            local removed=$(find "$dir" -type d -empty -delete 2>/dev/null | wc -l)
            empty_dirs=$((empty_dirs + removed))
        fi
    done
    
    if [ "$empty_dirs" -gt 0 ]; then
        echo -e "${GREEN}✓ 移除了 $empty_dirs 個空目錄${NC}"
    fi
    
    echo -e "${GREEN}✓ 存儲空間優化完成${NC}"
}

# 函數：監控存儲使用情況
monitor_storage() {
    echo -e "${YELLOW}監控存儲使用情況...${NC}"
    
    while true; do
        clear
        echo -e "${BLUE}=== ROSCA 存儲監控 (按 Ctrl+C 退出) ===${NC}"
        echo "更新時間: $(date)"
        echo ""
        
        show_storage_status
        
        echo ""
        echo -e "${BLUE}磁碟使用情況:${NC}"
        df -h "$UPLOADS_PATH" | tail -1
        
        sleep 30
    done
}

# 主函數
main() {
    case "${1:-status}" in
        "init")
            init_storage
            ;;
        "status")
            show_storage_status
            ;;
        "cleanup")
            cleanup_expired_files
            ;;
        "check")
            check_storage_health
            ;;
        "optimize")
            optimize_storage
            ;;
        "monitor")
            monitor_storage
            ;;
        "help"|"-h"|"--help")
            show_usage
            ;;
        *)
            echo -e "${RED}未知命令: $1${NC}"
            show_usage
            exit 1
            ;;
    esac
}

# 執行主函數
main "$@"