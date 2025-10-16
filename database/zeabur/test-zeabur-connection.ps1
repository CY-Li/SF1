# ROSCA System - Zeabur Database Connection Test Script
# Test connection to external MariaDB database (43.167.174.222:31500)

param(
    [string]$DbHost = "43.167.174.222",
    [int]$DbPort = 31500,
    [string]$Database = "zeabur",
    [string]$User = "root",
    [string]$Password = "dp17Itl608ZaMBXbWH5VAo49xJr3Ds2G"
)

# Color definitions
$Red = "Red"
$Green = "Green"
$Yellow = "Yellow"
$Blue = "Cyan"

Write-Host "=== ROSCA System Zeabur Database Connection Test ===" -ForegroundColor $Blue
Write-Host "Test Target: $DbHost`:$DbPort/$Database" -ForegroundColor $Blue
Write-Host ""

# Test network connection
function Test-NetworkConnection {
    param($DbHost, $DbPort)
    
    Write-Host "Testing network connection to $DbHost`:$DbPort..." -ForegroundColor $Yellow
    
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.ConnectAsync($DbHost, $DbPort).Wait(5000)
        
        if ($tcpClient.Connected) {
            Write-Host "✓ Network connection successful" -ForegroundColor $Green
            $tcpClient.Close()
            return $true
        }
        else {
            Write-Host "✗ Network connection failed" -ForegroundColor $Red
            return $false
        }
    }
    catch {
        Write-Host "✗ Network connection failed: $($_.Exception.Message)" -ForegroundColor $Red
        return $false
    }
}

# Generate .NET Core connection string
function Generate-DotNetConnectionString {
    param($DbHost, $DbPort, $Database, $User, $Password)
    
    Write-Host "Generating .NET Core connection string..." -ForegroundColor $Yellow
    
    $connectionString = "Server=$DbHost;Port=$DbPort;User Id=$User;Password=$Password;Database=$Database;CharSet=utf8mb4;AllowUserVariables=True;UseAffectedRows=False;ConnectionTimeout=30;CommandTimeout=60;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionLifeTime=300;"
    $maskedConnectionString = $connectionString -replace "Password=[^;]*", "Password=***"
    
    Write-Host "✓ .NET Core connection string generated" -ForegroundColor $Green
    Write-Host "Connection string (masked): $maskedConnectionString" -ForegroundColor $Blue
    
    # Save to file
    $connectionString | Out-File -FilePath "database/zeabur/connection-string.txt" -Encoding UTF8
    Write-Host "✓ Connection string saved to database/zeabur/connection-string.txt" -ForegroundColor $Green
    
    return $connectionString
}

# Main test flow
function Main {
    $allTestsPassed = $true
    
    # Test network connection
    if (-not (Test-NetworkConnection -DbHost $DbHost -DbPort $DbPort)) { 
        $allTestsPassed = $false 
    }
    
    # Generate connection string
    $connectionString = Generate-DotNetConnectionString -DbHost $DbHost -DbPort $DbPort -Database $Database -User $User -Password $Password
    
    Write-Host ""
    if ($allTestsPassed) {
        Write-Host "=== Basic Network Test Passed ===" -ForegroundColor $Green
        Write-Host "✓ External MariaDB network connection is available" -ForegroundColor $Green
        Write-Host ""
        Write-Host "Connection Details:" -ForegroundColor $Blue
        Write-Host "  Host: $DbHost" -ForegroundColor $Blue
        Write-Host "  Port: $DbPort" -ForegroundColor $Blue
        Write-Host "  Database: $Database" -ForegroundColor $Blue
        Write-Host "  User: $User" -ForegroundColor $Blue
        Write-Host ""
        Write-Host "Next Steps:" -ForegroundColor $Yellow
        Write-Host "  1. Use the generated connection string in your .NET applications" -ForegroundColor $Yellow
        Write-Host "  2. Test database schema and data using MySQL client if available" -ForegroundColor $Yellow
        Write-Host "  3. Deploy to Zeabur with the verified connection parameters" -ForegroundColor $Yellow
        return 0
    }
    else {
        Write-Host "=== Network Test Failed ===" -ForegroundColor $Red
        Write-Host "✗ Please check network connectivity and database server status" -ForegroundColor $Red
        return 1
    }
}

# Execute main function
exit (Main)