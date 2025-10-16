# Zeabur 部署前驗證腳本
# 此腳本檢查專案是否準備好部署到 Zeabur

Write-Host "=== ROSCA 系統 Zeabur 部署前檢查 ===" -ForegroundColor Green
Write-Host ""

$errors = 0
$warnings = 0

# 檢查必要檔案
Write-Host "檢查必要檔案..." -ForegroundColor Yellow

$requiredFiles = @(
    "zeabur.json",
    ".env.zeabur",
    "ZEABUR-DEPLOYMENT.md",
    "backendAPI/DotNetBackEndCleanArchitecture/Dockerfile",
    "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile",
    "frontend/Dockerfile",
    "backend/FontEnd/Dockerfile"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "✅ $file" -ForegroundColor Green
    } else {
        Write-Host "❌ $file (缺失)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# 檢查 zeabur.json 配置
Write-Host "檢查 zeabur.json 配置..." -ForegroundColor Yellow

if (Test-Path "zeabur.json") {
    try {
        $zeaburConfig = Get-Content "zeabur.json" | ConvertFrom-Json
        
        # 檢查專案名稱
        if ($zeaburConfig.name) {
            Write-Host "✅ 專案名稱: $($zeaburConfig.name)" -ForegroundColor Green
        } else {
            Write-Host "❌ 缺少專案名稱" -ForegroundColor Red
            $errors++
        }
        
        # 檢查服務配置
        $expectedServices = @("backend-service", "api-gateway", "frontend", "admin")
        foreach ($service in $expectedServices) {
            if ($zeaburConfig.services.$service) {
                Write-Host "✅ 服務配置: $service" -ForegroundColor Green
            } else {
                Write-Host "❌ 缺少服務配置: $service" -ForegroundColor Red
                $errors++
            }
        }
        
        # 檢查域名配置
        if ($zeaburConfig.domains) {
            Write-Host "✅ 域名配置已設定" -ForegroundColor Green
        } else {
            Write-Host "⚠️  未設定域名配置" -ForegroundColor Yellow
            $warnings++
        }
        
    } catch {
        Write-Host "❌ zeabur.json 格式錯誤: $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# 檢查環境變數模板
Write-Host "檢查環境變數模板..." -ForegroundColor Yellow

if (Test-Path ".env.zeabur") {
    $envContent = Get-Content ".env.zeabur" -Raw
    
    $requiredEnvVars = @(
        "DB_HOST",
        "DB_PORT", 
        "DB_NAME",
        "DB_USER",
        "DB_PASSWORD",
        "JWT_SECRET_KEY",
        "JWT_ISSUER",
        "JWT_AUDIENCE",
        "CORS_ALLOWED_ORIGINS"
    )
    
    foreach ($envVar in $requiredEnvVars) {
        if ($envContent -match $envVar) {
            Write-Host "✅ 環境變數: $envVar" -ForegroundColor Green
        } else {
            Write-Host "❌ 缺少環境變數: $envVar" -ForegroundColor Red
            $errors++
        }
    }
}

Write-Host ""

# 檢查資料庫連接（可選）
Write-Host "檢查外部資料庫連接..." -ForegroundColor Yellow

# 嘗試使用 Test-NetConnection 檢查資料庫主機連通性
try {
    $dbHost = "43.167.174.222"
    $dbPort = 31500
    
    $connection = Test-NetConnection -ComputerName $dbHost -Port $dbPort -WarningAction SilentlyContinue
    
    if ($connection.TcpTestSucceeded) {
        Write-Host "✅ 資料庫主機連通性: ${dbHost}:${dbPort}" -ForegroundColor Green
    } else {
        Write-Host "⚠️  資料庫主機連通性測試失敗: ${dbHost}:${dbPort}" -ForegroundColor Yellow
        Write-Host "   這可能是網路限制，部署時請確認資料庫可從 Zeabur 存取" -ForegroundColor Yellow
        $warnings++
    }
} catch {
    Write-Host "⚠️  無法測試資料庫連通性: $($_.Exception.Message)" -ForegroundColor Yellow
    $warnings++
}

Write-Host ""

# 檢查 Git 狀態
Write-Host "檢查 Git 狀態..." -ForegroundColor Yellow

try {
    $gitStatus = git status --porcelain 2>$null
    
    if ($gitStatus) {
        Write-Host "⚠️  有未提交的變更:" -ForegroundColor Yellow
        git status --short
        Write-Host "   建議在部署前提交所有變更" -ForegroundColor Yellow
        $warnings++
    } else {
        Write-Host "✅ 所有變更已提交" -ForegroundColor Green
    }
    
    # 檢查遠端分支
    $remoteBranch = git rev-parse --abbrev-ref --symbolic-full-name @{u} 2>$null
    if ($remoteBranch) {
        Write-Host "✅ 遠端分支: $remoteBranch" -ForegroundColor Green
        
        # 檢查是否需要推送
        $ahead = git rev-list --count HEAD..@{u} 2>$null
        $behind = git rev-list --count @{u}..HEAD 2>$null
        
        if ($behind -gt 0) {
            Write-Host "⚠️  本地分支領先遠端 $behind 個提交，建議推送到遠端" -ForegroundColor Yellow
            $warnings++
        }
    } else {
        Write-Host "⚠️  未設定遠端分支" -ForegroundColor Yellow
        $warnings++
    }
    
} catch {
    Write-Host "⚠️  無法檢查 Git 狀態: $($_.Exception.Message)" -ForegroundColor Yellow
    $warnings++
}

Write-Host ""

# 總結
Write-Host "=== 檢查結果 ===" -ForegroundColor Green
Write-Host "錯誤: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
Write-Host "警告: $warnings" -ForegroundColor $(if ($warnings -gt 0) { "Yellow" } else { "Green" })

Write-Host ""

if ($errors -eq 0) {
    Write-Host "🎉 專案已準備好部署到 Zeabur！" -ForegroundColor Green
    Write-Host ""
    Write-Host "下一步:" -ForegroundColor Yellow
    Write-Host "1. 登入 Zeabur 控制台 (https://dash.zeabur.com)" -ForegroundColor White
    Write-Host "2. 建立新專案 'rosca-system'" -ForegroundColor White
    Write-Host "3. 連接 Git 存儲庫" -ForegroundColor White
    Write-Host "4. 設定環境變數 (參考 .env.zeabur)" -ForegroundColor White
    Write-Host "5. 開始部署服務" -ForegroundColor White
} else {
    Write-Host "❌ 請修復上述錯誤後再進行部署" -ForegroundColor Red
}

Write-Host ""