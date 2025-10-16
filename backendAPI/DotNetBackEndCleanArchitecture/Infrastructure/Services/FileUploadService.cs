using Infrastructure.Common;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

/// <summary>
/// 檔案上傳服務
/// 提供安全的檔案上傳、驗證和管理功能
/// </summary>
public interface IFileUploadService
{
    Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, FileType fileType, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<Stream?> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
    Task<FileInfo?> GetFileInfoAsync(string filePath, CancellationToken cancellationToken = default);
    Task<long> GetDirectorySizeAsync(string directoryPath, CancellationToken cancellationToken = default);
    Task CleanupExpiredFilesAsync(CancellationToken cancellationToken = default);
    bool ValidateFile(string fileName, long fileSize, FileType fileType, out List<string> errors);
}

public class FileUploadService : IFileUploadService
{
    private readonly FileUploadConfiguration _config;
    private readonly ILogger<FileUploadService> _logger;
    private static readonly Regex FileNameRegex = new(@"^[a-zA-Z0-9._-]+$", RegexOptions.Compiled);

    public FileUploadService(FileUploadConfiguration config, ILogger<FileUploadService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // 確保上傳目錄存在
        _config.EnsureUploadDirectoriesExist();
    }

    /// <summary>
    /// 上傳檔案
    /// </summary>
    public async Task<FileUploadResult> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        FileType fileType, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 驗證輸入參數
            if (fileStream == null || !fileStream.CanRead)
                return FileUploadResult.Failure("無效的檔案流");

            if (string.IsNullOrWhiteSpace(fileName))
                return FileUploadResult.Failure("檔案名稱不能為空");

            // 驗證檔案
            if (!ValidateFile(fileName, fileStream.Length, fileType, out var errors))
                return FileUploadResult.Failure(string.Join(", ", errors));

            // 清理檔案名稱
            var sanitizedFileName = SanitizeFileName(fileName);
            var fileExtension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sanitizedFileName);

            // 生成唯一檔案名稱
            var uniqueFileName = GenerateUniqueFileName(fileNameWithoutExtension, fileExtension);
            var targetDirectory = _config.GetPathByFileType(fileType);
            var targetPath = Path.Combine(targetDirectory, uniqueFileName);

            // 確保目標目錄存在
            Directory.CreateDirectory(targetDirectory);

            // 檢查檔案是否已存在
            if (!_config.SecuritySettings.AllowOverwrite && File.Exists(targetPath))
            {
                // 生成新的唯一檔案名稱
                var counter = 1;
                do
                {
                    uniqueFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";
                    targetPath = Path.Combine(targetDirectory, uniqueFileName);
                    counter++;
                } while (File.Exists(targetPath) && counter < 1000);

                if (File.Exists(targetPath))
                    return FileUploadResult.Failure("無法生成唯一檔案名稱");
            }

            // 寫入檔案
            using (var fileStreamWriter = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);
            }

            // 驗證檔案完整性
            var fileHash = await CalculateFileHashAsync(targetPath, cancellationToken);

            // 記錄上傳日誌
            _logger.LogInformation(
                "檔案上傳成功: {FileName} -> {TargetPath}, 大小: {FileSize} bytes, 類型: {FileType}, 雜湊: {FileHash}",
                fileName, targetPath, new FileInfo(targetPath).Length, fileType, fileHash);

            return FileUploadResult.Success(targetPath, uniqueFileName, fileHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案上傳失敗: {FileName}, 類型: {FileType}", fileName, fileType);
            return FileUploadResult.Failure($"檔案上傳失敗: {ex.Message}");
        }
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    public async Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            // 檢查檔案是否在允許的目錄中
            if (!IsPathAllowed(filePath))
            {
                _logger.LogWarning("嘗試刪除不允許的路徑: {FilePath}", filePath);
                return false;
            }

            await Task.Run(() => File.Delete(filePath), cancellationToken);
            
            _logger.LogInformation("檔案刪除成功: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案刪除失敗: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 取得檔案流
    /// </summary>
    public async Task<Stream?> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            // 檢查檔案是否在允許的目錄中
            if (!IsPathAllowed(filePath))
            {
                _logger.LogWarning("嘗試存取不允許的路徑: {FilePath}", filePath);
                return null;
            }

            return await Task.FromResult(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得檔案流失敗: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 檢查檔案是否存在
    /// </summary>
    public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Task.FromResult(File.Exists(filePath) && IsPathAllowed(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查檔案存在失敗: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 取得檔案資訊
    /// </summary>
    public async Task<FileInfo?> GetFileInfoAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await FileExistsAsync(filePath, cancellationToken))
                return null;

            return await Task.FromResult(new FileInfo(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得檔案資訊失敗: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 取得目錄大小
    /// </summary>
    public async Task<long> GetDirectorySizeAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
                return 0;

            return await Task.Run(() =>
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                    .Sum(file => file.Length);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得目錄大小失敗: {DirectoryPath}", directoryPath);
            return 0;
        }
    }

    /// <summary>
    /// 清理過期檔案
    /// </summary>
    public async Task CleanupExpiredFilesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cleanupTasks = new List<Task>
            {
                CleanupDirectoryAsync(_config.UploadPath, _config.RetentionSettings.UploadRetentionDays, cancellationToken),
                CleanupDirectoryAsync(_config.DepositImagesPath, _config.RetentionSettings.DepositImageRetentionDays, cancellationToken),
                CleanupDirectoryAsync(_config.WithdrawImagesPath, _config.RetentionSettings.WithdrawImageRetentionDays, cancellationToken),
                CleanupDirectoryAsync(_config.AnnouncementImagesPath, _config.RetentionSettings.AnnouncementImageRetentionDays, cancellationToken),
                CleanupDirectoryAsync(_config.LogsPath, _config.RetentionSettings.LogRetentionDays, cancellationToken)
            };

            // KYC 圖片永久保留，不清理
            if (_config.RetentionSettings.KycImageRetentionDays > 0)
            {
                cleanupTasks.Add(CleanupDirectoryAsync(_config.KycImagesPath, _config.RetentionSettings.KycImageRetentionDays, cancellationToken));
            }

            await Task.WhenAll(cleanupTasks);
            _logger.LogInformation("檔案清理完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案清理失敗");
        }
    }

    /// <summary>
    /// 驗證檔案
    /// </summary>
    public bool ValidateFile(string fileName, long fileSize, FileType fileType, out List<string> errors)
    {
        errors = new List<string>();

        // 檢查檔案名稱
        if (string.IsNullOrWhiteSpace(fileName))
        {
            errors.Add("檔案名稱不能為空");
            return false;
        }

        // 檢查檔案名稱長度
        if (fileName.Length > _config.SecuritySettings.MaxFileNameLength)
        {
            errors.Add($"檔案名稱過長，最大長度為 {_config.SecuritySettings.MaxFileNameLength} 字符");
        }

        // 檢查檔案大小
        var maxSize = _config.GetMaxFileSizeByType(fileType);
        if (fileSize > maxSize)
        {
            errors.Add($"檔案大小超過限制，最大允許 {maxSize / (1024 * 1024)} MB");
        }

        if (fileSize <= 0)
        {
            errors.Add("檔案大小無效");
        }

        // 檢查副檔名
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            errors.Add("檔案必須有副檔名");
        }
        else
        {
            // 檢查是否為禁止的副檔名
            if (_config.SecuritySettings.ForbiddenExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"不允許的檔案類型: {extension}");
            }

            // 檢查是否為允許的副檔名
            if (!_config.IsExtensionAllowed(extension, fileType))
            {
                errors.Add($"不支援的檔案類型: {extension}");
            }
        }

        // 檢查檔案名稱字符
        if (_config.SecuritySettings.EnableFileNameSanitization)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            if (!FileNameRegex.IsMatch(fileNameWithoutExtension))
            {
                errors.Add("檔案名稱包含不允許的字符，只允許字母、數字、點、底線和連字符");
            }
        }

        return errors.Count == 0;
    }

    #region Private Methods

    /// <summary>
    /// 清理檔案名稱
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        if (!_config.SecuritySettings.EnableFileNameSanitization)
            return fileName;

        // 移除不安全的字符
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder();

        foreach (var c in fileName)
        {
            if (!invalidChars.Contains(c) && c != '<' && c != '>' && c != '"' && c != '|' && c != '?' && c != '*')
            {
                sanitized.Append(c);
            }
            else
            {
                sanitized.Append('_');
            }
        }

        return sanitized.ToString();
    }

    /// <summary>
    /// 生成唯一檔案名稱
    /// </summary>
    private string GenerateUniqueFileName(string fileNameWithoutExtension, string extension)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];
        return $"{fileNameWithoutExtension}_{timestamp}_{guid}{extension}";
    }

    /// <summary>
    /// 計算檔案雜湊值
    /// </summary>
    private async Task<string> CalculateFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// 檢查路徑是否允許
    /// </summary>
    private bool IsPathAllowed(string filePath)
    {
        var allowedPaths = new[]
        {
            _config.UploadPath,
            _config.KycImagesPath,
            _config.DepositImagesPath,
            _config.WithdrawImagesPath,
            _config.AnnouncementImagesPath,
            _config.LogsPath
        };

        return allowedPaths.Any(allowedPath => 
            filePath.StartsWith(Path.GetFullPath(allowedPath), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 清理目錄中的過期檔案
    /// </summary>
    private async Task CleanupDirectoryAsync(string directoryPath, int retentionDays, CancellationToken cancellationToken)
    {
        if (retentionDays <= 0 || !Directory.Exists(directoryPath))
            return;

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var directoryInfo = new DirectoryInfo(directoryPath);
            var filesToDelete = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
                .Where(file => file.LastWriteTimeUtc < cutoffDate)
                .ToList();

            var deletedCount = 0;
            foreach (var file in filesToDelete)
            {
                try
                {
                    await Task.Run(() => file.Delete(), cancellationToken);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "無法刪除過期檔案: {FilePath}", file.FullName);
                }
            }

            if (deletedCount > 0)
            {
                _logger.LogInformation("已清理 {DeletedCount} 個過期檔案，目錄: {DirectoryPath}", deletedCount, directoryPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理目錄失敗: {DirectoryPath}", directoryPath);
        }
    }

    #endregion
}

/// <summary>
/// 檔案上傳結果
/// </summary>
public class FileUploadResult
{
    public bool IsSuccess { get; private set; }
    public string? FilePath { get; private set; }
    public string? FileName { get; private set; }
    public string? FileHash { get; private set; }
    public string? ErrorMessage { get; private set; }

    private FileUploadResult() { }

    public static FileUploadResult Success(string filePath, string fileName, string fileHash)
    {
        return new FileUploadResult
        {
            IsSuccess = true,
            FilePath = filePath,
            FileName = fileName,
            FileHash = fileHash
        };
    }

    public static FileUploadResult Failure(string errorMessage)
    {
        return new FileUploadResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}