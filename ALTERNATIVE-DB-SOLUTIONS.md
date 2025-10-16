# 🔄 Alternative Database Solutions

## 問題分析
`Access denied for user 'appuser'` 一直重複發生，可能的原因：

1. **Zeabur 自動重置資料庫**：每次重新部署可能會重置 MariaDB 容器
2. **權限配置被覆蓋**：Zeabur 的初始化腳本可能覆蓋了手動創建的使用者
3. **網路隔離問題**：容器間的網路連接可能有問題
4. **配置衝突**：zeabur.json 中的配置可能有衝突

## 🎯 解決方案 1: 使用 Root 使用者

最直接的解決方案是使用 root 使用者連接：

### 修改 zeabur.json
```json
{
  "ConnectionStrings__BackEndDatabase": "Server=43.167.174.222;Port=31500;User Id=root;Password=${DB_ROOT_PASSWORD};Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;",
  "ConnectionStrings__DefaultConnection": "Server=43.167.174.222;Port=31500;User Id=root;Password=${DB_ROOT_PASSWORD};Database=zeabur;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=60;CommandTimeout=120;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;ConnectRetryCount=3;ConnectRetryInterval=10;"
}
```

## 🎯 解決方案 2: 使用 Zeabur 環境變數

讓 Zeabur 自動管理資料庫連接：

### 修改 zeabur.json
```json
{
  "ConnectionStrings__BackEndDatabase": "${DATABASE_URL}",
  "ConnectionStrings__DefaultConnection": "${DATABASE_URL}"
}
```

## 🎯 解決方案 3: 使用內建 MariaDB 服務

完全使用 zeabur.json 中定義的 MariaDB 服務：

### 修改連接字串指向內部服務
```json
{
  "ConnectionStrings__BackEndDatabase": "Server=rosca-mariadb;Port=3306;User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;",
  "ConnectionStrings__DefaultConnection": "Server=rosca-mariadb;Port=3306;User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME};CharSet=utf8mb4;"
}
```

## 🎯 解決方案 4: 修復初始化腳本

確保初始化腳本正確創建 appuser：

### 更新 03-default-user.sql
```sql
-- 創建應用使用者
CREATE USER IF NOT EXISTS 'appuser'@'%' IDENTIFIED BY 'dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G';
GRANT ALL PRIVILEGES ON *.* TO 'appuser'@'%' WITH GRANT OPTION;
FLUSH PRIVILEGES;

-- 驗證使用者創建
SELECT User, Host FROM mysql.user WHERE User IN ('appuser', 'root');
```

## 🎯 解決方案 5: 簡化配置

移除複雜的連接池設定，使用最簡單的連接字串：

```json
{
  "ConnectionStrings__BackEndDatabase": "Server=43.167.174.222;Port=31500;User Id=root;Password=your_root_password;Database=zeabur;",
  "ConnectionStrings__DefaultConnection": "Server=43.167.174.222;Port=31500;User Id=root;Password=your_root_password;Database=zeabur;"
}
```

## 🚀 推薦方案

**立即嘗試方案 1（使用 Root）**：
1. 最可靠，root 使用者一定存在
2. 避免權限問題
3. 快速驗證連接是否正常

**長期方案 3（內建 MariaDB）**：
1. 完全控制資料庫配置
2. 避免外部依賴
3. 更好的隔離性