#!/bin/bash
# MariaDB 健康檢查腳本

set -e

# 檢查 MariaDB 服務是否運行
if ! mysqladmin ping -h localhost -u root -p"${MYSQL_ROOT_PASSWORD}" --silent; then
    echo "MariaDB ping failed"
    exit 1
fi

# 檢查資料庫是否存在
if ! mysql -h localhost -u root -p"${MYSQL_ROOT_PASSWORD}" -e "USE ${MYSQL_DATABASE};" 2>/dev/null; then
    echo "Database ${MYSQL_DATABASE} not accessible"
    exit 1
fi

# 檢查關鍵表是否存在
if ! mysql -h localhost -u root -p"${MYSQL_ROOT_PASSWORD}" -e "SELECT 1 FROM ${MYSQL_DATABASE}.member_master LIMIT 1;" 2>/dev/null; then
    echo "Key tables not found or not accessible"
    exit 1
fi

# 檢查預設使用者是否存在
if ! mysql -h localhost -u root -p"${MYSQL_ROOT_PASSWORD}" -e "SELECT mm_id FROM ${MYSQL_DATABASE}.member_master WHERE mm_account='0938766349' LIMIT 1;" 2>/dev/null | grep -q "1"; then
    echo "Default user not found"
    exit 1
fi

echo "MariaDB health check passed"
exit 0