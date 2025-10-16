# 🔧 CommandTimeout 參數錯誤修復

## 問題描述
錯誤訊息從連接失敗改為：
```json
{
  "returnStatus": 999,
  "returnMsg": "取得提領申請列表失敗、Option 'commandtimeout' not supported."
}
```

## ✅ 進展
- ✅ 資料庫連接問題已解決
- ✅ 應用程式可以連接到 MariaDB
- ❌ 連接字串參數格式不相容

## 🎯 根本原因
MySQL/MariaDB 連接器不支援 `CommandTimeout` 參數，這是 SQL Server 特有的參數。

## 🔧 修復方案

### 1. 移除不支援的參數
已從連接字串中移除：
- `CommandTimeout=120` - MySQL/MariaDB 不支援
- 其他複雜的連接池參數

### 2. 使用簡化連接字串
**修正前：**
```
Server=rosca-mariadb;Port=3306;User Id=root;Password=rosca_root_2024!;Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;
```

**修正後：**
```
Server=rosca-mariadb;Port=3306;Uid=root;Pwd=rosca_root_2024!;Database=zeabur;CharSet=utf8mb4;
```

### 3. 更新的文件
- ✅ `backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/appsettings.json`
- ✅ `backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/appsettings.json`
- ✅ `zeabur-fixed.json`
- ✅ `zeabur-simple-connection.json` (新建，推薦使用)

## 🚀 部署步驟

### 選項 1: 使用簡化配置 (推薦)
```bash
# 使用最簡化的連接字串
cp zeabur-simple-connection.json zeabur.json
```

### 選項 2: 使用修正的配置
```bash
# 使用修正後的配置
cp zeabur-fixed.json zeabur.json
```

### 重新部署
在 Zeabur 控制台重新部署應用程式

## 🔍 驗證步驟

### 1. 檢查應用程式日誌
確認沒有 `commandtimeout` 錯誤

### 2. 測試 API 端點
```bash
# 測試提領申請列表
curl -X GET "https://your-app.zeabur.app/api/withdraw/list"
```

### 3. 檢查前台功能
- 登入系統
- 查看各種列表頁面
- 確認資料正常載入

## 📋 MySQL/MariaDB 支援的連接字串參數

**基本參數：**
- `Server` / `Host` - 伺服器地址
- `Port` - 端口號
- `Uid` / `User Id` - 用戶名
- `Pwd` / `Password` - 密碼
- `Database` - 資料庫名稱
- `CharSet` - 字符集

**進階參數 (可選)：**
- `ConnectionTimeout` - 連接超時 (秒)
- `AllowUserVariables` - 允許用戶變數
- `UseAffectedRows` - 使用受影響行數
- `Pooling` - 連接池
- `MinimumPoolSize` - 最小連接池大小
- `MaximumPoolSize` - 最大連接池大小

**不支援的參數：**
- ❌ `CommandTimeout` - SQL Server 專用
- ❌ `ConnectRetryCount` - SQL Server 專用
- ❌ `ConnectRetryInterval` - SQL Server 專用
- ❌ `ConnectionLifeTime` - 應為 `ConnectionLifeTime`

## 🎉 預期結果
修復後，API 應該能正常回傳資料：
```json
{
  "result": {
    "gridRespResult": [...],
    "gridTotalCount": 10
  },
  "returnStatus": 200,
  "returnMsg": "成功"
}
```

---

**狀態：** 已修復，等待部署驗證
**下一步：** 重新部署並測試 API 功能