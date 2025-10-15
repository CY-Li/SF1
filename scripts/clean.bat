@echo off
REM ROSCA 系統清理腳本 (Windows 版本)
REM 用於完全清理所有 Docker 資源

echo 🧹 清理 ROSCA 平安商會系統...

REM 檢查 docker-compose 是否可用
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ 錯誤: docker-compose 未安裝
    pause
    exit /b 1
)

REM 詢問用戶確認
echo ⚠️  警告: 此操作將會：
echo    - 停止並移除所有容器
echo    - 移除所有相關映像
echo    - 刪除所有資料 (包含資料庫)
echo    - 移除所有 volumes
echo.
set /p confirm=確定要繼續嗎？(y/N): 

if /i not "%confirm%"=="y" (
    echo ❌ 操作已取消
    pause
    exit /b 0
)

echo 🛑 停止所有服務...
docker-compose down

echo 🗑️  移除容器和映像...
docker-compose down --rmi all

echo 💾 移除 volumes (資料庫資料將被刪除)...
docker-compose down -v --rmi all

echo 🧹 清理未使用的 Docker 資源...
docker system prune -f

echo 🧹 清理未使用的 volumes...
docker volume prune -f

echo.
echo ✅ 清理完成！
echo.
echo 📋 重新開始:
echo    啟動系統: scripts\start.bat
echo.
pause