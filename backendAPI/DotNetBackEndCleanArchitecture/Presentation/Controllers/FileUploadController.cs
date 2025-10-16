using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Controllers;

/// <summary>
/// 檔案上傳控制器
/// 提供檔案上傳、下載和管理功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileUploadController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly FileUploadConfiguration _config;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(
        IFileUploadService fileUploadService,
        FileUploadConfiguration config,
        ILogger<FileUploadController> logger)
    {
        _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 上傳檔案
    /// </summary>
    /// <param name="file">要上傳的檔案</param>
    /// <param name="fileType">檔案類型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上傳結果</returns>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<IActionResult> UploadFile(
        [Required] IFormFile file,
        [Required] FileType fileType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "請選擇要上傳的檔案" });

            // 驗證檔案
            if (!_fileUploadService.ValidateFile(file.FileName, file.Length, fileType, out var errors))
            {
                return BadRequest(new { message = "檔案驗證失敗", errors });
            }

            // 上傳檔案
            using var stream = file.OpenReadStream();
            var result = await _fileUploadService.UploadFileAsync(stream, file.FileName, fileType, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                message = "檔案上傳成功",
                fileName = result.FileName,
                filePath = result.FilePath,
                fileHash = result.FileHash,
                fileSize = file.Length,
                fileType = fileType.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案上傳失敗: {FileName}", file?.FileName);
            return StatusCode(500, new { message = "檔案上傳失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 下載檔案
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>檔案內容</returns>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile(
        [Required] string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "檔案路徑不能為空" });

            var fileStream = await _fileUploadService.GetFileStreamAsync(filePath, cancellationToken);
            if (fileStream == null)
                return NotFound(new { message = "檔案不存在" });

            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);

            return File(fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案下載失敗: {FilePath}", filePath);
            return StatusCode(500, new { message = "檔案下載失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刪除結果</returns>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFile(
        [Required] string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "檔案路徑不能為空" });

            var success = await _fileUploadService.DeleteFileAsync(filePath, cancellationToken);
            if (!success)
                return NotFound(new { message = "檔案不存在或刪除失敗" });

            return Ok(new { message = "檔案刪除成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案刪除失敗: {FilePath}", filePath);
            return StatusCode(500, new { message = "檔案刪除失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 取得檔案資訊
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>檔案資訊</returns>
    [HttpGet("info")]
    public async Task<IActionResult> GetFileInfo(
        [Required] string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "檔案路徑不能為空" });

            var fileInfo = await _fileUploadService.GetFileInfoAsync(filePath, cancellationToken);
            if (fileInfo == null)
                return NotFound(new { message = "檔案不存在" });

            return Ok(new
            {
                fileName = fileInfo.Name,
                filePath = fileInfo.FullName,
                fileSize = fileInfo.Length,
                createdTime = fileInfo.CreationTimeUtc,
                lastModifiedTime = fileInfo.LastWriteTimeUtc,
                extension = fileInfo.Extension
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得檔案資訊失敗: {FilePath}", filePath);
            return StatusCode(500, new { message = "取得檔案資訊失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 取得存儲統計資訊
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存儲統計資訊</returns>
    [HttpGet("storage-stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStorageStats(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = new
            {
                uploads = new
                {
                    path = _config.UploadPath,
                    size = await _fileUploadService.GetDirectorySizeAsync(_config.UploadPath, cancellationToken),
                    retentionDays = _config.RetentionSettings.UploadRetentionDays
                },
                kycImages = new
                {
                    path = _config.KycImagesPath,
                    size = await _fileUploadService.GetDirectorySizeAsync(_config.KycImagesPath, cancellationToken),
                    retentionDays = _config.RetentionSettings.KycImageRetentionDays
                },
                depositImages = new
                {
                    path = _config.DepositImagesPath,
                    size = await _fileUploadService.GetDirectorySizeAsync(_config.DepositImagesPath, cancellationToken),
                    retentionDays = _config.RetentionSettings.DepositImageRetentionDays
                },
                withdrawImages = new
                {
                    path = _config.WithdrawImagesPath,
                    size = await _fileUploadService.GetDirectorySizeAsync(_config.WithdrawImagesPath, cancellationToken),
                    retentionDays = _config.RetentionSettings.WithdrawImageRetentionDays
                },
                announcementImages = new
                {
                    path = _config.AnnouncementImagesPath,
                    size = await _fileUploadService.GetDirectorySizeAsync(_config.AnnouncementImagesPath, cancellationToken),
                    retentionDays = _config.RetentionSettings.AnnouncementImageRetentionDays
                },
                logs = new
                {
                    path = _config.LogsPath,
                    size = await _fileUploadService.GetDirectorySizeAsync(_config.LogsPath, cancellationToken),
                    retentionDays = _config.RetentionSettings.LogRetentionDays
                }
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得存儲統計失敗");
            return StatusCode(500, new { message = "取得存儲統計失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 清理過期檔案
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>清理結果</returns>
    [HttpPost("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupExpiredFiles(CancellationToken cancellationToken = default)
    {
        try
        {
            await _fileUploadService.CleanupExpiredFilesAsync(cancellationToken);
            return Ok(new { message = "過期檔案清理完成" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理過期檔案失敗");
            return StatusCode(500, new { message = "清理過期檔案失敗，請稍後再試" });
        }
    }

    /// <summary>
    /// 取得檔案上傳配置
    /// </summary>
    /// <returns>檔案上傳配置</returns>
    [HttpGet("config")]
    public IActionResult GetUploadConfig()
    {
        try
        {
            var config = new
            {
                maxFileSize = _config.MaxFileSize,
                allowedExtensions = _config.AllowedExtensions,
                typeLimits = new
                {
                    kycImage = new
                    {
                        maxSize = _config.TypeLimits.KycImageMaxSize,
                        allowedExtensions = _config.TypeLimits.KycImageExtensions
                    },
                    depositImage = new
                    {
                        maxSize = _config.TypeLimits.DepositImageMaxSize,
                        allowedExtensions = _config.TypeLimits.DepositImageExtensions
                    },
                    withdrawImage = new
                    {
                        maxSize = _config.TypeLimits.WithdrawImageMaxSize,
                        allowedExtensions = _config.TypeLimits.WithdrawImageExtensions
                    },
                    announcementImage = new
                    {
                        maxSize = _config.TypeLimits.AnnouncementImageMaxSize,
                        allowedExtensions = _config.TypeLimits.AnnouncementImageExtensions
                    }
                },
                securitySettings = new
                {
                    maxFileNameLength = _config.SecuritySettings.MaxFileNameLength,
                    forbiddenExtensions = _config.SecuritySettings.ForbiddenExtensions
                }
            };

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得上傳配置失敗");
            return StatusCode(500, new { message = "取得上傳配置失敗，請稍後再試" });
        }
    }

    #region Private Methods

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

    #endregion
}