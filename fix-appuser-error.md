# 修復 appuser 錯誤問題

## 🎯 問題狀況

環境變數已正確設定為 `rosca_user`，但仍出現 `appuser` 錯誤：
```
Access denied for user 'appuser'@'10.42.0.23' (using password: YES)
```

## 🔍 可能原因

1. **MariaDB 服務未重新初始化**: 舊的資料庫仍存在，沒有用新環境變數重建
2. **應用快取舊配置**: 應用可能快取了舊的連接字串
3. **Zeabur 內部變數**: 可能有 Zeabur 自動生成的變數覆蓋了設定

## 🚀 立即修復步驟

### 1. 清理不必要的環境變數

在應用服務中**刪除**這個變數：
```bash
❌ PASSWORD=b9hugSyVwFC27GJLYM3zk01sc8pnT645
```
這個變數可能會干擾正確的密碼設定。

### 2. 強制重新初始化 MariaDB

**方法 A: 重新建立 MariaDB 服務**
1. 在 Zeabur 控制台刪除 MariaDB 服務
2. 重新添加 MariaDB 服務
3. 設定正確的環境變數
4. 等待初始化完成

**方法 B: 清除資料庫資料**
1. 停止 MariaDB 服務
2. 清除持久化存儲
3. 重新啟動服務

### 3. 檢查 Zeabur 自動變數

確認 Zeabur 沒有自動生成衝突的變數：
- 檢查是否有 `ZEABUR_MARIADB_USERNAME` 等自動變數
- 這些可能會覆蓋手動設定的變數

### 4. 驗證連接字串

檢查實際的連接字串是否正確：
```
Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_secure_password_2024!;Database=rosca_db;...
```

## 🔍 除錯步驟

### 1. 檢查 MariaDB 日誌

在 Zeabur 控制台查看 MariaDB 服務日誌：
- 確認初始化腳本是否執行
- 檢查是否有使用者建立的訊息
- 查看是否有權限錯誤

### 2. 檢查應用日誌

查看應用服務日誌中的完整錯誤訊息：
- 確認使用的連接字串
- 檢查是否有其他資料庫錯誤

### 3. 測試資料庫連接

如果可以進入 MariaDB 容器：
```sql
-- 檢查使用者是否存在
SELECT User, Host FROM mysql.user WHERE User = 'rosca_user';

-- 檢查資料庫是否存在
SHOW DATABASES LIKE 'rosca_db';

-- 檢查權限
SHOW GRANTS FOR 'rosca_user'@'%';
```

## 💡 快速解決方案

### 選項 1: 重新建立服務 (推薦)

1. **備份重要資料** (如果有)
2. **刪除 MariaDB 服務**
3. **重新建立 MariaDB 服務**，設定環境變數：
   ```bash
   MYSQL_ROOT_PASSWORD=your_secure_root_password_2024!
   MYSQL_DATABASE=rosca_db
   MYSQL_USER=rosca_user
   MYSQL_PASSWORD=your_secure_password_2024!
   MYSQL_CHARACTER_SET_SERVER=utf8mb4
   MYSQL_COLLATION_SERVER=utf8mb4_general_ci
   TZ=Asia/Taipei
   ```
4. **等待初始化完成** (3-5 分鐘)
5. **重新部署應用服務**

### 選項 2: 手動建立使用者

如果不想重建服務，可以手動建立 `appuser`：

1. 進入 MariaDB 容器
2. 執行 SQL：
   ```sql
   CREATE USER 'appuser'@'%' IDENTIFIED BY 'your_secure_password_2024!';
   GRANT ALL PRIVILEGES ON rosca_db.* TO 'appuser'@'%';
   FLUSH PRIVILEGES;
   ```
3. 修改應用環境變數使用 `appuser`

## 🎯 推薦做法

**立即執行**：
1. 刪除 `PASSWORD` 環境變數
2. 重新建立 MariaDB 服務
3. 等待初始化完成
4. 測試連接

這樣可以確保資料庫完全按照新的環境變數初始化，解決 `appuser` 問題。