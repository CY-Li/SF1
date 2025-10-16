using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Extensions;

/// <summary>
/// 檔案上傳服務擴展方法
/// </summary>
public static class FileUploadServiceExtensions
{
    /// <summary>
    /// 註冊檔案上傳服務
    /// </summary>
    public static IServiceCollection AddFileUploadServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 註冊檔案上傳配置
        services.AddFileUploadConfiguration(configuration);

        // 註冊記憶體快取
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; // 100MB 記憶體快取限制
        });

        // 註冊檔案快取服務
        services.AddScoped<IFileCacheService, FileCacheService>();

        // 註冊檔案上傳服務
        services.AddScoped<IFileUploadService, FileUploadService>();

        // 註冊背景服務進行定期清理
        services.AddHostedService<FileCleanupBackgroundService>();
        services.AddHostedService<FileCacheCleanupBackgroundService>();

        return services;
    }

    /// <summary>
    /// 配置檔案上傳中介軟體
    /// </summary>
    public static IServiceCollection ConfigureFileUploadMiddleware(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 配置檔案上傳大小限制
        services.Configure<IISServerOptions>(options =>
        {
            var maxFileSize = configuration.GetValue<long>("FileUpload:MaxFileSize", 10 * 1024 * 1024);
            options.MaxRequestBodySize = maxFileSize;
        });

        services.Configure<KestrelServerOptions>(options =>
        {
            var maxFileSize = configuration.GetValue<long>("FileUpload:MaxFileSize", 10 * 1024 * 1024);
            options.Limits.MaxRequestBodySize = maxFileSize;
        });

        // 配置表單選項
        services.Configure<FormOptions>(options =>
        {
            var maxFileSize = configuration.GetValue<long>("FileUpload:MaxFileSize", 10 * 1024 * 1024);
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = maxFileSize;
            options.MultipartHeadersLengthLimit = int.MaxValue;
        });

        return services;
    }
}

/// <summary>
/// 檔案清理背景服務
/// 定期清理過期檔案
/// </summary>
public class FileCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileCleanupBackgroundService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // 每24小時執行一次

    public FileCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<FileCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("檔案清理背景服務已啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不記錄錯誤
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檔案清理背景服務執行失敗");
                
                // 發生錯誤時等待較短時間後重試
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("檔案清理背景服務已停止");
    }

    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var fileUploadService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();

        _logger.LogInformation("開始執行檔案清理");
        
        var startTime = DateTime.UtcNow;
        await fileUploadService.CleanupExpiredFilesAsync(cancellationToken);
        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation("檔案清理完成，耗時: {Duration}", duration);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在停止檔案清理背景服務");
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// 檔案快取清理背景服務
/// 定期清理過期的檔案快取
/// </summary>
public class FileCacheCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileCacheCleanupBackgroundService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // 每6小時執行一次

    public FileCacheCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<FileCacheCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("檔案快取清理背景服務已啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCacheCleanupAsync(stoppingToken);
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不記錄錯誤
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檔案快取清理背景服務執行失敗");
                
                // 發生錯誤時等待較短時間後重試
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("檔案快取清理背景服務已停止");
    }

    private async Task PerformCacheCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var fileCacheService = scope.ServiceProvider.GetRequiredService<IFileCacheService>();

        _logger.LogInformation("開始執行檔案快取清理");
        
        var startTime = DateTime.UtcNow;
        await fileCacheService.ClearExpiredCacheAsync(cancellationToken);
        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation("檔案快取清理完成，耗時: {Duration}", duration);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在停止檔案快取清理背景服務");
        await base.StopAsync(cancellationToken);
    }
}