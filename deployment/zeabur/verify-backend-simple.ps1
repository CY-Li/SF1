# Zeabur 後端服務部署簡單驗證腳本

Write-Host "=== ROSCA 後端服務部署驗證 ===" -ForegroundColor Green
Write-Host ""

$errors = 0
$warnings = 0

# 檢查必要的 Dockerfile
Write-Host "檢查 Dockerfile..." -ForegroundColor Yellow

$dockerfiles = @(
    "backendAPI/DotNetBackEndCleanArchitecture/Dockerfile",
    "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile"
)

foreach ($dockerfile in $dockerfiles) {
    if (Test-Path $dockerfile) {
        Write-Host "✅ $dockerfile" -ForegroundColor Green
    } else {
        Write-Host "❌ $dockerfile (缺失)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# 檢查 zeabur.json
Write-Host "檢查 zeabur.json..." -ForegroundColor Yellow

if (Test-Path "zeabur.json") {
    Write-Host "✅ zeabur.json 存在" -ForegroundColor Green
    
    $content = Get-Content "zeabur.json" -Raw
    if ($content -match "backend-service") {
        Write-Host "✅ backend-service 配置存在" -ForegroundColor Green
    } else {
        Write-Host "❌ 缺少 backend-service 配置" -ForegroundColor Red
        $errors++
    }
    
    if ($content -match "api-gateway") {
        Write-Host "✅ api-gateway 配置存在" -ForegroundColor Green
    } else {
        Write-Host "❌ 缺少 api-gateway 配置" -ForegroundColor Red
        $errors++
    }
} else {
    Write-Host "❌ zeabur.json 不存在" -ForegroundColor Red
    $errors++
}

Write-Host ""

# 檢查環境變數模板
Write-Host "檢查環境變數模板..." -ForegroundColor Yellow

if (Test-Path ".env.zeabur") {
    Write-Host "✅ .env.zeabur 存在" -ForegroundColor Green
    
    $envContent = Get-Content ".env.zeabur" -Raw
    $requiredVars = @("DB_HOST", "DB_PORT", "DB_NAME", "DB_USER", "DB_PASSWORD", "JWT_SECRET_KEY")
    
    foreach ($var in $requiredVars) {
        if ($envContent -match $var) {
            Write-Host "✅ 環境變數: $var" -ForegroundColor Green
        } else {
            Write-Host "❌ 缺少環境變數: $var" -ForegroundColor Red
            $errors++
        }
    }
} else {
    Write-Host "❌ .env.zeabur 不存在" -ForegroundColor Red
    $errors++
}

Write-Host ""

# 檢查應用程式配置
Write-Host "檢查應用程式配置..." -ForegroundColor Yellow

$appSettings = @(
    "backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/appsettings.json",
    "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/appsettings.json"
)

foreach ($file in $appSettings) {
    if (Test-Path $file) {
        Write-Host "✅ $file" -ForegroundColor Green
    } else {
        Write-Host "❌ $file (缺失)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# 總結
Write-Host "=== 驗證結果 ===" -ForegroundColor Green
Write-Host "錯誤: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
Write-Host "警告: $warnings" -ForegroundColor $(if ($warnings -gt 0) { "Yellow" } else { "Green" })

Write-Host ""

if ($errors -eq 0) {
    Write-Host "🎉 後端服務配置正確，可以部署到 Zeabur！" -ForegroundColor Green
} else {
    Write-Host "❌ 請修復上述錯誤後再進行部署" -ForegroundColor Red
}

Write-Host ""