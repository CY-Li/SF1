-- ROSCA 平安商會系統資料庫結構初始化腳本
-- 此腳本會在 MariaDB 容器首次啟動時自動執行

-- 設定字符集和排序規則
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- 使用 rosca2 資料庫 (已由 Docker 環境變數自動建立)
USE rosca2;

-- 建立公告板表
CREATE TABLE `announcement_board` (
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
CREATE TABLE `apply_deposit` (
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
CREATE TABLE `apply_kyc_certification` (
    `akc_id` bigint(15) NOT NULL,
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
CREATE TABLE `apply_withdraw` (
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

-- 建立抽籤記錄表
CREATE TABLE `lottery_record` (
    `lr_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `lr_tm_id` bigint(15) NOT NULL,
    `lr_td_id` bigint(15) NOT NULL,
    `lr_td_period` int(15) NOT NULL,
    `lr_create_member` bigint(15) NOT NULL,
    `lr_create_datetime` datetime NOT NULL,
    `lr_update_member` bigint(15) NOT NULL,
    `lr_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`lr_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立會員餘額表
CREATE TABLE `member_balance` (
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
CREATE TABLE `member_master` (
    `mm_id` bigint(15) NOT NULL,
    `mm_account` varchar(20) COMMENT '帳號(現在是手機號碼)' NOT NULL,
    `mm_hash_pwd` varchar(100) COMMENT '雜湊密碼' NOT NULL,
    `mm_2nd_hash_pwd` varchar(100) COMMENT '第二組密碼，標會用' NOT NULL,
    `mm_name` varchar(30) COMMENT '會員真實姓名' NOT NULL,
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
    `mm_create_member` bigint(15) NOT NULL,
    `mm_create_datetime` datetime NOT NULL,
    `mm_update_member` bigint(15) NOT NULL,
    `mm_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`mm_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立會員錢包表
CREATE TABLE `member_wallet` (
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
    PRIMARY KEY(`mw_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立參數分類表
CREATE TABLE `parameter_category` (
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

-- 建立待付款表
CREATE TABLE `pending_payment` (
    `pp_sn` bigint(15) AUTO_INCREMENT NOT NULL,
    `pp_id` bigint(15) NOT NULL,
    `pp_mm_id` bigint(15) NOT NULL,
    `pp_tr_code` char(22) NOT NULL,
    `pp_tm_id` bigint(15) NOT NULL,
    `pp_td_id` bigint(15) NOT NULL,
    `pp_amount` decimal(15,5) COMMENT '待付款金額' NOT NULL,
    `pp_status` char(2) COMMENT '10:待繳費、20:已繳費待確認、30:逾期待處罰' NOT NULL DEFAULT '0',
    `pp_penalty_datetime` datetime COMMENT '超過時間逾期就要處罰' NOT NULL,
    `pp_create_member` bigint(15) NOT NULL,
    `pp_create_datetime` datetime NOT NULL,
    `pp_update_member` bigint(15) NOT NULL,
    `pp_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`pp_sn`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立系統參數設定表
CREATE TABLE `system_parameter_setting` (
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

-- 建立臨時標會記錄表
CREATE TABLE `temp_tender_record` (
    `ttr_sn` bigint(15) AUTO_INCREMENT NOT NULL,
    `ttr_tm_sn` bigint(15) NOT NULL,
    `ttr_tm_id` bigint(15) NOT NULL,
    `ttr_title` varchar(30) COMMENT '標題' NOT NULL,
    `ttr_detail01` varchar(50) COMMENT '標題明細01' NOT NULL,
    `ttr_detail02` varchar(50) COMMENT '標題明細02' NOT NULL,
    `ttr_detail03` varchar(50) COMMENT '標題明細03' NOT NULL,
    `ttr_detail04` varchar(50) COMMENT '標題明細04' NOT NULL,
    `ttr_top` varchar(25) COMMENT '會首' NOT NULL,
    `ttr_owner` varchar(25) COMMENT '標會負責人' NOT NULL,
    `ttr_phone` varchar(25) COMMENT '標會電話' NOT NULL,
    `ttr_address` varchar(100) COMMENT '標會聚會地址' NOT NULL,
    `ttr_tm_group_datetime` datetime COMMENT '標會聚會時間' NOT NULL,
    `ttr_detail` longtext COMMENT '標會聚會明細' NOT NULL,
    `ttr_create_datetime` datetime NOT NULL,
    PRIMARY KEY(`ttr_sn`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立標會活動記錄表
CREATE TABLE `tender_activity_record` (
    `tar_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `tar_mm_id` bigint(15) COMMENT '操作人(標會人、得標人)' NOT NULL,
    `tar_mm_introduce_code` bigint(15) NULL DEFAULT '0',
    `tar_tm_id` bigint(15) NOT NULL,
    `tar_tm_count` int(15) COMMENT '這裡紀錄的是總數量，不是一次標會的會期' NOT NULL,
    `tar_tr_type` char(2) COMMENT '交易代碼:11-標會存款點(存款、標會) 、12-標會提款點(存款、標會) 、13-標會獎勵(獎勵:標到標會時給的獎勵' NOT NULL,
    `tar_tr_code` char(22) NOT NULL,
    `tar_status` int(15) COMMENT '標會活動狀態，0:未處理、1已經處理' NOT NULL DEFAULT '0',
    `tar_json` longtext COMMENT '標會邀請清單明細(invitation_org)的完整json值，由排程來處理' NULL,
    `tar_create_member` bigint(15) NOT NULL,
    `tar_create_datetime` datetime NOT NULL,
    `tar_update_member` bigint(15) NOT NULL,
    `tar_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`tar_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立標會明細表
CREATE TABLE `tender_detail` (
    `td_id` bigint(15) AUTO_INCREMENT COMMENT 'RSOCA 明細檔ID (Rotating Savings and Credit Association ID_標會明細標' NOT NULL,
    `td_tm_id` bigint(15) COMMENT 'RSOCA 主檔ID (Rotating Savings and Credit Association ID_標會明細標會' NOT NULL,
    `td_participants` bigint(15) COMMENT '參加者mm_id' NULL,
    `td_sequence` int(15) COMMENT '標會順序排序列(用來亂數排序)' NOT NULL DEFAULT '0',
    `td_period` int(15) COMMENT '得標期數期數' NOT NULL DEFAULT '0',
    `td_status` char(1) COMMENT '標會狀態' NOT NULL DEFAULT '0',
    `td_pp_id` varchar(367) COMMENT '如果有待繳費明細就會寫入pending_payment' NULL,
    `td_pp_penalty_count` int(15) COMMENT '處罰次數' NOT NULL DEFAULT '0',
    `td_pp_paid` int(15) COMMENT '是否已繳費確認，如果沒繳費就會是0' NOT NULL DEFAULT '0',
    `td_tm_initiator_mm_id` bigint(15) COMMENT '發起者mm_id=tm_initiator_mm_id' NOT NULL,
    `td_create_member` bigint(15) NOT NULL,
    `td_create_datetime` datetime NOT NULL,
    `td_update_member` bigint(15) NOT NULL,
    `td_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`td_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立標會主檔表
CREATE TABLE `tender_master` (
    `tm_id` bigint(15) COMMENT 'RSOCA 主檔ID (Rotating Savings and Credit Association ID_標會明細標會' NOT NULL,
    `tm_sn` bigint(15) NOT NULL DEFAULT '0',
    `tm_name` varchar(25) COMMENT '標會名稱' NOT NULL,
    `tm_ticks` varchar(20) COMMENT '建立時間加字串(暫時沒用，用來記錄時間點)' NOT NULL,
    `tm_initiator_mm_id` bigint(15) COMMENT '發起者mm_id' NOT NULL,
    `tm_type` char(1) COMMENT '標會類型:A-一般做法(會首:可以標會次數)、B-4人做法(6會)、C-2人做法(12會)、D-1人做法(24會)' NOT NULL,
    `tm_bidder` varchar(150) COMMENT '標會人會首會員編號，EX:1|2|3' NULL,
    `tm_winners` varchar(150) COMMENT '得標者會首會員編號，EX:1|2|3' NULL,
    `tm_settlement_period` int(15) COMMENT '結算已經處理的期數' NOT NULL DEFAULT '1',
    `tm_current_period` int(15) COMMENT '目前期數(標會時，標會期數，寫入就是1)，這個會根據該期數異動' NOT NULL DEFAULT '0',
    `tm_status` char(1) COMMENT '是否啟用:0-尚未開始,1-進行中,2-已結束' NOT NULL DEFAULT '0',
    `tm_count` int(15) COMMENT '用來判斷是否結束' NOT NULL DEFAULT '0',
    `tm_settlement_datetime` datetime COMMENT '結算查詢過濾這個時間就要處理' NULL,
    `tm_win_first_datetime` datetime COMMENT '第一次開標時間' NULL,
    `tm_win_end_datetime` datetime COMMENT '最後最後一次開標時間' NULL,
    `tm_group_datetime` datetime COMMENT '聚會時間' NULL,
    `tm_create_member` bigint(15) NOT NULL,
    `tm_create_datetime` datetime NOT NULL,
    `tm_update_member` bigint(15) NOT NULL,
    `tm_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`tm_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立交易記錄表
CREATE TABLE `transaction_record` (
    `tr_id` bigint(15) AUTO_INCREMENT NOT NULL,
    `tr_code` char(22) COMMENT '自製交易序號' NOT NULL,
    `tr_mm_id` bigint(15) COMMENT '標會人mm_id' NOT NULL,
    `tr_payee_mm_id` bigint(15) COMMENT '收款人帳號(標會點數可以轉給其他人)' NOT NULL,
    `tr_tm_id` bigint(15) NOT NULL,
    `tr_td_id` bigint(15) COMMENT 'RSOCA 明細檔ID' NOT NULL,
    `tr_pp_id` bigint(15) COMMENT '如果有待繳費明細就會寫入pending_payment' NOT NULL DEFAULT '0',
    `tr_type` char(2) COMMENT '交易代碼:11-標會存款點(存款、標會) 、12-標會提款點(存款、標會) 、13-標會獎勵(獎勵:標到標會時給的獎勵' NOT NULL DEFAULT '1',
    `tr_status` char(2) COMMENT '交易狀態:30-待清算(入獎勵點數明細，tr_type:13、16) 、50-已清算(就是完成所有流程，tr_type' NOT NULL,
    `tr_settlement_time` datetime COMMENT 'tr_type:13、16會需要延時間來給獎勵所以會由大於這個時間的排程來給獎勵' NULL,
    `tr_mm_points` decimal(15,5) COMMENT '存款點數(每次8000在這個欄位會是確定數字，獎勵會會是10000)' NOT NULL,
    `tr_income_type` char(1) COMMENT '點數流向:I-收入，O-支出(寫入時用大寫，最終mm_id為準)' NOT NULL,
    `tr_settlement_period` int(15) COMMENT '結算已經處理的期數，參考來源tm_settlement_period' NOT NULL DEFAULT '0',
    `tr_current_period` int(15) COMMENT '目前期數(標會時，標會期數，寫入就是1)，這個會根據該期數異動，參考來源tm_current_period' NOT NULL DEFAULT '0',
    `tr_remark` varchar(100) NULL,
    `tr_create_member` bigint(15) NOT NULL,
    `tr_create_datetime` datetime NOT NULL,
    `tr_update_member` bigint(15) NOT NULL,
    `tr_update_datetime` datetime NOT NULL,
    PRIMARY KEY(`tr_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 建立唯一索引
ALTER TABLE `member_master`
    ADD CONSTRAINT `UNIQUE_member_master_1`
    UNIQUE (`mm_account`);

ALTER TABLE `member_wallet`
    ADD CONSTRAINT `UNIQUE_member_wallet_1`
    UNIQUE (`mw_mm_id`);

-- 建立索引以提升查詢效能
CREATE INDEX `INDEX_announcement_board_1`
    ON `announcement_board`(`ab_id` DESC);

CREATE INDEX `INDEX_apply_deposit_1`
    ON `apply_deposit`(`ad_mm_id` DESC);

CREATE INDEX `INDEX_apply_deposit_2`
    ON `apply_deposit`(`ad_status`);

CREATE INDEX `INDEX_apply_kyc_certification_1`
    ON `apply_kyc_certification`(`akc_mm_id`);

CREATE INDEX `INDEX_apply_kyc_certification_2`
    ON `apply_kyc_certification`(`akc_status`);

CREATE INDEX `INDEX_apply_withdraw_1`
    ON `apply_withdraw`(`aw_mm_id`);

CREATE INDEX `INDEX_apply_withdraw_2`
    ON `apply_withdraw`(`aw_status`);

CREATE INDEX `INDEX_member_balance_1`
    ON `member_balance`(`mb_mm_id`);

CREATE INDEX `INDEX_member_balance_2`
    ON `member_balance`(`mb_mm_id`, `mb_points_type`);

CREATE INDEX `INDEX_member_master_1`
    ON `member_master`(`mm_account`);

CREATE INDEX `INDEX_member_wallet_1`
    ON `member_wallet`(`mw_mm_id`);

CREATE INDEX `INDEX_parameter_category_1`
    ON `parameter_category`(`sp_key`);

CREATE INDEX `INDEX_pending_payment_1`
    ON `pending_payment`(`pp_id` DESC);

CREATE INDEX `INDEX_pending_payment_2`
    ON `pending_payment`(`pp_mm_id`, `pp_status`);

CREATE INDEX `INDEX_system_parameter_setting_1`
    ON `system_parameter_setting`(`sps_code`);

CREATE INDEX `INDEX_temp_tender_record_1`
    ON `temp_tender_record`(`ttr_tm_sn` DESC);

CREATE INDEX `INDEX_tender_activity_record_1`
    ON `tender_activity_record`(`tar_tr_code` DESC);

CREATE INDEX `INDEX_tender_activity_record_2`
    ON `tender_activity_record`(`tar_status`);

CREATE INDEX `INDEX_tender_detail_1`
    ON `tender_detail`(`td_tm_id`);

CREATE INDEX `INDEX_tender_detail_2`
    ON `tender_detail`(`td_participants`);

CREATE INDEX `INDEX_tender_master_1`
    ON `tender_master`(`tm_group_datetime` DESC);

CREATE INDEX `INDEX_tender_master_2`
    ON `tender_master`(`tm_status`);

CREATE INDEX `INDEX_transaction_record_1`
    ON `transaction_record`(`tr_mm_id`);

CREATE INDEX `INDEX_transaction_record_2`
    ON `transaction_record`(`tr_mm_id`, `tr_type` DESC);

CREATE INDEX `INDEX_transaction_record_3`
    ON `transaction_record`(`tr_mm_id`);