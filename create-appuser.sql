-- 配合 Zeabur 創建 appuser 使用者

-- 步驟 1: 創建 appuser 使用者
CREATE USER IF NOT EXISTS 'appuser'@'%' IDENTIFIED BY 'dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G';

-- 步驟 2: 授予權限
GRANT ALL PRIVILEGES ON zeabur.* TO 'appuser'@'%';
FLUSH PRIVILEGES;

-- 步驟 3: 驗證使用者創建
SELECT User, Host FROM mysql.user WHERE User = 'appuser';
-- 步驟 
4: 建立 member_master 表格
CREATE TABLE IF NOT EXISTS member_master (
    mm_id BIGINT AUTO_INCREMENT PRIMARY KEY,
    mm_account VARCHAR(20) NOT NULL UNIQUE,
    mm_hash_pwd VARCHAR(100) NOT NULL,
    mm_name VARCHAR(50) NOT NULL,
    mm_phone_number VARCHAR(20) NOT NULL UNIQUE,
    mm_role_type CHAR(2) NOT NULL DEFAULT '10',
    mm_status CHAR(2) NOT NULL DEFAULT '10',
    mm_create_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    mm_update_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 步驟 5: 插入管理員帳號 (admin / Admin123456)
INSERT INTO member_master (
    mm_id, mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    1, 'admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2', 
    '系統管理員', '0900000000', '20', '10'
) ON DUPLICATE KEY UPDATE mm_account = mm_account;

-- 步驟 6: 插入測試帳號 (0938766349 / 123456)
INSERT INTO member_master (
    mm_id, mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    2, '0938766349', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '測試使用者', '0938766349', '10', '10'
) ON DUPLICATE KEY UPDATE mm_account = mm_account;

-- 步驟 7: 驗證結果
SELECT 'Setup completed successfully' as status;
SELECT COUNT(*) as user_count FROM member_master;
SELECT mm_account, mm_name, mm_role_type FROM member_master;