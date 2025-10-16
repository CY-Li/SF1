#!/bin/bash

# ROSCA 平安商會系統 Docker 啟動腳本
# 一鍵啟動整個系統

set -e

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== ROSCA 平安商會系統 Docker 啟動 ===${NC}"
echo "啟動時間: $(date)"
echo ""

# 函數：檢查 Docker 和 Docker Compose
check_prerequisites() {
    echo -e "${YELLOW}檢查必要工具...${NC}"
    
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}✗ Docker 未安裝${NC}"
        echo "請先安裝 Docker: https://docs.docker.com/get-docker/"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        echo -e "${RED}✗ Docker Compose 未安裝${NC}"
        echo "請先安裝 Docker Compose: https://docs.docker.com/compose/install/"
        exit 1
    fi
    
    echo -e "${GREEN}✓ Docker 和 Docker Compose 已安裝${NC}"
}

# 函數：檢查環境變數檔案
check_env_file() {
    echo -e "${YELLOW}檢查環境變數檔案...${NC}"
    
    if [ ! -f ".env" ]; then
        echo -e "${YELLOW}⚠ .env 檔案不存在，使用預設值${NC}"
        echo "建議複製 .env 檔案並修改相關設定"
    else
        echo -e "${GREEN}✓ .env 檔案存在${NC}"
    fi
}

# 函數：檢查必要檔案
check_required_files() {
    echo -e "${YELLOW}檢查必要檔案...${NC}"
    
    local required_files=(
        "docker-compose.yml"
        "database/zeabur/my.cnf"
        "database/zeabur/docker-entrypoint-initdb.d/01-schema.sql"
        "nginx/nginx.conf"
    )
    
    local missing_files=()
    
    for file in "${required_files[@]}"; do
        if [ ! -f "$file" ]; then
            missing_files+=("$file")
        fi
    done
    
    if [ ${#missing_files[@]} -ne 0 ]; then
        echo -e "${RED}✗ 缺少必要檔案:${NC}"
        for file in "${missing_files[@]}"; do
            echo -e "${RED}  - $file${NC}"
        done
        exit 1
    fi
    
    echo -e "${GREEN}✓ 所有必要檔案存在${NC}"
}

# 函數：清理舊容器和映像檔
cleanup_old_containers() {
    echo -e "${YELLOW}清理舊容器...${NC}"
    
    # 停止並移除舊容器
    docker-compose down --remove-orphans 2>/dev/null || true
    
    # 清理未使用的映像檔 (可選)
    if [ "$1" = "--clean-images" ]; then
        echo -e "${YELLOW}清理未使用的映像檔...${NC}"
        docker image prune -f
    fi
    
    echo -e "${GREEN}✓ 清理完成${NC}"
}

# 函數：建置映像檔
build_images() {
    echo -e "${PURPLE}🔨 建置 Docker 映像檔...${NC}"
    
    # 建置所有服務的映像檔
    docker-compose build --no-cache
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ 映像檔建置完成${NC}"
    else
        echo -e "${RED}✗ 映像檔建置失敗${NC}"
        exit 1
    fi
}

# 函數：啟動服務
start_services() {
    echo -e "${PURPLE}🚀 啟動服務...${NC}"
    
    # 啟動所有服務
    docker-compose up -d
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ 服務啟動完成${NC}"
    else
        echo -e "${RED}✗ 服務啟動失敗${NC}"
        exit 1
    fi
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
            if docker-compose ps "$service" | grep -q "Up (healthy)"; then
                echo -e "${GREEN}✓ 服務就緒: $service${NC}"
                break
            elif docker-compose ps "$service" | grep -q "Up"; then
                echo -n "."
            else
                echo -e "${RED}✗ 服務啟動失敗: $service${NC}"
                docker-compose logs "$service"
                return 1
            fi
            
            sleep 5
            wait_time=$((wait_time + 5))
        done
        
        if [ $wait_time -ge $max_wait ]; then
            echo -e "${RED}✗ 服務啟動超時: $service${NC}"
            return 1
        fi
    done
    
    echo -e "${GREEN}✓ 所有服務已就緒${NC}"
}

# 函數：顯示服務狀態
show_service_status() {
    echo -e "${CYAN}📊 服務狀態${NC}"
    docker-compose ps
    echo ""
}

# 函數：顯示存取資訊
show_access_info() {
    echo -e "${CYAN}🌐 存取資訊${NC}"
    echo ""
    echo -e "${BLUE}前台系統:${NC}"
    echo "  直接存取: http://localhost:8080"
    echo "  透過 Nginx: http://localhost"
    echo ""
    echo -e "${BLUE}後台系統:${NC}"
    echo "  直接存取: http://localhost:4200"
    echo "  透過 Nginx: http://admin.localhost (需設定 hosts)"
    echo ""
    echo -e "${BLUE}API 服務:${NC}"
    echo "  API Gateway: http://localhost:5000"
    echo "  Backend Service: http://localhost:5001"
    echo "  健康檢查: http://localhost:5000/health"
    echo ""
    echo -e "${BLUE}資料庫:${NC}"
    echo "  MariaDB: localhost:3306"
    echo "  用戶名: rosca_user"
    echo "  資料庫: rosca_db"
    echo ""
}

# 函數：顯示日誌
show_logs() {
    local service="$1"
    
    if [ -z "$service" ]; then
        echo -e "${YELLOW}顯示所有服務日誌...${NC}"
        docker-compose logs -f
    else
        echo -e "${YELLOW}顯示服務日誌: $service${NC}"
        docker-compose logs -f "$service"
    fi
}

# 函數：停止服務
stop_services() {
    echo -e "${YELLOW}停止服務...${NC}"
    docker-compose down
    echo -e "${GREEN}✓ 服務已停止${NC}"
}

# 函數：重啟服務
restart_services() {
    echo -e "${YELLOW}重啟服務...${NC}"
    docker-compose restart
    echo -e "${GREEN}✓ 服務已重啟${NC}"
}

# 函數：顯示使用說明
show_usage() {
    echo "使用方法: $0 [命令] [選項]"
    echo ""
    echo "命令:"
    echo "  start         啟動所有服務 (預設)"
    echo "  stop          停止所有服務"
    echo "  restart       重啟所有服務"
    echo "  status        顯示服務狀態"
    echo "  logs          顯示服務日誌"
    echo "  build         重新建置映像檔"
    echo "  clean         清理容器和映像檔"
    echo "  help          顯示此說明"
    echo ""
    echo "選項:"
    echo "  --clean-images    清理未使用的映像檔"
    echo "  --no-build        跳過建置步驟"
    echo "  --service NAME    指定特定服務"
    echo ""
    echo "範例:"
    echo "  $0 start                    # 啟動所有服務"
    echo "  $0 logs --service backend   # 查看後端服務日誌"
    echo "  $0 build                    # 重新建置映像檔"
    echo "  $0 clean --clean-images     # 清理容器和映像檔"
}

# 主函數
main() {
    local command="${1:-start}"
    local service_name=""
    local no_build=false
    local clean_images=false
    
    # 解析參數
    while [[ $# -gt 0 ]]; do
        case $1 in
            --service)
                service_name="$2"
                shift 2
                ;;
            --no-build)
                no_build=true
                shift
                ;;
            --clean-images)
                clean_images=true
                shift
                ;;
            *)
                shift
                ;;
        esac
    done
    
    case "$command" in
        "start")
            check_prerequisites
            check_env_file
            check_required_files
            
            if [ "$no_build" = false ]; then
                build_images
            fi
            
            start_services
            wait_for_services
            show_service_status
            show_access_info
            
            echo -e "${GREEN}🎉 ROSCA 系統啟動完成！${NC}"
            ;;
        "stop")
            stop_services
            ;;
        "restart")
            restart_services
            ;;
        "status")
            show_service_status
            ;;
        "logs")
            show_logs "$service_name"
            ;;
        "build")
            check_prerequisites
            build_images
            ;;
        "clean")
            cleanup_old_containers "$clean_images"
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