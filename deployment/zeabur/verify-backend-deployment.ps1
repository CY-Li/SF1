# Zeabur 後端服務部署驗證腳本
# 此腳本檢查後端服務是否成功部署並正常運行

param(
    [string]$ApiGatewayUrl = "",
    [string]$BackendServiceUrl = "",
    [switch]$SkipExternalTests
)

Write-Host "=== ROSCA 後端服務部署驗證 ===" -ForegroundColor Green
Write-Host ""

$errors = 0
$warnings = 0

# 檢查 Dockerfile 配置
Write-Host "檢查 Dockerfile 配置..." -ForegroundColor Yellow

$dockerfiles = @{
    "API Gateway" = "backendAPI/DotNetBackEndCleanArchitecture/Dockerfile"
    "Backend Service" = "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/Dockerfile"
}

foreach ($service in $dockerfiles.Keys) {
    $dockerfile = $dockerfiles[$service]
    if (Test-Path $dockerfile) {
        Write-Host "✅ $service Dockerfile: $dockerfile" -ForegroundColor Green
        
        # 檢查 Dockerfile 內容
        $content = Get-Content $dockerfile -Raw
        
        # 檢查關鍵配置
        if ($content -match "EXPOSE \d+") {
            Write-Host "  ✅ 端口配置正確" -ForegroundColor Green
        } else {
            Write-Host "  ❌ 缺少端口配置" -ForegroundColor Red
            $errors++
        }
        
        if ($content -match "HEALTHCHECK") {
            Write-Host "  ✅ 健康檢查已配置" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  未配置健康檢查" -ForegroundColor Yellow
            $warnings++
        }
        
        if ($content -match "USER appuser") {
            Write-Host "  ✅ 非 root 用戶配置" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  未配置非 root 用戶" -ForegroundColor Yellow
            $warnings++
        }
        
    } else {
        Write-Host "❌ $service Dockerfile 不存在: $dockerfile" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# 檢查 zeabur.json 後端服務配置
Write-Host "檢查 zeabur.json 後端服務配置..." -ForegroundColor Yellow

if (Test-Path "zeabur.json") {
    try {
        $zeaburConfig = Get-Content "zeabur.json" | ConvertFrom-Json
        
        # 檢查後端服務配置
        $backendServices = @("backend-service", "api-gateway")
        
        foreach ($service in $backendServices) {
            if ($zeaburConfig.services.$service) {
                Write-Host "✅ $service 服務配置存在" -ForegroundColor Green
                
                $serviceConfig = $zeaburConfig.services.$service
                
                # 檢查 Dockerfile 路徑
                if ($serviceConfig.build.dockerfile) {
                    Write-Host "  ✅ Dockerfile 路徑: $($serviceConfig.build.dockerfile)" -ForegroundColor Green
                } else {
                    Write-Host "  ❌ 缺少 Dockerfile 路徑" -ForegroundColor Red
                    $errors++
                }
                
                # 檢查環境變數
                if ($serviceConfig.env) {
                    $requiredEnvVars = @("ASPNETCORE_ENVIRONMENT", "ASPNETCORE_URLS", "ConnectionStrings__BackEndDatabase")
                    foreach ($envVar in $requiredEnvVars) {
                        if ($serviceConfig.env.$envVar) {
                            Write-Host "  ✅ 環境變數: $envVar" -ForegroundColor Green
                        } else {
                            Write-Host "  ❌ 缺少環境變數: $envVar" -ForegroundColor Red
                            $errors++
                        }
                    }
                } else {
                    Write-Host "  ❌ 缺少環境變數配置" -ForegroundColor Red
                    $errors++
                }
                
                # 檢查資源配置
                if ($serviceConfig.resources) {
                    Write-Host "  ✅ 資源配置: CPU=$($serviceConfig.resources.cpu), Memory=$($serviceConfig.resources.memory)" -ForegroundColor Green
                } else {
                    Write-Host "  ⚠️  未配置資源限制" -ForegroundColor Yellow
                    $warnings++
                }
                
            } else {
                Write-Host "❌ 缺少 $service 服務配置" -ForegroundColor Red
                $errors++
            }
        }
        
        # 檢查 API Gateway 存儲卷配置
        if ($zeaburConfig.services."api-gateway".volumes) {
            Write-Host "✅ API Gateway 存儲卷已配置" -ForegroundColor Green
            $volumes = $zeaburConfig.services."api-gateway".volumes
            foreach ($volume in $volumes) {
                Write-Host "  ✅ 存儲卷: $($volume.name) -> $($volume.dir) ($($volume.size))" -ForegroundColor Green
            }
        } else {
            Write-Host "⚠️  API Gateway 未配置存儲卷" -ForegroundColor Yellow
            $warnings++
        }
        
    } catch {
        Write-Host "❌ zeabur.json 格式錯誤: $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# 檢查應用程式配置檔案
Write-Host "檢查應用程式配置檔案..." -ForegroundColor Yellow

$appSettingsFiles = @(
    "backendAPI/DotNetBackEndCleanArchitecture/DotNetBackEndApi/appsettings.json",
    "backendAPI/DotNetBackEndCleanArchitecture/Presentation/DotNetBackEndService/appsettings.json"
)

foreach ($file in $appSettingsFiles) {
    if (Test-Path $file) {
        Write-Host "✅ 配置檔案: $file" -ForegroundColor Green
        
        try {
            $config = Get-Content $file | ConvertFrom-Json
            
            # 檢查連接字串配置
            if ($config.ConnectionStrings) {
                Write-Host "  ✅ 連接字串配置存在" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️  連接字串配置可能在環境變數中" -ForegroundColor Yellow
            }
            
            # 檢查日誌配置
            if ($config.Serilog -or $config.Logging) {
                Write-Host "  ✅ 日誌配置存在" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️  未找到日誌配置" -ForegroundColor Yellow
                $warnings++
            }
            
        } catch {
            Write-Host "  ❌ 配置檔案格式錯誤: $($_.Exception.Message)" -ForegroundColor Red
            $errors++
        }
        
    } else {
        Write-Host "❌ 配置檔案不存在: $file" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# 外部服務測試 (如果提供了 URL)
if (-not $SkipExternalTests -and ($ApiGatewayUrl -or $BackendServiceUrl)) {
    Write-Host "測試外部服務連接..." -ForegroundColor Yellow
    
    if ($ApiGatewayUrl) {
        try {
            Write-Host "測試 API Gateway: $ApiGatewayUrl" -ForegroundColor White
            $response = Invoke-WebRequest -Uri "$ApiGatewayUrl/api/HomePicture/GetAnnImages" -Method GET -TimeoutSec 10 -UseBasicParsing
            
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ API Gateway 健康檢查通過" -ForegroundColor Green
            } else {
                Write-Host "⚠️  API Gateway 回應狀態碼: $($response.StatusCode)" -ForegroundColor Yellow
                $warnings++
            }
        } catch {
            Write-Host "❌ API Gateway 連接失敗: $($_.Exception.Message)" -ForegroundColor Red
            $errors++
        }
    }
    
    if ($BackendServiceUrl) {
        try {
            Write-Host "測試 Backend Service: $BackendServiceUrl" -ForegroundColor White
            $response = Invoke-WebRequest -Uri "$BackendServiceUrl/health" -Method GET -TimeoutSec 10 -UseBasicParsing
            
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ Backend Service 健康檢查通過" -ForegroundColor Green
            } else {
                Write-Host "⚠️  Backend Service 回應狀態碼: $($response.StatusCode)" -ForegroundColor Yellow
                $warnings++
            }
        } catch {
            Write-Host "❌ Backend Service 連接失敗: $($_.Exception.Message)" -ForegroundColor Red
            $errors++
        }
    }
}

Write-Host ""

# 檢查資料庫連接配置
Write-Host "檢查資料庫連接配置..." -ForegroundColor Yellow

if (Test-Path ".env.zeabur") {
    $envContent = Get-Content ".env.zeabur" -Raw
    
    $dbVars = @("DB_HOST", "DB_PORT", "DB_NAME", "DB_USER", "DB_PASSWORD")
    $allDbVarsPresent = $true
    
    foreach ($var in $dbVars) {
        if ($envContent -match "$var=") {
            Write-Host "✅ 資料庫變數: $var" -ForegroundColor Green
        } else {
            Write-Host "❌ 缺少資料庫變數: $var" -ForegroundColor Red
            $errors++
            $allDbVarsPresent = $false
        }
    }
    
    if ($allDbVarsPresent) {
        # 提取資料庫連接資訊進行測試
        $dbHost = ($envContent | Select-String "DB_HOST=(.+)" | ForEach-Object { $_.Matches[0].Groups[1].Value })
        $dbPort = ($envContent | Select-String "DB_PORT=(.+)" | ForEach-Object { $_.Matches[0].Groups[1].Value })
        
        if ($dbHost -and $dbPort) {
            try {
                Write-Host "測試資料庫連通性: ${dbHost}:${dbPort}" -ForegroundColor White
                $connection = Test-NetConnection -ComputerName $dbHost -Port $dbPort -WarningAction SilentlyContinue
                
                if ($connection.TcpTestSucceeded) {
                    Write-Host "✅ 資料庫主機連通性測試通過" -ForegroundColor Green
                } else {
                    Write-Host "⚠️  資料庫主機連通性測試失敗" -ForegroundColor Yellow
                    Write-Host "   這可能是網路限制，請確認 Zeabur 可以存取外部資料庫" -ForegroundColor Yellow
                    $warnings++
                }
            } catch {
                Write-Host "⚠️  無法測試資料庫連通性: $($_.Exception.Message)" -ForegroundColor Yellow
                $warnings++
            }
        }
    }
}

Write-Host ""

# 總結
Write-Host "=== 驗證結果 ===" -ForegroundColor Green
Write-Host "錯誤: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
Write-Host "警告: $warnings" -ForegroundColor $(if ($warnings -gt 0) { "Yellow" } else { "Green" })

Write-Host ""

if ($errors -eq 0) {
    Write-Host "🎉 後端服務已準備好部署到 Zeabur！" -ForegroundColor Green
    Write-Host ""
    Write-Host "部署步驟:" -ForegroundColor Yellow
    Write-Host "1. 確保所有程式碼已推送到 Git 存儲庫" -ForegroundColor White
    Write-Host "2. 在 Zeabur 控制台中檢查服務狀態" -ForegroundColor White
    Write-Host "3. 監控建置日誌" -ForegroundColor White
    Write-Host "4. 驗證服務健康檢查" -ForegroundColor White
    Write-Host "5. 測試服務間通信" -ForegroundColor White
    
    if ($warnings -gt 0) {
        Write-Host ""
        Write-Host "⚠️  請注意上述警告，雖然不會阻止部署，但建議修復以獲得最佳效能" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ 請修復上述錯誤後再進行部署" -ForegroundColor Red
}

Write-Host ""

# 使用範例
Write-Host "使用範例:" -ForegroundColor Cyan
Write-Host "  基本檢查: .\verify-backend-deployment.ps1" -ForegroundColor White
Write-Host "  包含外部測試: .\verify-backend-deployment.ps1 -ApiGatewayUrl 'https://api-gateway.zeabur.app' -BackendServiceUrl 'https://backend-service.zeabur.app'" -ForegroundColor White
Write-Host "  跳過外部測試: .\verify-backend-deployment.ps1 -SkipExternalTests" -ForegroundColor White