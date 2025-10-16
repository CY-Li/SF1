using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Presentation.Controllers;

/// <summary>
/// 檔案存取控制器
/// 提供安全的檔案存取、預覽和下載功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileAccessController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly FileUploadConfiguration _config;
    private readonly ILogger<FileAccessController> _logger;

    public FileAccessController(
        IFileUploadService fileUploadService,
        FileUploadConfiguration config,
        ILogger<FileAccessController> logger)
    {
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 下載檔案
    /// </summary>
    /// <param name="fileId">檔案 ID 或路徑</param>
    /// <param name="download">是否強制下載 (true) 或在瀏覽器中顯示 (false)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>檔案內容</returns>
    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFile(
        [Required] string fileId,
        [FromQuery] bool download = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 解碼檔案路徑
            var filePath = DecodeFileId(fileId);
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "無效的檔案 ID" });

            // 檢查檔案存取權限
            if (!await CheckFileAccessPermissionAsync(filePath, FileAccessType.Read))
                return Forbid("您沒有權限存取此檔案");

            // 取得檔案流
            var fileStream = await _fileUploadService.GetFileStreamAsync(filePath, cancellationToken);
            if (fileStream == null)
                return NotFound(new { message = "檔案不存在" });

            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);

            // 記錄存取日誌
            LogFileAccess(filePath, FileAccessType.Read, "下載");

            // 設定回應標頭
            var contentDisposition = download ? "attachment" : "inline";
            Response.Headers.Add("Content-Disposition", $"{contentDisposition}; filename=\"{fileName}\"");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            return File(fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案下載失敗: {FileId}", fileId);
            return StatusCode(500, new { message = "檔案下載失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 預覽檔案 (圖片縮圖)
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    /// <param name="width">縮圖寬度</param>
    /// <param name="height">縮圖高度</param>
    /// <param name="quality">圖片品質 (1-100)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>縮圖內容</returns>
    [HttpGet("preview/{fileId}")]
    public async Task<IActionResult> PreviewFile(
        [Required] string fileId,
        [FromQuery] int width = 300,
        [FromQuery] int height = 300,
        [FromQuery] int quality = 80,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 解碼檔案路徑
            var filePath = DecodeFileId(fileId);
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "無效的檔案 ID" });

            // 檢查是否為圖片檔案
            if (!IsImageFile(filePath))
                return BadRequest(new { message = "此檔案不支援預覽" });

            // 檢查檔案存取權限
            if (!await CheckFileAccessPermissionAsync(filePath, FileAccessType.Read))
                return Forbid("您沒有權限存取此檔案");

            // 檢查檔案是否存在
            if (!await _fileUploadService.FileExistsAsync(filePath, cancellationToken))
                return NotFound(new { message = "檔案不存在" });

            // 生成縮圖
            var thumbnailStream = await GenerateThumbnailAsync(filePath, width, height, quality, cancellationToken);
            if (thumbnailStream == null)
                return StatusCode(500, new { message = "無法生成縮圖" });

            // 記錄存取日誌
            LogFileAccess(filePath, FileAccessType.Read, "預覽");

            // 設定快取標頭
            Response.Headers.Add("Cache-Control", "public, max-age=3600");
            Response.Headers.Add("ETag", $"\"{GenerateETag(filePath)}\"");

            return File(thumbnailStream, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案預覽失敗: {FileId}", fileId);
            return StatusCode(500, new { message = "檔案預覽失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 取得檔案資訊
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>檔案資訊</returns>
    [HttpGet("info/{fileId}")]
    public async Task<IActionResult> GetFileInfo(
        [Required] string fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 解碼檔案路徑
            var filePath = DecodeFileId(fileId);
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "無效的檔案 ID" });

            // 檢查檔案存取權限
            if (!await CheckFileAccessPermissionAsync(filePath, FileAccessType.Read))
                return Forbid("您沒有權限存取此檔案");

            // 取得檔案資訊
            var fileInfo = await _fileUploadService.GetFileInfoAsync(filePath, cancellationToken);
            if (fileInfo == null)
                return NotFound(new { message = "檔案不存在" });

            // 記錄存取日誌
            LogFileAccess(filePath, FileAccessType.Read, "查看資訊");

            var result = new
            {
                fileId = fileId,
                fileName = fileInfo.Name,
                fileSize = fileInfo.Length,
                fileSizeFormatted = FormatFileSize(fileInfo.Length),
                extension = fileInfo.Extension,
                contentType = GetContentType(fileInfo.Name),
                createdTime = fileInfo.CreationTimeUtc,
                lastModifiedTime = fileInfo.LastWriteTimeUtc,
                isImage = IsImageFile(fileInfo.Name),
                canPreview = CanPreviewFile(fileInfo.Name),
                downloadUrl = Url.Action(nameof(DownloadFile), new { fileId }),
                previewUrl = IsImageFile(fileInfo.Name) ? Url.Action(nameof(PreviewFile), new { fileId }) : null
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得檔案資訊失敗: {FileId}", fileId);
            return StatusCode(500, new { message = "取得檔案資訊失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 檢查檔案是否存在
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>檔案存在狀態</returns>
    [HttpHead("exists/{fileId}")]
    public async Task<IActionResult> CheckFileExists(
        [Required] string fileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 解碼檔案路徑
            var filePath = DecodeFileId(fileId);
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest();

            // 檢查檔案存取權限
            if (!await CheckFileAccessPermissionAsync(filePath, FileAccessType.Read))
                return Forbid();

            // 檢查檔案是否存在
            var exists = await _fileUploadService.FileExistsAsync(filePath, cancellationToken);
            
            return exists ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查檔案存在失敗: {FileId}", fileId);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// 取得檔案存取記錄 (管理員)
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    /// <param name="pageIndex">頁面索引</param>
    /// <param name="pageSize">頁面大小</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存取記錄</returns>
    [HttpGet("access-log/{fileId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetFileAccessLog(
        [Required] string fileId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 解碼檔案路徑
            var filePath = DecodeFileId(fileId);
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "無效的檔案 ID" });

            // 這裡應該從資料庫或日誌系統取得存取記錄
            // 暫時返回模擬資料
            var accessLogs = new[]
            {
                new
                {
                    timestamp = DateTime.UtcNow.AddHours(-1),
                    userId = "user123",
                    userName = "測試用戶",
                    action = "下載",
                    ipAddress = "192.168.1.100",
                    userAgent = "Mozilla/5.0..."
                }
            };

            var result = new
            {
                fileId = fileId,
                filePath = filePath,
                totalCount = accessLogs.Length,
                pageIndex = pageIndex,
                pageSize = pageSize,
                logs = accessLogs.Skip(pageIndex * pageSize).Take(pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得檔案存取記錄失敗: {FileId}", fileId);
            return StatusCode(500, new { message = "取得存取記錄失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 批量下載檔案 (ZIP)
    /// </summary>
    /// <param name="request">批量下載請求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>ZIP 檔案</returns>
    [HttpPost("batch-download")]
    public async Task<IActionResult> BatchDownload(
        [FromBody] BatchDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request?.FileIds == null || request.FileIds.Length == 0)
                return BadRequest(new { message = "請選擇要下載的檔案" });

            if (request.FileIds.Length > 50)
                return BadRequest(new { message = "一次最多只能下載 50 個檔案" });

            // 驗證所有檔案的存取權限
            var validFiles = new List<string>();
            foreach (var fileId in request.FileIds)
            {
                var filePath = DecodeFileId(fileId);
                if (!string.IsNullOrWhiteSpace(filePath) && 
                    await CheckFileAccessPermissionAsync(filePath, FileAccessType.Read) &&
                    await _fileUploadService.FileExistsAsync(filePath, cancellationToken))
                {
                    validFiles.Add(filePath);
                }
            }

            if (validFiles.Count == 0)
                return BadRequest(new { message = "沒有可下載的檔案" });

            // 生成 ZIP 檔案
            var zipStream = await CreateZipArchiveAsync(validFiles, cancellationToken);
            if (zipStream == null)
                return StatusCode(500, new { message = "建立壓縮檔失敗" });

            // 記錄批量下載日誌
            foreach (var filePath in validFiles)
            {
                LogFileAccess(filePath, FileAccessType.Read, "批量下載");
            }

            var zipFileName = $"files_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            return File(zipStream, "application/zip", zipFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量下載失敗");
            return StatusCode(500, new { message = "批量下載失敗，請稍後再試" });
        }
    }

    #region Private Methods

    /// <summary>
    /// 解碼檔案 ID 為檔案路徑
    /// </summary>
    private string DecodeFileId(string fileId)
    {
        try
        {
            // 這裡可以實作檔案 ID 的編碼/解碼邏輯
            // 為了安全起見，不直接暴露檔案路徑
            var bytes = Convert.FromBase64String(fileId);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 編碼檔案路徑為檔案 ID
    /// </summary>
    private string EncodeFileId(string filePath)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(filePath);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 檢查檔案存取權限
    /// </summary>
    private async Task<bool> CheckFileAccessPermissionAsync(string filePath, FileAccessType accessType)
    {
        try
        {
            // 檢查檔案是否在允許的目錄中
            var allowedPaths = new[]
            {
                _config.UploadPath,
                _config.KycImagesPath,
                _config.DepositImagesPath,
                _config.WithdrawImagesPath,
                _config.AnnouncementImagesPath
            };

            var isInAllowedPath = allowedPaths.Any(allowedPath =>
                filePath.StartsWith(Path.GetFullPath(allowedPath), StringComparison.OrdinalIgnoreCase));

            if (!isInAllowedPath)
                return false;

            // 取得當前用戶資訊
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

            // 檢查角色權限
            if (userRoles.Contains("Admin"))
                return true; // 管理員可以存取所有檔案

            // KYC 圖片需要特殊權限
            if (filePath.StartsWith(Path.GetFullPath(_config.KycImagesPath), StringComparison.OrdinalIgnoreCase))
            {
                return userRoles.Contains("KycReviewer") || userRoles.Contains("Admin");
            }

            // 存款/提款憑證需要財務權限
            if (filePath.StartsWith(Path.GetFullPath(_config.DepositImagesPath), StringComparison.OrdinalIgnoreCase) ||
                filePath.StartsWith(Path.GetFullPath(_config.WithdrawImagesPath), StringComparison.OrdinalIgnoreCase))
            {
                return userRoles.Contains("FinanceReviewer") || userRoles.Contains("Admin");
            }

            // 公告圖片公開存取
            if (filePath.StartsWith(Path.GetFullPath(_config.AnnouncementImagesPath), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 一般上傳檔案需要登入即可存取
            return !string.IsNullOrWhiteSpace(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查檔案存取權限失敗: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 記錄檔案存取日誌
    /// </summary>
    private void LogFileAccess(string filePath, FileAccessType accessType, string action)
    {
        if (!_config.SecuritySettings.EnableAccessLogging)
            return;

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        _logger.LogInformation(
            "檔案存取: 用戶={UserId}({UserName}), 動作={Action}, 檔案={FilePath}, IP={IpAddress}, UserAgent={UserAgent}",
            userId, userName, action, filePath, ipAddress, userAgent);
    }

    /// <summary>
    /// 取得檔案的 MIME 類型
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".log" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// 檢查是否為圖片檔案
    /// </summary>
    private static bool IsImageFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp";
    }

    /// <summary>
    /// 檢查檔案是否可以預覽
    /// </summary>
    private static bool CanPreviewFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".pdf" or ".txt";
    }

    /// <summary>
    /// 格式化檔案大小
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// 生成 ETag
    /// </summary>
    private string GenerateETag(string filePath)
    {
        var lastModified = System.IO.File.GetLastWriteTimeUtc(filePath);
        var size = new FileInfo(filePath).Length;
        return $"{lastModified.Ticks:X}-{size:X}";
    }

    /// <summary>
    /// 生成縮圖 (簡化實作，實際應使用圖片處理庫)
    /// </summary>
    private async Task<Stream?> GenerateThumbnailAsync(string filePath, int width, int height, int quality, CancellationToken cancellationToken)
    {
        try
        {
            // 這裡應該使用圖片處理庫如 ImageSharp 或 SkiaSharp
            // 暫時返回原始檔案
            return await _fileUploadService.GetFileStreamAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成縮圖失敗: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 建立 ZIP 壓縮檔
    /// </summary>
    private async Task<Stream?> CreateZipArchiveAsync(List<string> filePaths, CancellationToken cancellationToken)
    {
        try
        {
            var memoryStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var filePath in filePaths)
                {
                    var fileName = Path.GetFileName(filePath);
                    var entry = archive.CreateEntry(fileName);
                    
                    using var entryStream = entry.Open();
                    using var fileStream = await _fileUploadService.GetFileStreamAsync(filePath, cancellationToken);
                    
                    if (fileStream != null)
                    {
                        await fileStream.CopyToAsync(entryStream, cancellationToken);
                    }
                }
            }
            
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立 ZIP 壓縮檔失敗");
            return null;
        }
    }

    #endregion
}

/// <summary>
/// 檔案存取類型
/// </summary>
public enum FileAccessType
{
    Read,
    Write,
    Delete
}

/// <summary>
/// 批量下載請求
/// </summary>
public class BatchDownloadRequest
{
    [Required]
    public string[] FileIds { get; set; } = Array.Empty<string>();
    
    public string? ArchiveName { get; set; }
}