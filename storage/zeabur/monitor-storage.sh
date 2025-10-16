#!/bin/bash

# ROSCA 平安商會系統 Zeabur 存儲監控腳本
# 用於監控 Zeabur 平台上的存儲使用情況和健康狀態

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 存儲路徑配置
UPLOADS_PATH="/app/uploads"
KYC_IMAGES_PATH="/app/KycImages"
DEPOSIT_IMAGES_PATH="/app/DepositImages"
WITHDRAW_IMAGES_PATH="/app/WithdrawImages"
ANN_IMAGES_PATH="/app/AnnImagessss"
LOGS_PATH="/app/logs"

# 存儲限制 (MB)
UPLOADS_LIMIT=5120
KYC_IMAGES_LIMIT=2048
DEPOSIT_IMAGES_LIMIT=2048
WITHDRAW_IMAGES_LIMIT=2048
ANN_IMAGES_LIMIT=2048
LOGS_LIMIT=1024

# 監控配置
ALERT_THRESHOLD=85
WARNING_THRESHOLD=70
CRITICAL_THRESHOLD=95

echo -e "${BLUE}=== ROSCA Zeabur 存儲監控系統 ===${NC}"
echo "監控時間: $(date)"
echo ""

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

# 函數：取得最大檔案大小 (MB)
get_largest_file_size() {
    local dir="$1"
    if [ -d "$dir" ]; then
        find "$dir" -type f -exec du -m {} + 2>/dev/null | sort -nr | head -1 | cut -f1 || echo "0"
    else
        echo "0"
    fi
}

# 函數：取得最近修改時間
get_last_modified() {
    local dir="$1"
    if [ -d "$dir" ]; then
        find "$dir" -type f -printf '%T@ %p\n' 2>/dev/null | sort -nr | head -1 | cut -d' ' -f2- | xargs stat -c %y 2>/dev/null | cut -d'.' -f1 || echo "無檔案"
    else
        echo "目錄不存在"
    fi
}

# 函數：顯示存儲狀態
show_storage_status() {
    echo -e "${CYAN}📊 存儲使用狀態${NC}"
    echo ""
    
    printf "%-20s %-8s %-8s %-8s %-8s %-8s %-12s\n" "存儲卷" "使用" "限制" "檔案數" "最大檔案" "使用率" "狀態"
    printf "%-20s %-8s %-8s %-8s %-8s %-8s %-12s\n" "----" "----" "----" "----" "----" "----" "----"
    
    local directories=(
        "uploads:$UPLOADS_PATH:$UPLOADS_LIMIT"
        "kyc-images:$KYC_IMAGES_PATH:$KYC_IMAGES_LIMIT"
        "deposit-images:$DEPOSIT_IMAGES_PATH:$DEPOSIT_IMAGES_LIMIT"
        "withdraw-images:$WITHDRAW_IMAGES_PATH:$WITHDRAW_IMAGES_LIMIT"
        "ann-images:$ANN_IMAGES_PATH:$ANN_IMAGES_LIMIT"
        "logs:$LOGS_PATH:$LOGS_LIMIT"
    )
    
    local total_used=0
    local total_limit=0
    local critical_volumes=0
    local warning_volumes=0
    
    for dir_info in "${directories[@]}"; do
        local name="${dir_info%%:*}"
        local path="${dir_info#*:}"
        path="${path%:*}"
        local limit="${dir_info##*:}"
        
        local size=$(get_directory_size "$path")
        local count=$(get_file_count "$path")
        local largest=$(get_largest_file_size "$path")
        local usage_percent=$((size * 100 / limit))
        
        total_used=$((total_used + size))
        total_limit=$((total_limit + limit))
        
        local status_color=""
        local status_text=""
        
        if [ $usage_percent -ge $CRITICAL_THRESHOLD ]; then
            status_color="$RED"
            status_text="🔴 危險"
            critical_volumes=$((critical_volumes + 1))
        elif [ $usage_percent -ge $ALERT_THRESHOLD ]; then
            status_color="$YELLOW"
            status_text="🟡 警告"
            warning_volumes=$((warning_volumes + 1))
        elif [ $usage_percent -ge $WARNING_THRESHOLD ]; then
            status_color="$YELLOW"
            status_text="🟠 注意"
        else
            status_color="$GREEN"
            status_text="🟢 正常"
        fi
        
        printf "${status_color}%-20s %-8s %-8s %-8s %-8s %-8s%% %-12s${NC}\n" \
            "$name" "${size}MB" "${limit}MB" "$count" "${largest}MB" "$usage_percent" "$status_text"
    done
    
    echo ""
    
    # 顯示總計
    local total_usage_percent=$((total_used * 100 / total_limit))
    echo -e "${BLUE}📈 總使用量: ${total_used}MB / ${total_limit}MB (${total_usage_percent}%)${NC}"
    
    # 顯示警告統計
    if [ $critical_volumes -gt 0 ]; then
        echo -e "${RED}⚠️  危險: $critical_volumes 個存儲卷使用率超過 ${CRITICAL_THRESHOLD}%${NC}"
    fi
    
    if [ $warning_volumes -gt 0 ]; then
        echo -e "${YELLOW}⚠️  警告: $warning_volumes 個存儲卷使用率超過 ${ALERT_THRESHOLD}%${NC}"
    fi
    
    if [ $critical_volumes -eq 0 ] && [ $warning_volumes -eq 0 ]; then
        echo -e "${GREEN}✅ 所有存儲卷狀態正常${NC}"
    fi
    
    echo ""
}

# 函數：顯示詳細資訊
show_detailed_info() {
    echo -e "${CYAN}📋 詳細存儲資訊${NC}"
    echo ""
    
    local directories=(
        "通用上傳:$UPLOADS_PATH"
        "KYC圖片:$KYC_IMAGES_PATH"
        "存款憑證:$DEPOSIT_IMAGES_PATH"
        "提款憑證:$WITHDRAW_IMAGES_PATH"
        "公告圖片:$ANN_IMAGES_PATH"
        "應用日誌:$LOGS_PATH"
    )
    
    for dir_info in "${directories[@]}"; do
        local name="${dir_info%:*}"
        local path="${dir_info#*:}"
        
        echo -e "${PURPLE}📁 $name ($path)${NC}"
        
        if [ -d "$path" ]; then
            local size=$(get_directory_size "$path")
            local count=$(get_file_count "$path")
            local last_modified=$(get_last_modified "$path")
            
            echo "   大小: ${size}MB"
            echo "   檔案數: $count"
            echo "   最後修改: $last_modified"
            
            # 顯示最近的檔案
            echo "   最近檔案:"
            find "$path" -type f -printf '%T@ %p\n' 2>/dev/null | sort -nr | head -3 | while read timestamp filepath; do
                local filename=$(basename "$filepath")
                local filesize=$(du -h "$filepath" 2>/dev/null | cut -f1)
                local filedate=$(date -d "@${timestamp%.*}" '+%Y-%m-%d %H:%M' 2>/dev/null || echo "未知")
                echo "     - $filename ($filesize, $filedate)"
            done
        else
            echo -e "   ${RED}目錄不存在${NC}"
        fi
        
        echo ""
    done
}

# 函數：顯示系統資訊
show_system_info() {
    echo -e "${CYAN}🖥️  系統資訊${NC}"
    echo ""
    
    # 磁碟使用情況
    echo -e "${BLUE}磁碟使用情況:${NC}"
    df -h "$UPLOADS_PATH" | tail -1 | while read filesystem size used avail use_percent mount; do
        echo "   檔案系統: $filesystem"
        echo "   總大小: $size"
        echo "   已使用: $used"
        echo "   可用: $avail"
        echo "   使用率: $use_percent"
        echo "   掛載點: $mount"
    done
    
    echo ""
    
    # 記憶體使用情況
    echo -e "${BLUE}記憶體使用情況:${NC}"
    free -h | grep -E "Mem|Swap" | while read line; do
        echo "   $line"
    done
    
    echo ""
    
    # 系統負載
    echo -e "${BLUE}系統負載:${NC}"
    uptime | sed 's/^/   /'
    
    echo ""
}

# 函數：檢查存儲健康狀態
check_storage_health() {
    echo -e "${CYAN}🏥 存儲健康檢查${NC}"
    echo ""
    
    local issues=0
    local warnings=0
    
    # 檢查目錄存在性和權限
    local directories=(
        "$UPLOADS_PATH"
        "$KYC_IMAGES_PATH"
        "$DEPOSIT_IMAGES_PATH"
        "$WITHDRAW_IMAGES_PATH"
        "$ANN_IMAGES_PATH"
        "$LOGS_PATH"
    )
    
    for dir in "${directories[@]}"; do
        local dir_name=$(basename "$dir")
        
        if [ ! -d "$dir" ]; then
            echo -e "${RED}✗ 目錄不存在: $dir_name${NC}"
            issues=$((issues + 1))
        elif [ ! -w "$dir" ]; then
            echo -e "${RED}✗ 目錄不可寫: $dir_name${NC}"
            issues=$((issues + 1))
        elif [ ! -r "$dir" ]; then
            echo -e "${RED}✗ 目錄不可讀: $dir_name${NC}"
            issues=$((issues + 1))
        else
            echo -e "${GREEN}✓ 目錄正常: $dir_name${NC}"
        fi
    done
    
    # 檢查磁碟空間
    local disk_usage=$(df "$UPLOADS_PATH" | tail -1 | awk '{print $5}' | sed 's/%//')
    if [ "$disk_usage" -gt $CRITICAL_THRESHOLD ]; then
        echo -e "${RED}✗ 磁碟空間危險: ${disk_usage}%${NC}"
        issues=$((issues + 1))
    elif [ "$disk_usage" -gt $ALERT_THRESHOLD ]; then
        echo -e "${YELLOW}⚠ 磁碟空間警告: ${disk_usage}%${NC}"
        warnings=$((warnings + 1))
    else
        echo -e "${GREEN}✓ 磁碟空間正常: ${disk_usage}%${NC}"
    fi
    
    # 檢查 inode 使用情況
    local inode_usage=$(df -i "$UPLOADS_PATH" | tail -1 | awk '{print $5}' | sed 's/%//')
    if [ "$inode_usage" -gt $ALERT_THRESHOLD ]; then
        echo -e "${YELLOW}⚠ inode 使用率高: ${inode_usage}%${NC}"
        warnings=$((warnings + 1))
    else
        echo -e "${GREEN}✓ inode 使用正常: ${inode_usage}%${NC}"
    fi
    
    echo ""
    
    # 總結
    if [ "$issues" -eq 0 ] && [ "$warnings" -eq 0 ]; then
        echo -e "${GREEN}🎉 存儲健康狀態良好${NC}"
        return 0
    elif [ "$issues" -eq 0 ]; then
        echo -e "${YELLOW}⚠️  存儲狀態: $warnings 個警告${NC}"
        return 1
    else
        echo -e "${RED}❌ 存儲狀態: $issues 個錯誤, $warnings 個警告${NC}"
        return 2
    fi
}

# 函數：生成監控報告
generate_report() {
    local report_file="/app/logs/storage-report-$(date +%Y%m%d-%H%M%S).json"
    
    echo -e "${YELLOW}生成監控報告...${NC}"
    
    # 收集資料
    local total_used=0
    local total_limit=0
    
    cat > "$report_file" << EOF
{
  "timestamp": "$(date -Iseconds)",
  "reportType": "storage-monitoring",
  "system": {
    "hostname": "$(hostname)",
    "uptime": "$(uptime -p)",
    "diskUsage": $(df "$UPLOADS_PATH" | tail -1 | awk '{print $5}' | sed 's/%//'),
    "inodeUsage": $(df -i "$UPLOADS_PATH" | tail -1 | awk '{print $5}' | sed 's/%//')
  },
  "volumes": {
EOF
    
    local directories=(
        "uploads:$UPLOADS_PATH:$UPLOADS_LIMIT"
        "kycImages:$KYC_IMAGES_PATH:$KYC_IMAGES_LIMIT"
        "depositImages:$DEPOSIT_IMAGES_PATH:$DEPOSIT_IMAGES_LIMIT"
        "withdrawImages:$WITHDRAW_IMAGES_PATH:$WITHDRAW_IMAGES_LIMIT"
        "annImages:$ANN_IMAGES_PATH:$ANN_IMAGES_LIMIT"
        "logs:$LOGS_PATH:$LOGS_LIMIT"
    )
    
    local first=true
    for dir_info in "${directories[@]}"; do
        local name="${dir_info%%:*}"
        local path="${dir_info#*:}"
        path="${path%:*}"
        local limit="${dir_info##*:}"
        
        local size=$(get_directory_size "$path")
        local count=$(get_file_count "$path")
        local usage_percent=$((size * 100 / limit))
        
        total_used=$((total_used + size))
        total_limit=$((total_limit + limit))
        
        if [ "$first" = true ]; then
            first=false
        else
            echo "," >> "$report_file"
        fi
        
        cat >> "$report_file" << EOF
    "$name": {
      "path": "$path",
      "sizeMB": $size,
      "limitMB": $limit,
      "fileCount": $count,
      "usagePercent": $usage_percent,
      "status": "$([ $usage_percent -ge $CRITICAL_THRESHOLD ] && echo "critical" || [ $usage_percent -ge $ALERT_THRESHOLD ] && echo "warning" || echo "normal")"
    }EOF
    done
    
    local total_usage_percent=$((total_used * 100 / total_limit))
    
    cat >> "$report_file" << EOF
  },
  "summary": {
    "totalUsedMB": $total_used,
    "totalLimitMB": $total_limit,
    "totalUsagePercent": $total_usage_percent,
    "healthStatus": "$(check_storage_health >/dev/null 2>&1 && echo "healthy" || echo "warning")"
  }
}
EOF
    
    echo -e "${GREEN}✓ 監控報告已生成: $report_file${NC}"
    echo ""
}

# 函數：顯示使用說明
show_usage() {
    echo "使用方法: $0 [選項]"
    echo ""
    echo "選項:"
    echo "  status      顯示存儲狀態 (預設)"
    echo "  detailed    顯示詳細資訊"
    echo "  health      檢查健康狀態"
    echo "  system      顯示系統資訊"
    echo "  report      生成監控報告"
    echo "  watch       持續監控模式"
    echo "  help        顯示此說明"
    echo ""
}

# 函數：持續監控模式
watch_mode() {
    echo -e "${BLUE}進入持續監控模式 (按 Ctrl+C 退出)${NC}"
    echo ""
    
    while true; do
        clear
        echo -e "${BLUE}=== ROSCA Zeabur 存儲監控 (實時) ===${NC}"
        echo "更新時間: $(date)"
        echo ""
        
        show_storage_status
        check_storage_health
        
        sleep 30
    done
}

# 主函數
main() {
    case "${1:-status}" in
        "status")
            show_storage_status
            ;;
        "detailed")
            show_storage_status
            show_detailed_info
            ;;
        "health")
            check_storage_health
            ;;
        "system")
            show_system_info
            ;;
        "report")
            show_storage_status
            check_storage_health
            generate_report
            ;;
        "watch")
            watch_mode
            ;;
        "help"|"-h"|"--help")
            show_usage
            ;;
        *)
            echo -e "${RED}未知選項: $1${NC}"
            show_usage
            exit 1
            ;;
    esac
}

# 執行主函數
main "$@"