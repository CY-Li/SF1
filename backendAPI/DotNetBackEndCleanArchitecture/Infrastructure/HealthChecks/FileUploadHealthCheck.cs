using Infrastructure.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Infrastructure.HealthChecks;

/// <summary>
/// 檔案上傳健康檢查
/// 檢查檔案上傳相關的目錄和配置是否正常
/// </summary>
public class FileUploadHealthCheck : IHealthCheck
{
    private readonly FileUploadConfiguration _config;

    public FileUploadHealthCheck(FileUploadConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var healthData = new Dictionary<string, object>();
            var issues = new List<string>();

            // 檢查配置有效性
            if (!_config.IsValid(out var configErrors))
            {
                issues.AddRange(configErrors);
            }

            // 檢查目錄存在性和權限
            var directories = new Dictionary<string, string>
            {
                ["uploads"] = _config.UploadPath,
                ["kycImages"] = _config.KycImagesPath,
                ["depositImages"] = _config.DepositImagesPath,
                ["withdrawImages"] = _config.WithdrawImagesPath,
                ["announcementImages"] = _config.AnnouncementImagesPath,
                ["logs"] = _config.LogsPath
            };

            var directoryStatus = new Dictionary<string, object>();

            foreach (var (name, path) in directories)
            {
                var status = await CheckDirectoryAsync(path, cancellationToken);
                directoryStatus[name] = status;

                if (!status.Exists)
                {
                    issues.Add($"目錄不存在: {path}");
                }
                else if (!status.Writable)
                {
                    issues.Add($"目錄不可寫: {path}");
                }
            }

            healthData["directories"] = directoryStatus;

            // 檢查磁碟空間
            var diskSpaceInfo = await CheckDiskSpaceAsync(_config.UploadPath, cancellationToken);
            healthData["diskSpace"] = diskSpaceInfo;

            if (diskSpaceInfo.UsagePercentage > 90)
            {
                issues.Add($"磁碟空間不足: {diskSpaceInfo.UsagePercentage:F1}%");
            }

            // 檢查配置設定
            healthData["configuration"] = new
            {
                maxFileSize = _config.MaxFileSize,
                allowedExtensions = _config.AllowedExtensions,
                enableCompression = _config.EnableCompression,
                enableEncryption = _config.EnableEncryption,
                enableVirusScanning = _config.EnableVirusScanning
            };

            // 計算總體健康狀態
            var status = issues.Count switch
            {
                0 => HealthStatus.Healthy,
                <= 2 => HealthStatus.Degraded,
                _ => HealthStatus.Unhealthy
            };

            var description = issues.Count == 0 
                ? "檔案上傳系統運行正常" 
                : $"發現 {issues.Count} 個問題: {string.Join(", ", issues)}";

            healthData["issues"] = issues;
            healthData["timestamp"] = DateTime.UtcNow;

            return new HealthCheckResult(status, description, data: healthData);
        }
        catch (Exception ex)
        {
            var errorData = new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            };

            return new HealthCheckResult(
                HealthStatus.Unhealthy,
                "檔案上傳健康檢查失敗",
                ex,
                errorData);
        }
    }

    private async Task<DirectoryStatus> CheckDirectoryAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var exists = Directory.Exists(path);
            var writable = false;
            var readable = false;
            long size = 0;
            int fileCount = 0;

            if (exists)
            {
                // 檢查可寫性
                try
                {
                    var testFile = Path.Combine(path, $"health_check_{Guid.NewGuid():N}.tmp");
                    await File.WriteAllTextAsync(testFile, "test", cancellationToken);
                    File.Delete(testFile);
                    writable = true;
                }
                catch
                {
                    writable = false;
                }

                // 檢查可讀性
                try
                {
                    Directory.GetFiles(path);
                    readable = true;
                }
                catch
                {
                    readable = false;
                }

                // 計算目錄大小和檔案數量
                try
                {
                    var directoryInfo = new DirectoryInfo(path);
                    var files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories);
                    
                    await Task.Run(() =>
                    {
                        foreach (var file in files)
                        {
                            size += file.Length;
                            fileCount++;
                        }
                    }, cancellationToken);
                }
                catch
                {
                    // 忽略計算錯誤
                }
            }

            return new DirectoryStatus
            {
                Path = path,
                Exists = exists,
                Writable = writable,
                Readable = readable,
                Size = size,
                FileCount = fileCount
            };
        }
        catch
        {
            return new DirectoryStatus
            {
                Path = path,
                Exists = false,
                Writable = false,
                Readable = false,
                Size = 0,
                FileCount = 0
            };
        }
    }

    private async Task<DiskSpaceInfo> CheckDiskSpaceAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                var driveInfo = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path))!);
                
                var totalSize = driveInfo.TotalSize;
                var availableSpace = driveInfo.AvailableFreeSpace;
                var usedSpace = totalSize - availableSpace;
                var usagePercentage = (double)usedSpace / totalSize * 100;

                return new DiskSpaceInfo
                {
                    TotalSize = totalSize,
                    UsedSpace = usedSpace,
                    AvailableSpace = availableSpace,
                    UsagePercentage = usagePercentage
                };
            }, cancellationToken);
        }
        catch
        {
            return new DiskSpaceInfo
            {
                TotalSize = 0,
                UsedSpace = 0,
                AvailableSpace = 0,
                UsagePercentage = 0
            };
        }
    }
}

/// <summary>
/// 目錄狀態資訊
/// </summary>
public class DirectoryStatus
{
    public string Path { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public bool Writable { get; set; }
    public bool Readable { get; set; }
    public long Size { get; set; }
    public int FileCount { get; set; }
}

/// <summary>
/// 磁碟空間資訊
/// </summary>
public class DiskSpaceInfo
{
    public long TotalSize { get; set; }
    public long UsedSpace { get; set; }
    public long AvailableSpace { get; set; }
    public double UsagePercentage { get; set; }
}

/// <summary>
/// 檔案上傳健康檢查擴展方法
/// </summary>
public static class FileUploadHealthCheckExtensions
{
    /// <summary>
    /// 添加檔案上傳健康檢查
    /// </summary>
    public static IServiceCollection AddFileUploadHealthCheck(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<FileUploadHealthCheck>(
                "file_upload",
                HealthStatus.Degraded,
                new[] { "file", "upload", "storage" });

        return services;
    }
}