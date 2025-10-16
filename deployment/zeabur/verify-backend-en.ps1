# Zeabur Backend Services Deployment Verification Script

Write-Host "=== ROSCA Backend Services Deployment Verification ===" -ForegroundColor Green
Write-Host ""

$errors = 0
$warnings = 0

# Check required Dockerfiles
Write-Host "Checking Dockerfiles..." -ForegroundColor Yellow

$dockerfiles = @(
    "backendAPI/DotNetBackEndCleanArchitecture/Dockerfile",
    "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile"
)

foreach ($dockerfile in $dockerfiles) {
    if (Test-Path $dockerfile) {
        Write-Host "✅ $dockerfile" -ForegroundColor Green
    } else {
        Write-Host "❌ $dockerfile (missing)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# Check zeabur.json
Write-Host "Checking zeabur.json..." -ForegroundColor Yellow

if (Test-Path "zeabur.json") {
    Write-Host "✅ zeabur.json exists" -ForegroundColor Green
    
    $content = Get-Content "zeabur.json" -Raw
    if ($content -match "backend-service") {
        Write-Host "✅ backend-service configuration exists" -ForegroundColor Green
    } else {
        Write-Host "❌ Missing backend-service configuration" -ForegroundColor Red
        $errors++
    }
    
    if ($content -match "api-gateway") {
        Write-Host "✅ api-gateway configuration exists" -ForegroundColor Green
    } else {
        Write-Host "❌ Missing api-gateway configuration" -ForegroundColor Red
        $errors++
    }
} else {
    Write-Host "❌ zeabur.json does not exist" -ForegroundColor Red
    $errors++
}

Write-Host ""

# Check environment variables template
Write-Host "Checking environment variables template..." -ForegroundColor Yellow

if (Test-Path ".env.zeabur") {
    Write-Host "✅ .env.zeabur exists" -ForegroundColor Green
    
    $envContent = Get-Content ".env.zeabur" -Raw
    $requiredVars = @("DB_HOST", "DB_PORT", "DB_NAME", "DB_USER", "DB_PASSWORD", "JWT_SECRET_KEY")
    
    foreach ($var in $requiredVars) {
        if ($envContent -match $var) {
            Write-Host "✅ Environment variable: $var" -ForegroundColor Green
        } else {
            Write-Host "❌ Missing environment variable: $var" -ForegroundColor Red
            $errors++
        }
    }
} else {
    Write-Host "❌ .env.zeabur does not exist" -ForegroundColor Red
    $errors++
}

Write-Host ""

# Check application configuration files
Write-Host "Checking application configuration files..." -ForegroundColor Yellow

$appSettings = @(
    "backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/appsettings.json",
    "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/appsettings.json"
)

foreach ($file in $appSettings) {
    if (Test-Path $file) {
        Write-Host "✅ $file" -ForegroundColor Green
    } else {
        Write-Host "❌ $file (missing)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# Summary
Write-Host "=== Verification Results ===" -ForegroundColor Green
Write-Host "Errors: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
Write-Host "Warnings: $warnings" -ForegroundColor $(if ($warnings -gt 0) { "Yellow" } else { "Green" })

Write-Host ""

if ($errors -eq 0) {
    Write-Host "🎉 Backend services are ready for Zeabur deployment!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Push all code to Git repository" -ForegroundColor White
    Write-Host "2. Create Zeabur project and connect repository" -ForegroundColor White
    Write-Host "3. Set environment variables in Zeabur console" -ForegroundColor White
    Write-Host "4. Deploy backend services" -ForegroundColor White
    Write-Host "5. Monitor build logs and service status" -ForegroundColor White
} else {
    Write-Host "❌ Please fix the above errors before deployment" -ForegroundColor Red
}

Write-Host ""