-- ROSCA 平安商會系統資料庫結構初始化腳本 (Zeabur 版本)
-- 此腳本會在 MariaDB 11.3.2 容器首次啟動時自動執行
-- 針對 Zeabur 環境優化

-- 設定字符集和排序規則
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;
SET collation_connection = utf8mb4_general_ci;

-- 使用環境變數指定的資料庫名稱
USE rosca_db;

-- 建立公告板表
CREATE TABLE IF NOT EXISTS `announcement_board` (
    `ab_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `ab_title` varchar(30) COMMENT '標題' NOT NULL,
    `ab_content` varchar(300) COMMENT '內容' NULL,
    `ab_image` longblob COMMENT '圖片檔案' NULL,
    `ab_image_path` varchar(200) COMMENT '圖片檔案路徑，可能只存檔案名稱或是網址' NULL,
    `ab_image_name` varchar(100) NULL,
    `ab_status` char(2) COMMENT '10:開啟，20:關閉' NOT NULL DEFAULT '20',
    `ab_datetime` datetime COMMENT '發布時間' NOT NULL,
    `ab_create_member` bigint(15) NOT NULL,
    `ab_create_datetime` datetime NOT NULL,
    `ab_update_member` bigint(15) NOT NULL,
    `ab_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`ab_id`),
    INDEX `idx_ab_status` (`ab_status`),
    INDEX `idx_ab_datetime` (`ab_datetime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立存款申請表
CREATE TABLE IF NOT EXISTS `apply_deposit` (
    `ad_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `ad_mm_id` bigint(15) COMMENT '存款人' NOT NULL,
    `ad_amount` int(15) COMMENT '存款金額' NOT NULL,
    `ad_key` varchar(200) COMMENT '存款產生的KEY(要讓我們才查得到)' NULL,
    `ad_url` varchar(200) COMMENT '存款網址(現在沒用到，想說QR CODE可能要輸入網址才能存款)' NULL,
    `ad_picture` longblob COMMENT '圖片(如果存款會需要QR CODE圖片就存這裡)' NULL,
    `ad_akc_mw_address` varchar(150) COMMENT '錢包地址' NULL,
    `ad_file_name` varchar(100) COMMENT '上傳檔案名稱或是轉帳憑證存檔名' NOT NULL,
    `ad_status` char(2) COMMENT '申請狀態:10-待會員存款、11-已輸入存款金額、12-申請退回、13-輸入完成' NOT NULL DEFAULT '10',
    `ad_type` char(2) COMMENT '存款類型:10-虛擬幣、50-銀行帳戶' NULL,
    `ad_kyc_id` bigint(15) NOT NULL,
    `ad_create_member` bigint(15) NOT NULL,
    `ad_create_datetime` datetime NOT NULL,
    `ad_update_member` bigint(15) NOT NULL,
    `ad_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`ad_id`),
    INDEX `idx_ad_mm_id` (`ad_mm_id`),
    INDEX `idx_ad_status` (`ad_status`),
    INDEX `idx_ad_create_datetime` (`ad_create_datetime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立KYC認證申請表
CREATE TABLE IF NOT EXISTS `apply_kyc_certification` (
    `akc_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `akc_mm_id` bigint(15) COMMENT 'KYC申請人' NOT NULL,
    `akc_id_card_front` longblob COMMENT '身分證正面' NULL,
    `akc_id_card_back` longblob COMMENT '身分證背面' NULL,
    `akc_id_card_front_name` varchar(100) COMMENT '身分證正面檔案名稱' NULL,
    `akc_id_card_back_name` varchar(100) COMMENT '身分證背面檔案名稱' NULL,
    `akc_bank_book` longblob COMMENT '存摺封面' NULL,
    `akc_bank_book_name` varchar(100) COMMENT '存摺封面檔案名稱' NULL,
    `akc_mw_address` varchar(150) COMMENT '錢包地址' NULL,
    `akc_status` char(2) COMMENT '申請狀態:10-待審核、20-審核通過、30-審核不通過' NOT NULL DEFAULT '10',
    `akc_review_comment` varchar(500) COMMENT '審核備註' NULL,
    `akc_create_member` bigint(15) NOT NULL,
    `akc_create_datetime` datetime NOT NULL,
    `akc_update_member` bigint(15) NOT NULL,
    `akc_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`akc_id`),
    INDEX `idx_akc_mm_id` (`akc_mm_id`),
    INDEX `idx_akc_status` (`akc_status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立提款申請表
CREATE TABLE IF NOT EXISTS `apply_withdraw` (
    `aw_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `aw_mm_id` bigint(15) COMMENT '提款人' NOT NULL,
    `aw_amount` int(15) COMMENT '提款金額' NOT NULL,
    `aw_bank_code` varchar(10) COMMENT '銀行代碼' NULL,
    `aw_bank_account` varchar(20) COMMENT '銀行帳號' NULL,
    `aw_bank_account_name` varchar(50) COMMENT '銀行帳戶名稱' NULL,
    `aw_status` char(2) COMMENT '申請狀態:10-待審核、20-審核通過、30-審核不通過、40-已撥款' NOT NULL DEFAULT '10',
    `aw_review_comment` varchar(500) COMMENT '審核備註' NULL,
    `aw_create_member` bigint(15) NOT NULL,
    `aw_create_datetime` datetime NOT NULL,
    `aw_update_member` bigint(15) NOT NULL,
    `aw_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`aw_id`),
    INDEX `idx_aw_mm_id` (`aw_mm_id`),
    INDEX `idx_aw_status` (`aw_status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立抽籤記錄表
CREATE TABLE IF NOT EXISTS `lottery_record` (
    `lr_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `lr_tm_id` bigint(15) COMMENT '標會ID' NOT NULL,
    `lr_period` int(3) COMMENT '期數' NOT NULL,
    `lr_winner_mm_id` bigint(15) COMMENT '得標人' NOT NULL,
    `lr_amount` int(15) COMMENT '得標金額' NOT NULL,
    `lr_datetime` datetime COMMENT '抽籤時間' NOT NULL,
    `lr_create_member` bigint(15) NOT NULL,
    `lr_create_datetime` datetime NOT NULL,
    `lr_update_member` bigint(15) NOT NULL,
    `lr_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`lr_id`),
    INDEX `idx_lr_tm_id` (`lr_tm_id`),
    INDEX `idx_lr_period` (`lr_period`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立會員餘額表
CREATE TABLE IF NOT EXISTS `member_balance` (
    `mb_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `mb_mm_id` bigint(15) COMMENT '會員ID' NOT NULL,
    `mb_sp_id` bigint(15) COMMENT '餘額類型' NOT NULL,
    `mb_balance` decimal(15,2) COMMENT '餘額' NOT NULL DEFAULT 0.00,
    `mb_frozen_balance` decimal(15,2) COMMENT '凍結餘額' NOT NULL DEFAULT 0.00,
    `mb_total_deposit` decimal(15,2) COMMENT '總存款' NOT NULL DEFAULT 0.00,
    `mb_total_withdraw` decimal(15,2) COMMENT '總提款' NOT NULL DEFAULT 0.00,
    `mb_last_transaction_datetime` datetime COMMENT '最後交易時間' NULL,
    `mb_create_member` bigint(15) NOT NULL,
    `mb_create_datetime` datetime NOT NULL,
    `mb_update_member` bigint(15) NOT NULL,
    `mb_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`mb_id`),
    UNIQUE KEY `uk_mb_mm_sp` (`mb_mm_id`, `mb_sp_id`),
    INDEX `idx_mb_mm_id` (`mb_mm_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立會員主檔表
CREATE TABLE IF NOT EXISTS `member_master` (
    `mm_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `mm_account` varchar(20) COMMENT '帳號' NOT NULL,
    `mm_hash_pwd` varchar(100) COMMENT '密碼雜湊' NOT NULL,
    `mm_2nd_hash_pwd` varchar(100) COMMENT '二級密碼雜湊' NULL,
    `mm_name` varchar(50) COMMENT '姓名' NOT NULL,
    `mm_introduce_user` bigint(15) COMMENT '介紹人' NULL,
    `mm_introduce_code` int(10) COMMENT '介紹碼' NOT NULL DEFAULT 0,
    `mm_invite_user` bigint(15) COMMENT '邀請人' NULL,
    `mm_invite_code` int(10) COMMENT '邀請碼' NOT NULL DEFAULT 0,
    `mm_gender` char(1) COMMENT '性別:1-男、2-女' NULL,
    `mm_country_code` varchar(5) COMMENT '國家代碼' NULL,
    `mm_personal_id` varchar(20) COMMENT '身分證字號' NULL,
    `mm_phone_number` varchar(20) COMMENT '手機號碼' NOT NULL,
    `mm_mw_address` varchar(150) COMMENT '錢包地址' NULL,
    `mm_email` varchar(100) COMMENT '電子郵件' NULL,
    `mm_bank_code` varchar(10) COMMENT '銀行代碼' NULL,
    `mm_bank_account` varchar(20) COMMENT '銀行帳號' NULL,
    `mm_bank_account_name` varchar(50) COMMENT '銀行帳戶名稱' NULL,
    `mm_branch` varchar(100) COMMENT '分行' NULL,
    `mm_beneficiary_name` varchar(50) COMMENT '受益人姓名' NULL,
    `mm_beneficiary_phone` varchar(20) COMMENT '受益人電話' NULL,
    `mm_beneficiary_relationship` varchar(20) COMMENT '受益人關係' NULL,
    `mm_level` int(3) COMMENT '會員等級' NOT NULL DEFAULT 1,
    `mm_role_type` char(2) COMMENT '角色類型:10-一般會員、20-管理員' NOT NULL DEFAULT '10',
    `mm_status` char(2) COMMENT '狀態:10-正常、20-停用、30-鎖定' NOT NULL DEFAULT '10',
    `mm_kyc_id` bigint(15) COMMENT 'KYC認證ID' NULL,
    `mm_kyc` char(2) COMMENT 'KYC狀態:10-未認證、20-已認證、30-認證失敗' NOT NULL DEFAULT '10',
    `mm_create_member` bigint(15) NOT NULL,
    `mm_create_datetime` datetime NOT NULL,
    `mm_update_member` bigint(15) NOT NULL,
    `mm_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`mm_id`),
    UNIQUE KEY `uk_mm_account` (`mm_account`),
    UNIQUE KEY `uk_mm_phone` (`mm_phone_number`),
    INDEX `idx_mm_status` (`mm_status`),
    INDEX `idx_mm_kyc` (`mm_kyc`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立會員錢包表
CREATE TABLE IF NOT EXISTS `member_wallet` (
    `mw_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `mw_mm_id` bigint(15) COMMENT '會員ID' NOT NULL,
    `mw_sp_id` bigint(15) COMMENT '錢包類型' NOT NULL,
    `mw_balance` decimal(15,2) COMMENT '餘額' NOT NULL DEFAULT 0.00,
    `mw_frozen_balance` decimal(15,2) COMMENT '凍結餘額' NOT NULL DEFAULT 0.00,
    `mw_address` varchar(150) COMMENT '錢包地址' NULL,
    `mw_private_key` varchar(200) COMMENT '私鑰(加密存儲)' NULL,
    `mw_status` char(2) COMMENT '狀態:10-正常、20-停用' NOT NULL DEFAULT '10',
    `mw_create_member` bigint(15) NOT NULL,
    `mw_create_datetime` datetime NOT NULL,
    `mw_update_member` bigint(15) NOT NULL,
    `mw_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`mw_id`),
    UNIQUE KEY `uk_mw_mm_sp` (`mw_mm_id`, `mw_sp_id`),
    INDEX `idx_mw_mm_id` (`mw_mm_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立參數分類表
CREATE TABLE IF NOT EXISTS `parameter_category` (
    `sp_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `sp_key` varchar(50) COMMENT '參數鍵值' NOT NULL,
    `sp_code` varchar(50) COMMENT '參數代碼' NOT NULL,
    `sp_name` varchar(100) COMMENT '參數名稱' NOT NULL,
    `sp_description` varchar(200) COMMENT '參數描述' NULL,
    `sp_create_member` bigint(15) NOT NULL,
    `sp_create_datetime` datetime NOT NULL,
    `sp_update_member` bigint(15) NOT NULL,
    `sp_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`sp_id`),
    UNIQUE KEY `uk_sp_key_code` (`sp_key`, `sp_code`),
    INDEX `idx_sp_key` (`sp_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立待付款表
CREATE TABLE IF NOT EXISTS `pending_payment` (
    `pp_sn` varchar(50) NOT NULL,
    `pp_mm_id` bigint(15) COMMENT '付款人' NOT NULL,
    `pp_tm_id` bigint(15) COMMENT '標會ID' NOT NULL,
    `pp_period` int(3) COMMENT '期數' NOT NULL,
    `pp_amount` decimal(15,2) COMMENT '付款金額' NOT NULL,
    `pp_due_date` date COMMENT '到期日' NOT NULL,
    `pp_status` char(2) COMMENT '狀態:10-待付款、20-已付款、30-逾期' NOT NULL DEFAULT '10',
    `pp_payment_datetime` datetime COMMENT '付款時間' NULL,
    `pp_create_member` bigint(15) NOT NULL,
    `pp_create_datetime` datetime NOT NULL,
    `pp_update_member` bigint(15) NOT NULL,
    `pp_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`pp_sn`),
    INDEX `idx_pp_mm_id` (`pp_mm_id`),
    INDEX `idx_pp_tm_id` (`pp_tm_id`),
    INDEX `idx_pp_status` (`pp_status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立系統參數設定表
CREATE TABLE IF NOT EXISTS `system_parameter_setting` (
    `sps_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `sps_code` varchar(50) COMMENT '參數代碼' NOT NULL,
    `sps_name` varchar(100) COMMENT '參數名稱' NOT NULL,
    `sps_description` varchar(200) COMMENT '參數描述' NULL,
    `sps_parameter01` varchar(200) NULL,
    `sps_parameter02` varchar(200) NULL,
    `sps_parameter03` varchar(200) NULL,
    `sps_parameter04` varchar(200) NULL,
    `sps_parameter05` varchar(200) NULL,
    `sps_parameter06` varchar(200) NULL,
    `sps_parameter07` varchar(200) NULL,
    `sps_parameter08` varchar(200) NULL,
    `sps_parameter09` varchar(200) NULL,
    `sps_parameter10` varchar(200) NULL,
    `sps_create_member` bigint(15) NOT NULL,
    `sps_create_datetime` datetime NOT NULL,
    `sps_update_member` bigint(15) NOT NULL,
    `sps_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`sps_id`),
    UNIQUE KEY `uk_sps_code` (`sps_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立臨時標會記錄表
CREATE TABLE IF NOT EXISTS `temp_tender_record` (
    `ttr_sn` varchar(50) NOT NULL,
    `ttr_tm_id` bigint(15) COMMENT '標會ID' NOT NULL,
    `ttr_mm_id` bigint(15) COMMENT '會員ID' NOT NULL,
    `ttr_period` int(3) COMMENT '期數' NOT NULL,
    `ttr_bid_amount` decimal(15,2) COMMENT '標金' NOT NULL,
    `ttr_status` char(2) COMMENT '狀態:10-有效、20-無效' NOT NULL DEFAULT '10',
    `ttr_create_member` bigint(15) NOT NULL,
    `ttr_create_datetime` datetime NOT NULL,
    PRIMARY KEY(`ttr_sn`),
    INDEX `idx_ttr_tm_id` (`ttr_tm_id`),
    INDEX `idx_ttr_mm_id` (`ttr_mm_id`),
    INDEX `idx_ttr_period` (`ttr_period`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立標會活動記錄表
CREATE TABLE IF NOT EXISTS `tender_activity_record` (
    `tar_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `tar_tm_id` bigint(15) COMMENT '標會ID' NOT NULL,
    `tar_period` int(3) COMMENT '期數' NOT NULL,
    `tar_activity_type` char(2) COMMENT '活動類型:10-開標、20-流標、30-得標' NOT NULL,
    `tar_winner_mm_id` bigint(15) COMMENT '得標人' NULL,
    `tar_bid_amount` decimal(15,2) COMMENT '得標金額' NULL,
    `tar_activity_datetime` datetime COMMENT '活動時間' NOT NULL,
    `tar_description` varchar(500) COMMENT '活動描述' NULL,
    `tar_create_member` bigint(15) NOT NULL,
    `tar_create_datetime` datetime NOT NULL,
    `tar_update_member` bigint(15) NOT NULL,
    `tar_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`tar_id`),
    INDEX `idx_tar_tm_id` (`tar_tm_id`),
    INDEX `idx_tar_period` (`tar_period`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立標會明細表
CREATE TABLE IF NOT EXISTS `tender_detail` (
    `td_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `td_tm_id` bigint(15) COMMENT '標會ID' NOT NULL,
    `td_mm_id` bigint(15) COMMENT '會員ID' NOT NULL,
    `td_period` int(3) COMMENT '期數' NOT NULL,
    `td_bid_amount` decimal(15,2) COMMENT '標金' NOT NULL,
    `td_payment_amount` decimal(15,2) COMMENT '應付金額' NOT NULL,
    `td_status` char(2) COMMENT '狀態:10-待付款、20-已付款、30-得標、40-死會' NOT NULL DEFAULT '10',
    `td_payment_datetime` datetime COMMENT '付款時間' NULL,
    `td_create_member` bigint(15) NOT NULL,
    `td_create_datetime` datetime NOT NULL,
    `td_update_member` bigint(15) NOT NULL,
    `td_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`td_id`),
    INDEX `idx_td_tm_id` (`td_tm_id`),
    INDEX `idx_td_mm_id` (`td_mm_id`),
    INDEX `idx_td_period` (`td_period`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立標會主檔表
CREATE TABLE IF NOT EXISTS `tender_master` (
    `tm_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `tm_name` varchar(100) COMMENT '標會名稱' NOT NULL,
    `tm_host_mm_id` bigint(15) COMMENT '會首' NOT NULL,
    `tm_total_amount` decimal(15,2) COMMENT '標會總金額' NOT NULL,
    `tm_period_amount` decimal(15,2) COMMENT '每期金額' NOT NULL,
    `tm_total_periods` int(3) COMMENT '總期數' NOT NULL,
    `tm_current_period` int(3) COMMENT '目前期數' NOT NULL DEFAULT 0,
    `tm_member_count` int(3) COMMENT '會員人數' NOT NULL,
    `tm_start_date` date COMMENT '開始日期' NOT NULL,
    `tm_bid_date` date COMMENT '開標日期' NOT NULL,
    `tm_payment_due_date` date COMMENT '繳款截止日' NOT NULL,
    `tm_status` char(2) COMMENT '狀態:10-招募中、20-進行中、30-已結束、40-已取消' NOT NULL DEFAULT '10',
    `tm_description` varchar(500) COMMENT '標會說明' NULL,
    `tm_create_member` bigint(15) NOT NULL,
    `tm_create_datetime` datetime NOT NULL,
    `tm_update_member` bigint(15) NOT NULL,
    `tm_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`tm_id`),
    INDEX `idx_tm_host_mm_id` (`tm_host_mm_id`),
    INDEX `idx_tm_status` (`tm_status`),
    INDEX `idx_tm_start_date` (`tm_start_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立交易記錄表
CREATE TABLE IF NOT EXISTS `transaction_record` (
    `tr_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `tr_mm_id` bigint(15) COMMENT '會員ID' NOT NULL,
    `tr_type` char(2) COMMENT '交易類型:10-存款、20-提款、30-標會、40-轉帳' NOT NULL,
    `tr_amount` decimal(15,2) COMMENT '交易金額' NOT NULL,
    `tr_balance_before` decimal(15,2) COMMENT '交易前餘額' NOT NULL,
    `tr_balance_after` decimal(15,2) COMMENT '交易後餘額' NOT NULL,
    `tr_reference_id` varchar(50) COMMENT '關聯ID' NULL,
    `tr_description` varchar(200) COMMENT '交易描述' NULL,
    `tr_status` char(2) COMMENT '狀態:10-成功、20-失敗、30-處理中' NOT NULL DEFAULT '10',
    `tr_transaction_datetime` datetime COMMENT '交易時間' NOT NULL,
    `tr_create_member` bigint(15) NOT NULL,
    `tr_create_datetime` datetime NOT NULL,
    `tr_update_member` bigint(15) NOT NULL,
    `tr_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`tr_id`),
    INDEX `idx_tr_mm_id` (`tr_mm_id`),
    INDEX `idx_tr_type` (`tr_type`),
    INDEX `idx_tr_datetime` (`tr_transaction_datetime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立外鍵約束
ALTER TABLE `apply_deposit` ADD CONSTRAINT `fk_ad_mm_id` FOREIGN KEY (`ad_mm_id`) REFERENCES `member_master` (`mm_id`);
ALTER TABLE `apply_kyc_certification` ADD CONSTRAINT `fk_akc_mm_id` FOREIGN KEY (`akc_mm_id`) REFERENCES `member_master` (`mm_id`);
ALTER TABLE `apply_withdraw` ADD CONSTRAINT `fk_aw_mm_id` FOREIGN KEY (`aw_mm_id`) REFERENCES `member_master` (`mm_id`);
ALTER TABLE `member_balance` ADD CONSTRAINT `fk_mb_mm_id` FOREIGN KEY (`mb_mm_id`) REFERENCES `member_master` (`mm_id`);
ALTER TABLE `member_balance` ADD CONSTRAINT `fk_mb_sp_id` FOREIGN KEY (`mb_sp_id`) REFERENCES `parameter_category` (`sp_id`);
ALTER TABLE `member_wallet` ADD CONSTRAINT `fk_mw_mm_id` FOREIGN KEY (`mw_mm_id`) REFERENCES `member_master` (`mm_id`);
ALTER TABLE `member_wallet` ADD CONSTRAINT `fk_mw_sp_id` FOREIGN KEY (`mw_sp_id`) REFERENCES `parameter_category` (`sp_id`);
ALTER TABLE `tender_master` ADD CONSTRAINT `fk_tm_host_mm_id` FOREIGN KEY (`tm_host_mm_id`) REFERENCES `member_master` (`mm_id`);
ALTER TABLE `tender_detail` ADD CONSTRAINT `fk_td_tm_id` FOREIGN KEY (`td_tm_id`) REFERENCES `tender_master` (`tm_id`);
ALTER TABLE `tender_detail` ADD CONSTRAINT `fk_td_mm_id` FOREIGN KEY (`td_mm_id`) REFERENCES `member_master` (`mm_id`);
ALTER TABLE `transaction_record` ADD CONSTRAINT `fk_tr_mm_id` FOREIGN KEY (`tr_mm_id`) REFERENCES `member_master` (`mm_id`);

-- 建立觸發器用於自動更新時間戳
DELIMITER $$

CREATE TRIGGER `tr_member_master_update` 
    BEFORE UPDATE ON `member_master` 
    FOR EACH ROW 
BEGIN 
    SET NEW.mm_update_datetime = NOW(); 
END$$

CREATE TRIGGER `tr_member_balance_update` 
    BEFORE UPDATE ON `member_balance` 
    FOR EACH ROW 
BEGIN 
    SET NEW.mb_update_datetime = NOW(); 
END$$

CREATE TRIGGER `tr_transaction_record_insert` 
    BEFORE INSERT ON `transaction_record` 
    FOR EACH ROW 
BEGIN 
    SET NEW.tr_create_datetime = NOW(); 
    SET NEW.tr_update_datetime = NOW(); 
END$$

DELIMITER ;

-- 初始化完成
SELECT 'Database schema initialization completed for Zeabur deployment' as message;