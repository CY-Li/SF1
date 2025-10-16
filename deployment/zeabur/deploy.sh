#!/bin/bash

# ROSCA 平安商會系統 Zeabur 自動化部署腳本
# 用於自動化部署到 Zeabur 平台

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# 配置變數
PROJECT_NAME="rosca-system"
GITHUB_REPO=""
ZEABUR_TOKEN=""
DEPLOY_ENV="production"

# 服務配置
declare -A SERVICES=(
    ["mariadb"]="marketplace:mariadb:11.3.2"
    ["backend-service"]="git:./backendAPI/DotNetBackEndCleanArchitecture:Presentation/DotNetBackEndService/Dockerfile"
    ["backend"]="git:./backendAPI/DotNetBackEndCleanArchitecture:Dockerfile"
    ["frontend"]="git:./frontend:Dockerfile"
    ["admin"]="git:./backend/FontEnd:Dockerfile"
)

echo -e "${BLUE}=== ROSCA Zeabur 自動化部署工具 ===${NC}"
echo "部署時間: $(date)"
echo "專案名稱: $PROJECT_NAME"
echo "部署環境: $DEPLOY_ENV"
echo ""

# 函數：顯示使用說明
show_usage() {
    echo "使用方法: $0 [命令] [選項]"
    echo ""
    echo "命令:"
    echo "  init          初始化部署環境"
    echo "  deploy        執行完整部署"
    echo "  update        更新現有服務"
    echo "  status        檢查部署狀態"
    echo "  logs          查看服務日誌"
    echo "  rollback      回滾到上一版本"
    echo "  cleanup       清理部署資源"
    echo "  help          顯示此說明"
    echo ""
    echo "選項:"
    echo "  --project NAME    指定專案名稱"
    echo "  --env ENV         指定部署環境 (dev/staging/production)"
    echo "  --token TOKEN     指定 Zeabur API Token"
    echo "  --repo REPO       指定 GitHub 儲存庫"
    echo "  --service NAME    指定特定服務"
    echo "  --skip-build      跳過建置步驟"
    echo "  --dry-run         模擬執行 (不實際部署)"
    echo ""
    echo "範例:"
    echo "  $0 init --project rosca-system --repo username/rosca-repo"
    echo "  $0 deploy --env production"
    echo "  $0 update --service backend"
    echo "  $0 logs --service frontend"
}

# 函數：檢查必要工具
check_prerequisites() {
    echo -e "${YELLOW}檢查必要工具...${NC}"
    
    local missing_tools=()
    
    # 檢查 curl
    if ! command -v curl &> /dev/null; then
        missing_tools+=("curl")
    fi
    
    # 檢查 jq
    if ! command -v jq &> /dev/null; then
        missing_tools+=("jq")
    fi
    
    # 檢查 git
    if ! command -v git &> /dev/null; then
        missing_tools+=("git")
    fi
    
    if [ ${#missing_tools[@]} -ne 0 ]; then
        echo -e "${RED}✗ 缺少必要工具: ${missing_tools[*]}${NC}"
        echo "請安裝缺少的工具後重新執行"
        exit 1
    fi
    
    echo -e "${GREEN}✓ 所有必要工具已安裝${NC}"
}

# 函數：驗證配置
validate_config() {
    echo -e "${YELLOW}驗證配置...${NC}"
    
    local errors=()
    
    if [ -z "$PROJECT_NAME" ]; then
        errors+=("專案名稱不能為空")
    fi
    
    if [ -z "$GITHUB_REPO" ]; then
        errors+=("GitHub 儲存庫不能為空")
    fi
    
    if [ -z "$ZEABUR_TOKEN" ]; then
        errors+=("Zeabur API Token 不能為空")
    fi
    
    if [ ${#errors[@]} -ne 0 ]; then
        echo -e "${RED}✗ 配置驗證失敗:${NC}"
        for error in "${errors[@]}"; do
            echo -e "${RED}  - $error${NC}"
        done
        exit 1
    fi
    
    echo -e "${GREEN}✓ 配置驗證通過${NC}"
}

# 函數：初始化部署環境
init_deployment() {
    echo -e "${PURPLE}🚀 初始化部署環境${NC}"
    
    # 檢查 Zeabur CLI
    if ! command -v zeabur &> /dev/null; then
        echo -e "${YELLOW}安裝 Zeabur CLI...${NC}"
        curl -fsSL https://zeabur.com/install.sh | bash
        export PATH="$HOME/.zeabur/bin:$PATH"
    fi
    
    # 登入 Zeabur
    echo -e "${YELLOW}登入 Zeabur...${NC}"
    echo "$ZEABUR_TOKEN" | zeabur auth login --token
    
    # 檢查專案是否存在
    if zeabur project list | grep -q "$PROJECT_NAME"; then
        echo -e "${BLUE}ℹ 專案已存在: $PROJECT_NAME${NC}"
    else
        echo -e "${YELLOW}建立新專案: $PROJECT_NAME${NC}"
        zeabur project create "$PROJECT_NAME"
    fi
    
    echo -e "${GREEN}✓ 部署環境初始化完成${NC}"
}

# 函數：部署資料庫服務
deploy_database() {
    echo -e "${PURPLE}🗄️ 部署資料庫服務${NC}"
    
    # 檢查 MariaDB 服務是否已存在
    if zeabur service list --project "$PROJECT_NAME" | grep -q "mariadb"; then
        echo -e "${BLUE}ℹ MariaDB 服務已存在${NC}"
    else
        echo -e "${YELLOW}部署 MariaDB 服務...${NC}"
        zeabur service deploy \
            --project "$PROJECT_NAME" \
            --name "mariadb" \
            --template "mariadb:11.3.2"
    fi
    
    # 配置資料庫環境變數
    echo -e "${YELLOW}配置資料庫環境變數...${NC}"
    zeabur env set \
        --project "$PROJECT_NAME" \
        --service "mariadb" \
        MYSQL_ROOT_PASSWORD="$DB_ROOT_PASSWORD" \
        MYSQL_DATABASE="$DB_NAME" \
        MYSQL_USER="$DB_USER" \
        MYSQL_PASSWORD="$DB_PASSWORD" \
        MYSQL_CHARACTER_SET_SERVER="utf8mb4" \
        MYSQL_COLLATION_SERVER="utf8mb4_general_ci" \
        TZ="Asia/Taipei"
    
    echo -e "${GREEN}✓ 資料庫服務部署完成${NC}"
}

# 函數：部署後端服務
deploy_backend_services() {
    echo -e "${PURPLE}⚙️ 部署後端服務${NC}"
    
    # 部署 Backend Service
    echo -e "${YELLOW}部署 Backend Service...${NC}"
    zeabur service deploy \
        --project "$PROJECT_NAME" \
        --name "backend-service" \
        --git "$GITHUB_REPO" \
        --build-command "cd backendAPI/DotNetBackEndCleanArchitecture && dotnet publish Presentation/DotNetBackEndService -c Release -o out" \
        --dockerfile "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile"
    
    # 配置 Backend Service 環境變數
    configure_backend_service_env
    
    # 部署 API Gateway
    echo -e "${YELLOW}部署 API Gateway...${NC}"
    zeabur service deploy \
        --project "$PROJECT_NAME" \
        --name "backend" \
        --git "$GITHUB_REPO" \
        --build-command "cd backendAPI/DotNetBackEndCleanArchitecture && dotnet publish -c Release -o out" \
        --dockerfile "backendAPI/DotNetBackEndCleanArchitecture/Dockerfile"
    
    # 配置 API Gateway 環境變數
    configure_backend_env
    
    echo -e "${GREEN}✓ 後端服務部署完成${NC}"
}

# 函數：配置 Backend Service 環境變數
configure_backend_service_env() {
    echo -e "${YELLOW}配置 Backend Service 環境變數...${NC}"
    
    zeabur env set \
        --project "$PROJECT_NAME" \
        --service "backend-service" \
        ASPNETCORE_ENVIRONMENT="Production" \
        ASPNETCORE_URLS="http://+:5001" \
        TZ="Asia/Taipei" \
        JWT__SecretKey="$JWT_SECRET_KEY" \
        JWT__Issuer="$JWT_ISSUER" \
        JWT__Audience="$JWT_AUDIENCE" \
        JWT__ExpiryMinutes="$JWT_EXPIRY_MINUTES" \
        Serilog__MinimumLevel__Default="$LOG_LEVEL"
    
    # 配置資料庫連接字串
    local connection_string="Server=\${ZEABUR_MARIADB_CONNECTION_HOST};Port=\${ZEABUR_MARIADB_CONNECTION_PORT};User Id=$DB_USER;Password=$DB_PASSWORD;Database=$DB_NAME;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;"
    
    zeabur env set \
        --project "$PROJECT_NAME" \
        --service "backend-service" \
        "ConnectionStrings__BackEndDatabase=$connection_string"
}

# 函數：配置 API Gateway 環境變數
configure_backend_env() {
    echo -e "${YELLOW}配置 API Gateway 環境變數...${NC}"
    
    zeabur env set \
        --project "$PROJECT_NAME" \
        --service "backend" \
        ASPNETCORE_ENVIRONMENT="Production" \
        ASPNETCORE_URLS="http://+:5000" \
        TZ="Asia/Taipei" \
        JWT__SecretKey="$JWT_SECRET_KEY" \
        JWT__Issuer="$JWT_ISSUER" \
        JWT__Audience="$JWT_AUDIENCE" \
        JWT__ExpiryMinutes="$JWT_EXPIRY_MINUTES" \
        CORS__AllowedOrigins="$CORS_ALLOWED_ORIGINS" \
        FileUpload__MaxFileSize="$FILE_UPLOAD_MAX_SIZE" \
        FileUpload__AllowedExtensions="$FILE_UPLOAD_EXTENSIONS" \
        Hangfire__DashboardEnabled="$HANGFIRE_DASHBOARD_ENABLED" \
        Serilog__MinimumLevel__Default="$LOG_LEVEL"
    
    # 配置資料庫連接字串
    local connection_string="Server=\${ZEABUR_MARIADB_CONNECTION_HOST};Port=\${ZEABUR_MARIADB_CONNECTION_PORT};User Id=$DB_USER;Password=$DB_PASSWORD;Database=$DB_NAME;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;"
    
    zeabur env set \
        --project "$PROJECT_NAME" \
        --service "backend" \
        "ConnectionStrings__DefaultConnection=$connection_string" \
        "APIUrl=http://\${ZEABUR_BACKEND_SERVICE_DOMAIN}:5001/"
}

# 函數：部署前端服務
deploy_frontend_services() {
    echo -e "${PURPLE}🎨 部署前端服務${NC}"
    
    # 部署前台系統
    echo -e "${YELLOW}部署前台系統...${NC}"
    zeabur service deploy \
        --project "$PROJECT_NAME" \
        --name "frontend" \
        --git "$GITHUB_REPO" \
        --dockerfile "frontend/Dockerfile"
    
    # 配置前台環境變數
    zeabur env set \
        --project "$PROJECT_NAME" \
        --service "frontend" \
        VUE_APP_API_BASE_URL="https://\${ZEABUR_BACKEND_DOMAIN}" \
        VUE_APP_ENVIRONMENT="production"
    
    # 部署後台系統
    echo -e "${YELLOW}部署後台系統...${NC}"
    zeabur service deploy \
        --project "$PROJECT_NAME" \
        --name "admin" \
        --git "$GITHUB_REPO" \
        --dockerfile "backend/FontEnd/Dockerfile"
    
    # 配置後台環境變數
    zeabur env set \
        --project "$PROJECT_NAME" \
        --service "admin" \
        NG_APP_API_BASE_URL="https://\${ZEABUR_BACKEND_DOMAIN}" \
        NG_APP_ENVIRONMENT="production"
    
    echo -e "${GREEN}✓ 前端服務部署完成${NC}"
}

# 函數：配置存儲卷
configure_storage() {
    echo -e "${PURPLE}💾 配置存儲卷${NC}"
    
    local volumes=(
        "uploads:/app/uploads:5GB"
        "kyc-images:/app/KycImages:2GB"
        "deposit-images:/app/DepositImages:2GB"
        "withdraw-images:/app/WithdrawImages:2GB"
        "ann-images:/app/AnnImagessss:2GB"
        "logs:/app/logs:1GB"
    )
    
    for volume in "${volumes[@]}"; do
        IFS=':' read -r name path size <<< "$volume"
        echo -e "${YELLOW}配置存儲卷: $name ($size)${NC}"
        
        # 為 backend-service 添加存儲卷
        zeabur volume create \
            --project "$PROJECT_NAME" \
            --service "backend-service" \
            --name "$name" \
            --path "$path" \
            --size "$size"
        
        # 為 backend 添加存儲卷
        zeabur volume create \
            --project "$PROJECT_NAME" \
            --service "backend" \
            --name "$name" \
            --path "$path" \
            --size "$size"
    done
    
    echo -e "${GREEN}✓ 存儲卷配置完成${NC}"
}

# 函數：等待服務就緒
wait_for_services() {
    echo -e "${YELLOW}等待服務就緒...${NC}"
    
    local services=("mariadb" "backend-service" "backend" "frontend" "admin")
    local max_wait=300 # 5分鐘
    local wait_time=0
    
    for service in "${services[@]}"; do
        echo -e "${BLUE}等待服務: $service${NC}"
        
        while [ $wait_time -lt $max_wait ]; do
            local status=$(zeabur service status --project "$PROJECT_NAME" --service "$service" --format json | jq -r '.status')
            
            if [ "$status" = "running" ]; then
                echo -e "${GREEN}✓ 服務就緒: $service${NC}"
                break
            elif [ "$status" = "failed" ]; then
                echo -e "${RED}✗ 服務啟動失敗: $service${NC}"
                return 1
            fi
            
            sleep 10
            wait_time=$((wait_time + 10))
        done
        
        if [ $wait_time -ge $max_wait ]; then
            echo -e "${RED}✗ 服務啟動超時: $service${NC}"
            return 1
        fi
    done
    
    echo -e "${GREEN}✓ 所有服務已就緒${NC}"
}

# 函數：執行健康檢查
health_check() {
    echo -e "${PURPLE}🏥 執行健康檢查${NC}"
    
    local services=("backend-service" "backend" "frontend" "admin")
    local failed_services=()
    
    for service in "${services[@]}"; do
        echo -e "${YELLOW}檢查服務: $service${NC}"
        
        local url=$(zeabur service url --project "$PROJECT_NAME" --service "$service")
        
        if [ "$service" = "backend-service" ] || [ "$service" = "backend" ]; then
            url="$url/health"
        fi
        
        if curl -f -s "$url" > /dev/null; then
            echo -e "${GREEN}✓ 健康檢查通過: $service${NC}"
        else
            echo -e "${RED}✗ 健康檢查失敗: $service${NC}"
            failed_services+=("$service")
        fi
    done
    
    if [ ${#failed_services[@]} -eq 0 ]; then
        echo -e "${GREEN}🎉 所有服務健康檢查通過${NC}"
        return 0
    else
        echo -e "${RED}❌ 健康檢查失敗的服務: ${failed_services[*]}${NC}"
        return 1
    fi
}

# 函數：顯示部署摘要
show_deployment_summary() {
    echo -e "${CYAN}📋 部署摘要${NC}"
    echo ""
    
    echo -e "${BLUE}專案資訊:${NC}"
    echo "  專案名稱: $PROJECT_NAME"
    echo "  部署環境: $DEPLOY_ENV"
    echo "  部署時間: $(date)"
    echo ""
    
    echo -e "${BLUE}服務 URL:${NC}"
    local services=("frontend" "admin" "backend" "backend-service")
    
    for service in "${services[@]}"; do
        local url=$(zeabur service url --project "$PROJECT_NAME" --service "$service" 2>/dev/null || echo "未取得")
        echo "  $service: $url"
    done
    
    echo ""
    echo -e "${BLUE}下一步:${NC}"
    echo "  1. 驗證所有服務正常運行"
    echo "  2. 測試用戶註冊登入功能"
    echo "  3. 檢查檔案上傳功能"
    echo "  4. 配置監控和告警"
    echo "  5. 設定備份策略"
}

# 函數：載入環境變數
load_environment() {
    local env_file=".env.zeabur"
    
    if [ -f "$env_file" ]; then
        echo -e "${YELLOW}載入環境變數: $env_file${NC}"
        set -a
        source "$env_file"
        set +a
    else
        echo -e "${RED}✗ 環境變數檔案不存在: $env_file${NC}"
        echo "請建立 .env.zeabur 檔案並設定必要的環境變數"
        exit 1
    fi
}

# 函數：執行完整部署
deploy_all() {
    echo -e "${PURPLE}🚀 開始完整部署${NC}"
    
    check_prerequisites
    load_environment
    validate_config
    init_deployment
    
    deploy_database
    sleep 30 # 等待資料庫啟動
    
    deploy_backend_services
    deploy_frontend_services
    configure_storage
    
    wait_for_services
    health_check
    
    show_deployment_summary
    
    echo -e "${GREEN}🎉 部署完成！${NC}"
}

# 函數：更新特定服務
update_service() {
    local service_name="$1"
    
    if [ -z "$service_name" ]; then
        echo -e "${RED}請指定要更新的服務名稱${NC}"
        exit 1
    fi
    
    echo -e "${YELLOW}更新服務: $service_name${NC}"
    
    zeabur service redeploy \
        --project "$PROJECT_NAME" \
        --service "$service_name"
    
    echo -e "${GREEN}✓ 服務更新完成: $service_name${NC}"
}

# 函數：查看服務日誌
view_logs() {
    local service_name="$1"
    
    if [ -z "$service_name" ]; then
        echo -e "${YELLOW}可用的服務:${NC}"
        zeabur service list --project "$PROJECT_NAME"
        return
    fi
    
    echo -e "${YELLOW}查看服務日誌: $service_name${NC}"
    zeabur logs --project "$PROJECT_NAME" --service "$service_name" --follow
}

# 函數：檢查部署狀態
check_status() {
    echo -e "${PURPLE}📊 檢查部署狀態${NC}"
    
    echo -e "${BLUE}專案資訊:${NC}"
    zeabur project info --project "$PROJECT_NAME"
    
    echo ""
    echo -e "${BLUE}服務狀態:${NC}"
    zeabur service list --project "$PROJECT_NAME"
    
    echo ""
    echo -e "${BLUE}環境變數:${NC}"
    zeabur env list --project "$PROJECT_NAME"
}

# 主函數
main() {
    local command="${1:-help}"
    local service_name=""
    
    # 解析參數
    while [[ $# -gt 0 ]]; do
        case $1 in
            --project)
                PROJECT_NAME="$2"
                shift 2
                ;;
            --env)
                DEPLOY_ENV="$2"
                shift 2
                ;;
            --token)
                ZEABUR_TOKEN="$2"
                shift 2
                ;;
            --repo)
                GITHUB_REPO="$2"
                shift 2
                ;;
            --service)
                service_name="$2"
                shift 2
                ;;
            *)
                shift
                ;;
        esac
    done
    
    case "$command" in
        "init")
            check_prerequisites
            load_environment
            validate_config
            init_deployment
            ;;
        "deploy")
            deploy_all
            ;;
        "update")
            load_environment
            update_service "$service_name"
            ;;
        "status")
            load_environment
            check_status
            ;;
        "logs")
            load_environment
            view_logs "$service_name"
            ;;
        "health")
            load_environment
            health_check
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