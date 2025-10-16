using Infrastructure.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

/// <summary>
/// 檔案快取服務介面
/// </summary>
public interface IFileCacheService
{
    Task<Stream?> GetCachedFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task SetCachedFileAsync(string filePath, Stream fileStream, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<Stream?> GetCachedThumbnailAsync(string filePath, int width, int height, int quality, CancellationToken cancellationToken = default);
    Task SetCachedThumbnailAsync(string filePath, int width, int height, int quality, Stream thumbnailStream, CancellationToken cancellationToken = default);
    Task InvalidateCacheAsync(string filePath, CancellationToken cancellationToken = default);
    Task ClearExpiredCacheAsync(CancellationToken cancellationToken = default);
    Task<FileCacheStats> GetCacheStatsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 檔案快取服務實作
/// 提供檔案和縮圖的記憶體快取功能
/// </summary>
public class FileCacheService : IFileCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly FileUploadConfiguration _config;
    private readonly ILogger<FileCacheService> _logger;
    private readonly string _diskCachePath;
    private readonly SemaphoreSlim _cacheSemaphore;

    // 快取統計
    private long _cacheHits = 0;
    private long _cacheMisses = 0;
    private long _cacheSize = 0;

    public FileCacheService(
        IMemoryCache memoryCache,
        FileUploadConfiguration config,
        ILogger<FileCacheService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _diskCachePath = Path.Combine(_config.LogsPath, "cache");
        _cacheSemaphore = new SemaphoreSlim(1, 1);

        // 確保快取目錄存在
        Directory.CreateDirectory(_diskCachePath);
    }

    /// <summary>
    /// 取得快取的檔案
    /// </summary>
    public async Task<Stream?> GetCachedFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GenerateFileCacheKey(filePath);
            
            // 先檢查記憶體快取
            if (_memoryCache.TryGetValue(cacheKey, out byte[] cachedData))
            {
                Interlocked.Increment(ref _cacheHits);
                _logger.LogDebug("記憶體快取命中: {FilePath}", filePath);
                return new MemoryStream(cachedData);
            }

            // 檢查磁碟快取
            var diskCacheFile = GetDiskCacheFilePath(cacheKey);
            if (File.Exists(diskCacheFile))
            {
                var fileInfo = new FileInfo(diskCacheFile);
                var originalFileInfo = new FileInfo(filePath);

                // 檢查快取是否過期
                if (fileInfo.LastWriteTimeUtc >= originalFileInfo.LastWriteTimeUtc)
                {
                    var diskData = await File.ReadAllBytesAsync(diskCacheFile, cancellationToken);
                    
                    // 將磁碟快取載入記憶體快取
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        Size = diskData.Length,
                        SlidingExpiration = TimeSpan.FromMinutes(30),
                        Priority = CacheItemPriority.Normal
                    };
                    _memoryCache.Set(cacheKey, diskData, cacheOptions);

                    Interlocked.Increment(ref _cacheHits);
                    _logger.LogDebug("磁碟快取命中: {FilePath}", filePath);
                    return new MemoryStream(diskData);
                }
                else
                {
                    // 快取過期，刪除舊快取
                    File.Delete(diskCacheFile);
                }
            }

            Interlocked.Increment(ref _cacheMisses);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得快取檔案失敗: {FilePath}", filePath);
            Interlocked.Increment(ref _cacheMisses);
            return null;
        }
    }

    /// <summary>
    /// 設定檔案快取
    /// </summary>
    public async Task SetCachedFileAsync(
        string filePath, 
        Stream fileStream, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fileStream == null || !fileStream.CanRead)
                return;

            var cacheKey = GenerateFileCacheKey(filePath);
            var data = new byte[fileStream.Length];
            
            fileStream.Position = 0;
            await fileStream.ReadAsync(data, 0, data.Length, cancellationToken);

            // 檢查檔案大小限制 (不快取超過 10MB 的檔案)
            if (data.Length > 10 * 1024 * 1024)
            {
                _logger.LogDebug("檔案過大，跳過快取: {FilePath} ({Size} bytes)", filePath, data.Length);
                return;
            }

            await _cacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                // 設定記憶體快取
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    Size = data.Length,
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1),
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    Priority = CacheItemPriority.Normal,
                    PostEvictionCallbacks = { new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = OnCacheEvicted
                    }}
                };

                _memoryCache.Set(cacheKey, data, cacheOptions);
                Interlocked.Add(ref _cacheSize, data.Length);

                // 設定磁碟快取
                var diskCacheFile = GetDiskCacheFilePath(cacheKey);
                await File.WriteAllBytesAsync(diskCacheFile, data, cancellationToken);

                _logger.LogDebug("檔案已快取: {FilePath} ({Size} bytes)", filePath, data.Length);
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定檔案快取失敗: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// 取得快取的縮圖
    /// </summary>
    public async Task<Stream?> GetCachedThumbnailAsync(
        string filePath, 
        int width, 
        int height, 
        int quality, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GenerateThumbnailCacheKey(filePath, width, height, quality);
            
            // 檢查記憶體快取
            if (_memoryCache.TryGetValue(cacheKey, out byte[] cachedData))
            {
                Interlocked.Increment(ref _cacheHits);
                _logger.LogDebug("縮圖記憶體快取命中: {FilePath}", filePath);
                return new MemoryStream(cachedData);
            }

            // 檢查磁碟快取
            var diskCacheFile = GetDiskCacheFilePath(cacheKey);
            if (File.Exists(diskCacheFile))
            {
                var fileInfo = new FileInfo(diskCacheFile);
                var originalFileInfo = new FileInfo(filePath);

                // 檢查快取是否過期
                if (fileInfo.LastWriteTimeUtc >= originalFileInfo.LastWriteTimeUtc)
                {
                    var diskData = await File.ReadAllBytesAsync(diskCacheFile, cancellationToken);
                    
                    // 將磁碟快取載入記憶體快取
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        Size = diskData.Length,
                        SlidingExpiration = TimeSpan.FromHours(2),
                        Priority = CacheItemPriority.High // 縮圖快取優先級較高
                    };
                    _memoryCache.Set(cacheKey, diskData, cacheOptions);

                    Interlocked.Increment(ref _cacheHits);
                    _logger.LogDebug("縮圖磁碟快取命中: {FilePath}", filePath);
                    return new MemoryStream(diskData);
                }
                else
                {
                    // 快取過期，刪除舊快取
                    File.Delete(diskCacheFile);
                }
            }

            Interlocked.Increment(ref _cacheMisses);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得縮圖快取失敗: {FilePath}", filePath);
            Interlocked.Increment(ref _cacheMisses);
            return null;
        }
    }

    /// <summary>
    /// 設定縮圖快取
    /// </summary>
    public async Task SetCachedThumbnailAsync(
        string filePath, 
        int width, 
        int height, 
        int quality, 
        Stream thumbnailStream, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (thumbnailStream == null || !thumbnailStream.CanRead)
                return;

            var cacheKey = GenerateThumbnailCacheKey(filePath, width, height, quality);
            var data = new byte[thumbnailStream.Length];
            
            thumbnailStream.Position = 0;
            await thumbnailStream.ReadAsync(data, 0, data.Length, cancellationToken);

            await _cacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                // 設定記憶體快取
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    Size = data.Length,
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                    SlidingExpiration = TimeSpan.FromHours(2),
                    Priority = CacheItemPriority.High,
                    PostEvictionCallbacks = { new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = OnCacheEvicted
                    }}
                };

                _memoryCache.Set(cacheKey, data, cacheOptions);
                Interlocked.Add(ref _cacheSize, data.Length);

                // 設定磁碟快取
                var diskCacheFile = GetDiskCacheFilePath(cacheKey);
                await File.WriteAllBytesAsync(diskCacheFile, data, cancellationToken);

                _logger.LogDebug("縮圖已快取: {FilePath} ({Width}x{Height}, {Size} bytes)", 
                    filePath, width, height, data.Length);
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定縮圖快取失敗: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// 使快取失效
    /// </summary>
    public async Task InvalidateCacheAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                // 移除檔案快取
                var fileCacheKey = GenerateFileCacheKey(filePath);
                _memoryCache.Remove(fileCacheKey);
                
                var diskCacheFile = GetDiskCacheFilePath(fileCacheKey);
                if (File.Exists(diskCacheFile))
                {
                    File.Delete(diskCacheFile);
                }

                // 移除相關的縮圖快取
                var cacheDirectory = new DirectoryInfo(_diskCachePath);
                var thumbnailPrefix = GenerateThumbnailCacheKeyPrefix(filePath);
                
                var thumbnailCacheFiles = cacheDirectory.GetFiles($"{thumbnailPrefix}*");
                foreach (var file in thumbnailCacheFiles)
                {
                    var thumbnailCacheKey = Path.GetFileNameWithoutExtension(file.Name);
                    _memoryCache.Remove(thumbnailCacheKey);
                    file.Delete();
                }

                _logger.LogDebug("已清除檔案快取: {FilePath}", filePath);
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除檔案快取失敗: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// 清理過期快取
    /// </summary>
    public async Task ClearExpiredCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                var cacheDirectory = new DirectoryInfo(_diskCachePath);
                if (!cacheDirectory.Exists)
                    return;

                var expiredFiles = cacheDirectory.GetFiles()
                    .Where(f => f.LastAccessTimeUtc < DateTime.UtcNow.AddHours(-24))
                    .ToList();

                var deletedCount = 0;
                var deletedSize = 0L;

                foreach (var file in expiredFiles)
                {
                    try
                    {
                        var cacheKey = Path.GetFileNameWithoutExtension(file.Name);
                        _memoryCache.Remove(cacheKey);
                        
                        deletedSize += file.Length;
                        file.Delete();
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "刪除過期快取檔案失敗: {FileName}", file.Name);
                    }
                }

                if (deletedCount > 0)
                {
                    _logger.LogInformation("已清理 {Count} 個過期快取檔案，釋放 {Size} bytes", 
                        deletedCount, deletedSize);
                }
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理過期快取失敗");
        }
    }

    /// <summary>
    /// 取得快取統計資訊
    /// </summary>
    public async Task<FileCacheStats> GetCacheStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheDirectory = new DirectoryInfo(_diskCachePath);
            var diskCacheSize = 0L;
            var diskCacheCount = 0;

            if (cacheDirectory.Exists)
            {
                await Task.Run(() =>
                {
                    var files = cacheDirectory.GetFiles();
                    diskCacheCount = files.Length;
                    diskCacheSize = files.Sum(f => f.Length);
                }, cancellationToken);
            }

            var hitRate = _cacheHits + _cacheMisses > 0 
                ? (double)_cacheHits / (_cacheHits + _cacheMisses) * 100 
                : 0;

            return new FileCacheStats
            {
                MemoryCacheSize = Interlocked.Read(ref _cacheSize),
                DiskCacheSize = diskCacheSize,
                DiskCacheCount = diskCacheCount,
                CacheHits = Interlocked.Read(ref _cacheHits),
                CacheMisses = Interlocked.Read(ref _cacheMisses),
                HitRate = hitRate,
                CachePath = _diskCachePath
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得快取統計失敗");
            return new FileCacheStats();
        }
    }

    #region Private Methods

    /// <summary>
    /// 生成檔案快取鍵
    /// </summary>
    private string GenerateFileCacheKey(string filePath)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(filePath));
        return $"file_{Convert.ToHexString(hashBytes)[..16]}";
    }

    /// <summary>
    /// 生成縮圖快取鍵
    /// </summary>
    private string GenerateThumbnailCacheKey(string filePath, int width, int height, int quality)
    {
        var key = $"{filePath}_{width}x{height}_q{quality}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        return $"thumb_{Convert.ToHexString(hashBytes)[..16]}";
    }

    /// <summary>
    /// 生成縮圖快取鍵前綴
    /// </summary>
    private string GenerateThumbnailCacheKeyPrefix(string filePath)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(filePath));
        return $"thumb_{Convert.ToHexString(hashBytes)[..8]}";
    }

    /// <summary>
    /// 取得磁碟快取檔案路徑
    /// </summary>
    private string GetDiskCacheFilePath(string cacheKey)
    {
        return Path.Combine(_diskCachePath, $"{cacheKey}.cache");
    }

    /// <summary>
    /// 快取項目被移除時的回調
    /// </summary>
    private void OnCacheEvicted(object key, object value, EvictionReason reason, object state)
    {
        if (value is byte[] data)
        {
            Interlocked.Add(ref _cacheSize, -data.Length);
        }
    }

    #endregion

    public void Dispose()
    {
        _cacheSemaphore?.Dispose();
    }
}

/// <summary>
/// 檔案快取統計資訊
/// </summary>
public class FileCacheStats
{
    public long MemoryCacheSize { get; set; }
    public long DiskCacheSize { get; set; }
    public int DiskCacheCount { get; set; }
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public double HitRate { get; set; }
    public string CachePath { get; set; } = string.Empty;
}