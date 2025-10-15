-- ROSCA 平安商會系統預設資料載入腳本
-- 此腳本會在資料庫結構建立完成後執行

USE rosca2;

-- 插入系統參數設定
INSERT INTO `system_parameter_setting` (
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

-- 插入參數分類
INSERT INTO `parameter_category` (
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

-- 插入範例公告
INSERT INTO `announcement_board` (
    `ab_id`, `ab_title`, `ab_content`, `ab_status`, `ab_datetime`,
    `ab_create_member`, `ab_create_datetime`, `ab_update_datetime`, `ab_update_member`
) VALUES 
(1, '歡迎使用平安商會系統', '歡迎使用平安商會標會系統，請先完成 KYC 認證後即可開始參與標會活動。', '10', NOW(), 1, NOW(), 1, NOW()),
(2, '系統維護通知', '系統將於每週日凌晨 2:00-4:00 進行例行維護，期間可能無法正常使用，敬請見諒。', '10', NOW(), 1, NOW(), 1, NOW()),
(3, '標會規則說明', '請詳細閱讀標會規則，確保了解相關權利義務後再參與標會活動。', '10', NOW(), 1, NOW(), 1, NOW());