-- ROSCA 平安商會系統預設使用者帳號建立腳本 (Zeabur 版本)
-- 建立預設測試帳號和管理員帳號

USE rosca_db;

-- 建立預設管理員帳號
-- 帳號: admin / 密碼: Admin123456
-- 密碼 'Admin123456' 的 BCrypt hash (cost=12)
INSERT INTO `member_master` (
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
    'admin',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '系統管理員',
    NULL,
    0,
    NULL,
    0,
    '1',
    '+886',
    'A123456789',
    '0900000000',
    NULL,
    'admin@rosca.com',
    '004',
    '1234567890123456',
    '系統管理員',
    '總行',
    '系統管理員',
    '0900000000',
    '本人',
    99,
    '20',
    '10',
    NULL,
    '20',
    1,
    NOW(),
    1,
    NOW()
);

-- 建立預設測試使用者
-- 帳號: 0938766349 / 密碼: 123456
-- 密碼 '123456' 的 BCrypt hash (cost=12)
INSERT INTO `member_master` (
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
    '0938766349',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '測試使用者',
    1,
    100001,
    1,
    100001,
    '1',
    '+886',
    'B123456789',
    '0938766349',
    NULL,
    'test@rosca.com',
    '004',
    '9876543210987654',
    '測試使用者',
    '分行',
    '測試受益人',
    '0938766349',
    '配偶',
    1,
    '10',
    '10',
    NULL,
    '10',
    1,
    NOW(),
    1,
    NOW()
);

-- 為預設使用者建立錢包餘額
INSERT INTO `member_balance` (
    `mb_mm_id`, `mb_sp_id`, `mb_balance`, `mb_frozen_balance`, 
    `mb_total_deposit`, `mb_total_withdraw`, `mb_last_transaction_datetime`,
    `mb_create_member`, `mb_create_datetime`, `mb_update_member`, `mb_update_datetime`
) VALUES 
-- 管理員餘額
(1, 11, 1000000.00, 0.00, 1000000.00, 0.00, NOW(), 1, NOW(), 1, NOW()),
(1, 12, 0.00, 0.00, 0.00, 0.00, NULL, 1, NOW(), 1, NOW()),
(1, 13, 0.00, 0.00, 0.00, 0.00, NULL, 1, NOW(), 1, NOW()),
-- 測試使用者餘額
(2, 11, 50000.00, 0.00, 50000.00, 0.00, NOW(), 1, NOW(), 1, NOW()),
(2, 12, 0.00, 0.00, 0.00, 0.00, NULL, 1, NOW(), 1, NOW()),
(2, 13, 0.00, 0.00, 0.00, 0.00, NULL, 1, NOW(), 1, NOW());

-- 為預設使用者建立錢包
INSERT INTO `member_wallet` (
    `mw_mm_id`, `mw_sp_id`, `mw_balance`, `mw_frozen_balance`, 
    `mw_address`, `mw_private_key`, `mw_status`,
    `mw_create_member`, `mw_create_datetime`, `mw_update_member`, `mw_update_datetime`
) VALUES 
-- 管理員錢包
(1, 11, 1000000.00, 0.00, NULL, NULL, '10', 1, NOW(), 1, NOW()),
(1, 12, 0.00, 0.00, NULL, NULL, '10', 1, NOW(), 1, NOW()),
(1, 13, 0.00, 0.00, NULL, NULL, '10', 1, NOW(), 1, NOW()),
-- 測試使用者錢包
(2, 11, 50000.00, 0.00, NULL, NULL, '10', 1, NOW(), 1, NOW()),
(2, 12, 0.00, 0.00, NULL, NULL, '10', 1, NOW(), 1, NOW()),
(2, 13, 0.00, 0.00, NULL, NULL, '10', 1, NOW(), 1, NOW());

-- 建立初始交易記錄
INSERT INTO `transaction_record` (
    `tr_mm_id`, `tr_type`, `tr_amount`, `tr_balance_before`, `tr_balance_after`,
    `tr_reference_id`, `tr_description`, `tr_status`, `tr_transaction_datetime`,
    `tr_create_member`, `tr_create_datetime`, `tr_update_member`, `tr_update_datetime`
) VALUES 
(1, '10', 1000000.00, 0.00, 1000000.00, 'INIT_001', '系統初始化 - 管理員帳戶', '10', NOW(), 1, NOW(), 1, NOW()),
(2, '10', 50000.00, 0.00, 50000.00, 'INIT_002', '系統初始化 - 測試帳戶', '10', NOW(), 1, NOW(), 1, NOW());

-- 建立範例標會 (由管理員建立)
INSERT INTO `tender_master` (
    `tm_id`, `tm_name`, `tm_host_mm_id`, `tm_total_amount`, `tm_period_amount`,
    `tm_total_periods`, `tm_current_period`, `tm_member_count`, 
    `tm_start_date`, `tm_bid_date`, `tm_payment_due_date`,
    `tm_status`, `tm_description`,
    `tm_create_member`, `tm_create_datetime`, `tm_update_member`, `tm_update_datetime`
) VALUES (
    1,
    '範例標會 - 10萬元12期',
    1,
    100000.00,
    8500.00,
    12,
    0,
    10,
    DATE_ADD(CURDATE(), INTERVAL 7 DAY),
    DATE_ADD(CURDATE(), INTERVAL 14 DAY),
    DATE_ADD(CURDATE(), INTERVAL 21 DAY),
    '10',
    '這是一個範例標會，用於系統測試。總金額10萬元，分12期，每期8500元。',
    1,
    NOW(),
    1,
    NOW()
);

-- 預設使用者帳號建立完成
SELECT 'Default users initialization completed for Zeabur deployment' as message;
SELECT 'Admin account: admin / Admin123456' as admin_info;
SELECT 'Test account: 0938766349 / 123456' as test_info;