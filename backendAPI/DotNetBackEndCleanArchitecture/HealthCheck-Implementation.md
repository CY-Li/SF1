# 健康檢查端點實作建議

為了讓 Docker 健康檢查正常運作，需要在 .NET Core API 中實作 `/health` 端點。

## 實作步驟

### 1. 在 Program.cs 或 Startup.cs 中添加健康檢查服務

```csharp
// 添加健康檢查服務
builder.Services.AddHealthChecks()
    .AddDbContextCheck<YourDbContext>() // 檢查資料庫連接
    .AddCheck("self", () => HealthCheckResult.Healthy());

// 在 Configure 方法中添加健康檢查端點
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 2. 安裝必要的 NuGet 套件

```xml
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="7.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.UI.Client" Version="6.0.5" />
```

### 3. 自定義健康檢查

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<YourDbContext> _contextFactory;

    public DatabaseHealthCheck(IDbContextFactory<YourDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            await dbContext.Database.CanConnectAsync(cancellationToken);
            
            // 檢查關鍵表是否存在
            var memberCount = await dbContext.MemberMaster.CountAsync(cancellationToken);
            
            return HealthCheckResult.Healthy($"Database is healthy. Member count: {memberCount}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}");
        }
    }
}
```

### 4. 註冊自定義健康檢查

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck("self", () => HealthCheckResult.Healthy());
```

## 健康檢查回應格式

健康的回應：
```json
{
    "status": "Healthy",
    "totalDuration": "00:00:00.0123456",
    "entries": {
        "database": {
            "status": "Healthy",
            "duration": "00:00:00.0098765",
            "data": {}
        },
        "self": {
            "status": "Healthy",
            "duration": "00:00:00.0001234",
            "data": {}
        }
    }
}
```

不健康的回應：
```json
{
    "status": "Unhealthy",
    "totalDuration": "00:00:00.0123456",
    "entries": {
        "database": {
            "status": "Unhealthy",
            "duration": "00:00:00.0098765",
            "exception": "Connection failed",
            "data": {}
        }
    }
}
```

## 注意事項

1. 健康檢查端點應該輕量且快速回應
2. 避免在健康檢查中執行複雜的業務邏輯
3. 考慮添加快取以避免頻繁的資料庫查詢
4. 在生產環境中可能需要限制健康檢查端點的訪問