#!/bin/bash

# ROSCA Docker 生產環境啟動腳本
echo "🚀 啟動 ROSCA 生產環境..."

# 檢查 Docker 是否運行
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker 未運行，請先啟動 Docker"
    exit 1
fi

# 檢查生產環境配置檔案
if [ ! -f .env.prod ]; then
    echo "❌ 生產環境配置檔案 .env.prod 不存在"
    echo "請複製 .env.example 到 .env.prod 並修改生產環境設定"
    exit 1
fi

# 檢查 SSL 憑證（如果使用 HTTPS）
if [ -d "ssl" ]; then
    echo "🔒 檢查 SSL 憑證..."
    if [ ! -f "ssl/cert.pem" ] || [ ! -f "ssl/key.pem" ]; then
        echo "⚠️ SSL 憑證檔案不完整，請確認 ssl/cert.pem 和 ssl/key.pem 存在"
    fi
fi

# 建立必要的目錄
echo "📁 建立必要的目錄..."
mkdir -p database/data
mkdir -p logs
mkdir -p backups/{database,files,config}

# 設定環境變數檔案
echo "⚙️ 設定生產環境變數..."
export $(cat .env.prod | grep -v '^#' | xargs)

# 停止並移除現有容器（如果存在）
echo "🧹 清理現有容器..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# 拉取最新的基礎映像
echo "📥 拉取最新映像..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml pull

# 建置並啟動服務
echo "🔨 建置並啟動生產環境服務..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d

# 等待服務啟動
echo "⏳ 等待服務啟動..."
sleep 30

# 檢查服務狀態
echo "🔍 檢查服務狀態..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps

# 執行健康檢查
echo "🏥 執行健康檢查..."
sleep 10

# 檢查各服務健康狀態
services=(\"mariadb\" \"backend\" \"frontend\" \"admin\")
all_healthy=true

for service in ${services[@]}; do
    health_status=$(docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps --format \"table {{.Name}}\t{{.Status}}\" | grep $service | awk '{print $2}')
    if [[ $health_status == *\"healthy\"* ]] || [[ $health_status == *\"Up\"* ]]; then
        echo \"✅ $service: 健康\"
    else
        echo \"❌ $service: 異常 ($health_status)\"
        all_healthy=false
    fi
done

if [ \"$all_healthy\" = true ]; then
    echo \"\"
    echo \"🎉 ROSCA 生產環境啟動完成！\"
    echo \"\"
    echo \"🌐 訪問地址:\"
    echo \"   前台: https://yourdomain.com (或 http://localhost)\"
    echo \"   後台: https://yourdomain.com:8080 (或 http://localhost:8080)\"
    echo \"   API: https://yourdomain.com/api (或 http://localhost:5000)\"
    echo \"\"
    echo \"🔒 安全提醒:\"
    echo \"   - 請確認已修改預設密碼\"
    echo \"   - 請確認防火牆設定正確\"
    echo \"   - 請定期備份資料\"
    echo \"\"
    echo \"📋 管理指令:\"
    echo \"   查看日誌: docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f [service_name]\"
    echo \"   停止系統: ./scripts/stop-prod.sh\"
    echo \"   健康檢查: ./scripts/health-check.sh\"
    echo \"   備份系統: ./scripts/backup-full.sh\"
else
    echo \"\"
    echo \"⚠️ 部分服務啟動異常，請檢查日誌:\"
    echo \"docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs\"
    exit 1
fi"