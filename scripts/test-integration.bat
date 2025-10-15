@echo off
REM ROSCA 系統整合測試腳本 (Windows 版本)
REM 測試 Docker Compose 部署的完整性

setlocal enabledelayedexpansion

echo 🧪 開始 ROSCA 系統整合測試...
echo ==================================

REM 測試結果統計
set /a PASSED=0
set /a FAILED=0
set /a TOTAL=0

REM 檢查前置條件
echo 🔍 檢查前置條件...
echo --------------------------------

echo 測試 Docker 是否運行:
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo 測試 Docker Compose 是否可用:
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo 測試 .env 檔案是否存在:
if exist ".env" (
    echo ✅ 通過
    set /a PASSED+=1
) else (
    echo ❌ 失敗
    set /a FAILED+=1
)
set /a TOTAL+=1

echo 測試 docker-compose.yml 是否有效:
docker-compose config >nul 2>&1
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo.

REM 測試 Docker Compose 啟動
echo 🚀 測試 Docker Compose 啟動...
echo --------------------------------

echo 啟動所有服務...
docker-compose up -d
if errorlevel 1 (
    echo ❌ 服務啟動失敗
    set /a FAILED+=1
) else (
    echo ✅ 服務啟動成功
    set /a PASSED+=1
)
set /a TOTAL+=1

REM 等待服務就緒
echo ⏳ 等待服務就緒 (60秒)...
timeout /t 60 /nobreak >nul

echo.

REM 測試容器狀態
echo 📦 測試容器狀態...
echo --------------------------------

echo 測試 rosca-mariadb 容器運行:
docker ps --format "{{.Names}}" | findstr /r "^rosca-mariadb$" >nul
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo 測試 rosca-backend 容器運行:
docker ps --format "{{.Names}}" | findstr /r "^rosca-backend$" >nul
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo 測試 rosca-frontend 容器運行:
docker ps --format "{{.Names}}" | findstr /r "^rosca-frontend$" >nul
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo 測試 rosca-admin 容器運行:
docker ps --format "{{.Names}}" | findstr /r "^rosca-admin$" >nul
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo.

REM 等待健康檢查穩定
echo 等待健康檢查穩定...
timeout /t 30 /nobreak >nul

REM 測試 HTTP 服務
echo 🌐 測試 HTTP 服務回應...
echo --------------------------------

REM 等待 HTTP 服務完全就緒
timeout /t 10 /nobreak >nul

echo 測試前台 HTTP 回應:
curl -f -s http://localhost >nul 2>&1
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo 測試後台 HTTP 回應:
curl -f -s http://localhost:8080 >nul 2>&1
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo.

REM 測試資料持久化
echo 💾 測試資料持久化...
echo --------------------------------

echo 測試資料庫 volume 存在:
docker volume ls | findstr "db-data" >nul
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo 測試上傳 volume 存在:
docker volume ls | findstr "uploads" >nul
if errorlevel 1 (
    echo ❌ 失敗
    set /a FAILED+=1
) else (
    echo ✅ 通過
    set /a PASSED+=1
)
set /a TOTAL+=1

echo.

REM 顯示服務狀態
echo 📊 當前服務狀態...
echo --------------------------------
docker-compose ps

echo.

REM 顯示資源使用情況
echo 💻 資源使用情況...
echo --------------------------------
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}"

echo.

REM 測試結果統計
echo 📈 測試結果統計
echo ==================================
echo ✅ 通過: %PASSED%
echo ❌ 失敗: %FAILED%
echo 📊 總計: %TOTAL%

set /a SUCCESS_RATE=PASSED*100/TOTAL
echo 📈 成功率: %SUCCESS_RATE%%%

if %FAILED% equ 0 (
    echo.
    echo 🎉 所有整合測試通過！
    echo 系統已成功部署並運行正常。
    echo.
    echo 🌐 訪問地址:
    echo    前台系統: http://localhost
    echo    後台管理: http://localhost:8080
    echo.
    echo 👤 預設測試帳號:
    echo    帳號: 0938766349
    echo    密碼: 123456
    echo.
) else (
    echo.
    echo ⚠️ 有 %FAILED% 個測試失敗！
    echo.
    echo 🔧 故障排除建議:
    echo 1. 檢查服務日誌: docker-compose logs [service_name]
    echo 2. 檢查服務狀態: docker-compose ps
    echo 3. 檢查系統資源: docker stats
    echo 4. 重啟服務: docker-compose restart [service_name]
    echo 5. 完全重啟: docker-compose down ^&^& docker-compose up -d
    echo.
    echo 📋 詳細故障排除請參考: TROUBLESHOOTING.md
    echo.
)

pause