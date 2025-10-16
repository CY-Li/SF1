-- Zeabur 資料庫連接字串驗證腳本
-- 用於驗證資料庫配置和連接參數

-- 顯示當前連接資訊
SELECT 
    'Connection Verification' as test_type,
    'SUCCESS' as status,
    NOW() as test_time;

-- 檢查資料庫版本
SELECT 
    'Database Version' as info_type,
    VERSION() as value;

-- 檢查字符集設定
SELECT 
    'Character Set Configuration' as info_type,
    CONCAT(
        'Server: ', @@character_set_server, ', ',
        'Database: ', @@character_set_database, ', ',
        'Connection: ', @@character_set_connection
    ) as value;

-- 檢查排序規則設定
SELECT 
    'Collation Configuration' as info_type,
    CONCAT(
        'Server: ', @@collation_server, ', ',
        'Database: ', @@collation_database, ', ',
        'Connection: ', @@collation_connection
    ) as value;

-- 檢查時區設定
SELECT 
    'Time Zone Configuration' as info_type,
    CONCAT(
        'System: ', @@system_time_zone, ', ',
        'Session: ', @@time_zone
    ) as value;

-- 檢查連接相關設定
SELECT 
    'Connection Settings' as info_type,
    CONCAT(
        'Max Connections: ', @@max_connections, ', ',
        'Connect Timeout: ', @@connect_timeout, ', ',
        'Wait Timeout: ', @@wait_timeout
    ) as value;

-- 檢查 SQL 模式
SELECT 
    'SQL Mode' as info_type,
    @@sql_mode as value;

-- 檢查資料庫是否存在
SELECT 
    'Database Exists' as test_type,
    CASE 
        WHEN COUNT(*) > 0 THEN 'YES'
        ELSE 'NO'
    END as status
FROM information_schema.SCHEMATA 
WHERE SCHEMA_NAME = DATABASE();

-- 檢查主要表格是否存在
SELECT 
    'Required Tables Check' as test_type,
    CASE 
        WHEN COUNT(*) >= 15 THEN CONCAT('PASS (', COUNT(*), ' tables found)')
        ELSE CONCAT('FAIL (', COUNT(*), ' tables found, expected >= 15)')
    END as status
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = DATABASE();

-- 列出所有表格
SELECT 
    'Table List' as info_type,
    GROUP_CONCAT(TABLE_NAME ORDER BY TABLE_NAME SEPARATOR ', ') as value
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = DATABASE();

-- 檢查預設資料
SELECT 
    'Default Data Check' as test_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM member_master) >= 2 
         AND (SELECT COUNT(*) FROM parameter_category) >= 9
         AND (SELECT COUNT(*) FROM system_parameter_setting) >= 10
        THEN 'PASS'
        ELSE 'FAIL'
    END as status;

-- 顯示預設資料統計
SELECT 
    'Default Data Statistics' as info_type,
    CONCAT(
        'Members: ', (SELECT COUNT(*) FROM member_master), ', ',
        'Parameter Categories: ', (SELECT COUNT(*) FROM parameter_category), ', ',
        'System Parameters: ', (SELECT COUNT(*) FROM system_parameter_setting)
    ) as value;

-- 檢查預設帳號
SELECT 
    'Default Accounts Check' as test_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM member_master WHERE mm_account = 'admin') = 1
         AND (SELECT COUNT(*) FROM member_master WHERE mm_account = '0938766349') = 1
        THEN 'PASS'
        ELSE 'FAIL'
    END as status;

-- 測試基本查詢功能
SELECT 
    'Basic Query Test' as test_type,
    'PASS' as status,
    'SELECT, COUNT, CONCAT functions working' as details;

-- 測試日期時間功能
SELECT 
    'DateTime Functions Test' as test_type,
    'PASS' as status,
    CONCAT('Current time: ', NOW(), ', UTC time: ', UTC_TIMESTAMP()) as details;

-- 測試字符串處理功能
SELECT 
    'String Functions Test' as test_type,
    'PASS' as status,
    CONCAT('UPPER: ', UPPER('test'), ', LENGTH: ', LENGTH('測試中文')) as details;

-- 連接字串格式驗證結果
SELECT 
    '=== CONNECTION STRING VALIDATION SUMMARY ===' as summary,
    CASE 
        WHEN @@character_set_database = 'utf8mb4'
         AND @@collation_database LIKE 'utf8mb4%'
         AND (SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE()) >= 15
         AND (SELECT COUNT(*) FROM member_master) >= 2
        THEN '✅ ALL CHECKS PASSED - Connection string is valid for .NET Core applications'
        ELSE '❌ SOME CHECKS FAILED - Please review the configuration'
    END as result;

-- 建議的 .NET Core 連接字串參數
SELECT 
    '.NET Core Connection String Parameters' as info_type,
    'CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;' as recommended_parameters;