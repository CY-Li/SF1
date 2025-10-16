-- Zeabur MariaDB - 創建 appuser 和初始化資料庫
-- 在 Zeabur MariaDB 控制台執行此腳本

-- 1. 創建 appuser 使用者
CREATE USER IF NOT EXISTS 'appuser'@'%' IDENTIFIED BY 'dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G';

-- 2. 授予所有權限
GRANT ALL PRIVILEGES ON zeabur.* TO 'appuser'@'%';
FLUSH PRIVILEGES;

-- 3. 驗證使用者創建
SELECT User, Host FROM mysql.user WHERE User = 'appuser';

-- 4. 創建 member_master 表格（如果不存在）
CREATE TABLE IF NOT EXISTS `member_master` (
  `mm_id` int(11) NOT NULL AUTO_INCREMENT,
  `mm_account` varchar(50) NOT NULL,
  `mm_hash_pwd` varchar(255) NOT NULL,
  `mm_name` varchar(100) DEFAULT NULL,
  `mm_phone_number` varchar(20) DEFAULT NULL,
  `mm_role_type` varchar(10) DEFAULT '10',
  `mm_status` varchar(10) DEFAULT '10',
  `mm_create_time` datetime DEFAULT CURRENT_TIMESTAMP,
  `mm_update_time` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`mm_id`),
  UNIQUE KEY `uk_mm_account` (`mm_account`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 5. 插入測試帳號（密碼: Admin123456）
INSERT IGNORE INTO member_master (
    mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    'admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '系統管理員', '0938766349', '10', '10'
);

-- 6. 驗證資料插入成功
SELECT mm_id, mm_account, mm_name, mm_role_type FROM member_master;

-- 7. 顯示權限確認
SHOW GRANTS FOR 'appuser'@'%';

SELECT 'AppUser 創建完成！' as status;