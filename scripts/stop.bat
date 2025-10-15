@echo off
REM ROSCA 系統停止腳本 (Windows 版本)
REM 用於停止所有 Docker 服務

echo 🛑 停止 ROSCA 平安商會系統...

REM 檢查 docker-compose 是否可用
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 錯誤: docker-compose 未安裝
    pause
    exit /b 1
)

REM 檢查是否有運行中的服務
docker-compose ps -q >nul 2>&1
if not errorlevel 1 (
    echo 📋 當前運行的服務:
    docker-compose ps
    
    echo.
    echo 🛑 停止所有服務...
    docker-compose down
    
    echo ✅ 所有服務已停止
) else (
    echo ℹ️  沒有運行中的服務
)

echo.
echo 📋 管理命令:
echo    重新啟動: scripts\start.bat
echo    完全清理: scripts\clean.bat
echo    查看狀態: docker-compose ps
echo.
pause