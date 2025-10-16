-- ROSCA 平安商會系統 - Zeabur 完整資料庫初始化腳本
-- 此腳本包含資料庫結構建立和預設資料載入

-- 設定字符集和排序規則
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- 使用 zeabur 資料庫
USE zeabur;

-- 顯示初始化開始訊息
SELECT 'Starting ROSCA system database initialization for Zeabur deployment...' as status;

-- 檢查資料庫是否已經初始化
SET @initialized = (SELECT COUNT(*) FROM information_schema.tables 
                   WHERE table_schema = 'zeabur' AND table_name = 'member_master');

-- 如果已經初始化，顯示訊息並退出
SELECT CASE 
    WHEN @initialized > 0 THEN 'Database already initialized. Skipping initialization.'
    ELSE 'Database not initialized. Starting initialization process...'
END as initialization_status;

-- 執行初始化腳本（包含在同一個檔案中）
SOURCE database/zeabur/zeabur-init.sql;
SOURCE database/zeabur/zeabur-default-data.sql;

-- 最終驗證
SELECT 'Verifying database initialization...' as status;

SELECT 
    'Database Verification Results' as check_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'zeabur') >= 15
        AND (SELECT COUNT(*) FROM member_master) >= 2
        AND (SELECT COUNT(*) FROM parameter_category) >= 9
        AND (SELECT COUNT(*) FROM system_parameter_setting) >= 10
        THEN 'SUCCESS - Database initialized successfully for Zeabur deployment'
        ELSE 'FAILED - Database initialization incomplete'
    END as result;

-- 顯示資料庫統計
SELECT 'Database Statistics' as info_type;
SELECT 
    'Tables' as item,
    COUNT(*) as count
FROM information_schema.tables 
WHERE table_schema = 'zeabur';

SELECT 
    'Members' as item,
    COUNT(*) as count
FROM member_master;

SELECT 
    'Parameter Categories' as item,
    COUNT(*) as count
FROM parameter_category;

SELECT 
    'System Parameters' as item,
    COUNT(*) as count
FROM system_parameter_setting;

SELECT 
    'Announcements' as item,
    COUNT(*) as count
FROM announcement_board;

-- 顯示預設帳號資訊
SELECT 'Default Account Information' as info_type;
SELECT 
    mm_account as account,
    mm_name as name,
    CASE mm_role_type 
        WHEN '10' THEN 'User'
        WHEN '20' THEN 'Admin'
        ELSE 'Unknown'
    END as role,
    mm_status as status
FROM member_master
ORDER BY mm_id;

SELECT 'ROSCA system database initialization completed successfully!' as final_status;