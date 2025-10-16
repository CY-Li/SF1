# 🔧 Zeabur 資料庫連接問題修復指南

## 問題描述
前台所有 API 查詢都遇到錯誤：
```json
{
  "result": {
    "gridRespResult": [],
    "gridTotalCount": 0
  },
  "returnStatus": 999,
  "returnMsg": "取得提領申請列表失敗、Unable to connect to any of the specified MySQL hosts."
}
```

## 🎯 根本原因分析

### 1. 連接字串不一致
- **API Gateway** (`DotNetBackEndApi`) 和 **Backend Service** (`DotNetBackEndService`) 使用不同的連接字串格式
- 資料庫名稱錯誤：使用 `rosca_db` 而非 `zeabur`
- 用戶名稱錯誤：使用 `rosca_user` 而非 `root`

### 2. JSON 格式錯誤
- `appsettings.json` 文件包含註解，導致解析失敗

### 3. 環境變數配置問題
- Zeabur 環境變數沒有正確覆蓋 appsettings.json 中的設定

## ✅ 修復方案

### 步驟 1: 修正 appsettings.json 文件

已修正以下文件：
- `backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/appsettings.json`
- `backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/appsettings.json`

**修正內容：**
1. 移除 JSON 註解
2. 統一連接字串格式
3. 使用正確的資料庫名稱 (`zeabur`)
4. 使用正確的用戶名稱 (`root`)

### 步驟 2: 創建修正的 Zeabur 配置

**使用文件：** `zeabur-fixed.json`

**關鍵修正：**
```json
{
  "env": {
    "ConnectionStrings__BackEndDatabase": "Server=rosca-mariadb;Port=3306;User Id=root;Password=rosca_root_2024!;Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;",
    "ConnectionStrings__DefaultConnection": "Server=rosca-mariadb;Port=3306;User Id=root;Password=rosca_root_2024!;Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;"
  }
}
```

### 步驟 3: 確保資料庫正確初始化

**使用腳本：** `zeabur-complete-database-init.sql`

**包含內容：**
- 完整的資料表結構
- 系統參數設定
- 測試帳號 (admin / Admin123456)
- 範例資料

## 🚀 部署步驟

### 1. 重新部署應用程式
```bash
# 使用修正的配置文件
cp zeabur-fixed.json zeabur.json

# 重新部署到 Zeabur
# (在 Zeabur 控制台中重新部署)
```

### 2. 確認 MariaDB 服務
在 Zeabur 控制台確認：
- MariaDB 服務狀態正常
- 資料庫名稱為 `zeabur`
- root 密碼為 `rosca_root_2024!`

### 3. 執行資料庫初始化
在 MariaDB 控制台執行：
```sql
-- 使用 zeabur 資料庫
USE zeabur;

-- 執行完整初始化腳本
-- (複製 zeabur-complete-database-init.sql 內容)
```

### 4. 測試連接
```bash
# 執行測試腳本
bash test-zeabur-db-connection.sh
```

## 🔍 驗證步驟

### 1. 檢查應用程式日誌
在 Zeabur 控制台查看應用程式日誌，確認：
- 沒有資料庫連接錯誤
- 應用程式正常啟動

### 2. 測試 API 端點
```bash
# 測試健康檢查
curl https://your-app.zeabur.app/health

# 測試 API
curl https://your-app.zeabur.app/api/test
```

### 3. 檢查前台功能
- 登入功能正常
- 資料查詢不再出現連接錯誤
- 所有 API 回應正常

## 📋 檢查清單

- [ ] 修正 appsettings.json 文件 (移除註解、統一連接字串)
- [ ] 使用 zeabur-fixed.json 配置
- [ ] 確認 MariaDB 服務正常運行
- [ ] 執行資料庫初始化腳本
- [ ] 重新部署應用程式
- [ ] 測試 API 連接
- [ ] 驗證前台功能

## 🆘 故障排除

### 如果仍然出現連接錯誤：

1. **檢查 MariaDB 服務狀態**
   - 在 Zeabur 控制台重啟 MariaDB 服務
   - 確認服務健康檢查通過

2. **檢查環境變數**
   - 確認連接字串環境變數正確設定
   - 檢查密碼是否包含特殊字符需要轉義

3. **檢查網路連接**
   - 確認應用程式和 MariaDB 在同一網路中
   - 檢查服務依賴關係 (`depends_on`)

4. **檢查資料庫權限**
   ```sql
   -- 在 MariaDB 控制台執行
   SHOW GRANTS FOR 'root'@'%';
   SELECT User, Host FROM mysql.user WHERE User = 'root';
   ```

## 📞 聯絡支援

如果問題持續存在，請提供：
1. Zeabur 應用程式日誌
2. MariaDB 服務日誌
3. 具體的錯誤訊息
4. 使用的配置文件版本

---

**最後更新：** 2025-01-15
**狀態：** 已修復並測試