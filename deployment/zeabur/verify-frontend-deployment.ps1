# Zeabur Frontend Services Deployment Verification Script

Write-Host "=== ROSCA Frontend Services Deployment Verification ===" -ForegroundColor Green
Write-Host ""

$errors = 0
$warnings = 0

# Check required Dockerfiles
Write-Host "Checking Dockerfiles..." -ForegroundColor Yellow

$dockerfiles = @{
    "Frontend (Vue.js)" = "frontend/Dockerfile"
    "Admin (Angular)" = "backend/FontEnd/Dockerfile"
}

foreach ($service in $dockerfiles.Keys) {
    $dockerfile = $dockerfiles[$service]
    if (Test-Path $dockerfile) {
        Write-Host "✅ $service Dockerfile: $dockerfile" -ForegroundColor Green
        
        # Check Dockerfile content
        $content = Get-Content $dockerfile -Raw
        
        # Check for nginx base image
        if ($content -match "FROM nginx:alpine" -or $content -match "nginx:alpine") {
            Write-Host "  ✅ Using nginx:alpine base image" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Not using nginx:alpine base image" -ForegroundColor Yellow
            $warnings++
        }
        
        # Check for port exposure
        if ($content -match "EXPOSE 80") {
            Write-Host "  ✅ Port 80 exposed" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Port 80 not exposed" -ForegroundColor Red
            $errors++
        }
        
        # Check for health check
        if ($content -match "HEALTHCHECK") {
            Write-Host "  ✅ Health check configured" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  No health check configured" -ForegroundColor Yellow
            $warnings++
        }
        
    } else {
        Write-Host "❌ $service Dockerfile missing: $dockerfile" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# Check nginx configurations
Write-Host "Checking nginx configurations..." -ForegroundColor Yellow

$nginxConfigs = @{
    "Frontend nginx.conf" = "frontend/nginx.conf"
    "Admin nginx.conf" = "backend/FontEnd/admin-nginx.conf"
}

foreach ($service in $nginxConfigs.Keys) {
    $configFile = $nginxConfigs[$service]
    if (Test-Path $configFile) {
        Write-Host "✅ ${service}: $configFile" -ForegroundColor Green
        
        $content = Get-Content $configFile -Raw
        
        # Check for API proxy configuration
        if ($content -match "location /api/") {
            Write-Host "  ✅ API proxy configuration found" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Missing API proxy configuration" -ForegroundColor Red
            $errors++
        }
        
        # Check for SPA routing support
        if ($content -match "try_files.*index\.html") {
            Write-Host "  ✅ SPA routing support configured" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Missing SPA routing support" -ForegroundColor Red
            $errors++
        }
        
        # Check for gzip compression
        if ($content -match "gzip on") {
            Write-Host "  ✅ Gzip compression enabled" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Gzip compression not configured" -ForegroundColor Yellow
            $warnings++
        }
        
        # Check for CORS handling
        if ($content -match "Access-Control-Allow-Origin") {
            Write-Host "  ✅ CORS handling configured" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  CORS handling not found" -ForegroundColor Yellow
            $warnings++
        }
        
    } else {
        Write-Host "❌ ${service} missing: $configFile" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# Check zeabur.json frontend services configuration
Write-Host "Checking zeabur.json frontend services configuration..." -ForegroundColor Yellow

if (Test-Path "zeabur.json") {
    try {
        $zeaburConfig = Get-Content "zeabur.json" | ConvertFrom-Json
        
        # Check frontend services configuration
        $frontendServices = @("frontend", "admin")
        
        foreach ($service in $frontendServices) {
            if ($zeaburConfig.services.$service) {
                Write-Host "✅ $service service configuration exists" -ForegroundColor Green
                
                $serviceConfig = $zeaburConfig.services.$service
                
                # Check Dockerfile path
                if ($serviceConfig.build.dockerfile) {
                    Write-Host "  ✅ Dockerfile path: $($serviceConfig.build.dockerfile)" -ForegroundColor Green
                } else {
                    Write-Host "  ❌ Missing Dockerfile path" -ForegroundColor Red
                    $errors++
                }
                
                # Check resource configuration
                if ($serviceConfig.resources) {
                    Write-Host "  ✅ Resource configuration: CPU=$($serviceConfig.resources.cpu), Memory=$($serviceConfig.resources.memory)" -ForegroundColor Green
                } else {
                    Write-Host "  ⚠️  No resource limits configured" -ForegroundColor Yellow
                    $warnings++
                }
                
            } else {
                Write-Host "❌ Missing $service service configuration" -ForegroundColor Red
                $errors++
            }
        }
        
        # Check domain configuration
        if ($zeaburConfig.domains) {
            Write-Host "✅ Domain configuration exists" -ForegroundColor Green
            foreach ($domain in $zeaburConfig.domains) {
                Write-Host "  ✅ Domain: $($domain.name) -> $($domain.service)" -ForegroundColor Green
            }
        } else {
            Write-Host "⚠️  No domain configuration found" -ForegroundColor Yellow
            $warnings++
        }
        
    } catch {
        Write-Host "❌ zeabur.json format error: $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
} else {
    Write-Host "❌ zeabur.json does not exist" -ForegroundColor Red
    $errors++
}

Write-Host ""

# Check static files for frontend
Write-Host "Checking frontend static files..." -ForegroundColor Yellow

$frontendFiles = @(
    "frontend/index.html",
    "frontend/css",
    "frontend/js",
    "frontend/images"
)

foreach ($item in $frontendFiles) {
    if (Test-Path $item) {
        Write-Host "✅ Frontend asset: $item" -ForegroundColor Green
    } else {
        Write-Host "❌ Missing frontend asset: $item" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""

# Check Angular build configuration
Write-Host "Checking Angular build configuration..." -ForegroundColor Yellow

$angularFiles = @(
    "backend/FontEnd/FontEnd/package.json",
    "backend/FontEnd/FontEnd/angular.json",
    "backend/FontEnd/FontEnd/src"
)

foreach ($item in $angularFiles) {
    if (Test-Path $item) {
        Write-Host "✅ Angular asset: $item" -ForegroundColor Green
    } else {
        Write-Host "❌ Missing Angular asset: $item" -ForegroundColor Red
        $errors++
    }
}

# Check Angular package.json for build script
if (Test-Path "backend/FontEnd/FontEnd/package.json") {
    try {
        $packageJson = Get-Content "backend/FontEnd/FontEnd/package.json" | ConvertFrom-Json
        if ($packageJson.scripts.build) {
            Write-Host "✅ Angular build script configured" -ForegroundColor Green
        } else {
            Write-Host "❌ Missing Angular build script" -ForegroundColor Red
            $errors++
        }
    } catch {
        Write-Host "⚠️  Could not parse Angular package.json" -ForegroundColor Yellow
        $warnings++
    }
}

Write-Host ""

# Check Docker entrypoint script for admin
Write-Host "Checking Docker entrypoint script..." -ForegroundColor Yellow

if (Test-Path "backend/FontEnd/docker-entrypoint.sh") {
    Write-Host "✅ Docker entrypoint script exists" -ForegroundColor Green
    
    # Check if script is executable (on Unix systems)
    # Note: This check might not work on Windows
    try {
        $content = Get-Content "backend/FontEnd/docker-entrypoint.sh" -Raw
        if ($content -match "#!/") {
            Write-Host "  ✅ Script has shebang" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Script missing shebang" -ForegroundColor Yellow
            $warnings++
        }
    } catch {
        Write-Host "  ⚠️  Could not check script content" -ForegroundColor Yellow
        $warnings++
    }
} else {
    Write-Host "❌ Docker entrypoint script missing: backend/FontEnd/docker-entrypoint.sh" -ForegroundColor Red
    $errors++
}

Write-Host ""

# Summary
Write-Host "=== Verification Results ===" -ForegroundColor Green
Write-Host "Errors: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
Write-Host "Warnings: $warnings" -ForegroundColor $(if ($warnings -gt 0) { "Yellow" } else { "Green" })

Write-Host ""

if ($errors -eq 0) {
    Write-Host "🎉 Frontend services are ready for Zeabur deployment!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Deployment steps:" -ForegroundColor Yellow
    Write-Host "1. Ensure all code is pushed to Git repository" -ForegroundColor White
    Write-Host "2. Deploy frontend and admin services in Zeabur console" -ForegroundColor White
    Write-Host "3. Monitor build logs for both services" -ForegroundColor White
    Write-Host "4. Configure custom domains (optional)" -ForegroundColor White
    Write-Host "5. Update CORS settings in backend with frontend URLs" -ForegroundColor White
    Write-Host "6. Test frontend functionality and API connectivity" -ForegroundColor White
    
    if ($warnings -gt 0) {
        Write-Host ""
        Write-Host "⚠️  Please review the warnings above for optimal performance" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ Please fix the above errors before deployment" -ForegroundColor Red
}

Write-Host ""

# Additional recommendations
Write-Host "Additional recommendations:" -ForegroundColor Cyan
Write-Host "• Test API proxy connectivity after deployment" -ForegroundColor White
Write-Host "• Verify SPA routing works for all routes" -ForegroundColor White
Write-Host "• Check browser console for any errors" -ForegroundColor White
Write-Host "• Monitor nginx access and error logs" -ForegroundColor White
Write-Host "• Update CORS_ALLOWED_ORIGINS environment variable" -ForegroundColor White