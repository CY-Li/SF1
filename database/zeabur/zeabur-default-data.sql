-- ROSCA 平安商會系統 - Zeabur 預設資料載入腳本
-- 此腳本用於載入預設資料到 Zeabur 部署的外部 MariaDB 資料庫

USE zeabur;

-- 檢查是否已經有預設資料
SET @data_exists = (SELECT COUNT(*) FROM member_master WHERE mm_account = 'admin');

-- 只有在沒有預設資料時才執行插入
-- 插入系統參數設定
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
(5, 'TENDER_MIN_AMOUNT', '標會最小金額', '標會最小金額設定', '8000', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(6, 'JWT_SECRET', 'JWT密鑰', 'JWT Token 加密密鑰', 'your-super-secret-jwt-key-change-in-production', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(7, 'CORS_ORIGINS', 'CORS設定', '允許的跨域來源', 'https://your-app.zeabur.app,https://admin.your-app.zeabur.app', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(8, 'FILE_UPLOAD_MAX', '檔案上傳限制', '最大檔案上傳大小(bytes)', '10485760', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(9, 'HANGFIRE_ENABLED', 'Hangfire設定', '是否啟用Hangfire儀表板', 'true', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW()),
(10, 'LOG_LEVEL', '日誌等級', '系統日誌記錄等級', 'Information', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NOW(), 1, NOW());

-- 插入參數分類
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
(18, 'WALLET_TYPE', 'PUNISH_POINTS', '懲罰點數', '違規懲罰點數', 1, NOW(), 1, NOW()),
(19, 'MB_POINTS_TYPE', '10', '下標數(自己)', NULL, 1, NOW(), 1, NOW()),
(20, 'MB_POINTS_TYPE', '11', '儲值點數', NULL, 1, NOW(), 1, NOW()),
(21, 'MB_POINTS_TYPE', '12', '紅利點數', NULL, 1, NOW(), 1, NOW()),
(22, 'MB_POINTS_TYPE', '13', '平安點數', NULL, 1, NOW(), 1, NOW()),
(23, 'MB_POINTS_TYPE', '14', '商城點數', NULL, 1, NOW(), 1, NOW()),
(24, 'MB_POINTS_TYPE', '15', '註冊點數', NULL, 1, NOW(), 1, NOW()),
(25, 'MB_POINTS_TYPE', '16', '死會點數', NULL, 1, NOW(), 1, NOW()),
(26, 'MB_POINTS_TYPE', '17', '累積獎勵', NULL, 1, NOW(), 1, NOW()),
(27, 'MB_POINTS_TYPE', '18', '累積獎勵', NULL, 1, NOW(), 1, NOW()),
(28, 'MB_TYPE', '11', '下標扣除押金', NULL, 1, NOW(), 1, NOW()),
(29, 'MB_TYPE', '12', '下標扣除額', NULL, 1, NOW(), 1, NOW()),
(30, 'MB_TYPE', '13', '下標紅利', NULL, 1, NOW(), 1, NOW());

-- 插入範例公告
INSERT IGNORE INTO `announcement_board` (
    `ab_id`, `ab_title`, `ab_content`, `ab_status`, `ab_datetime`,
    `ab_create_member`, `ab_create_datetime`, `ab_update_datetime`, `ab_update_member`
) VALUES 
(1, '歡迎使用平安商會系統', '歡迎使用平安商會標會系統，請先完成 KYC 認證後即可開始參與標會活動。', '10', NOW(), 1, NOW(), 1, NOW()),
(2, '系統維護通知', '系統將於每週日凌晨 2:00-4:00 進行例行維護，期間可能無法正常使用，敬請見諒。', '10', NOW(), 1, NOW(), 1, NOW()),
(3, '標會規則說明', '請詳細閱讀標會規則，確保了解相關權利義務後再參與標會活動。', '10', NOW(), 1, NOW(), 1, NOW()),
(4, 'Zeabur 部署成功', '系統已成功部署到 Zeabur 雲端平台，享受更穩定的服務體驗。', '10', NOW(), 1, NOW(), 1, NOW());

-- 建立預設測試使用者
-- 密碼 '123456' 的 SHA256 hash
INSERT IGNORE INTO `member_master` (
    `mm_id`,
    `mm_account`,
    `mm_hash_pwd`,
    `mm_2nd_hash_pwd`,
    `mm_name`,
    `mm_introduce_user`,
    `mm_introduce_code`,
    `mm_invite_user`,
    `mm_invite_code`,
    `mm_gender`,
    `mm_country_code`,
    `mm_personal_id`,
    `mm_phone_number`,
    `mm_mw_address`,
    `mm_email`,
    `mm_bank_code`,
    `mm_bank_account`,
    `mm_bank_account_name`,
    `mm_branch`,
    `mm_beneficiary_name`,
    `mm_beneficiary_phone`,
    `mm_beneficiary_relationship`,
    `mm_level`,
    `mm_role_type`,
    `mm_status`,
    `mm_kyc_id`,
    `mm_kyc`,
    `mm_create_member`,
    `mm_create_datetime`,
    `mm_update_member`,
    `mm_update_datetime`
) VALUES (
    1,
    '0938766349',
    '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92',
    '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92',
    '測試使用者',
    NULL,
    0,
    NULL,
    0,
    '1',
    '+886',
    'A123456789',
    '0938766349',
    '台北市信義區信義路五段7號',
    'test@example.com',
    '004',
    '1234567890123456',
    '測試使用者',
    '總行',
    '測試受益人',
    '0938766349',
    '本人',
    '1',
    '10',
    'Y',
    1,
    'Y',
    1,
    NOW(),
    1,
    NOW()
);

-- 建立對應的會員錢包
INSERT IGNORE INTO `member_wallet` (
    `mw_id`,
    `mw_mm_id`,
    `mw_currency`,
    `mw_address`,
    `mw_subscripts_count`,
    `mw_stored`,
    `mw_reward`,
    `mw_peace`,
    `mw_mall`,
    `mw_registration`,
    `mw_death`,
    `mw_accumulation`,
    `mw_punish`,
    `mw_level`,
    `mw_create_member`,
    `mw_create_datetime`,
    `mw_update_member`,
    `mw_update_datetime`
) VALUES (
    1,
    1,
    'TWD',
    '台北市信義區信義路五段7號',
    0,
    10000.00000,  -- 給予初始存款點數 10000
    0.00000,
    0.00000,
    0.00000,
    1000.00000,   -- 給予註冊點數 1000
    0.00000,
    0.00000,
    0.00000,
    '1',
    1,
    NOW(),
    1,
    NOW()
);

-- 建立預設 KYC 認證記錄
INSERT IGNORE INTO `apply_kyc_certification` (
    `akc_id`,
    `akc_mm_id`,
    `akc_name`,
    `akc_front`,
    `akc_back`,
    `akc_gender`,
    `akc_personal_id`,
    `akc_mw_address`,
    `akc_email`,
    `akc_bank_code`,
    `akc_bank_account`,
    `akc_bank_account_name`,
    `akc_branch`,
    `akc_beneficiary_name`,
    `akc_beneficiary_phone`,
    `akc_beneficiary_relationship`,
    `akc_status`,
    `akc_create_member`,
    `akc_create_datetime`,
    `akc_update_member`,
    `akc_update_datetime`
) VALUES (
    1,
    1,
    '測試使用者',
    'default_id_front.jpg',
    'default_id_back.jpg',
    '1',
    'A123456789',
    '台北市信義區信義路五段7號',
    'test@example.com',
    '004',
    '1234567890123456',
    '測試使用者',
    '總行',
    '測試受益人',
    '0938766349',
    '本人',
    '20',  -- 審核通過
    1,
    NOW(),
    1,
    NOW()
);

-- 建立管理員帳號
-- 密碼 'Admin123456' 的 SHA256 hash
INSERT IGNORE INTO `member_master` (
    `mm_id`,
    `mm_account`,
    `mm_hash_pwd`,
    `mm_2nd_hash_pwd`,
    `mm_name`,
    `mm_introduce_user`,
    `mm_introduce_code`,
    `mm_invite_user`,
    `mm_invite_code`,
    `mm_gender`,
    `mm_country_code`,
    `mm_personal_id`,
    `mm_phone_number`,
    `mm_mw_address`,
    `mm_email`,
    `mm_bank_code`,
    `mm_bank_account`,
    `mm_bank_account_name`,
    `mm_branch`,
    `mm_beneficiary_name`,
    `mm_beneficiary_phone`,
    `mm_beneficiary_relationship`,
    `mm_level`,
    `mm_role_type`,
    `mm_status`,
    `mm_kyc_id`,
    `mm_kyc`,
    `mm_create_member`,
    `mm_create_datetime`,
    `mm_update_member`,
    `mm_update_datetime`
) VALUES (
    2,
    'admin',
    'C7AD44CBAD762A5DA0A452F9E854FDC1E0E7A52A38015F23F3EAB1D80B931DD4',
    'C7AD44CBAD762A5DA0A452F9E854FDC1E0E7A52A38015F23F3EAB1D80B931DD4',
    '系統管理員',
    NULL,
    0,
    NULL,
    0,
    '1',
    '+886',
    'B987654321',
    'admin',
    '台北市信義區信義路五段7號',
    'admin@example.com',
    '004',
    '9876543210987654',
    '系統管理員',
    '總行',
    '系統管理員',
    'admin',
    '本人',
    '9',
    '20',  -- 管理員角色
    'Y',
    2,
    'Y',
    1,
    NOW(),
    1,
    NOW()
);

-- 建立管理員錢包
INSERT IGNORE INTO `member_wallet` (
    `mw_id`,
    `mw_mm_id`,
    `mw_currency`,
    `mw_address`,
    `mw_subscripts_count`,
    `mw_stored`,
    `mw_reward`,
    `mw_peace`,
    `mw_mall`,
    `mw_registration`,
    `mw_death`,
    `mw_accumulation`,
    `mw_punish`,
    `mw_level`,
    `mw_create_member`,
    `mw_create_datetime`,
    `mw_update_member`,
    `mw_update_datetime`
) VALUES (
    2,
    2,
    'TWD',
    '台北市信義區信義路五段7號',
    0,
    999999.00000,  -- 管理員大額點數
    0.00000,
    0.00000,
    0.00000,
    0.00000,
    0.00000,
    0.00000,
    0.00000,
    '9',
    1,
    NOW(),
    1,
    NOW()
);

-- 建立管理員 KYC 認證記錄
INSERT IGNORE INTO `apply_kyc_certification` (
    `akc_id`,
    `akc_mm_id`,
    `akc_name`,
    `akc_front`,
    `akc_back`,
    `akc_gender`,
    `akc_personal_id`,
    `akc_mw_address`,
    `akc_email`,
    `akc_bank_code`,
    `akc_bank_account`,
    `akc_bank_account_name`,
    `akc_branch`,
    `akc_beneficiary_name`,
    `akc_beneficiary_phone`,
    `akc_beneficiary_relationship`,
    `akc_status`,
    `akc_create_member`,
    `akc_create_datetime`,
    `akc_update_member`,
    `akc_update_datetime`
) VALUES (
    2,
    2,
    '系統管理員',
    'admin_id_front.jpg',
    'admin_id_back.jpg',
    '1',
    'B987654321',
    '台北市信義區信義路五段7號',
    'admin@example.com',
    '004',
    '9876543210987654',
    '系統管理員',
    '總行',
    '系統管理員',
    'admin',
    '本人',
    '20',  -- 審核通過
    1,
    NOW(),
    1,
    NOW()
);

SELECT 'Default data insertion completed.' as status;
SELECT 'Default accounts created:' as info;
SELECT 'Admin: admin / Admin123456' as admin_account;
SELECT 'Test User: 0938766349 / 123456' as test_account;