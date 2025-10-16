#!/bin/bash

# ROSCA 平安商會系統 Zeabur 配置驗證腳本
# 用於驗證部署前的配置是否正確

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== ROSCA Zeabur 配置驗證工具 ===${NC}"
echo "驗證時間: $(date)"
echo ""

# 驗證結果統計
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0
WARNING_CHECKS=0

# 函數：執行檢查
run_check() {
    local check_name="$1"
    local check_function="$2"
    
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo -e "${YELLOW}檢查: $check_name${NC}"
    
    if $check_function; then
        echo -e "${GREEN}✓ 通過: $check_name${NC}"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
        return 0
    else
        echo -e "${RED}✗ 失敗: $check_name${NC}"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
        return 1
    fi
}

# 函數：執行警告檢查
run_warning_check() {
    local check_name="$1"
    local check_function="$2"
    
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    echo -e "${YELLOW}檢查: $check_name${NC}"
    
    if $check_function; then
        echo -e "${GREEN}✓ 通過: $check_name${NC}"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
        return 0
    else
        echo -e "${YELLOW}⚠ 警告: $check_name${NC}"
        WARNING_CHECKS=$((WARNING_CHECKS + 1))
        return 1
    fi
}

# 檢查：環境變數檔案存在
check_env_file_exists() {
    [ -f ".env.zeabur" ]
}

# 檢查：zeabur.json 配置檔案
check_zeabur_config() {
    if [ ! -f "zeabur.json" ]; then
        echo "  錯誤: zeabur.json 檔案不存在"
        return 1
    fi
    
    # 驗證 JSON 格式
    if ! jq empty zeabur.json 2>/dev/null; then
        echo "  錯誤: zeabur.json 格式無效"
        return 1
    fi
    
    # 檢查必要欄位
    local required_fields=("name" "services")
    for field in "${required_fields[@]}"; do
        if ! jq -e ".$field" zeabur.json >/dev/null 2>&1; then
            echo "  錯誤: 缺少必要欄位 '$field'"
            return 1
        fi
    done
    
    return 0
}

# 檢查：Dockerfile 檔案存在
check_dockerfiles() {
    local dockerfiles=(
        "backendAPI/DotNetBackEndCleanArchitecture/Dockerfile"
        "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile"
        "frontend/Dockerfile"
        "backend/FontEnd/Dockerfile"
    )
    
    local missing_files=()
    
    for dockerfile in "${dockerfiles[@]}"; do
        if [ ! -f "$dockerfile" ]; then
            missing_files+=("$dockerfile")
        fi
    done
    
    if [ ${#missing_files[@]} -ne 0 ]; then
        echo "  錯誤: 缺少 Dockerfile 檔案:"
        for file in "${missing_files[@]}"; do
            echo "    - $file"
        done
        return 1
    fi
    
    return 0
}

# 檢查：資料庫初始化腳本
check_database_scripts() {
    local scripts=(
        "database/zeabur/docker-entrypoint-initdb.d/01-schema.sql"
        "database/zeabur/docker-entrypoint-initdb.d/02-default-data.sql"
        "database/zeabur/docker-entrypoint-initdb.d/03-default-user.sql"
    )
    
    local missing_scripts=()
    
    for script in "${scripts[@]}"; do
        if [ ! -f "$script" ]; then
            missing_scripts+=("$script")
        fi
    done
    
    if [ ${#missing_scripts[@]} -ne 0 ]; then
        echo "  錯誤: 缺少資料庫初始化腳本:"
        for script in "${missing_scripts[@]}"; do
            echo "    - $script"
        done
        return 1
    fi
    
    return 0
}

# 檢查：環境變數完整性
check_environment_variables() {
    if [ ! -f ".env.zeabur" ]; then
        echo "  錯誤: .env.zeabur 檔案不存在"
        return 1
    fi
    
    source .env.zeabur
    
    local required_vars=(
        "DB_NAME"
        "DB_USER"
        "DB_PASSWORD"
        "DB_ROOT_PASSWORD"
        "JWT_SECRET_KEY"
        "JWT_ISSUER"
        "JWT_AUDIENCE"
        "CORS_ALLOWED_ORIGINS"
    )
    
    local missing_vars=()
    
    for var in "${required_vars[@]}"; do
        if [ -z "${!var}" ]; then
            missing_vars+=("$var")
        fi
    done
    
    if [ ${#missing_vars[@]} -ne 0 ]; then
        echo "  錯誤: 缺少必要環境變數:"
        for var in "${missing_vars[@]}"; do
            echo "    - $var"
        done
        return 1
    fi
    
    return 0
}

# 檢查：JWT 密鑰強度
check_jwt_security() {
    if [ ! -f ".env.zeabur" ]; then
        return 1
    fi
    
    source .env.zeabur
    
    if [ -z "$JWT_SECRET_KEY" ]; then
        echo "  錯誤: JWT_SECRET_KEY 未設定"
        return 1
    fi
    
    if [ ${#JWT_SECRET_KEY} -lt 32 ]; then
        echo "  錯誤: JWT_SECRET_KEY 長度不足 (至少需要 32 字符)"
        return 1
    fi
    
    return 0
}

# 檢查：資料庫密碼強度
check_database_password_strength() {
    if [ ! -f ".env.zeabur" ]; then
        return 1
    fi
    
    source .env.zeabur
    
    local passwords=("$DB_PASSWORD" "$DB_ROOT_PASSWORD")
    
    for password in "${passwords[@]}"; do
        if [ -z "$password" ]; then
            echo "  警告: 資料庫密碼未設定"
            return 1
        fi
        
        if [ ${#password} -lt 12 ]; then
            echo "  警告: 資料庫密碼長度不足 (建議至少 12 字符)"
            return 1
        fi
        
        # 檢查密碼複雜度
        if ! echo "$password" | grep -q '[A-Z]' || \
           ! echo "$password" | grep -q '[a-z]' || \
           ! echo "$password" | grep -q '[0-9]'; then
            echo "  警告: 資料庫密碼複雜度不足 (建議包含大小寫字母和數字)"
            return 1
        fi
    done
    
    return 0
}

# 檢查：存儲配置
check_storage_configuration() {
    local storage_files=(
        "storage/zeabur/zeabur-volumes.json"
        "storage/zeabur/storage-config.json"
        "storage/zeabur/persistent-volumes.yml"
    )
    
    local missing_files=()
    
    for file in "${storage_files[@]}"; do
        if [ ! -f "$file" ]; then
            missing_files+=("$file")
        fi
    done
    
    if [ ${#missing_files[@]} -ne 0 ]; then
        echo "  錯誤: 缺少存儲配置檔案:"
        for file in "${missing_files[@]}"; do
            echo "    - $file"
        done
        return 1
    fi
    
    return 0
}

# 檢查：Nginx 配置
check_nginx_configuration() {
    local nginx_configs=(
        "frontend/nginx.conf"
        "backend/FontEnd/nginx.conf"
    )
    
    local missing_configs=()
    
    for config in "${nginx_configs[@]}"; do
        if [ ! -f "$config" ]; then
            missing_configs+=("$config")
        fi
    done
    
    if [ ${#missing_configs[@]} -ne 0 ]; then
        echo "  錯誤: 缺少 Nginx 配置檔案:"
        for config in "${missing_configs[@]}"; do
            echo "    - $config"
        done
        return 1
    fi
    
    return 0
}

# 檢查：Git 儲存庫狀態
check_git_status() {
    if [ ! -d ".git" ]; then
        echo "  警告: 不是 Git 儲存庫"
        return 1
    fi
    
    # 檢查是否有未提交的變更
    if ! git diff-index --quiet HEAD --; then
        echo "  警告: 有未提交的變更"
        return 1
    fi
    
    # 檢查是否有未推送的提交
    local unpushed=$(git log --oneline @{u}.. 2>/dev/null | wc -l)
    if [ "$unpushed" -gt 0 ]; then
        echo "  警告: 有 $unpushed 個未推送的提交"
        return 1
    fi
    
    return 0
}

# 檢查：依賴套件
check_dependencies() {
    local missing_deps=()
    
    # 檢查 Node.js 專案
    if [ -f "frontend/package.json" ] && [ ! -d "frontend/node_modules" ]; then
        missing_deps+=("frontend/node_modules (執行 npm install)")
    fi
    
    if [ -f "backend/FontEnd/package.json" ] && [ ! -d "backend/FontEnd/node_modules" ]; then
        missing_deps+=("backend/FontEnd/node_modules (執行 npm install)")
    fi
    
    # 檢查 .NET 專案
    if [ -f "backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/DotNetBackEndApi.csproj" ]; then
        if ! dotnet restore backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/DotNetBackEndApi.csproj --dry-run >/dev/null 2>&1; then
            missing_deps+=(".NET 套件 (執行 dotnet restore)")
        fi
    fi
    
    if [ ${#missing_deps[@]} -ne 0 ]; then
        echo "  警告: 缺少依賴套件:"
        for dep in "${missing_deps[@]}"; do
            echo "    - $dep"
        done
        return 1
    fi
    
    return 0
}

# 檢查：檔案權限
check_file_permissions() {
    local script_files=(
        "deployment/zeabur/deploy.sh"
        "storage/zeabur/init-storage.sh"
        "storage/zeabur/manage-storage.sh"
        "storage/zeabur/monitor-storage.sh"
        "storage/zeabur/backup-storage.sh"
    )
    
    local permission_issues=()
    
    for script in "${script_files[@]}"; do
        if [ -f "$script" ] && [ ! -x "$script" ]; then
            permission_issues+=("$script")
        fi
    done
    
    if [ ${#permission_issues[@]} -ne 0 ]; then
        echo "  警告: 腳本檔案缺少執行權限:"
        for script in "${permission_issues[@]}"; do
            echo "    - $script (執行 chmod +x $script)"
        done
        return 1
    fi
    
    return 0
}

# 檢查：CORS 配置
check_cors_configuration() {
    if [ ! -f ".env.zeabur" ]; then
        return 1
    fi
    
    source .env.zeabur
    
    if [ -z "$CORS_ALLOWED_ORIGINS" ]; then
        echo "  錯誤: CORS_ALLOWED_ORIGINS 未設定"
        return 1
    fi
    
    # 檢查是否包含不安全的萬用字元
    if echo "$CORS_ALLOWED_ORIGINS" | grep -q '\*'; then
        echo "  警告: CORS 配置包含萬用字元 (*), 可能存在安全風險"
        return 1
    fi
    
    return 0
}

# 執行所有檢查
echo -e "${PURPLE}🔍 開始配置驗證${NC}"
echo ""

# 必要檢查 (失敗會阻止部署)
echo -e "${BLUE}=== 必要檢查 ===${NC}"
run_check "環境變數檔案存在" check_env_file_exists
run_check "Zeabur 配置檔案" check_zeabur_config
run_check "Dockerfile 檔案" check_dockerfiles
run_check "資料庫初始化腳本" check_database_scripts
run_check "環境變數完整性" check_environment_variables
run_check "JWT 密鑰安全性" check_jwt_security
run_check "存儲配置" check_storage_configuration
run_check "Nginx 配置" check_nginx_configuration
run_check "CORS 配置" check_cors_configuration

echo ""

# 建議檢查 (失敗會顯示警告但不阻止部署)
echo -e "${BLUE}=== 建議檢查 ===${NC}"
run_warning_check "資料庫密碼強度" check_database_password_strength
run_warning_check "Git 儲存庫狀態" check_git_status
run_warning_check "依賴套件" check_dependencies
run_warning_check "檔案權限" check_file_permissions

echo ""

# 顯示驗證結果
echo -e "${PURPLE}📊 驗證結果摘要${NC}"
echo "總檢查項目: $TOTAL_CHECKS"
echo -e "${GREEN}通過: $PASSED_CHECKS${NC}"
echo -e "${RED}失敗: $FAILED_CHECKS${NC}"
echo -e "${YELLOW}警告: $WARNING_CHECKS${NC}"

echo ""

if [ $FAILED_CHECKS -eq 0 ]; then
    echo -e "${GREEN}🎉 配置驗證通過！可以開始部署。${NC}"
    
    if [ $WARNING_CHECKS -gt 0 ]; then
        echo -e "${YELLOW}⚠️  有 $WARNING_CHECKS 個警告項目，建議修正後再部署。${NC}"
    fi
    
    echo ""
    echo -e "${BLUE}下一步:${NC}"
    echo "1. 執行部署腳本: ./deployment/zeabur/deploy.sh deploy"
    echo "2. 或手動在 Zeabur 控制台部署"
    
    exit 0
else
    echo -e "${RED}❌ 配置驗證失敗！請修正上述問題後重新驗證。${NC}"
    
    echo ""
    echo -e "${BLUE}修正建議:${NC}"
    echo "1. 檢查並修正所有標記為 '✗ 失敗' 的項目"
    echo "2. 確保所有必要檔案存在且格式正確"
    echo "3. 驗證環境變數設定"
    echo "4. 重新執行此驗證腳本"
    
    exit 1
fi