# 🔍 Zeabur 資料庫連接診斷

## 問題現況
持續出現 `"Unable to connect to any of the specified MySQL hosts"` 錯誤

## 🎯 可能的根本原因

### 1. IP 地址或端口錯誤
當前使用：`43.167.174.222:31500`
**需要確認**：這是否是正確的 Zeabur MariaDB 外部訪問地址

### 2. 資料庫服務配置問題
**可能問題**：
- MariaDB 服務沒有啟用外部訪問
- 防火牆阻擋連接
- 服務重啟後 IP 改變

### 3. 認證問題
**可能問題**：
- root 密碼不正確
- root 使用者沒有外部訪問權限

## 🔧 診斷步驟

### 步驟 1: 確認 MariaDB 服務資訊
在 Zeabur 控制台檢查：
1. **MariaDB 服務狀態**：是否正常運行
2. **外部訪問地址**：確認正確的 IP 和端口
3. **連接資訊**：檢查 Zeabur 提供的連接字串

### 步驟 2: 測試資料庫連接
在 Zeabur MariaDB 控制台執行：
```sql
-- 檢查當前連接資訊
SELECT USER(), @@hostname, @@port;

-- 檢查 root 使用者權限
SELECT User, Host FROM mysql.user WHERE User = 'root';
SHOW GRANTS FOR 'root'@'%';

-- 確保外部訪問權限
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY 'dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G' WITH GRANT OPTION;
FLUSH PRIVILEGES;
```

### 步驟 3: 檢查網路連接
```bash
# 測試端口連接（在本地執行）
telnet 43.167.174.222 31500

# 或使用 nc
nc -zv 43.167.174.222 31500
```

## 🎯 解決方案選項

### 方案 A: 使用 Zeabur 環境變數
```json
{
  "ConnectionStrings__DefaultConnection": "${DATABASE_URL}"
}
```
**優點**：Zeabur 自動管理連接字串

### 方案 B: 修正硬編碼連接字串
確認正確的連接資訊後更新：
```json
{
  "ConnectionStrings__DefaultConnection": "Server=正確IP;Port=正確端口;Uid=root;Pwd=正確密碼;Database=zeabur;"
}
```

### 方案 C: 使用內部 MariaDB 服務
修改 zeabur.json 添加 MariaDB 服務：
```json
{
  "services": {
    "mariadb": {
      "template": "mariadb:11.3.2"
    }
  }
}
```

## 🚀 立即行動

### 1. 檢查 Zeabur 控制台
- 進入 MariaDB 服務頁面
- 確認服務狀態和連接資訊
- 記錄正確的外部訪問地址

### 2. 測試連接字串
在 MariaDB 控制台測試：
```sql
SELECT 'Connection Test' as status;
```

### 3. 根據結果選擇方案
- **如果 MariaDB 正常**：更新正確的連接資訊
- **如果 MariaDB 有問題**：重啟服務或使用環境變數
- **如果持續失敗**：考慮使用內部 MariaDB 服務

## 📝 需要確認的資訊

請在 Zeabur 控制台確認：
1. **MariaDB 服務的實際外部 IP 和端口**
2. **root 使用者的密碼**
3. **是否啟用了外部訪問**
4. **DATABASE_URL 環境變數的值**（如果有的話）

確認這些資訊後，我們可以提供更精確的修復方案。