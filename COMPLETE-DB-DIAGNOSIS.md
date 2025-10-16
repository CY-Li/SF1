# 完整資料庫問題診斷與修復

## 🚨 發現的問題

### 1. zeabur.json 語法錯誤
```json
    },  // ← 這個逗號是多餘的，會導致 JSON 無效

  },
  "domains": [
```

### 2. 可能的初始化問題
- Zeabur 可能因為 JSON 語法錯誤而忽略了 MariaDB 配置
- 初始化腳本可能沒有被正確載入

## 🔧 完整修復方案

### 步驟 1: 修復 zeabur.json 語法錯誤

需要移除多餘的逗號：

```json
{
  "name": "rosca-system",
  "services": {
    "app": {
      // ... app 配置
    },
    "mariadb": {
      "template": "mariadb:11.3.2",
      "name": "rosca-mariadb",
      // ... mariadb 配置
    }
  },
  "domains": [
    {
      "name": "frontend",
      "service": "frontend"
    },
    {
      "name": "admin", 
      "service": "admin"
    }
  ]
}
```

### 步驟 2: 驗證初始化腳本路徑

確認以下文件存在：
- ✅ `database/zeabur/my.cnf`
- ✅ `database/zeabur/docker-entrypoint-initdb.d/01-schema.sql`
- ✅ `database/zeabur/docker-entrypoint-initdb.d/02-default-data.sql`
- ✅ `database/zeabur/docker-entrypoint-initdb.d/03-default-user.sql`

### 步驟 3: 檢查 Zeabur 是否支援 configs

Zeabur 可能不支援 `configs` 配置方式。我們需要改用其他方法：

#### 方法 A: 使用 Zeabur 的 MariaDB 模板 (推薦)

```json
{
  "mariadb": {
    "template": "mariadb:11.3.2",
    "name": "rosca-mariadb",
    "env": {
      "MYSQL_ROOT_PASSWORD": "${DB_ROOT_PASSWORD}",
      "MYSQL_DATABASE": "${DB_NAME}",
      "MYSQL_USER": "${DB_USER}",
      "MYSQL_PASSWORD": "${DB_PASSWORD}",
      "MYSQL_CHARACTER_SET_SERVER": "utf8mb4",
      "MYSQL_COLLATION_SERVER": "utf8mb4_general_ci",
      "TZ": "Asia/Taipei"
    }
  }
}
```

#### 方法 B: 使用自定義 MariaDB Dockerfile

創建專門的 MariaDB Dockerfile：

```dockerfile
FROM mariadb:11.3.2

# 複製配置文件
COPY database/zeabur/my.cnf /etc/mysql/my.cnf

# 複製初始化腳本
COPY database/zeabur/docker-entrypoint-initdb.d/ /docker-entrypoint-initdb.d/

# 設定權限
RUN chmod +x /docker-entrypoint-initdb.d/*.sql
```

### 步驟 4: 簡化的 MariaDB 配置

如果 Zeabur 不支援複雜配置，我們可以簡化：

```json
{
  "mariadb": {
    "template": "mariadb:11.3.2",
    "name": "rosca-mariadb",
    "env": {
      "MYSQL_ROOT_PASSWORD": "your_secure_root_password_2024!",
      "MYSQL_DATABASE": "rosca_db",
      "MYSQL_USER": "rosca_user",
      "MYSQL_PASSWORD": "your_secure_password_2024!",
      "MYSQL_CHARACTER_SET_SERVER": "utf8mb4",
      "MYSQL_COLLATION_SERVER": "utf8mb4_general_ci"
    }
  }
}
```

然後手動執行初始化腳本。

## 🚀 立即執行步驟

### 1. 修復 JSON 語法
```bash
git add zeabur.json
git commit -m "fix: 修復 zeabur.json 語法錯誤"
git push origin main
```

### 2. 測試簡化配置

先嘗試最簡單的 MariaDB 配置，不使用 `configs`：

```json
{
  "name": "rosca-system",
  "services": {
    "app": {
      "build": {
        "dockerfile": "Dockerfile"
      },
      "name": "rosca-app",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "ConnectionStrings__BackEndDatabase": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_secure_password_2024!;Database=rosca_db;CharSet=utf8mb4;",
        "ConnectionStrings__DefaultConnection": "Server=${ZEABUR_MARIADB_CONNECTION_HOST};Port=${ZEABUR_MARIADB_CONNECTION_PORT};User Id=rosca_user;Password=your_secure_password_2024!;Database=rosca_db;CharSet=utf8mb4;"
      },
      "depends_on": ["mariadb"]
    },
    "mariadb": {
      "template": "mariadb:11.3.2",
      "name": "rosca-mariadb",
      "env": {
        "MYSQL_ROOT_PASSWORD": "your_secure_root_password_2024!",
        "MYSQL_DATABASE": "rosca_db",
        "MYSQL_USER": "rosca_user",
        "MYSQL_PASSWORD": "your_secure_password_2024!",
        "MYSQL_CHARACTER_SET_SERVER": "utf8mb4",
        "MYSQL_COLLATION_SERVER": "utf8mb4_general_ci"
      }
    }
  }
}
```

### 3. 手動初始化資料庫

如果自動初始化不工作，我們可以：

1. **連接到 MariaDB**
2. **手動執行 SQL 腳本**
3. **建立必要的表格和使用者**

### 4. 驗證連接

```bash
# 測試基本連接
curl -X POST https://sf-test.zeabur.app/api/health

# 測試資料庫連接 (會失敗但能看到具體錯誤)
curl -X POST https://sf-test.zeabur.app/api/Login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'
```

## 🔍 除錯檢查清單

- [ ] zeabur.json 語法正確
- [ ] MariaDB 服務成功啟動
- [ ] 環境變數正確設定
- [ ] 資料庫和使用者成功建立
- [ ] 應用能連接到資料庫
- [ ] 初始化腳本執行成功

## 💡 下一步

1. **先修復 JSON 語法錯誤**
2. **使用簡化的 MariaDB 配置**
3. **確認基本連接正常**
4. **再考慮初始化腳本**

這樣可以逐步排除問題，確定每個環節都正常運作。