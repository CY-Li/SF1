# 🔍 Zeabur 資料庫連接測試

## 當前錯誤
`"Unable to connect to any of the specified MySQL hosts"`

## 🔧 已修復的問題

### 1. 統一連接字串格式
```json
// zeabur.json - 修改前
"User Id=root;Password=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G"

// zeabur.json - 修改後
"Uid=root;Pwd=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G"
```

### 2. 簡化連接字串
移除了複雜的連接池參數，使用最簡單的格式：
```
Server=43.167.174.222;Port=31500;Uid=root;Pwd=dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G;Database=zeabur;CharSet=utf8mb4;
```

## 🧪 測試步驟

### 1. 在 Zeabur MariaDB 控制台測試
```sql
-- 測試 root 使用者連接
SELECT USER(), DATABASE(), VERSION();

-- 檢查 zeabur 資料庫是否存在
SHOW DATABASES;

-- 使用 zeabur 資料庫
USE zeabur;

-- 檢查表格
SHOW TABLES;

-- 測試 member_master 表格
SELECT COUNT(*) FROM member_master;
```

### 2. 重新部署應用
在 Zeabur 控制台重新部署 `rosca-app` 服務

### 3. 測試 API 連接
```bash
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"mm_account":"admin","mm_pwd":"Admin123456"}'
```

## 🎯 可能的問題和解決方案

### 問題 1: 資料庫服務器不可達
**檢查**: Zeabur MariaDB 服務是否正常運行
**解決**: 在 Zeabur 控制台重啟 MariaDB 服務

### 問題 2: 防火牆或網路問題
**檢查**: IP `43.167.174.222:31500` 是否可達
**解決**: 確認 Zeabur MariaDB 的公網訪問設定

### 問題 3: 認證問題
**檢查**: root 使用者密碼是否正確
**解決**: 在 MariaDB 控制台重置 root 密碼

### 問題 4: 資料庫不存在
**檢查**: `zeabur` 資料庫是否存在
**解決**: 執行 `CREATE DATABASE IF NOT EXISTS zeabur;`

## 🔍 故障排除命令

### 在 Zeabur MariaDB 控制台執行
```sql
-- 檢查使用者權限
SELECT User, Host FROM mysql.user WHERE User = 'root';
SHOW GRANTS FOR 'root'@'%';

-- 檢查資料庫
SHOW DATABASES;

-- 創建 zeabur 資料庫（如果不存在）
CREATE DATABASE IF NOT EXISTS zeabur CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

-- 確保 root 有完整權限
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;
FLUSH PRIVILEGES;
```

## 📝 預期結果
- ✅ 資料庫連接成功
- ✅ 不再出現 "Unable to connect" 錯誤
- ✅ API 正常回應
- ✅ 登入功能正常運作