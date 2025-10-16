# 資料庫使用者不匹配問題修復

## 🚨 問題確認

錯誤訊息顯示：
```
Access denied for user 'appuser'@'10.42.0.23' (using password: YES)
```

這表示：
1. 應用嘗試使用 `appuser` 連接資料庫
2. 但資料庫初始化腳本建立的使用者是 `rosca_user`
3. 環境變數 `DB_USER` 設定為 `appuser`，與資料庫不匹配

## 🔍 問題根源

### 當前環境變數 (錯誤)
```bash
DB_USER=appuser          # ❌ 錯誤：資料庫中沒有這個使用者
DB_PASSWORD=???          # 密碼也可能不匹配
```

### 資料庫實際建立的使用者
根據初始化腳本 `03-default-user.sql`：
```sql
MYSQL_USER=rosca_user
MYSQL_PASSWORD=your_secure_password_2024!
```

## ✅ 修復方案

### 方案 1: 修改環境變數 (推薦)

在 Zeabur 控制台修改應用服務的環境變數：

```bash
# 修改這些變數
DB_USER=rosca_user                           # ✅ 改為正確的使用者名稱
DB_PASSWORD=your_secure_password_2024!       # ✅ 改為正確的密碼
DB_NAME=rosca_db                            # ✅ 確認資料庫名稱正確
```

### 方案 2: 修改資料庫初始化腳本

如果你想保持 `appuser`，需要修改初始化腳本：

```sql
-- 在 03-default-user.sql 中修改
MYSQL_USER=appuser
MYSQL_PASSWORD=your_actual_password
```

## 🚀 立即修復步驟

### 1. 檢查當前環境變數

在 Zeabur 控制台檢查以下變數的值：
- `DB_USER` (應用服務)
- `DB_PASSWORD` (應用服務)  
- `DB_NAME` (應用服務)
- `MYSQL_USER` (MariaDB 服務)
- `MYSQL_PASSWORD` (MariaDB 服務)
- `MYSQL_DATABASE` (MariaDB 服務)

### 2. 統一使用者配置

**選項 A: 使用 rosca_user (推薦)**
```bash
# 應用服務環境變數
DB_USER=rosca_user
DB_PASSWORD=your_secure_password_2024!
DB_NAME=rosca_db

# MariaDB 服務環境變數
MYSQL_USER=rosca_user
MYSQL_PASSWORD=your_secure_password_2024!
MYSQL_DATABASE=rosca_db
```

**選項 B: 使用 appuser**
```bash
# 應用服務環境變數
DB_USER=appuser
DB_PASSWORD=your_actual_password
DB_NAME=rosca_db

# MariaDB 服務環境變數  
MYSQL_USER=appuser
MYSQL_PASSWORD=your_actual_password
MYSQL_DATABASE=rosca_db
```

### 3. 重新部署

1. **先重新部署 MariaDB 服務**
   - 這會重新執行初始化腳本
   - 建立正確的使用者

2. **再重新部署應用服務**
   - 使用新的環境變數連接

### 4. 驗證修復

```bash
# 測試資料庫連接
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123456"}'

# 應該返回成功的登入回應，而不是資料庫錯誤
```

## 🔍 除錯檢查清單

### 1. 確認環境變數一致性

| 變數 | 應用服務 | MariaDB 服務 | 必須一致 |
|------|----------|--------------|----------|
| 使用者名稱 | `DB_USER` | `MYSQL_USER` | ✅ |
| 密碼 | `DB_PASSWORD` | `MYSQL_PASSWORD` | ✅ |
| 資料庫名稱 | `DB_NAME` | `MYSQL_DATABASE` | ✅ |

### 2. 檢查連接字串

連接字串應該是：
```
Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_secure_password_2024!;Database=rosca_db;...
```

### 3. 檢查 MariaDB 日誌

在 Zeabur 控制台查看 MariaDB 服務日誌，確認：
- 初始化腳本是否成功執行
- 使用者是否成功建立
- 是否有權限錯誤

## ⚠️ 常見錯誤

1. **大小寫敏感**: 確保使用者名稱大小寫正確
2. **特殊字符**: 密碼中的特殊字符可能需要轉義
3. **環境變數未更新**: 修改後需要重新部署才會生效
4. **服務啟動順序**: 確保 MariaDB 完全啟動後再啟動應用

## 📞 如果仍有問題

請提供：
1. Zeabur 控制台中應用服務的環境變數截圖
2. Zeabur 控制台中 MariaDB 服務的環境變數截圖  
3. MariaDB 服務的啟動日誌
4. 應用服務的錯誤日誌