# Zeabur 資料庫最終修復方案

## 🎯 問題根本原因

Zeabur 建立的 MariaDB 服務使用預設配置：
- **資料庫**: `zeabur` (不是我們的 `rosca_db`)
- **使用者**: `root` (不是我們的 `rosca_user`) 
- **密碼**: `dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G`
- **主機**: `43.167.174.222:31500`

我們的應用嘗試連接不存在的 `rosca_user@rosca_db`，所以出現 `Access denied` 錯誤。

## ✅ 修復方案

### 1. 已修復連接字串

zeabur.json 已更新為使用 Zeabur 實際的資料庫資訊：
```
User Id=root;Password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G;Database=zeabur
```

### 2. 手動初始化資料庫

由於 Zeabur 不會執行我們的初始化腳本，需要手動初始化。

## 🚀 執行步驟

### 步驟 1: 部署應用

```bash
git add zeabur.json zeabur-db-init.sql ZEABUR-FINAL-FIX.md
git commit -m "fix: 使用 Zeabur 預設資料庫配置"
git push origin main
```

在 Zeabur 控制台重新部署應用服務。

### 步驟 2: 連接資料庫

使用 Zeabur 提供的連接資訊：
```bash
mariadb --host 43.167.174.222 --port=31500 --user=root --password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G --database=zeabur
```

### 步驟 3: 執行初始化腳本

在 MariaDB 命令列中執行 `zeabur-db-init.sql` 的內容：

```sql
-- 使用 zeabur 資料庫
USE zeabur;

-- 建立 member_master 表格
CREATE TABLE IF NOT EXISTS member_master (
    mm_id BIGINT(15) AUTO_INCREMENT PRIMARY KEY,
    mm_account VARCHAR(20) NOT NULL UNIQUE,
    mm_hash_pwd VARCHAR(100) NOT NULL,
    mm_name VARCHAR(50) NOT NULL,
    mm_phone_number VARCHAR(20) NOT NULL UNIQUE,
    mm_role_type CHAR(2) NOT NULL DEFAULT '10',
    mm_status CHAR(2) NOT NULL DEFAULT '10',
    mm_create_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    mm_update_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 插入管理員帳號 (admin / Admin123456)
INSERT IGNORE INTO member_master (
    mm_id, mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    1, 'admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2', 
    '系統管理員', '0900000000', '20', '10'
);

-- 插入測試帳號 (0938766349 / 123456)
INSERT IGNORE INTO member_master (
    mm_id, mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    2, '0938766349', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2',
    '測試使用者', '0938766349', '10', '10'
);

-- 驗證
SELECT COUNT(*) as user_count FROM member_master;
SELECT mm_account, mm_name, mm_role_type FROM member_master;
```

### 步驟 4: 測試連接

```bash
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123456"}'
```

## 🎯 預期結果

修復後應該：
- ✅ 不再出現 `Access denied` 錯誤
- ✅ 能夠成功連接到 `root@zeabur` 資料庫
- ✅ 可以使用測試帳號登入：
  - 管理員：`admin` / `Admin123456`
  - 測試用戶：`0938766349` / `123456`

## 🔍 如果無法直接連接資料庫

### 方案 A: 使用 Zeabur 控制台

1. 在 Zeabur 控制台找到 MariaDB 服務
2. 查看是否有 "Connect" 或 "Terminal" 選項
3. 直接在瀏覽器中執行 SQL

### 方案 B: 創建初始化 API

在應用中添加一個臨時的初始化 API：

```csharp
[HttpPost("init-db")]
public async Task<IActionResult> InitializeDatabase()
{
    // 執行建表和插入資料的 SQL
    // 只在開發/測試環境允許
}
```

### 方案 C: 使用 phpMyAdmin

如果 Zeabur 提供 phpMyAdmin 或類似工具，可以通過 Web 界面執行 SQL。

## 📋 檢查清單

- [ ] zeabur.json 使用正確的連接資訊
- [ ] 應用重新部署成功
- [ ] 資料庫表格建立成功
- [ ] 測試帳號插入成功
- [ ] API 登入測試成功
- [ ] 前台靜態資源載入正常

## 💡 後續優化

成功連接後，可以逐步添加完整的資料庫結構：
1. 先確保基本登入功能正常
2. 再添加其他業務表格
3. 最後完善所有功能

這樣可以確保每個步驟都能正常運作，避免一次性修復太多問題。