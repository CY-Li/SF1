-- ROSCA 平安商會系統 - 檢查是否需要初始化資料庫
-- 此腳本用於檢查 Zeabur 資料庫是否需要初始化

USE zeabur;

-- 檢查資料庫是否已經初始化
SELECT 'Checking database initialization status...' as status;

-- 檢查主要表格是否存在
SELECT 
    'Table Existence Check' as check_type,
    CASE 
        WHEN COUNT(*) >= 15 THEN 'TABLES_EXIST'
        ELSE 'TABLES_MISSING'
    END as result,
    COUNT(*) as table_count
FROM information_schema.tables 
WHERE table_schema = 'zeabur';

-- 檢查預設資料是否存在
SELECT 
    'Default Data Check' as check_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM member_master WHERE mm_account = 'admin') > 0 
        THEN 'DATA_EXISTS'
        ELSE 'DATA_MISSING'
    END as result;

-- 綜合判斷
SELECT 
    'Initialization Required' as check_type,
    CASE 
        WHEN (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'zeabur') < 15
        OR (SELECT COUNT(*) FROM member_master WHERE mm_account = 'admin') = 0
        THEN 'YES - Database needs initialization'
        ELSE 'NO - Database already initialized'
    END as result;