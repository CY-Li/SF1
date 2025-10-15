# 實作計劃

- [x] 1. 建立 Docker 基礎設施和配置文件



  - 建立 docker-compose.yml 主配置文件，定義所有服務（MariaDB、後端 API、前台、後台管理系統）
  - 建立 .env 環境變數文件，包含資料庫密碼、API 設定等敏感資訊
  - 建立專案根目錄的基本檔案結構和說明文件
  - _需求: 1.1, 7.1, 7.2, 7.3_

- [x] 2. 設定 MariaDB 資料庫容器和初始化


  - [x] 2.1 建立資料庫初始化腳本



    - 將 SqlScript.txt 轉換為 Docker 初始化腳本 01-schema.sql
    - 建立預設資料載入腳本 02-default-data.sql（從 default data 資料夾）
    - 建立預設使用者帳號腳本 03-default-user.sql（帳號: 0938766349, 密碼: 123456）
    - _需求: 2.1, 2.2, 2.3, 6.1, 6.2, 6.3, 6.4_

  - [x] 2.2 配置 MariaDB 容器設定


    - 在 docker-compose.yml 中配置 MariaDB 11.3.2 服務
    - 設定環境變數（MYSQL_ROOT_PASSWORD, MYSQL_DATABASE, MYSQL_USER, MYSQL_PASSWORD）
    - 配置資料持久化 volume 和初始化腳本掛載
    - 建立健康檢查機制確保資料庫就緒
    - _需求: 2.1, 2.2, 5.1_

- [x] 3. 建立後端 API 容器

  - [x] 3.1 建立 .NET Core API Dockerfile


    - 建立多階段 Dockerfile（build stage 和 runtime stage）
    - 配置 .NET Core 7.0 SDK 和 ASP.NET Core runtime
    - 設定工作目錄、檔案複製和建置流程
    - 配置容器內部端口 5000 和啟動命令
    - _需求: 1.1, 1.2, 3.1, 3.2_

  - [x] 3.2 配置後端 API 服務設定


    - 在 docker-compose.yml 中配置後端服務
    - 設定環境變數（ASPNETCORE_ENVIRONMENT, ConnectionStrings）
    - 配置檔案上傳目錄的 volume 掛載
    - 設定服務依賴關係（depends_on MariaDB）和健康檢查
    - _需求: 2.4, 3.1, 3.2, 5.2_

- [x] 4. 建立前台使用者系統容器


  - [x] 4.1 建立前台 Dockerfile 和 Nginx 配置




    - 建立前台 Dockerfile 使用 nginx:alpine 基礎映像
    - 建立 nginx.conf 配置文件支援靜態檔案服務
    - 配置 API 代理設定（/api/ -> http://backend:5000/api/）
    - 設定正確的 MIME types 和快取策略
    - _需求: 1.1, 1.2, 3.1, 3.3_

  - [x] 4.2 修改前台 JavaScript 配置


    - 更新 frontend/js/library/axios.min.js 中的 baseUrl 設定
    - 將 baseUrl 改為相對路徑 "/api/" 以配合 Docker 環境
    - 確保所有 API 呼叫都能正確路由到後端容器
    - _需求: 3.1, 3.2, 3.3_

- [x] 5. 建立後台管理系統容器

  - [x] 5.1 建立後台 Angular Dockerfile


    - 建立多階段 Dockerfile（Node.js build stage 和 Nginx runtime stage）
    - 配置 Node.js 18 環境進行 Angular 專案建置
    - 設定 npm install 和 ng build 建置流程
    - 配置 Nginx 運行時環境和靜態檔案服務
    - _需求: 1.1, 1.2, 3.1, 3.2_

  - [x] 5.2 建立後台 Nginx 配置和服務設定


    - 建立 admin-nginx.conf 支援 Angular SPA 路由
    - 配置 API 代理設定指向後端容器
    - 在 docker-compose.yml 中配置後台服務（端口 8080）
    - 設定服務依賴關係和健康檢查
    - _需求: 3.1, 3.2, 3.3_



- [x] 6. 配置容器間網路和通信


  - 在 docker-compose.yml 中建立自定義網路
  - 配置所有服務使用相同的內部網路
  - 設定正確的服務名稱解析（backend, mariadb, frontend, admin）

  - 配置端口映射（80 前台，8080 後台，內部通信使用服務名稱）


  - _需求: 3.1, 3.2, 3.3_

- [x] 7. 建立管理腳本和文件

  - [x] 7.1 建立啟動和管理腳本


    - 建立 scripts/start.sh 腳本用於啟動所有服務


    - 建立 scripts/stop.sh 腳本用於停止所有服務
    - 建立 scripts/clean.sh 腳本用於清理容器和 volumes
    - 設定腳本執行權限和錯誤處理
    - _需求: 4.1, 4.2, 4.3, 4.4_


  - [x] 7.2 建立部署文件和說明


    - 建立 README.md 包含完整的部署說明
    - 建立 .env.example 範例環境變數文件
    - 建立故障排除指南和常見問題解答
    - 建立服務端口和訪問方式說明
    - _需求: 4.1, 4.2, 4.3, 4.4_

- [x] 8. 測試和驗證部署

  - [x] 8.1 執行整合測試



    - 測試 docker-compose up 命令能成功啟動所有服務
    - 驗證所有容器健康檢查通過
    - 測試容器間網路連通性
    - 驗證資料庫初始化和預設資料載入
    - _需求: 1.1, 1.2, 1.3, 2.1, 2.2, 2.3, 2.4_

  - [x] 8.2 測試應用程式功能


    - 測試前台系統訪問（http://localhost）
    - 測試後台管理系統訪問（http://localhost:8080）
    - 測試預設使用者帳號登入功能（0938766349/123456）
    - 驗證 API 呼叫和資料庫操作正常運作
    - _需求: 1.3, 3.1, 3.2, 3.3, 6.3, 6.4_

- [x] 9. 優化和文件完善




  - 優化 Docker 映像大小和建置時間
  - 完善錯誤處理和日誌記錄配置
  - 建立開發環境和生產環境的配置差異說明
  - 建立備份和恢復資料庫的操作指南
  - _需求: 4.1, 4.2, 4.3, 4.4, 5.1, 5.2, 5.3_