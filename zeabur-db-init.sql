-- Zeabur 資料庫初始化腳本
-- 適用於 Zeabur 預設的 MariaDB 服務 (資料庫名稱: zeabur, 使用者: root)

-- 使用 Zeabur 預設資料庫
USE zeabur;

-- 建立基本的 member_master 表格
CREATE TABLE IF NOT EXISTS member_master (
    mm_id BIGINT(15) AUTO_INCREMENT PRIMARY KEY,
    mm_account VARCHAR(20) NOT NULL UNIQUE,
    mm_hash_pwd VARCHAR(100) NOT NULL,
    mm_name VARCHAR(50) NOT NULL,
    mm_phone_number VARCHAR(20) NOT NULL UNIQUE,
    mm_role_type CHAR(2) NOT NULL DEFAULT '10' COMMENT '10-一般會員, 20-管理員',
    mm_status CHAR(2) NOT NULL DEFAULT '10' COMMENT '10-正常, 20-停用',
    mm_create_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    mm_update_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立公告板表格
CREATE TABLE IF NOT EXISTS announcement_board (
    ab_id BIGINT(15) AUTO_INCREMENT PRIMARY KEY,
    ab_title VARCHAR(30) NOT NULL COMMENT '標題',
    ab_content VARCHAR(300) NULL COMMENT '內容',
    ab_image_path VARCHAR(200) NULL COMMENT '圖片路徑',
    ab_status CHAR(2) NOT NULL DEFAULT '20' COMMENT '10-開啟, 20-關閉',
    ab_datetime DATETIME NOT NULL COMMENT '發布時間',
    ab_create_member BIGINT(15) NOT NULL,
    ab_create_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ab_update_member BIGINT(15) NOT NULL,
    ab_update_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
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

-- 插入範例公告
INSERT IGNORE INTO announcement_board (
    ab_id, ab_title, ab_content, ab_status, ab_datetime, ab_create_member, ab_update_member
) VALUES 
(1, '歡迎使用平安商會系統', '歡迎使用平安商會標會系統，請先完成 KYC 認證後即可開始參與標會活動。', '10', NOW(), 1, 1),
(2, '系統維護通知', '系統將於每週日凌晨 2:00-4:00 進行例行維護，期間可能無法正常使用。', '10', NOW(), 1, 1);

-- 驗證初始化結果
SELECT 'Zeabur database initialization completed' as status;
SELECT COUNT(*) as user_count FROM member_master;
SELECT mm_account, mm_name, mm_role_type FROM member_master;
SELECT COUNT(*) as announcement_count FROM announcement_board;