@echo off
REM ROSCA 系統啟動腳本 (Windows 版本)
REM 用於啟動所有 Docker 服務

echo 🚀 啟動 ROSCA 平安商會系統...

REM 檢查 Docker 是否運行
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ 錯誤: Docker 未運行，請先啟動 Docker
    pause
    exit /b 1
)

REM 檢查 docker-compose 是否可用
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 錯誤: docker-compose 未安裝
    pause
    exit /b 1
)

REM 檢查 .env 文件是否存在
if not exist ".env" (
    echo ⚠️  警告: .env 文件不存在，從 .env.example 複製...
    if exist ".env.example" (
        copy .env.example .env
        echo ✅ 已建立 .env 文件，請檢查並修改配置
    ) else (
        echo ❌ 錯誤: .env.example 文件不存在
        pause
        exit /b 1
    )
)

echo 📦 拉取最新映像...
docker-compose pull

echo 🔨 建置服務映像...
docker-compose build

echo 🚀 啟動服務...
docker-compose up -d

echo ⏳ 等待服務啟動...
timeout /t 10 /nobreak >nul

echo 🔍 檢查服務狀態...
docker-compose ps

echo.
echo ✅ ROSCA 系統啟動完成！
echo.
echo 📱 前台使用者系統: http://localhost
echo 🖥️  後台管理系統: http://localhost:8080
echo.
echo 👤 預設測試帳號:
echo    帳號: 0938766349
echo    密碼: 123456
echo.
echo 📋 管理命令:
echo    查看狀態: docker-compose ps
echo    查看日誌: docker-compose logs -f
echo    停止服務: docker-compose down
echo.
pause