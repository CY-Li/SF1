-- =====================================================
-- ROSCA 平安商會系統 - Zeabur 完整資料庫初始化腳本
-- =====================================================
-- 此腳本包含完整的資料庫結構、預設資料和測試帳號
-- 在 Zeabur MariaDB 控制台執行此腳本

-- 使用 zeabur 資料庫
USE zeabur;

-- 設定字符集和排序規則
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- =====================================================
-- 1. 建立資料庫結構
-- =====================================================

-- 建立公告板表
CREATE TABLE IF NOT EXISTS `announcement_board` (
    `ab_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `ab_title` varchar(30) COMMENT '標題' NOT NULL,
    `ab_content` varchar(300) COMMENT '內容' NULL,
    `ab_image` blob COMMENT '圖片檔案' NULL,
    `ab_image_path` varchar(200) COMMENT '圖片檔案路徑，可能只存檔案名稱或是網址' NULL,
    `ab_image_name` varchar(100) NULL,
    `ab_status` char(2) COMMENT '10:開啟，20:關閉' NOT NULL DEFAULT '20',
    `ab_datetime` datetime COMMENT '發布時間' NOT NULL,
    `ab_create_member` bigint(15) NOT NULL,
    `ab_create_datetime` datetime NOT NULL,
    `ab_update_datetime` bigint(15) NOT NULL,
    `ab_update_member` datetime NOT NULL,
    PRIMARY KEY(`ab_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立存款申請表
CREATE TABLE IF NOT EXISTS `apply_deposit` (
    `ad_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `ad_mm_id` bigint(15) COMMENT '存款人' NOT NULL,
    `ad_amount` int(15) COMMENT '存款金額' NOT NULL,
    `ad_key` varchar(200) COMMENT '存款產生的KEY(要讓我們才查得到)' NULL,
    `ad_url` varchar(200) COMMENT '存款網址(現在沒用到，想說QR CODE可能要輸入網址才能存款)' NULL,
    `ad_picture` blob COMMENT '圖片(如果存款會需要QR CODE圖片就存這裡)' NULL,
    `ad_akc_mw_address` varchar(150) COMMENT '錢包地址' NULL,
    `ad_file_name` varchar(100) COMMENT '上傳檔案名稱或是轉帳憑證存檔名' NOT NULL,
    `ad_status` char(2) COMMENT '申請狀態:10-待會員存款、11-已輸入存款金額、12-申請退回、13-輸入完成' NOT NULL DEFAULT '0',
    `ad_type` char(2) COMMENT '存款類型:10-虛擬幣、50-銀行帳戶' NULL,
    `ad_kyc_id` bigint(15) NOT NULL,
    `ad_create_member` bigint(15) NOT NULL,
    `ad_create_datetime` datetime NOT NULL,
    `ad_update_member` bigint(15) NOT NULL,
    `ad_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`ad_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立KYC認證申請表
CREATE TABLE IF NOT EXISTS `apply_kyc_certification` (
    `akc_id` bigint(15) NOT NULL AUTO_INCREMENT,
    `akc_mm_id` bigint(15) NOT NULL,
    `akc_name` varchar(30) NOT NULL DEFAULT '會員真實姓名',
    `akc_front` varchar(100) COMMENT '身分證正面' NOT NULL,
    `akc_back` varchar(100) COMMENT '身分證背面' NOT NULL,
    `akc_gender` char(1) COMMENT '性別(1:男性、2:女性)' NOT NULL,
    `akc_personal_id` char(10) COMMENT '身分證字號' NOT NULL,
    `akc_mw_address` varchar(150) COMMENT '錢包地址' NOT NULL,
    `akc_email` varchar(50) COMMENT '信箱' NOT NULL,
    `akc_bank_code` varchar(10) NOT NULL DEFAULT '銀行代碼',
    `akc_bank_account` varchar(25) COMMENT '銀行帳號' NOT NULL,
    `akc_bank_account_name` varchar(20) COMMENT '戶名' NOT NULL,
    `akc_branch` varchar(10) COMMENT '分行' NOT NULL,
    `akc_beneficiary_name` varchar(30) COMMENT '受益人姓名' NOT NULL,
    `akc_beneficiary_phone` varchar(15) COMMENT '受益人電話' NOT NULL,
    `akc_beneficiary_relationship` varchar(30) COMMENT '受益人關係' NOT NULL,
    `akc_status` char(2) COMMENT '申請狀態:10-待審核、20-審核通過、30-審核退回、40-審核失敗' NOT NULL,
    `akc_create_member` bigint(15) NOT NULL,
    `akc_create_datetime` datetime NOT NULL,
    `akc_update_member` bigint(15) NOT NULL,
    `akc_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`akc_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立提款申請表
CREATE TABLE IF NOT EXISTS `apply_withdraw` (
    `aw_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `aw_mm_id` bigint(15) COMMENT '申請人ID' NOT NULL,
    `aw_amount` decimal(15,5) COMMENT '申請提領金額' NOT NULL,
    `aw_key` varchar(200) COMMENT '管理員KEY' NULL,
    `aw_url` varchar(200) COMMENT '管理員網址' NULL,
    `aw_wallet_address` varchar(200) COMMENT '錢包地址' NULL,
    `aw_file_name` varchar(100) NULL,
    `aw_status` varchar(2) COMMENT '申請狀態:10-待管理員付款、11-已輸入會員網址、12-申請退回、13-輸入完成' NOT NULL DEFAULT '10',
    `aw_type` char(2) COMMENT '10:虛擬幣、50:銀行帳戶' NULL,
    `aw_kyc_id` bigint(15) NOT NULL,
    `aw_create_member` bigint(15) NOT NULL,
    `aw_create_datetime` datetime NOT NULL,
    `aw_update_member` bigint(15) NOT NULL,
    `aw_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`aw_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立會員餘額表
CREATE TABLE IF NOT EXISTS `member_balance` (
    `mb_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `mb_mm_id` bigint(15) NOT NULL,
    `mb_payee_mm_id` bigint(15) COMMENT '收款人帳號(標會點數可以轉給其他人)' NOT NULL,
    `mb_tm_id` bigint(15) COMMENT '標會主檔ID' NOT NULL,
    `mb_td_id` bigint(15) COMMENT 'RSOCA 明細檔ID' NOT NULL,
    `mb_income_type` char(1) COMMENT '點數流向:I-收入，O-支出(寫入時用大寫，最終mm_id為準)' NOT NULL,
    `mb_tr_code` char(22) COMMENT '自製交易序號' NOT NULL,
    `mb_tr_type` char(2) COMMENT '交易代碼:11-標會存款點(存款、標會) 、12-標會提款點(存款、標會) 、13-標會獎勵(獎勵:標到標會時給的獎勵' NOT NULL,
    `mb_type` char(2) COMMENT '帳戶類型， 11-標會存款點(存款、標會) 、12標會提款點(存款、標會) 、13-標會獎勵(獎勵:標到標會時給的獎勵' NOT NULL,
    `mb_points_type` char(2) COMMENT '這裡會寫入點數的類型代碼(參考system_parameters:sp_code)，11-存款點數、12-獎勵點數、13-' NOT NULL,
    `mb_change_points` decimal(15,5) COMMENT '點數異動金額(點數異動多少所以這個數字會寫入點數明細)' NOT NULL,
    `mb_points` decimal(15,5) COMMENT '點數異動後金額餘(點數異動多少所以這個數字會寫入點數明細))' NOT NULL,
    `mb_settlement_period` int(15) COMMENT '結算已經處理的期數，參考來源tm_settlement_period' NOT NULL DEFAULT '0',
    `mb_current_period` int(15) COMMENT '目前期數(標會時，標會期數，寫入就是1)，這個會根據該期數異動，參考來源tm_current_period' NOT NULL DEFAULT '0',
    `mb_remark` varchar(100) NULL,
    `mb_create_member` bigint(15) NOT NULL,
    `mb_create_datetime` datetime NOT NULL,
    `mb_update_member` bigint(15) NOT NULL,
    `mb_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`mb_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立會員主檔表
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

-- 建立會員錢包表
CREATE TABLE IF NOT EXISTS `member_wallet` (
    `mw_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `mw_mm_id` bigint(15) COMMENT '會員ID' NOT NULL,
    `mw_currency` varchar(10) COMMENT '幣別類型' NULL,
    `mw_address` varchar(150) COMMENT '錢包地址' NULL,
    `mw_subscripts_count` int(15) COMMENT '標會次數(自己)，system_parameters:10' NOT NULL DEFAULT '0',
    `mw_stored` decimal(15,5) COMMENT '存款點數(可用點數)，system_parameters:11' NOT NULL DEFAULT '0',
    `mw_reward` decimal(15,5) COMMENT '獎勵點數，system_parameters:12' NOT NULL DEFAULT '0',
    `mw_peace` decimal(15,5) COMMENT '平安點數，system_parameters:13' NOT NULL DEFAULT '0',
    `mw_mall` decimal(15,5) COMMENT '商城點數，system_parameters:14' NOT NULL DEFAULT '0',
    `mw_registration` decimal(15,5) COMMENT '註冊點數，system_parameters:15' NOT NULL DEFAULT '0',
    `mw_death` decimal(15,5) COMMENT '死會點數，system_parameters:16' NOT NULL DEFAULT '0',
    `mw_accumulation` decimal(15,5) COMMENT '累積流水，system_parameters:17' NOT NULL DEFAULT '0',
    `mw_punish` decimal(15,5) COMMENT '懲罰點數，system_parameters:18' NOT NULL DEFAULT '0',
    `mw_level` char(1) NOT NULL DEFAULT '0',
    `mw_create_member` bigint(15) NOT NULL,
    `mw_create_datetime` datetime NOT NULL,
    `mw_update_member` bigint(15) NOT NULL,
    `mw_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`mw_id`),
    UNIQUE KEY `uk_mw_mm_id` (`mw_mm_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立參數分類表
CREATE TABLE IF NOT EXISTS `parameter_category` (
    `sp_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `sp_key` varchar(25) COMMENT '查詢參數設定索引' NOT NULL,
    `sp_code` varchar(25) COMMENT '參數代碼' NOT NULL,
    `sp_name` varchar(25) COMMENT '參數名稱' NOT NULL,
    `sp_description` varchar(50) COMMENT '參數說明' NULL,
    `sp_create_member` bigint(15) NOT NULL,
    `sp_create_datetime` datetime NOT NULL,
    `sp_update_member` bigint(15) NOT NULL,
    `sp_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`sp_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立系統參數設定表
CREATE TABLE IF NOT EXISTS `system_parameter_setting` (
    `sps_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `sps_code` varchar(25) COMMENT '設定索引' NOT NULL,
    `sps_name` varchar(25) COMMENT '設定名稱' NOT NULL,
    `sps_description` varchar(100) COMMENT '參數說明' NULL,
    `sps_picture` blob COMMENT '圖片(如果存款會需要QR CODE圖片就存這裡)' NULL,
    `sps_parameter01` varchar(100) NULL,
    `sps_parameter02` varchar(100) NULL,
    `sps_parameter03` varchar(100) NULL,
    `sps_parameter04` varchar(100) NULL,
    `sps_parameter05` varchar(100) NULL,
    `sps_parameter06` varchar(100) NULL,
    `sps_parameter07` varchar(100) NULL,
    `sps_parameter08` varchar(100) NULL,
    `sps_parameter09` varchar(100) NULL,
    `sps_parameter10` varchar(100) NULL,
    `sps_create_member` bigint(15) NOT NULL,
    `sps_create_datetime` datetime NOT NULL,
    `sps_update_member` bigint(15) NOT NULL,
    `sps_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`sps_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- =====================================================
-- 2. 建立索引以提升查詢效能
-- =====================================================

CREATE INDEX IF NOT EXISTS `idx_ab_status` ON `announcement_board`(`ab_status`);
CREATE INDEX IF NOT EXISTS `idx_ad_mm_id` ON `apply_deposit`(`ad_mm_id`);
CREATE INDEX IF NOT EXISTS `idx_ad_status` ON `apply_deposit`(`ad_status`);
CREATE INDEX IF NOT EXISTS `idx_akc_mm_id` ON `apply_kyc_certification`(`akc_mm_id`);
CREATE INDEX IF NOT EXISTS `idx_akc_status` ON `apply_kyc_certification`(`akc_status`);
CREATE INDEX IF NOT EXISTS `idx_aw_mm_id` ON `apply_withdraw`(`aw_mm_id`);
CREATE INDEX IF NOT EXISTS `idx_aw_status` ON `apply_withdraw`(`aw_status`);
CREATE INDEX IF NOT EXISTS `idx_mb_mm_id` ON `member_balance`(`mb_mm_id`);
CREATE INDEX IF NOT EXISTS `idx_mm_account` ON `member_master`(`mm_account`);
CREATE INDEX IF NOT EXISTS `idx_mm_status` ON `member_master`(`mm_status`);
CREATE INDEX IF NOT EXISTS `idx_mw_mm_id` ON `member_wallet`(`mw_mm_id`);
CREATE INDEX IF NOT EXISTS `idx_sp_key` ON `parameter_category`(`sp_key`);
CREATE INDEX IF NOT EXISTS `idx_sps_code` ON `system_parameter_setting`(`sps_code`);

-- =====================================================
-- 3. 插入系統參數設定
-- =====================================================

INSERT IGNORE INTO `system_parameter_setting` (
    `sps_id`, `sps_code`, `sps_name`, `sps_description`,
    `sps_parameter01`, `sps_parameter02`, `sps_parameter03`, `sps_parameter04`, `sps_parameter05`,
    `sps_parameter06`, `sps_parameter07`, `sps_parameter08`, `sps_parameter09`, `sps_parameter10`,
    `sps_create_member`, `sps_create_datetime`, `sps_update_member`, `sps_update_datetime`
) VALUES 
(1, 'DEPOSIT_RATE', '存款匯率', '存款時的匯率設定', '1.0', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(2, 'WITHDRAW_RATE', '提款匯率', '提款時的匯率設定', '1.0', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(3, 'MIN_DEPOSIT', '最小存款金額', '最小存款金額限制', '1000', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(4, 'MIN_WITHDRAW', '最小提款金額', '最小提款金額限制', '1000', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(5, 'TENDER_MIN_AMOUNT', '標會最小金額', '標會最小金額設定', '8000', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW());

-- =====================================================
-- 4. 插入參數分類
-- =====================================================

INSERT IGNORE INTO `parameter_category` (
    `sp_id`, `sp_key`, `sp_code`, `sp_name`, `sp_description`,
    `sp_create_member`, `sp_create_datetime`, `sp_update_member`, `sp_update_datetime`
) VALUES 
(10, 'WALLET_TYPE', 'SUBSCRIPTS_COUNT', '標會次數', '會員標會次數統計', 1, NOW(), 1, NOW()),
(11, 'WALLET_TYPE', 'STORED_POINTS', '存款點數', '可用存款點數', 1, NOW(), 1, NOW()),
(12, 'WALLET_TYPE', 'REWARD_POINTS', '獎勵點數', '標會獎勵點數', 1, NOW(), 1, NOW()),
(13, 'WALLET_TYPE', 'PEACE_POINTS', '平安點數', '平安保險點數', 1, NOW(), 1, NOW()),
(14, 'WALLET_TYPE', 'MALL_POINTS', '商城點數', '商城購物點數', 1, NOW(), 1, NOW()),
(15, 'WALLET_TYPE', 'REGISTRATION_POINTS', '註冊點數', '註冊獎勵點數', 1, NOW(), 1, NOW()),
(16, 'WALLET_TYPE', 'DEATH_POINTS', '死會點數', '死會相關點數', 1, NOW(), 1, NOW()),
(17, 'WALLET_TYPE', 'ACCUMULATION', '累積流水', '累積交易流水', 1, NOW(), 1, NOW()),
(18, 'WALLET_TYPE', 'PUNISH_POINTS', '懲罰點數', '違規懲罰點數', 1, NOW(), 1, NOW());

-- =====================================================
-- 5. 插入範例公告
-- =====================================================

INSERT IGNORE INTO `announcement_board` (
    `ab_id`, `ab_title`, `ab_content`, `ab_status`, `ab_datetime`,
    `ab_create_member`, `ab_create_datetime`, `ab_update_datetime`, `ab_update_member`
) VALUES 
(1, '歡迎使用平安商會系統', '歡迎使用平安商會標會系統，請先完成 KYC 認證後即可開始參與標會活動。', '10', NOW(), 1, NOW(), 1, NOW()),
(2, '系統維護通知', '系統將於每週日凌晨 2:00-4:00 進行例行維護，期間可能無法正常使用，敬請見諒。', '10', NOW(), 1, NOW(), 1, NOW()),
(3, '標會規則說明', '請詳細閱讀標會規則，確保了解相關權利義務後再參與標會活動。', '10', NOW(), 1, NOW(), 1, NOW());

-- =====================================================
-- 6. 建立測試帳號 (SHA256 密碼格式)
-- =====================================================

-- 測試使用者帳號 (帳號: 0938766349, 密碼: Admin123456)
INSERT IGNORE INTO `member_master` (
    `mm_id`, `mm_account`, `mm_hash_pwd`, `mm_2nd_hash_pwd`, `mm_name`,
    `mm_introduce_user`, `mm_introduce_code`, `mm_invite_user`, `mm_invite_code`,
    `mm_gender`, `mm_country_code`, `mm_personal_id`, `mm_phone_number`,
    `mm_mw_address`, `mm_email`, `mm_bank_code`, `mm_bank_account`,
    `mm_bank_account_name`, `mm_branch`, `mm_beneficiary_name`,
    `mm_beneficiary_phone`, `mm_beneficiary_relationship`, `mm_level`,
    `mm_role_type`, `mm_status`, `mm_kyc_id`, `mm_kyc`,
    `mm_create_member`, `mm_create_datetime`, `mm_update_member`, `mm_update_datetime`
) VALUES (
    1, '0938766349', 
    '1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A',
    '1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A',
    '測試使用者', NULL, 0, NULL, 0, '1', '+886', 'A123456789', '0938766349',
    '台北市信義區信義路五段7號', 'test@example.com', '004', '1234567890123456',
    '測試使用者', '總行', '測試受益人', '0938766349', '本人', '1',
    '1', 'Y', 1, 'Y', 1, NOW(), 1, NOW()
);

-- 系統管理員帳號 (帳號: admin, 密碼: Admin123456)
INSERT IGNORE INTO `member_master` (
    `mm_id`, `mm_account`, `mm_hash_pwd`, `mm_2nd_hash_pwd`, `mm_name`,
    `mm_introduce_user`, `mm_introduce_code`, `mm_invite_user`, `mm_invite_code`,
    `mm_gender`, `mm_country_code`, `mm_personal_id`, `mm_phone_number`,
    `mm_mw_address`, `mm_email`, `mm_bank_code`, `mm_bank_account`,
    `mm_bank_account_name`, `mm_branch`, `mm_beneficiary_name`,
    `mm_beneficiary_phone`, `mm_beneficiary_relationship`, `mm_level`,
    `mm_role_type`, `mm_status`, `mm_kyc_id`, `mm_kyc`,
    `mm_create_member`, `mm_create_datetime`, `mm_update_member`, `mm_update_datetime`
) VALUES (
    2, 'admin',
    '1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A',
    '1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A',
    '系統管理員', NULL, 0, NULL, 0, '1', '+886', 'B987654321', 'admin',
    '台北市信義區信義路五段7號', 'admin@example.com', '004', '9876543210987654',
    '系統管理員', '總行', '系統管理員', 'admin', '本人', '9',
    '2', 'Y', 2, 'Y', 1, NOW(), 1, NOW()
);

-- =====================================================
-- 7. 建立會員錢包
-- =====================================================

-- 測試使用者錢包
INSERT IGNORE INTO `member_wallet` (
    `mw_id`, `mw_mm_id`, `mw_currency`, `mw_address`, `mw_subscripts_count`,
    `mw_stored`, `mw_reward`, `mw_peace`, `mw_mall`, `mw_registration`,
    `mw_death`, `mw_accumulation`, `mw_punish`, `mw_level`,
    `mw_create_member`, `mw_create_datetime`, `mw_update_member`, `mw_update_datetime`
) VALUES (
    1, 1, 'TWD', '台北市信義區信義路五段7號', 0,
    10000.00000, 0.00000, 0.00000, 0.00000, 1000.00000,
    0.00000, 0.00000, 0.00000, '1',
    1, NOW(), 1, NOW()
);

-- 管理員錢包
INSERT IGNORE INTO `member_wallet` (
    `mw_id`, `mw_mm_id`, `mw_currency`, `mw_address`, `mw_subscripts_count`,
    `mw_stored`, `mw_reward`, `mw_peace`, `mw_mall`, `mw_registration`,
    `mw_death`, `mw_accumulation`, `mw_punish`, `mw_level`,
    `mw_create_member`, `mw_create_datetime`, `mw_update_member`, `mw_update_datetime`
) VALUES (
    2, 2, 'TWD', '台北市信義區信義路五段7號', 0,
    999999.00000, 0.00000, 0.00000, 0.00000, 0.00000,
    0.00000, 0.00000, 0.00000, '9',
    1, NOW(), 1, NOW()
);

-- =====================================================
-- 8. 建立 KYC 認證記錄
-- =====================================================

-- 測試使用者 KYC
INSERT IGNORE INTO `apply_kyc_certification` (
    `akc_id`, `akc_mm_id`, `akc_name`, `akc_front`, `akc_back`, `akc_gender`,
    `akc_personal_id`, `akc_mw_address`, `akc_email`, `akc_bank_code`,
    `akc_bank_account`, `akc_bank_account_name`, `akc_branch`,
    `akc_beneficiary_name`, `akc_beneficiary_phone`, `akc_beneficiary_relationship`,
    `akc_status`, `akc_create_member`, `akc_create_datetime`,
    `akc_update_member`, `akc_update_datetime`
) VALUES (
    1, 1, '測試使用者', 'default_id_front.jpg', 'default_id_back.jpg', '1',
    'A123456789', '台北市信義區信義路五段7號', 'test@example.com', '004',
    '1234567890123456', '測試使用者', '總行', '測試受益人', '0938766349', '本人',
    '20', 1, NOW(), 1, NOW()
);

-- 管理員 KYC
INSERT IGNORE INTO `apply_kyc_certification` (
    `akc_id`, `akc_mm_id`, `akc_name`, `akc_front`, `akc_back`, `akc_gender`,
    `akc_personal_id`, `akc_mw_address`, `akc_email`, `akc_bank_code`,
    `akc_bank_account`, `akc_bank_account_name`, `akc_branch`,
    `akc_beneficiary_name`, `akc_beneficiary_phone`, `akc_beneficiary_relationship`,
    `akc_status`, `akc_create_member`, `akc_create_datetime`,
    `akc_update_member`, `akc_update_datetime`
) VALUES (
    2, 2, '系統管理員', 'admin_id_front.jpg', 'admin_id_back.jpg', '1',
    'B987654321', '台北市信義區信義路五段7號', 'admin@example.com', '004',
    '9876543210987654', '系統管理員', '總行', '系統管理員', 'admin', '本人',
    '20', 1, NOW(), 1, NOW()
);

-- =====================================================
-- 9. 驗證初始化結果
-- =====================================================

-- 檢查會員資料
SELECT 
    mm_id, mm_account, mm_name, mm_role_type, mm_status, mm_kyc,
    LENGTH(mm_hash_pwd) as password_length,
    mm_create_datetime
FROM member_master 
ORDER BY mm_id;

-- 檢查錢包資料
SELECT 
    mw_id, mw_mm_id, mw_currency, mw_stored, mw_registration
FROM member_wallet 
ORDER BY mw_id;

-- 檢查 KYC 資料
SELECT 
    akc_id, akc_mm_id, akc_name, akc_status
FROM apply_kyc_certification 
ORDER BY akc_id;

-- 檢查系統參數
SELECT COUNT(*) as parameter_count FROM system_parameter_setting;

-- 檢查公告
SELECT COUNT(*) as announcement_count FROM announcement_board;

SELECT '🎉 Zeabur 資料庫初始化完成！' as status;
SELECT '📋 測試帳號資訊:' as info;
SELECT 'admin / Admin123456 (管理員)' as account_1;
SELECT '0938766349 / Admin123456 (使用者)' as account_2;