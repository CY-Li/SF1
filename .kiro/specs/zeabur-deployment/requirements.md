# Zeabur 部署需求文件

## 簡介

本文件定義了將現有的 Docker 容器化應用程式部署到 Zeabur 雲端平台的需求。系統包含前台用戶系統、後台管理系統、.NET Core API 服務和 MariaDB 資料庫。

## 需求

### 需求 1：Zeabur 服務配置

**用戶故事：** 作為開發者，我希望能夠在 Zeabur 平台上部署多個服務，以便提供完整的應用程式功能。

#### 驗收標準

1. WHEN 部署到 Zeabur THEN 系統 SHALL 包含以下服務：
   - MariaDB 資料庫服務
   - .NET Core 後端 API Gateway (port 5000)
   - .NET Core 後端微服務 (port 5001)
   - 前台用戶系統 (Vue.js + Nginx, port 80)
   - 後台管理系統 (Angular + Nginx, port 8080)

2. WHEN 配置服務 THEN 系統 SHALL 正確設定服務間的網路連接

3. WHEN 部署完成 THEN 每個服務 SHALL 能夠正常啟動並通過健康檢查

### 需求 2：環境變數配置

**用戶故事：** 作為運維人員，我希望能夠透過環境變數配置應用程式，以便在不同環境中靈活部署。

#### 驗收標準

1. WHEN 設定環境變數 THEN 系統 SHALL 支援以下配置：
   - 資料庫連接字串
   - JWT 設定 (SecretKey, Issuer, Audience, ExpiryMinutes)
   - CORS 設定
   - 檔案上傳設定
   - 日誌等級設定

2. WHEN 環境變數缺失 THEN 系統 SHALL 使用合理的預設值

3. WHEN 敏感資訊配置 THEN 系統 SHALL 使用 Zeabur 的環境變數功能保護敏感資料

### 需求 3：資料庫初始化

**用戶故事：** 作為系統管理員，我希望資料庫能夠自動初始化，以便快速部署新環境。

#### 驗收標準

1. WHEN 首次部署 THEN 資料庫 SHALL 自動執行 schema 初始化腳本

2. WHEN 初始化完成 THEN 資料庫 SHALL 包含預設資料

3. WHEN 初始化完成 THEN 系統 SHALL 包含預設管理員帳戶

### 需求 4：域名和 SSL 配置

**用戶故事：** 作為用戶，我希望能夠透過安全的 HTTPS 連接訪問應用程式，以確保資料傳輸安全。

#### 驗收標準

1. WHEN 配置域名 THEN 前台系統 SHALL 可透過主域名訪問

2. WHEN 配置域名 THEN 後台系統 SHALL 可透過子域名或路徑訪問

3. WHEN 啟用 SSL THEN 所有服務 SHALL 支援 HTTPS 連接

4. WHEN 使用 HTTP THEN 系統 SHALL 自動重定向到 HTTPS

### 需求 5：檔案存儲配置

**用戶故事：** 作為用戶，我希望上傳的檔案能夠持久化存儲，以便在服務重啟後仍能訪問。

#### 驗收標準

1. WHEN 上傳檔案 THEN 系統 SHALL 將檔案存儲到持久化存儲

2. WHEN 服務重啟 THEN 上傳的檔案 SHALL 仍然可以訪問

3. WHEN 配置存儲 THEN 系統 SHALL 支援以下檔案類型：
   - KYC 圖片
   - 存款圖片
   - 提款圖片
   - 公告圖片

### 需求 6：監控和日誌

**用戶故事：** 作為運維人員，我希望能夠監控應用程式狀態和查看日誌，以便及時發現和解決問題。

#### 驗收標準

1. WHEN 服務運行 THEN 系統 SHALL 提供健康檢查端點

2. WHEN 發生錯誤 THEN 系統 SHALL 記錄詳細的錯誤日誌

3. WHEN 查看日誌 THEN 運維人員 SHALL 能夠透過 Zeabur 控制台查看服務日誌

### 需求 7：性能和擴展性

**用戶故事：** 作為產品負責人，我希望系統能夠處理預期的用戶負載，並能夠根據需要擴展。

#### 驗收標準

1. WHEN 用戶訪問量增加 THEN 系統 SHALL 能夠自動擴展服務實例

2. WHEN 配置資源 THEN 每個服務 SHALL 有適當的 CPU 和記憶體限制

3. WHEN 負載測試 THEN 系統 SHALL 能夠處理預期的並發用戶數

### 需求 8：備份和恢復

**用戶故事：** 作為系統管理員，我希望能夠定期備份資料，以便在災難發生時能夠快速恢復。

#### 驗收標準

1. WHEN 配置備份 THEN 系統 SHALL 支援資料庫自動備份

2. WHEN 需要恢復 THEN 系統 SHALL 能夠從備份快速恢復資料

3. WHEN 備份完成 THEN 系統 SHALL 驗證備份檔案的完整性