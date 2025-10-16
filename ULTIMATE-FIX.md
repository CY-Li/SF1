# 終極修復方案 - 完全硬編碼連接

## 🚨 問題確認

即使重新部署，仍然出現 `appuser` 錯誤，這表示：

1. **Zeabur 有內建覆蓋機制** - 自動將某些變數設為預設值
2. **環境變數被忽略** - zeabur.json 中的設定可能被覆蓋
3. **需要完全硬編碼** - 不使用任何 Zeabur 變數

## ✅ 最終修復方案

### 已執行：完全硬編碼連接字串

```json
{
  "ConnectionStrings__BackEndDatabase": "Server=43.167.174.222;Port=31500;User Id=root;Password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G;Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;",
  "ConnectionStrings__DefaultConnection": "Server=43.167.174.222;Port=31500;User Id=root;Password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G;Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;"
}
```

### 關鍵變更：
- ❌ 移除 `${ZEABUR_MARIADB_CONNECTION_HOST}`
- ❌ 移除 `${ZEABUR_MARIADB_CONNECTION_PORT}`
- ✅ 直接使用 `43.167.174.222:31500`
- ✅ 直接使用 `root` 使用者
- ✅ 直接使用實際密碼

## 🚀 執行步驟

### 1. 立即部署
```bash
git add zeabur.json ULTIMATE-FIX.md
git commit -m "fix: 完全硬編碼資料庫連接，繞過 Zeabur 變數覆蓋"
git push origin main
```

### 2. 重新部署應用
在 Zeabur 控制台重新部署 `rosca-app` 服務

### 3. 測試連接
```bash
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'
```

## 🎯 預期結果

這次應該會看到不同的錯誤：

### 成功案例 1: 連接成功但表格不存在
```
Table 'zeabur.member_master' doesn't exist
```
**這是好消息！** 表示連接成功，只需要初始化表格。

### 成功案例 2: 連接成功但使用者不存在
```
Invalid username or password
```
**這也是好消息！** 表示連接和表格都正常，只是測試帳號不存在。

### 失敗案例: 仍然是 appuser
```
Access denied for user 'appuser'
```
**如果還是這個錯誤**，表示 Zeabur 有更深層的覆蓋機制。

## 🔍 如果還是 appuser 錯誤

### 方案 A: 檢查應用配置
可能應用程式內部有硬編碼的連接字串或配置檔案。

### 方案 B: 創建 appuser
既然 Zeabur 堅持使用 `appuser`，我們就配合它：

1. 在 MariaDB 中創建 `appuser`：
```sql
CREATE USER 'appuser'@'%' IDENTIFIED BY 'dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G';
GRANT ALL PRIVILEGES ON zeabur.* TO 'appuser'@'%';
FLUSH PRIVILEGES;
```

2. 修改連接字串使用 `appuser`：
```
User Id=appuser;Password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G
```

### 方案 C: 檢查 .NET 配置
檢查應用程式是否有 `appsettings.json` 或其他配置檔案覆蓋了連接字串。

## 💡 下一步計劃

1. **先測試硬編碼版本** - 看是否能繞過 Zeabur 的覆蓋
2. **如果成功** - 初始化資料庫表格
3. **如果失敗** - 配合 Zeabur 使用 `appuser`

## 🔧 備用方案：配合 appuser

如果硬編碼還是不行，我們就完全配合 Zeabur：

```sql
-- 在 MariaDB 中執行
USE zeabur;

-- 創建 appuser
CREATE USER IF NOT EXISTS 'appuser'@'%' IDENTIFIED BY 'dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G';
GRANT ALL PRIVILEGES ON zeabur.* TO 'appuser'@'%';
FLUSH PRIVILEGES;

-- 建立表格
CREATE TABLE IF NOT EXISTS member_master (
    mm_id BIGINT AUTO_INCREMENT PRIMARY KEY,
    mm_account VARCHAR(20) NOT NULL UNIQUE,
    mm_hash_pwd VARCHAR(100) NOT NULL,
    mm_name VARCHAR(50) NOT NULL,
    mm_phone_number VARCHAR(20) NOT NULL UNIQUE,
    mm_role_type CHAR(2) NOT NULL DEFAULT '10',
    mm_status CHAR(2) NOT NULL DEFAULT '10',
    mm_create_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    mm_update_datetime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 插入測試帳號
INSERT INTO member_master (
    mm_id, mm_account, mm_hash_pwd, mm_name, mm_phone_number, mm_role_type, mm_status
) VALUES (
    1, 'admin', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBdXwtGtrKm9K2', 
    '系統管理員', '0900000000', '20', '10'
) ON DUPLICATE KEY UPDATE mm_account = mm_account;
```

然後修改連接字串使用 `appuser`。

**現在先重新部署，看看硬編碼版本的結果如何！**