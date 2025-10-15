#!/bin/bash

# ROSCA 日誌管理腳本

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
LOGS_DIR="$PROJECT_DIR/logs"

# 建立日誌目錄
mkdir -p "$LOGS_DIR"/{application,nginx,database,system}

show_help() {
    echo "ROSCA 日誌管理工具"
    echo ""
    echo "使用方式:"
    echo "  $0 [選項] [服務名稱]"
    echo ""
    echo "選項:"
    echo "  -f, --follow     即時跟蹤日誌"
    echo "  -t, --tail N     顯示最後 N 行日誌 (預設: 100)"
    echo "  -s, --since      顯示指定時間後的日誌 (例: 1h, 30m, 2024-01-01)"
    echo "  -l, --level      過濾日誌等級 (error, warn, info, debug)"
    echo "  -e, --export     匯出日誌到檔案"
    echo "  -c, --clean      清理舊日誌"
    echo "  -r, --rotate     輪轉日誌檔案"
    echo "  -h, --help       顯示此說明"
    echo ""
    echo "服務名稱:"
    echo "  mariadb          資料庫服務"
    echo "  backend          後端 API 服務"
    echo "  frontend         前台服務"
    echo "  admin            後台管理服務"
    echo "  all              所有服務 (預設)"
    echo ""
    echo "範例:"
    echo "  $0 -f backend                    # 即時跟蹤後端日誌"
    echo "  $0 -t 50 mariadb                # 顯示資料庫最後 50 行日誌"
    echo "  $0 -s 1h -l error               # 顯示最近 1 小時的錯誤日誌"
    echo "  $0 -e backend                   # 匯出後端日誌"
    echo "  $0 -c                           # 清理所有舊日誌"
}

# 解析參數
FOLLOW=false
TAIL_LINES=100
SINCE=""
LEVEL=""
EXPORT=false
CLEAN=false
ROTATE=false
SERVICE="all"

while [[ $# -gt 0 ]]; do
    case $1 in
        -f|--follow)
            FOLLOW=true
            shift
            ;;
        -t|--tail)
            TAIL_LINES="$2"
            shift 2
            ;;
        -s|--since)
            SINCE="$2"
            shift 2
            ;;
        -l|--level)
            LEVEL="$2"
            shift 2
            ;;
        -e|--export)
            EXPORT=true
            shift
            ;;
        -c|--clean)
            CLEAN=true
            shift
            ;;
        -r|--rotate)
            ROTATE=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        mariadb|backend|frontend|admin|all)
            SERVICE="$1"
            shift
            ;;
        *)
            echo "未知選項: $1"
            show_help
            exit 1
            ;;
    esac
done

# 清理舊日誌
clean_logs() {
    echo "🧹 清理舊日誌檔案..."
    
    # 清理 7 天前的應用程式日誌
    find "$LOGS_DIR" -name "*.log" -mtime +7 -delete
    
    # 清理 Docker 日誌
    docker system prune -f --filter "until=168h"
    
    # 清理壓縮的日誌檔案 (30 天前)
    find "$LOGS_DIR" -name "*.gz" -mtime +30 -delete
    
    echo "✅ 日誌清理完成"
}

# 輪轉日誌
rotate_logs() {
    echo "🔄 輪轉日誌檔案..."
    
    DATE=$(date +%Y%m%d_%H%M%S)
    
    # 輪轉各服務日誌
    for service in mariadb backend frontend admin; do
        if [ -f "$LOGS_DIR/${service}.log" ]; then
            mv "$LOGS_DIR/${service}.log" "$LOGS_DIR/${service}_${DATE}.log"
            gzip "$LOGS_DIR/${service}_${DATE}.log"
            echo "✅ ${service} 日誌已輪轉"
        fi
    done
    
    echo "✅ 日誌輪轉完成"
}

# 匯出日誌
export_logs() {
    local service=$1
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local export_file="$LOGS_DIR/export_${service}_${timestamp}.log"
    
    echo "📤 匯出 $service 日誌到 $export_file"
    
    if [ "$service" = "all" ]; then
        docker-compose logs --no-color > "$export_file"
    else
        docker-compose logs --no-color "$service" > "$export_file"
    fi
    
    # 壓縮匯出檔案
    gzip "$export_file"
    echo "✅ 日誌已匯出並壓縮: ${export_file}.gz"
}

# 過濾日誌等級
filter_by_level() {
    local level=$1
    case $level in
        error|ERROR)
            grep -i "error\|exception\|fail"
            ;;
        warn|WARNING)
            grep -i "warn\|warning"
            ;;
        info|INFO)
            grep -i "info"
            ;;
        debug|DEBUG)
            grep -i "debug\|trace"
            ;;
        *)
            cat
            ;;
    esac
}

# 主要日誌顯示功能
show_logs() {
    local service=$1
    local cmd="docker-compose logs --no-color"
    
    # 添加參數
    if [ "$FOLLOW" = true ]; then
        cmd="$cmd -f"
    fi
    
    if [ -n "$TAIL_LINES" ] && [ "$FOLLOW" = false ]; then
        cmd="$cmd --tail=$TAIL_LINES"
    fi
    
    if [ -n "$SINCE" ]; then
        cmd="$cmd --since=$SINCE"
    fi
    
    # 添加服務名稱
    if [ "$service" != "all" ]; then
        cmd="$cmd $service"
    fi
    
    echo "📋 顯示 $service 服務日誌..."
    echo "指令: $cmd"
    echo "----------------------------------------"
    
    # 執行指令並過濾
    if [ -n "$LEVEL" ]; then
        eval "$cmd" | filter_by_level "$LEVEL"
    else
        eval "$cmd"
    fi
}

# 檢查 Docker Compose 是否可用
if ! command -v docker-compose &> /dev/null; then
    echo "❌ docker-compose 未安裝或不在 PATH 中"
    exit 1
fi

# 切換到專案目錄
cd "$PROJECT_DIR"

# 執行相應操作
if [ "$CLEAN" = true ]; then
    clean_logs
elif [ "$ROTATE" = true ]; then
    rotate_logs
elif [ "$EXPORT" = true ]; then
    export_logs "$SERVICE"
else
    show_logs "$SERVICE"
fi