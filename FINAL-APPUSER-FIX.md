# 🔧 Final AppUser Fix for Zeabur Database

## 問題分析
錯誤 `Access denied for user 'appuser'@'10.42.0.42'` 表示：
1. appuser 在 Zeabur MariaDB 中不存在
2. 或者 appuser 存在但沒有正確的權限

## ✅ 解決方案

### 步驟 1: 在 Zeabur MariaDB 中創建 appuser

在 Zeabur 控制台的 MariaDB 服務中執行以下 SQL：

```sql
-- 創建 appuser 使用者
CREATE USER IF NOT EXISTS 'appuser'@'%' IDENTIFIED BY 'dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G';

-- 授予所有權限給 appuser
GRANT ALL PRIVILEGES ON zeabur.* TO 'appuser'@'%';

-- 刷新權限
FLUSH PRIVILEGES;

-- 驗證使用者創建成功
SELECT User, Host FROM mysql.user WHERE User = 'appuser';

-- 驗證權限
SHOW GRANTS FOR 'appuser'@'%';
```

### 步驟 2: 驗證資料庫連接

```sql
-- 測試連接（在 Zeabur MariaDB 控制台執行）
SELECT 'Connection successful' as status;

-- 檢查現有表格
SHOW TABLES;

-- 檢查 member_master 表格
SELECT COUNT(*) FROM member_master;
```

### 步驟 3: 如果需要，重新創建表格

如果表格不存在，執行：

```sql
-- 創建 member_master 表格
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

-- 插入測試帳號
INSERT IGNORE INTO member_master (
    mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    'admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '系統管理員', '0938766349', '10', '10'
);
```

### 步驟 4: 測試應用連接

重新部署應用後測試：

```bash
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123456"}'
```

## 🎯 預期結果

執行完上述步驟後：
- ✅ appuser 應該能成功連接到資料庫
- ✅ 不再出現 "Access denied" 錯誤
- ✅ 登入 API 應該正常運作

## 🔍 故障排除

如果仍有問題：

1. **檢查使用者是否存在**：
```sql
SELECT User, Host FROM mysql.user WHERE User = 'appuser';
```

2. **檢查權限**：
```sql
SHOW GRANTS FOR 'appuser'@'%';
```

3. **重置密碼**：
```sql
ALTER USER 'appuser'@'%' IDENTIFIED BY 'dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G';
FLUSH PRIVILEGES;
```

4. **檢查連接字串**：
確認 zeabur.json 中的密碼正確：`dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G`

## 📝 重要提醒

- 在 Zeabur MariaDB 控制台執行 SQL 命令
- 確保使用正確的密碼
- 執行完畢後重新部署應用
- 測試 API 連接確認修復成功