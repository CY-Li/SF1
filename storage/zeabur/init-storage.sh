#!/bin/bash

# ROSCA 平安商會系統 Zeabur 存儲初始化腳本
# 用於在 Zeabur 平台上初始化持久化存儲卷

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 存儲路徑配置 (Zeabur 環境)
UPLOADS_PATH="/app/uploads"
KYC_IMAGES_PATH="/app/KycImages"
DEPOSIT_IMAGES_PATH="/app/DepositImages"
WITHDRAW_IMAGES_PATH="/app/WithdrawImages"
ANN_IMAGES_PATH="/app/AnnImagessss"
LOGS_PATH="/app/logs"

echo -e "${BLUE}=== ROSCA Zeabur 存儲初始化 ===${NC}"
echo "初始化時間: $(date)"
echo ""

# 函數：建立目錄並設定權限
create_directory() {
    local dir="$1"
    local description="$2"
    
    echo -e "${YELLOW}初始化 $description...${NC}"
    
    if [ ! -d "$dir" ]; then
        mkdir -p "$dir"
        echo -e "${GREEN}✓ 建立目錄: $dir${NC}"
    else
        echo -e "${BLUE}ℹ 目錄已存在: $dir${NC}"
    fi
    
    # 設定適當的權限
    chmod 755 "$dir"
    
    # 建立 .gitkeep 檔案以確保目錄結構
    touch "$dir/.gitkeep"
    
    # 建立 README 檔案說明用途
    cat > "$dir/README.md" << EOF
# $description

此目錄用於存儲 ROSCA 平安商會系統的檔案。

## 配置資訊
- 路徑: $dir
- 權限: 755
- 建立時間: $(date)

## 注意事項
- 請勿手動刪除此目錄
- 檔案會根據設定的保留政策自動清理
- 如有問題請聯繫系統管理員
EOF
    
    echo -e "${GREEN}✓ $description 初始化完成${NC}"
    echo ""
}

# 函數：設定環境變數
setup_environment() {
    echo -e "${YELLOW}設定環境變數...${NC}"
    
    # 建立環境變數檔案
    cat > /app/.env.storage << EOF
# ROSCA 存儲環境變數
UPLOADS_PATH=$UPLOADS_PATH
KYC_IMAGES_PATH=$KYC_IMAGES_PATH
DEPOSIT_IMAGES_PATH=$DEPOSIT_IMAGES_PATH
WITHDRAW_IMAGES_PATH=$WITHDRAW_IMAGES_PATH
ANN_IMAGES_PATH=$ANN_IMAGES_PATH
LOGS_PATH=$LOGS_PATH

# 存儲限制
UPLOADS_MAX_SIZE=5368709120
KYC_IMAGES_MAX_SIZE=2147483648
DEPOSIT_IMAGES_MAX_SIZE=2147483648
WITHDRAW_IMAGES_MAX_SIZE=2147483648
ANN_IMAGES_MAX_SIZE=2147483648
LOGS_MAX_SIZE=1073741824

# 檔案類型限制
UPLOADS_ALLOWED_EXTENSIONS=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx
KYC_IMAGES_ALLOWED_EXTENSIONS=.jpg,.jpeg,.png
DEPOSIT_IMAGES_ALLOWED_EXTENSIONS=.jpg,.jpeg,.png,.pdf
WITHDRAW_IMAGES_ALLOWED_EXTENSIONS=.jpg,.jpeg,.png,.pdf
ANN_IMAGES_ALLOWED_EXTENSIONS=.jpg,.jpeg,.png,.gif
LOGS_ALLOWED_EXTENSIONS=.log,.txt,.json

# 清理設定
CLEANUP_ENABLED=true
UPLOADS_RETENTION_DAYS=365
KYC_IMAGES_RETENTION_DAYS=-1
DEPOSIT_IMAGES_RETENTION_DAYS=1095
WITHDRAW_IMAGES_RETENTION_DAYS=1095
ANN_IMAGES_RETENTION_DAYS=730
LOGS_RETENTION_DAYS=30

# 備份設定
BACKUP_ENABLED=true
BACKUP_SCHEDULE="0 2 * * *"
BACKUP_RETENTION_DAYS=30

# 監控設定
MONITORING_ENABLED=true
DISK_USAGE_THRESHOLD=85
FILE_COUNT_THRESHOLD=10000
ERROR_RATE_THRESHOLD=5
EOF
    
    echo -e "${GREEN}✓ 環境變數設定完成${NC}"
    echo ""
}

# 函數：建立存儲配置檔案
create_storage_config() {
    echo -e "${YELLOW}建立存儲配置檔案...${NC}"
    
    # 建立運行時配置檔案
    cat > /app/storage-runtime.json << EOF
{
  "storageInitialized": true,
  "initializationTime": "$(date -Iseconds)",
  "version": "1.0.0",
  "paths": {
    "uploads": "$UPLOADS_PATH",
    "kycImages": "$KYC_IMAGES_PATH",
    "depositImages": "$DEPOSIT_IMAGES_PATH",
    "withdrawImages": "$WITHDRAW_IMAGES_PATH",
    "annImages": "$ANN_IMAGES_PATH",
    "logs": "$LOGS_PATH"
  },
  "status": "ready"
}
EOF
    
    echo -e "${GREEN}✓ 存儲配置檔案建立完成${NC}"
    echo ""
}

# 函數：驗證存儲設定
verify_storage() {
    echo -e "${YELLOW}驗證存儲設定...${NC}"
    
    local errors=0
    
    # 檢查所有目錄
    local directories=(
        "$UPLOADS_PATH:通用檔案上傳"
        "$KYC_IMAGES_PATH:KYC 身份認證圖片"
        "$DEPOSIT_IMAGES_PATH:存款憑證圖片"
        "$WITHDRAW_IMAGES_PATH:提款憑證圖片"
        "$ANN_IMAGES_PATH:公告圖片"
        "$LOGS_PATH:應用程式日誌"
    )
    
    for dir_info in "${directories[@]}"; do
        local dir="${dir_info%:*}"
        local desc="${dir_info#*:}"
        
        if [ -d "$dir" ] && [ -w "$dir" ]; then
            echo -e "${GREEN}✓ $desc: $dir${NC}"
        else
            echo -e "${RED}✗ $desc: $dir (不存在或不可寫)${NC}"
            errors=$((errors + 1))
        fi
    done
    
    # 檢查磁碟空間
    local available_space=$(df "$UPLOADS_PATH" | tail -1 | awk '{print $4}')
    local required_space=15728640  # 15GB in KB
    
    if [ "$available_space" -gt "$required_space" ]; then
        echo -e "${GREEN}✓ 磁碟空間充足: $(($available_space / 1024 / 1024))GB 可用${NC}"
    else
        echo -e "${YELLOW}⚠ 磁碟空間警告: $(($available_space / 1024 / 1024))GB 可用 (建議 15GB+)${NC}"
    fi
    
    echo ""
    
    if [ "$errors" -eq 0 ]; then
        echo -e "${GREEN}✓ 存儲驗證通過${NC}"
        return 0
    else
        echo -e "${RED}✗ 存儲驗證失敗，發現 $errors 個問題${NC}"
        return 1
    fi
}

# 函數：建立健康檢查腳本
create_health_check() {
    echo -e "${YELLOW}建立健康檢查腳本...${NC}"
    
    cat > /app/storage-health-check.sh << 'EOF'
#!/bin/bash

# 存儲健康檢查腳本
check_storage_health() {
    local issues=0
    
    # 檢查目錄存在性和權限
    local directories=(
        "/app/uploads"
        "/app/KycImages"
        "/app/DepositImages"
        "/app/WithdrawImages"
        "/app/AnnImagessss"
        "/app/logs"
    )
    
    for dir in "${directories[@]}"; do
        if [ ! -d "$dir" ] || [ ! -w "$dir" ]; then
            echo "ERROR: Directory issue - $dir"
            issues=$((issues + 1))
        fi
    done
    
    # 檢查磁碟空間
    local disk_usage=$(df /app | tail -1 | awk '{print $5}' | sed 's/%//')
    if [ "$disk_usage" -gt 90 ]; then
        echo "ERROR: Disk usage too high - ${disk_usage}%"
        issues=$((issues + 1))
    fi
    
    if [ "$issues" -eq 0 ]; then
        echo "OK: Storage health check passed"
        exit 0
    else
        echo "ERROR: Storage health check failed with $issues issues"
        exit 1
    fi
}

check_storage_health
EOF
    
    chmod +x /app/storage-health-check.sh
    
    echo -e "${GREEN}✓ 健康檢查腳本建立完成${NC}"
    echo ""
}

# 主要初始化流程
main() {
    echo -e "${BLUE}開始初始化 ROSCA 存儲系統...${NC}"
    echo ""
    
    # 建立所有必要的目錄
    create_directory "$UPLOADS_PATH" "通用檔案上傳存儲"
    create_directory "$KYC_IMAGES_PATH" "KYC 身份認證圖片存儲"
    create_directory "$DEPOSIT_IMAGES_PATH" "存款憑證圖片存儲"
    create_directory "$WITHDRAW_IMAGES_PATH" "提款憑證圖片存儲"
    create_directory "$ANN_IMAGES_PATH" "公告圖片存儲"
    create_directory "$LOGS_PATH" "應用程式日誌存儲"
    
    # 設定環境變數
    setup_environment
    
    # 建立配置檔案
    create_storage_config
    
    # 建立健康檢查腳本
    create_health_check
    
    # 驗證設定
    if verify_storage; then
        echo -e "${GREEN}🎉 ROSCA 存儲系統初始化成功！${NC}"
        echo ""
        echo -e "${BLUE}存儲資訊:${NC}"
        echo "- 總存儲空間: 15GB"
        echo "- 通用上傳: 5GB"
        echo "- KYC 圖片: 2GB"
        echo "- 存款憑證: 2GB"
        echo "- 提款憑證: 2GB"
        echo "- 公告圖片: 2GB"
        echo "- 應用日誌: 1GB"
        echo "- 資料庫資料: 10GB"
        echo "- 資料庫日誌: 1GB"
        echo ""
        echo -e "${BLUE}下一步:${NC}"
        echo "1. 啟動應用程式服務"
        echo "2. 驗證檔案上傳功能"
        echo "3. 設定監控和備份"
        
        exit 0
    else
        echo -e "${RED}❌ 存儲系統初始化失敗${NC}"
        exit 1
    fi
}

# 執行主函數
main "$@"