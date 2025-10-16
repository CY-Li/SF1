-- ROSCA 平安商會系統資料庫驗證腳本 (Zeabur 版本)
-- 用於驗證資料庫初始化是否成功

USE rosca_db;

-- 檢查字符集設定
SELECT 
    'Character Set Verification' as check_type,
    @@character_set_server as server_charset,
    @@collation_server as server_collation,
    @@character_set_database as db_charset,
    @@collation_database as db_collation;

-- 檢查所有表是否建立成功
SELECT 
    'Table Count Verification' as check_type,
    COUNT(*) as total_tables
FROM information_schema.tables 
WHERE table_schema = 'rosca_db';

-- 列出所有表
SELECT 
    'Tables List' as check_type,
    table_name,
    engine,
    table_collation
FROM information_schema.tables 
WHERE table_schema = 'rosca_db'
ORDER BY table_name;

-- 檢查參數分類資料
SELECT 
    'Parameter Category Data' as check_type,
    COUNT(*) as total_categories
FROM parameter_category;

-- 檢查系統參數設定資料
SELECT 
    'System Parameter Data' as check_type,
    COUNT(*) as total_parameters
FROM system_parameter_setting;

-- 檢查公告資料
SELECT 
    'Announcement Data' as check_type,
    COUNT(*) as total_announcements
FROM announcement_board;

-- 檢查會員資料
SELECT 
    'Member Data' as check_type,
    COUNT(*) as total_members,
    SUM(CASE WHEN mm_role_type = '20' THEN 1 ELSE 0 END) as admin_count,
    SUM(CASE WHEN mm_role_type = '10' THEN 1 ELSE 0 END) as user_count
FROM member_master;

-- 檢查會員餘額資料
SELECT 
    'Member Balance Data' as check_type,
    COUNT(*) as total_balances,
    SUM(mb_balance) as total_balance_amount
FROM member_balance;

-- 檢查交易記錄
SELECT 
    'Transaction Record Data' as check_type,
    COUNT(*) as total_transactions,
    SUM(tr_amount) as total_transaction_amount
FROM transaction_record;

-- 檢查標會資料
SELECT 
    'Tender Master Data' as check_type,
    COUNT(*) as total_tenders
FROM tender_master;

-- 檢查外鍵約束
SELECT 
    'Foreign Key Constraints' as check_type,
    COUNT(*) as total_constraints
FROM information_schema.key_column_usage 
WHERE referenced_table_schema = 'rosca_db';

-- 檢查索引
SELECT 
    'Index Verification' as check_type,
    table_name,
    index_name,
    column_name,
    non_unique
FROM information_schema.statistics 
WHERE table_schema = 'rosca_db'
ORDER BY table_name, index_name;

-- 檢查觸發器
SELECT 
    'Trigger Verification' as check_type,
    trigger_name,
    event_manipulation,
    event_object_table
FROM information_schema.triggers 
WHERE trigger_schema = 'rosca_db';

-- 最終驗證結果
SELECT 
    'Database Initialization Status' as check_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'rosca_db') >= 15
        AND (SELECT COUNT(*) FROM member_master) >= 2
        AND (SELECT COUNT(*) FROM parameter_category) >= 9
        AND (SELECT COUNT(*) FROM system_parameter_setting) >= 10
        THEN 'SUCCESS - Database initialized successfully'
        ELSE 'FAILED - Database initialization incomplete'
    END as status;