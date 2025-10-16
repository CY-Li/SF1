-- 更新現有帳號的密碼為 SHA256 格式
-- 密碼: Admin123456
-- SHA256: 1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A

USE zeabur;

-- 更新 admin 帳號密碼
UPDATE member_master 
SET mm_hash_pwd = '1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A'
WHERE mm_account = 'admin';

-- 更新 0938766349 帳號密碼
UPDATE member_master 
SET mm_hash_pwd = '1CD73874884DB6A0F9F21D853D7E9EACDC773C39EE389060F5E96AE0BCB4773A'
WHERE mm_account = '0938766349';

-- 驗證更新結果
SELECT 
    mm_account, 
    mm_hash_pwd, 
    mm_name, 
    mm_role_type,
    LENGTH(mm_hash_pwd) as password_length
FROM member_master 
WHERE mm_account IN ('admin', '0938766349');

SELECT 'SHA256 密碼更新完成！' as status;