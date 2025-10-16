-- 手動資料庫初始化腳本
-- 如果自動初始化失敗，可以手動執行此腳本

-- 建立資料庫 (如果不存在)
CREATE DATABASE IF NOT EXISTS rosca_db CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

-- 建立使用者 (如果不存在)
CREATE USER IF NOT EXISTS 'rosca_user'@'%' IDENTIFIED BY 'your_secure_password_2024!';

-- 授予權限
GRANT ALL PRIVILEGES ON rosca_db.* TO 'rosca_user'@'%';
FLUSH PRIVILEGES;

-- 使用資料庫
USE rosca_db;

-- 建立基本測試表格
CREATE TABLE IF NOT EXISTS test_connection (
    id INT AUTO_INCREMENT PRIMARY KEY,
    message VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 插入測試資料
INSERT INTO test_connection (message) VALUES ('Database connection test successful');

-- 建立基本的 member_master 表格 (簡化版)
CREATE TABLE IF NOT EXISTS member_master (
    mm_id BIGINT(15) AUTO_INCREMENT PRIMARY KEY,
    mm_account VARCHAR(20) NOT NULL UNIQUE,
    mm_hash_pwd VARCHAR(100) NOT NULL,
    mm_name VARCHAR(50) NOT NULL,
    mm_phone_number VARCHAR(20) NOT NULL UNIQUE,
    mm_role_type CHAR(2) NOT NULL DEFAULT '10',
    mm_status CHAR(2) NOT NULL DEFAULT '10',
    mm_create_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    mm_update_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 插入測試管理員帳號
-- 帳號: admin, 密碼: Admin123456 (BCrypt hash)
INSERT IGNORE INTO member_master (
    mm_id, mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    1, 'admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2', 
    '系統管理員', '0900000000', '20', '10'
);

-- 插入測試使用者帳號  
-- 帳號: 0938766349, 密碼: 123456 (BCrypt hash)
INSERT IGNORE INTO member_master (
    mm_id, mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    2, '0938766349', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '測試使用者', '0938766349', '10', '10'
);

-- 驗證初始化
SELECT 'Manual database initialization completed' as status;
SELECT COUNT(*) as user_count FROM member_master;
SELECT mm_account, mm_name, mm_role_type FROM member_master;