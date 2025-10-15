#!/bin/bash

# ROSCA 系統監控腳本

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
MONITOR_LOG="$PROJECT_DIR/logs/monitor.log"

# 建立日誌目錄
mkdir -p "$PROJECT_DIR/logs"

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 記錄日誌
log_message() {
    local level=$1
    local message=$2
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] [$level] $message" >> "$MONITOR_LOG"
}

# 檢查服務狀態
check_service_status() {
    echo -e "${BLUE}📦 檢查容器狀態${NC}"
    echo "=================================="
    
    local all_healthy=true
    local services=("mariadb" "backend" "frontend" "admin")
    
    for service in "${services[@]}"; do
        local status=$(docker-compose ps --format "table {{.Name}}\t{{.Status}}" | grep "$service" | awk '{print $2}')
        
        if [[ $status == *"Up"* ]] || [[ $status == *"healthy"* ]]; then
            echo -e "✅ $service: ${GREEN}運行中${NC}"
            log_message "INFO" "$service service is running"
        else
            echo -e "❌ $service: ${RED}異常${NC} ($status)"
            log_message "ERROR" "$service service is down: $status"
            all_healthy=false
        fi
    done
    
    echo ""
    return $all_healthy
}

# 檢查資源使用情況
check_resource_usage() {
    echo -e "${BLUE}📊 資源使用情況${NC}"
    echo "=================================="
    
    # Docker 容器資源使用
    echo "容器資源使用:"
    docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.NetIO}}\t{{.BlockIO}}"
    
    echo ""
    
    # 系統資源使用
    echo "系統資源使用:"
    
    # CPU 使用率
    local cpu_usage=$(top -bn1 | grep "Cpu(s)" | awk '{print $2}' | awk -F'%' '{print $1}')
    echo "CPU 使用率: ${cpu_usage}%"
    
    # 記憶體使用率
    local mem_info=$(free | grep Mem)
    local mem_total=$(echo $mem_info | awk '{print $2}')
    local mem_used=$(echo $mem_info | awk '{print $3}')
    local mem_percent=$(( mem_used * 100 / mem_total ))
    echo "記憶體使用率: ${mem_percent}%"
    
    # 磁碟使用率
    echo "磁碟使用率:"
    df -h | grep -E "/$|/var|/home"
    
    # 檢查資源警告
    if (( $(echo "$cpu_usage > 80" | bc -l) )); then
        echo -e "${YELLOW}⚠️ CPU 使用率過高: ${cpu_usage}%${NC}"
        log_message "WARNING" "High CPU usage: ${cpu_usage}%"
    fi
    
    if [ "$mem_percent" -gt 80 ]; then
        echo -e "${YELLOW}⚠️ 記憶體使用率過高: ${mem_percent}%${NC}"
        log_message "WARNING" "High memory usage: ${mem_percent}%"
    fi
    
    echo ""
}

# 檢查網路連通性
check_network_connectivity() {
    echo -e "${BLUE}🌐 網路連通性檢查${NC}"
    echo "=================================="
    
    # 檢查容器間網路
    echo "容器間網路連通性:"
    
    # 前台到後端
    if docker-compose exec -T frontend ping -c 1 backend > /dev/null 2>&1; then
        echo -e "✅ 前台 → 後端: ${GREEN}正常${NC}"
    else
        echo -e "❌ 前台 → 後端: ${RED}異常${NC}"
        log_message "ERROR" "Frontend to backend network connectivity failed"
    fi
    
    # 後台到後端
    if docker-compose exec -T admin ping -c 1 backend > /dev/null 2>&1; then
        echo -e "✅ 後台 → 後端: ${GREEN}正常${NC}"
    else
        echo -e "❌ 後台 → 後端: ${RED}異常${NC}"
        log_message "ERROR" "Admin to backend network connectivity failed"
    fi
    
    # 後端到資料庫
    if docker-compose exec -T backend ping -c 1 mariadb > /dev/null 2>&1; then
        echo -e "✅ 後端 → 資料庫: ${GREEN}正常${NC}"
    else
        echo -e "❌ 後端 → 資料庫: ${RED}異常${NC}"
        log_message "ERROR" "Backend to database network connectivity failed"
    fi
    
    echo ""
}

# 檢查應用程式健康狀態
check_application_health() {
    echo -e "${BLUE}🏥 應用程式健康檢查${NC}"
    echo "=================================="
    
    # 檢查資料庫連線
    echo -n "資料庫連線: "
    if docker-compose exec -T mariadb mysqladmin ping -h localhost -u root -p$DB_ROOT_PASSWORD > /dev/null 2>&1; then
        echo -e "${GREEN}正常${NC}"
    else
        echo -e "${RED}異常${NC}"
        log_message "ERROR" "Database connection failed"
    fi
    
    # 檢查後端 API
    echo -n "後端 API: "
    if curl -f http://localhost:5000/health > /dev/null 2>&1; then
        echo -e "${GREEN}正常${NC}"
    else
        echo -e "${RED}異常${NC}"
        log_message "ERROR" "Backend API health check failed"
    fi
    
    # 檢查前台
    echo -n "前台服務: "
    if curl -f http://localhost > /dev/null 2>&1; then
        echo -e "${GREEN}正常${NC}"
    else
        echo -e "${RED}異常${NC}"
        log_message "ERROR" "Frontend service health check failed"
    fi
    
    # 檢查後台
    echo -n "後台服務: "
    if curl -f http://localhost:8080 > /dev/null 2>&1; then
        echo -e "${GREEN}正常${NC}"
    else
        echo -e "${RED}異常${NC}"
        log_message "ERROR" "Admin service health check failed"
    fi
    
    echo ""
}

# 檢查磁碟空間
check_disk_space() {
    echo -e "${BLUE}💾 磁碟空間檢查${NC}"
    echo "=================================="
    
    # 檢查 Docker volumes
    echo "Docker Volumes 使用情況:"
    docker system df -v | grep -A 20 "VOLUME NAME"
    
    echo ""
    
    # 檢查備份目錄
    if [ -d "$PROJECT_DIR/backups" ]; then
        echo "備份目錄使用情況:"
        du -sh "$PROJECT_DIR/backups"/*/ 2>/dev/null || echo "無備份檔案"
    fi
    
    echo ""
    
    # 檢查日誌目錄
    if [ -d "$PROJECT_DIR/logs" ]; then
        echo "日誌目錄使用情況:"
        du -sh "$PROJECT_DIR/logs" 2>/dev/null || echo "無日誌檔案"
    fi
    
    echo ""
}

# 檢查安全狀態
check_security_status() {
    echo -e "${BLUE}🔒 安全狀態檢查${NC}"
    echo "=================================="
    
    # 檢查容器是否以 root 運行
    echo "容器使用者檢查:"
    for container in $(docker-compose ps -q); do
        local container_name=$(docker inspect --format='{{.Name}}' $container | sed 's/\///')
        local user=$(docker exec $container whoami 2>/dev/null || echo "unknown")
        if [ "$user" = "root" ]; then
            echo -e "⚠️ $container_name: ${YELLOW}以 root 運行${NC}"
            log_message "WARNING" "$container_name is running as root"
        else
            echo -e "✅ $container_name: ${GREEN}非 root 使用者 ($user)${NC}"
        fi
    done
    
    echo ""
    
    # 檢查開放端口
    echo "開放端口檢查:"
    netstat -tlnp 2>/dev/null | grep -E ":80|:8080|:5000|:3306" | while read line; do
        echo "$line"
    done
    
    echo ""
}

# 生成監控報告
generate_report() {
    local report_file="$PROJECT_DIR/logs/monitor_report_$(date +%Y%m%d_%H%M%S).txt"
    
    echo "📋 生成監控報告: $report_file"
    
    {
        echo "ROSCA 系統監控報告"
        echo "=================="
        echo "生成時間: $(date)"
        echo ""
        
        echo "系統概況:"
        echo "--------"
        docker-compose ps
        echo ""
        
        echo "資源使用:"
        echo "--------"
        docker stats --no-stream
        echo ""
        
        echo "磁碟使用:"
        echo "--------"
        df -h
        echo ""
        
        echo "最近錯誤 (最近 1 小時):"
        echo "--------------------"
        docker-compose logs --since=1h 2>&1 | grep -i "error\|exception\|fail" | tail -20
        
    } > "$report_file"
    
    echo "✅ 報告已生成: $report_file"
}

# 主監控函數
run_monitoring() {
    local mode=$1
    
    echo -e "${GREEN}🔍 ROSCA 系統監控${NC}"
    echo "監控時間: $(date)"
    echo "========================================"
    echo ""
    
    case $mode in
        "quick")
            check_service_status
            check_application_health
            ;;
        "full")
            check_service_status
            check_resource_usage
            check_network_connectivity
            check_application_health
            check_disk_space
            check_security_status
            ;;
        "report")
            generate_report
            ;;
        "continuous")
            echo "開始連續監控 (每 30 秒檢查一次，按 Ctrl+C 停止)..."
            while true; do
                clear
                run_monitoring "quick"
                sleep 30
            done
            ;;
        *)
            echo "使用方式: $0 [quick|full|report|continuous]"
            echo ""
            echo "  quick      - 快速檢查 (服務狀態 + 健康檢查)"
            echo "  full       - 完整檢查 (所有項目)"
            echo "  report     - 生成監控報告"
            echo "  continuous - 連續監控模式"
            exit 1
            ;;
    esac
}

# 檢查依賴
if ! command -v docker-compose &> /dev/null; then
    echo "❌ docker-compose 未安裝"
    exit 1
fi

if ! command -v curl &> /dev/null; then
    echo "❌ curl 未安裝"
    exit 1
fi

# 切換到專案目錄
cd "$PROJECT_DIR"

# 載入環境變數
if [ -f .env ]; then
    export $(cat .env | grep -v '^#' | xargs)
fi

# 執行監控
run_monitoring "${1:-quick}"