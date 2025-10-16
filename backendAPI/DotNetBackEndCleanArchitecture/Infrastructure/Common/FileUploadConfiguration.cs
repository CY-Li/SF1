using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Common;

/// <summary>
/// 檔案上傳配置類
/// 用於管理 Zeabur 環境下的檔案上傳設定
/// </summary>
public class FileUploadConfiguration
{
    public const string SectionName = "FileUpload";

    /// <summary>
    /// 最大檔案大小 (位元組)
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "檔案大小必須大於 0")]
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// 允許的檔案副檔名
    /// </summary>
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 通用上傳路徑
    /// </summary>
    [Required(ErrorMessage = "上傳路徑不能為空")]
    public string UploadPath { get; set; } = "/app/uploads";

    /// <summary>
    /// KYC 身份認證圖片路徑
    /// </summary>
    [Required(ErrorMessage = "KYC 圖片路徑不能為空")]
    public string KycImagesPath { get; set; } = "/app/KycImages";

    /// <summary>
    /// 存款憑證圖片路徑
    /// </summary>
    [Required(ErrorMessage = "存款憑證路徑不能為空")]
    public string DepositImagesPath { get; set; } = "/app/DepositImages";

    /// <summary>
    /// 提款憑證圖片路徑
    /// </summary>
    [Required(ErrorMessage = "提款憑證路徑不能為空")]
    public string WithdrawImagesPath { get; set; } = "/app/WithdrawImages";

    /// <summary>
    /// 公告圖片路徑
    /// </summary>
    [Required(ErrorMessage = "公告圖片路徑不能為空")]
    public string AnnouncementImagesPath { get; set; } = "/app/AnnImagessss";

    /// <summary>
    /// 日誌檔案路徑
    /// </summary>
    [Required(ErrorMessage = "日誌路徑不能為空")]
    public string LogsPath { get; set; } = "/app/logs";

    /// <summary>
    /// 是否啟用檔案壓縮
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// 是否啟用檔案加密
    /// </summary>
    public bool EnableEncryption { get; set; } = false;

    /// <summary>
    /// 是否啟用病毒掃描
    /// </summary>
    public bool EnableVirusScanning { get; set; } = true;

    /// <summary>
    /// 檔案保留天數設定
    /// </summary>
    public FileRetentionSettings RetentionSettings { get; set; } = new();

    /// <summary>
    /// 檔案類型限制設定
    /// </summary>
    public FileTypeLimits TypeLimits { get; set; } = new();

    /// <summary>
    /// 安全設定
    /// </summary>
    public FileSecuritySettings SecuritySettings { get; set; } = new();

    /// <summary>
    /// 驗證配置是否有效
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (MaxFileSize <= 0)
            errors.Add("最大檔案大小必須大於 0");

        if (AllowedExtensions == null || AllowedExtensions.Length == 0)
            errors.Add("必須設定允許的檔案副檔名");

        var paths = new[]
        {
            UploadPath, KycImagesPath, DepositImagesPath,
            WithdrawImagesPath, AnnouncementImagesPath, LogsPath
        };

        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
                errors.Add($"路徑不能為空: {path}");
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// 取得指定檔案類型的路徑
    /// </summary>
    public string GetPathByFileType(FileType fileType)
    {
        return fileType switch
        {
            FileType.Upload => UploadPath,
            FileType.KycImage => KycImagesPath,
            FileType.DepositImage => DepositImagesPath,
            FileType.WithdrawImage => WithdrawImagesPath,
            FileType.AnnouncementImage => AnnouncementImagesPath,
            FileType.Log => LogsPath,
            _ => UploadPath
        };
    }

    /// <summary>
    /// 檢查檔案副檔名是否允許
    /// </summary>
    public bool IsExtensionAllowed(string extension, FileType fileType)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return false;

        extension = extension.ToLowerInvariant();
        if (!extension.StartsWith("."))
            extension = "." + extension;

        // 檢查全域允許的副檔名
        if (AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return true;

        // 檢查特定檔案類型的限制
        return fileType switch
        {
            FileType.KycImage => TypeLimits.KycImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase),
            FileType.DepositImage => TypeLimits.DepositImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase),
            FileType.WithdrawImage => TypeLimits.WithdrawImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase),
            FileType.AnnouncementImage => TypeLimits.AnnouncementImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase),
            FileType.Log => TypeLimits.LogExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase),
            _ => AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// 取得檔案類型的最大大小限制
    /// </summary>
    public long GetMaxFileSizeByType(FileType fileType)
    {
        return fileType switch
        {
            FileType.KycImage => TypeLimits.KycImageMaxSize,
            FileType.DepositImage => TypeLimits.DepositImageMaxSize,
            FileType.WithdrawImage => TypeLimits.WithdrawImageMaxSize,
            FileType.AnnouncementImage => TypeLimits.AnnouncementImageMaxSize,
            FileType.Log => TypeLimits.LogMaxSize,
            _ => MaxFileSize
        };
    }
}

/// <summary>
/// 檔案保留設定
/// </summary>
public class FileRetentionSettings
{
    /// <summary>
    /// 通用上傳檔案保留天數
    /// </summary>
    public int UploadRetentionDays { get; set; } = 365;

    /// <summary>
    /// KYC 圖片保留天數 (-1 表示永久保留)
    /// </summary>
    public int KycImageRetentionDays { get; set; } = -1;

    /// <summary>
    /// 存款憑證保留天數
    /// </summary>
    public int DepositImageRetentionDays { get; set; } = 1095; // 3年

    /// <summary>
    /// 提款憑證保留天數
    /// </summary>
    public int WithdrawImageRetentionDays { get; set; } = 1095; // 3年

    /// <summary>
    /// 公告圖片保留天數
    /// </summary>
    public int AnnouncementImageRetentionDays { get; set; } = 730; // 2年

    /// <summary>
    /// 日誌檔案保留天數
    /// </summary>
    public int LogRetentionDays { get; set; } = 30;
}

/// <summary>
/// 檔案類型限制設定
/// </summary>
public class FileTypeLimits
{
    /// <summary>
    /// KYC 圖片允許的副檔名
    /// </summary>
    public string[] KycImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png" };

    /// <summary>
    /// KYC 圖片最大大小 (5MB)
    /// </summary>
    public long KycImageMaxSize { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// 存款憑證允許的副檔名
    /// </summary>
    public string[] DepositImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".pdf" };

    /// <summary>
    /// 存款憑證最大大小 (5MB)
    /// </summary>
    public long DepositImageMaxSize { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// 提款憑證允許的副檔名
    /// </summary>
    public string[] WithdrawImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".pdf" };

    /// <summary>
    /// 提款憑證最大大小 (5MB)
    /// </summary>
    public long WithdrawImageMaxSize { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// 公告圖片允許的副檔名
    /// </summary>
    public string[] AnnouncementImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif" };

    /// <summary>
    /// 公告圖片最大大小 (3MB)
    /// </summary>
    public long AnnouncementImageMaxSize { get; set; } = 3 * 1024 * 1024;

    /// <summary>
    /// 日誌檔案允許的副檔名
    /// </summary>
    public string[] LogExtensions { get; set; } = { ".log", ".txt", ".json" };

    /// <summary>
    /// 日誌檔案最大大小 (100MB)
    /// </summary>
    public long LogMaxSize { get; set; } = 100 * 1024 * 1024;
}

/// <summary>
/// 檔案安全設定
/// </summary>
public class FileSecuritySettings
{
    /// <summary>
    /// 是否啟用檔案內容驗證
    /// </summary>
    public bool EnableContentValidation { get; set; } = true;

    /// <summary>
    /// 是否啟用檔案名稱清理
    /// </summary>
    public bool EnableFileNameSanitization { get; set; } = true;

    /// <summary>
    /// 是否啟用存取日誌
    /// </summary>
    public bool EnableAccessLogging { get; set; } = true;

    /// <summary>
    /// 禁止的檔案副檔名
    /// </summary>
    public string[] ForbiddenExtensions { get; set; } = 
    {
        ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar",
        ".asp", ".aspx", ".php", ".jsp", ".sh", ".ps1", ".py", ".rb", ".pl"
    };

    /// <summary>
    /// 最大檔案名稱長度
    /// </summary>
    public int MaxFileNameLength { get; set; } = 255;

    /// <summary>
    /// 是否允許覆蓋現有檔案
    /// </summary>
    public bool AllowOverwrite { get; set; } = false;
}

/// <summary>
/// 檔案類型枚舉
/// </summary>
public enum FileType
{
    Upload,
    KycImage,
    DepositImage,
    WithdrawImage,
    AnnouncementImage,
    Log
}

/// <summary>
/// 檔案上傳配置擴展方法
/// </summary>
public static class FileUploadConfigurationExtensions
{
    /// <summary>
    /// 註冊檔案上傳配置服務
    /// </summary>
    public static IServiceCollection AddFileUploadConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var fileUploadConfig = new FileUploadConfiguration();
        configuration.GetSection(FileUploadConfiguration.SectionName).Bind(fileUploadConfig);

        // 從環境變數覆蓋設定
        if (long.TryParse(configuration["FILE_UPLOAD_MAX_SIZE"], out var maxSize))
        {
            fileUploadConfig.MaxFileSize = maxSize;
        }

        var extensions = configuration["FILE_UPLOAD_EXTENSIONS"];
        if (!string.IsNullOrWhiteSpace(extensions))
        {
            fileUploadConfig.AllowedExtensions = extensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim())
                .ToArray();
        }

        // 驗證配置
        if (!fileUploadConfig.IsValid(out var errors))
        {
            var logger = services.BuildServiceProvider().GetService<ILogger<FileUploadConfiguration>>();
            logger?.LogError("檔案上傳配置驗證失敗: {Errors}", string.Join(", ", errors));
            throw new InvalidOperationException($"檔案上傳配置無效: {string.Join(", ", errors)}");
        }

        services.AddSingleton(fileUploadConfig);
        return services;
    }

    /// <summary>
    /// 確保所有上傳目錄存在
    /// </summary>
    public static void EnsureUploadDirectoriesExist(this FileUploadConfiguration config)
    {
        var directories = new[]
        {
            config.UploadPath,
            config.KycImagesPath,
            config.DepositImagesPath,
            config.WithdrawImagesPath,
            config.AnnouncementImagesPath,
            config.LogsPath
        };

        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}