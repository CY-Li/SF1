-- Zeabur MariaDB 完整初始化腳本
-- 在 Zeabur MariaDB 控制台執行此腳本

-- 使用 zeabur 資料庫
USE zeabur;

-- 設定字符集
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- 建立會員主檔表 (完整版本)
CREATE TABLE IF NOT EXISTS `member_master` (
    `mm_id` bigint(15) NOT NULL AUTO_INCREMENT,
    `mm_account` varchar(20) COMMENT '帳號(現在是手機號碼)' NOT NULL,
    `mm_hash_pwd` varchar(100) COMMENT '雜湊密碼' NOT NULL,
    `mm_2nd_hash_pwd` varchar(100) COMMENT '第二組密碼，標會用' NULL,
    `mm_name` varchar(30) COMMENT '會員真實姓名' NULL,
    `mm_introduce_user` varchar(10) NULL,
    `mm_introduce_code` bigint(15) COMMENT '介紹人' NOT NULL DEFAULT '0',
    `mm_invite_user` varchar(10) NULL,
    `mm_invite_code` bigint(15) COMMENT '邀請碼-Invitation code' NOT NULL DEFAULT '0',
    `mm_gender` char(1) COMMENT '性別(1:男性、2:女性)' NULL,
    `mm_country_code` varchar(10) COMMENT '國碼' NULL,
    `mm_personal_id` char(10) COMMENT '身分證字號' NULL,
    `mm_phone_number` varchar(15) COMMENT '電話號碼(目前是手機號碼)' NULL,
    `mm_mw_address` varchar(150) COMMENT '錢包地址' NULL,
    `mm_email` varchar(50) COMMENT '信箱' NULL,
    `mm_bank_code` varchar(10) NULL DEFAULT '銀行代碼',
    `mm_bank_account` varchar(25) COMMENT '銀行帳號' NULL,
    `mm_bank_account_name` varchar(20) COMMENT '戶名' NULL,
    `mm_branch` varchar(10) COMMENT '分行' NULL,
    `mm_beneficiary_name` varchar(30) NULL DEFAULT '受益人姓名',
    `mm_beneficiary_phone` varchar(15) NULL DEFAULT '受益人電話',
    `mm_beneficiary_relationship` varchar(30) NULL DEFAULT '受益人關係',
    `mm_level` char(1) COMMENT '會員等級(暫時沒用)' NOT NULL DEFAULT '0',
    `mm_role_type` char(1) COMMENT '角色權限:1-使用者、2-管理員' NOT NULL DEFAULT '1',
    `mm_status` char(1) COMMENT '帳號是否可以正常使用' NOT NULL DEFAULT 'Y',
    `mm_kyc_id` bigint(15) COMMENT '最新通過認證編號，所以會存目前是哪個kyc' NOT NULL DEFAULT '0',
    `mm_kyc` char(1) COMMENT 'KYC審核，沒有審核不能標會' NOT NULL DEFAULT 'N',
    `mm_create_member` bigint(15) NOT NULL DEFAULT '1',
    `mm_create_datetime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `mm_update_member` bigint(15) NOT NULL DEFAULT '1',
    `mm_update_datetime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY(`mm_id`),
    UNIQUE KEY `uk_mm_account` (`mm_account`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 插入測試管理員帳號 (密碼: Admin123456)
INSERT IGNORE INTO member_master (
    mm_account, 
    mm_hash_pwd, 
    mm_name, 
    mm_phone_number, 
    mm_role_type, 
    mm_status,
    mm_kyc,
    mm_create_member,
    mm_update_member
) VALUES (
    'admin', 
    '1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A',
    '系統管理員', 
    '0938766349', 
    '2', 
    'Y',
    'Y',
    1,
    1
);

-- 插入測試使用者帳號 (密碼: Admin123456)
INSERT IGNORE INTO member_master (
    mm_account, 
    mm_hash_pwd, 
    mm_name, 
    mm_phone_number, 
    mm_role_type, 
    mm_status,
    mm_kyc,
    mm_create_member,
    mm_update_member
) VALUES (
    '0938766349', 
    '1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A',
    '測試使用者', 
    '0938766349', 
    '1', 
    'Y',
    'Y',
    1,
    1
);

-- 驗證資料插入成功
SELECT 
    mm_id, 
    mm_account, 
    mm_name, 
    mm_role_type, 
    mm_status,
    mm_kyc,
    mm_create_datetime
FROM member_master;

-- 檢查表格結構
DESCRIBE member_master;

SELECT 'Zeabur 資料庫初始化完成！' as status;