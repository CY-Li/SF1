using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.Middleware;

/// <summary>
/// 檔案存取中介軟體
/// 處理檔案存取的安全性、快取和日誌記錄
/// </summary>
public class FileAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FileAccessMiddleware> _logger;

    public FileAccessMiddleware(RequestDelegate next, ILogger<FileAccessMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, IFileCacheService fileCacheService)
    {
        // 只處理檔案存取相關的請求
        if (!IsFileAccessRequest(context.Request.Path))
        {
            await _next(context);
            return;
        }

        try
        {
            // 記錄請求開始
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N")[..8];
            
            _logger.LogInformation(
                "檔案存取請求開始: RequestId={RequestId}, Path={Path}, Method={Method}, User={User}, IP={IP}",
                requestId, context.Request.Path, context.Request.Method,
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous",
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // 設定安全標頭
            SetSecurityHeaders(context.Response);

            // 處理 CORS 預檢請求
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                return;
            }

            // 檢查請求頻率限制
            if (!await CheckRateLimitAsync(context))
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("請求過於頻繁，請稍後再試");
                return;
            }

            // 執行下一個中介軟體
            await _next(context);

            // 記錄請求完成
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "檔案存取請求完成: RequestId={RequestId}, StatusCode={StatusCode}, Duration={Duration}ms",
                requestId, context.Response.StatusCode, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案存取中介軟體處理失敗: Path={Path}", context.Request.Path);
            
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("內部伺服器錯誤");
            }
        }
    }

    /// <summary>
    /// 檢查是否為檔案存取請求
    /// </summary>
    private static bool IsFileAccessRequest(PathString path)
    {
        return path.StartsWithSegments("/api/fileupload") || 
               path.StartsWithSegments("/api/fileaccess");
    }

    /// <summary>
    /// 設定安全標頭
    /// </summary>
    private static void SetSecurityHeaders(HttpResponse response)
    {
        // 防止 MIME 類型嗅探
        response.Headers.Add("X-Content-Type-Options", "nosniff");
        
        // 防止點擊劫持
        response.Headers.Add("X-Frame-Options", "DENY");
        
        // XSS 保護
        response.Headers.Add("X-XSS-Protection", "1; mode=block");
        
        // 內容安全政策
        response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; img-src 'self' data:; script-src 'none'; style-src 'self' 'unsafe-inline';");
        
        // 引用者政策
        response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // 權限政策
        response.Headers.Add("Permissions-Policy", 
            "geolocation=(), microphone=(), camera=(), payment=(), usb=()");
    }

    /// <summary>
    /// 檢查請求頻率限制
    /// </summary>
    private async Task<bool> CheckRateLimitAsync(HttpContext context)
    {
        try
        {
            // 這裡可以實作更複雜的頻率限制邏輯
            // 例如使用 Redis 或記憶體快取來追蹤請求頻率
            
            var clientId = GetClientIdentifier(context);
            var cacheKey = $"rate_limit_{clientId}";
            
            // 簡單的記憶體快取實作 (生產環境建議使用 Redis)
            // 這裡暫時返回 true，表示允許請求
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查請求頻率限制失敗");
            return true; // 發生錯誤時允許請求通過
        }
    }

    /// <summary>
    /// 取得客戶端識別符
    /// </summary>
    private static string GetClientIdentifier(HttpContext context)
    {
        // 優先使用用戶 ID
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
            return $"user_{userId}";

        // 其次使用 IP 地址
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(ipAddress))
            return $"ip_{ipAddress}";

        // 最後使用 User-Agent 的雜湊值
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userAgent));
            return $"ua_{Convert.ToHexString(hashBytes)[..16]}";
        }

        return "anonymous";
    }
}

/// <summary>
/// 檔案存取中介軟體擴展方法
/// </summary>
public static class FileAccessMiddlewareExtensions
{
    /// <summary>
    /// 使用檔案存取中介軟體
    /// </summary>
    public static IApplicationBuilder UseFileAccessMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FileAccessMiddleware>();
    }
}