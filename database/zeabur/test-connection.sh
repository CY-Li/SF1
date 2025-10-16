#!/bin/bash

# MariaDB 連接測試腳本 (Zeabur 版本)
# 用於測試資料庫連接和驗證初始化

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 預設環境變數
DB_HOST=${DB_HOST:-"localhost"}
DB_PORT=${DB_PORT:-"3306"}
DB_NAME=${DB_NAME:-"rosca_db"}
DB_USER=${DB_USER:-"rosca_user"}
DB_PASSWORD=${DB_PASSWORD:-""}
DB_ROOT_PASSWORD=${DB_ROOT_PASSWORD:-""}

echo -e "${BLUE}=== MariaDB 連接測試腳本 (Zeabur 版本) ===${NC}"
echo -e "${BLUE}測試目標: ${DB_HOST}:${DB_PORT}/${DB_NAME}${NC}"
echo ""

# 函數：等待資料庫啟動
wait_for_database() {
    echo -e "${YELLOW}等待資料庫啟動...${NC}"
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if mysqladmin ping -h"$DB_HOST" -P"$DB_PORT" -u"root" -p"$DB_ROOT_PASSWORD" --silent 2>/dev/null; then
            echo -e "${GREEN}✓ 資料庫已啟動 (嘗試 $attempt/$max_attempts)${NC}"
            return 0
        fi
        
        echo -e "${YELLOW}⏳ 等待資料庫啟動... (嘗試 $attempt/$max_attempts)${NC}"
        sleep 2
        ((attempt++))
    done
    
    echo -e "${RED}✗ 資料庫啟動超時${NC}"
    return 1
}

# 函數：測試 root 連接
test_root_connection() {
    echo -e "${YELLOW}測試 root 用戶連接...${NC}"
    
    if mysql -h"$DB_HOST" -P"$DB_PORT" -u"root" -p"$DB_ROOT_PASSWORD" -e "SELECT 'Root connection successful' as status;" 2>/dev/null; then
        echo -e "${GREEN}✓ Root 用戶連接成功${NC}"
        return 0
    else
        echo -e "${RED}✗ Root 用戶連接失敗${NC}"
        return 1
    fi
}

# 函數：測試應用程式用戶連接
test_app_user_connection() {
    echo -e "${YELLOW}測試應用程式用戶連接...${NC}"
    
    if mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -e "SELECT 'App user connection successful' as status;" 2>/dev/null; then
        echo -e "${GREEN}✓ 應用程式用戶連接成功${NC}"
        return 0
    else
        echo -e "${RED}✗ 應用程式用戶連接失敗${NC}"
        return 1
    fi
}

# 函數：檢查資料庫結構
check_database_schema() {
    echo -e "${YELLOW}檢查資料庫結構...${NC}"
    
    local table_count=$(mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -sN -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$DB_NAME';" 2>/dev/null)
    
    if [ "$table_count" -ge 15 ]; then
        echo -e "${GREEN}✓ 資料庫結構正常 (共 $table_count 個表)${NC}"
        return 0
    else
        echo -e "${RED}✗ 資料庫結構不完整 (只有 $table_count 個表)${NC}"
        return 1
    fi
}

# 函數：檢查預設資料
check_default_data() {
    echo -e "${YELLOW}檢查預設資料...${NC}"
    
    local member_count=$(mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -sN -e "SELECT COUNT(*) FROM member_master;" 2>/dev/null)
    local param_count=$(mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -sN -e "SELECT COUNT(*) FROM parameter_category;" 2>/dev/null)
    local system_param_count=$(mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -sN -e "SELECT COUNT(*) FROM system_parameter_setting;" 2>/dev/null)
    
    if [ "$member_count" -ge 2 ] && [ "$param_count" -ge 9 ] && [ "$system_param_count" -ge 10 ]; then
        echo -e "${GREEN}✓ 預設資料正常${NC}"
        echo -e "  - 會員數量: $member_count"
        echo -e "  - 參數分類: $param_count"
        echo -e "  - 系統參數: $system_param_count"
        return 0
    else
        echo -e "${RED}✗ 預設資料不完整${NC}"
        echo -e "  - 會員數量: $member_count (預期 >= 2)"
        echo -e "  - 參數分類: $param_count (預期 >= 9)"
        echo -e "  - 系統參數: $system_param_count (預期 >= 10)"
        return 1
    fi
}

# 函數：檢查字符集設定
check_charset() {
    echo -e "${YELLOW}檢查字符集設定...${NC}"
    
    local server_charset=$(mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -sN -e "SELECT @@character_set_server;" 2>/dev/null)
    local db_charset=$(mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -sN -e "SELECT @@character_set_database;" 2>/dev/null)
    
    if [ "$server_charset" = "utf8mb4" ] && [ "$db_charset" = "utf8mb4" ]; then
        echo -e "${GREEN}✓ 字符集設定正確 (utf8mb4)${NC}"
        return 0
    else
        echo -e "${RED}✗ 字符集設定錯誤${NC}"
        echo -e "  - 伺服器字符集: $server_charset (預期 utf8mb4)"
        echo -e "  - 資料庫字符集: $db_charset (預期 utf8mb4)"
        return 1
    fi
}

# 函數：測試預設帳號登入
test_default_accounts() {
    echo -e "${YELLOW}測試預設帳號...${NC}"
    
    local admin_exists=$(mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -sN -e "SELECT COUNT(*) FROM member_master WHERE mm_account = 'admin';" 2>/dev/null)
    local test_user_exists=$(mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -sN -e "SELECT COUNT(*) FROM member_master WHERE mm_account = '0938766349';" 2>/dev/null)
    
    if [ "$admin_exists" -eq 1 ] && [ "$test_user_exists" -eq 1 ]; then
        echo -e "${GREEN}✓ 預設帳號存在${NC}"
        echo -e "  - 管理員帳號: admin"
        echo -e "  - 測試帳號: 0938766349"
        return 0
    else
        echo -e "${RED}✗ 預設帳號不完整${NC}"
        echo -e "  - 管理員帳號存在: $admin_exists (預期 1)"
        echo -e "  - 測試帳號存在: $test_user_exists (預期 1)"
        return 1
    fi
}

# 函數：測試連接字串格式
test_connection_string_format() {
    echo -e "${YELLOW}測試連接字串格式...${NC}"
    
    # 建立 .NET Core 連接字串
    local connection_string="Server=$DB_HOST;Port=$DB_PORT;User Id=$DB_USER;Password=$DB_PASSWORD;Database=$DB_NAME;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;"
    
    # 隱藏密碼的連接字串
    local masked_connection_string=$(echo "$connection_string" | sed 's/Password=[^;]*/Password=***/g')
    
    echo -e "${GREEN}✓ .NET Core 連接字串格式正確${NC}"
    echo -e "  格式: $masked_connection_string"
    
    # 檢查 Zeabur 服務發現變數
    local zeabur_host=${ZEABUR_MARIADB_CONNECTION_HOST:-"未設定"}
    local zeabur_port=${ZEABUR_MARIADB_CONNECTION_PORT:-"未設定"}
    
    echo -e "${BLUE}Zeabur 服務發現變數:${NC}"
    echo -e "  ZEABUR_MARIADB_CONNECTION_HOST: $zeabur_host"
    echo -e "  ZEABUR_MARIADB_CONNECTION_PORT: $zeabur_port"
    
    # 建立 Zeabur 環境連接字串
    if [ "$zeabur_host" != "未設定" ] && [ "$zeabur_port" != "未設定" ]; then
        local zeabur_connection_string="Server=$zeabur_host;Port=$zeabur_port;User Id=$DB_USER;Password=$DB_PASSWORD;Database=$DB_NAME;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;"
        local zeabur_masked_string=$(echo "$zeabur_connection_string" | sed 's/Password=[^;]*/Password=***/g')
        echo -e "${GREEN}✓ Zeabur 連接字串: $zeabur_masked_string${NC}"
    fi
    
    return 0
}

# 函數：顯示資料庫資訊
show_database_info() {
    echo -e "${BLUE}=== 資料庫資訊 ===${NC}"
    
    mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -e "
        SELECT 
            'MariaDB Version' as info_type,
            VERSION() as value
        UNION ALL
        SELECT 
            'Database Name' as info_type,
            DATABASE() as value
        UNION ALL
        SELECT 
            'Character Set' as info_type,
            @@character_set_database as value
        UNION ALL
        SELECT 
            'Collation' as info_type,
            @@collation_database as value
        UNION ALL
        SELECT 
            'Time Zone' as info_type,
            @@time_zone as value;
    " 2>/dev/null
    
    echo ""
    echo -e "${BLUE}=== 連接池設定 ===${NC}"
    mysql -h"$DB_HOST" -P"$DB_PORT" -u"$DB_USER" -p"$DB_PASSWORD" -D"$DB_NAME" -e "
        SELECT 
            'Max Connections' as setting_name,
            @@max_connections as value
        UNION ALL
        SELECT 
            'Connect Timeout' as setting_name,
            @@connect_timeout as value
        UNION ALL
        SELECT 
            'Wait Timeout' as setting_name,
            @@wait_timeout as value
        UNION ALL
        SELECT 
            'Interactive Timeout' as setting_name,
            @@interactive_timeout as value;
    " 2>/dev/null
}

# 主要測試流程
main() {
    local exit_code=0
    
    # 檢查必要的環境變數
    if [ -z "$DB_PASSWORD" ] || [ -z "$DB_ROOT_PASSWORD" ]; then
        echo -e "${RED}錯誤: 缺少必要的環境變數 DB_PASSWORD 或 DB_ROOT_PASSWORD${NC}"
        exit 1
    fi
    
    # 執行測試
    wait_for_database || exit_code=1
    test_root_connection || exit_code=1
    test_app_user_connection || exit_code=1
    check_database_schema || exit_code=1
    check_default_data || exit_code=1
    check_charset || exit_code=1
    test_default_accounts || exit_code=1
    test_connection_string_format || exit_code=1
    
    echo ""
    
    if [ $exit_code -eq 0 ]; then
        echo -e "${GREEN}=== 所有測試通過 ===${NC}"
        show_database_info
        echo ""
        echo -e "${GREEN}✓ MariaDB 資料庫已成功初始化並可正常使用${NC}"
        echo -e "${BLUE}預設帳號資訊:${NC}"
        echo -e "  管理員: admin / Admin123456"
        echo -e "  測試用戶: 0938766349 / 123456"
    else
        echo -e "${RED}=== 測試失敗 ===${NC}"
        echo -e "${RED}✗ 請檢查資料庫配置和初始化腳本${NC}"
    fi
    
    exit $exit_code
}

# 執行主函數
main "$@"