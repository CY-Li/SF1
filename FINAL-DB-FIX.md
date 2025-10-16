# 最終資料庫修復方案

## 🎯 問題確認

從日誌可以看到：
- 錯誤：`Access denied for user 'appuser'@'10.42.0.34'`
- 回應時間：440ms (已改善，之前是 15 秒)
- 問題：`${DB_USER}` 變數解析為 `appuser`，不是我們設定的 `rosca_user`

## 🔍 根本原因

**Zeabur 可能有內建的變數覆蓋機制**：
- `DB_USER` 可能被 Zeabur 自動設為 `appuser`
- 我們的環境變數設定被覆蓋了

## 🚀 最終解決方案

### 方案 1: 硬編碼連接字串 (立即可用)

修改 zeabur.json，不使用變數：

```json
{
  "ConnectionStrings__BackEndDatabase": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_secure_password_2024!;Database=rosca_db;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;",
  "ConnectionStrings__DefaultConnection": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_secure_password_2024!;Database=rosca_db;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;"
}
```

### 方案 2: 配合 Zeabur 預設值

既然 Zeabur 堅持使用 `appuser`，我們就配合它：

1. **修改 MariaDB 環境變數**：
   ```bash
   MYSQL_USER=appuser
   MYSQL_PASSWORD=your_secure_password_2024!
   ```

2. **修改應用環境變數**：
   ```bash
   DB_USER=appuser
   DB_PASSWORD=your_secure_password_2024!
   ```

3. **修改初始化腳本**：
   將所有 `rosca_user` 改為 `appuser`

### 方案 3: 使用不同的變數名稱

使用 Zeabur 不會覆蓋的變數名稱：

```json
{
  "env": {
    "ROSCA_DB_USER": "rosca_user",
    "ROSCA_DB_PASSWORD": "your_secure_password_2024!",
    "ROSCA_DB_NAME": "rosca_db"
  }
}
```

然後在連接字串中使用：
```
User Id=${ROSCA_DB_USER};Password=${ROSCA_DB_PASSWORD};Database=${ROSCA_DB_NAME}
```

## 💡 推薦執行順序

### 立即修復 (方案 1)

1. **修改 zeabur.json 連接字串**：
   ```bash
   # 直接硬編碼，避免變數問題
   "ConnectionStrings__BackEndDatabase": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_secure_password_2024!;Database=rosca_db;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;"
   ```

2. **確保 MariaDB 環境變數正確**：
   ```bash
   MYSQL_USER=rosca_user
   MYSQL_PASSWORD=your_secure_password_2024!
   MYSQL_DATABASE=rosca_db
   ```

3. **重新部署**：
   - 先重新部署 MariaDB (清除舊資料)
   - 等待初始化完成
   - 再重新部署應用

4. **測試連接**：
   ```bash
   curl -X POST https://sf-test.zeabur.app/api/Login \
     -H "Content-Type: application/json" \
     -d '{"username":"admin","password":"Admin123456"}'
   ```

### 如果還是不行 (方案 2)

就配合 Zeabur 使用 `appuser`：

1. **修改所有配置使用 `appuser`**
2. **修改初始化腳本**
3. **重新部署**

## 🔍 除錯檢查

### 1. 確認實際的連接字串

在應用日誌中應該能看到實際使用的連接字串，檢查：
- User Id 是什麼
- Password 是什麼  
- Database 是什麼

### 2. 檢查 MariaDB 日誌

確認：
- 初始化腳本是否執行
- 使用者是否成功建立
- 權限是否正確設定

### 3. 手動測試連接

如果可以進入 MariaDB 容器：
```sql
SELECT User, Host FROM mysql.user;
SHOW DATABASES;
```

## 🎯 預期結果

修復後應該看到：
- 連接成功，無 `Access denied` 錯誤
- 能夠成功登入測試帳號
- API 回應時間保持在 500ms 以內

## 📋 修復腳本

我會創建一個修復版的 zeabur.json，直接硬編碼連接字串。