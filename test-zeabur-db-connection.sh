#!/bin/bash

# 測試 Zeabur 資料庫連接腳本
echo "🔍 測試 Zeabur 資料庫連接..."

# 設定連接參數
DB_HOST="rosca-mariadb"
DB_PORT="3306"
DB_USER="root"
DB_PASSWORD="rosca_root_2024!"
DB_NAME="zeabur"

echo "📋 連接資訊:"
echo "  主機: $DB_HOST"
echo "  端口: $DB_PORT"
echo "  用戶: $DB_USER"
echo "  資料庫: $DB_NAME"
echo ""

# 測試連接字串格式
CONNECTION_STRING="Server=$DB_HOST;Port=$DB_PORT;User Id=$DB_USER;Password=$DB_PASSWORD;Database=$DB_NAME;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;"

echo "🔗 完整連接字串:"
echo "$CONNECTION_STRING"
echo ""

# 如果在容器內，測試實際連接
if command -v mysql &> /dev/null; then
    echo "🧪 測試 MySQL 連接..."
    mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -p"$DB_PASSWORD" -e "SELECT 'Connection successful!' as status, NOW() as current_time;" "$DB_NAME"
    
    if [ $? -eq 0 ]; then
        echo "✅ 資料庫連接成功！"
        
        echo ""
        echo "📊 檢查資料表..."
        mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -p"$DB_PASSWORD" -e "SHOW TABLES;" "$DB_NAME"
        
        echo ""
        echo "👥 檢查會員資料..."
        mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -p"$DB_PASSWORD" -e "SELECT mm_id, mm_account, mm_name, mm_role_type FROM member_master LIMIT 5;" "$DB_NAME"
        
    else
        echo "❌ 資料庫連接失敗！"
        echo ""
        echo "🔧 可能的解決方案:"
        echo "1. 檢查 MariaDB 服務是否正在運行"
        echo "2. 確認連接參數是否正確"
        echo "3. 檢查網路連接"
        echo "4. 驗證用戶權限"
    fi
else
    echo "⚠️  MySQL 客戶端未安裝，無法測試實際連接"
    echo "請在有 MySQL 客戶端的環境中運行此腳本"
fi

echo ""
echo "🚀 部署建議:"
echo "1. 使用 zeabur-fixed.json 進行部署"
echo "2. 確保 MariaDB 服務先啟動"
echo "3. 檢查環境變數是否正確設定"
echo "4. 監控應用程式日誌"