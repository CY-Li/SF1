using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.SystemMgmt.Announcement;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
namespace DotNetBackEndApi.Controllers.SystemMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "AnnouncementController";

        public AnnouncementController(
            ILogger<AnnouncementController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台-取得公告列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryAnnBoardAdmin")]
        public ApiResultModel<BaseGridRespModel<QueryAnnBoardAdminRespModel>> QueryAnnBoardAdmin([FromQuery] QueryAnnBoardAdminReqModel reqModel)
        {
            string mActionLog = "QueryAnnBoardAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<BaseGridRespModel<QueryAnnBoardAdminRespModel>> mResult = new ApiResultModel<BaseGridRespModel<QueryAnnBoardAdminRespModel>>();

            try
            {
                mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryAnnBoardAdminRespModel>>("SystemMgmt/AnnouncementService/QueryAnnBoardAdmin", reqModel);

                string mAnnBoardFolderPath = Path.Combine("."
                                                     , _apiAppConfig.RootFolderPath ?? "assets"
                                                     , _apiAppConfig.AnnBoardFolderPath ?? "AnnBoardImages");

                if (mResult.Result != null && mResult.Result.GridRespResult.Count() > 0)
                {
                    foreach (var item in mResult.Result.GridRespResult)
                    {
                        string mAnnBoardFullpath = Path.Combine(item.ab_image_path, item.ab_image_name);
                        string? mFileUrl = Url.Action("GetAnnBoardImage", "Announcement", new { imageName = item.ab_image_name }, Request.Scheme);
                        if (!string.IsNullOrEmpty(mFileUrl))
                        {
                            item.ab_image_url = mFileUrl;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得公告失敗 {ex.Message}";
            }
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        [HttpGet]
        [Route("QueryAnnBoardUser")]
        public ApiResultModel<BaseGridRespModel<QueryAnnBoardUserRespModel>> QueryAnnBoardUser([FromQuery] QueryAnnBoardUserReqModel reqModel)
        {
            string mActionLog = "QueryAnnBoardUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<BaseGridRespModel<QueryAnnBoardUserRespModel>> mResult = new ApiResultModel<BaseGridRespModel<QueryAnnBoardUserRespModel>>();
            try
            {
                mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryAnnBoardUserRespModel>>("SystemMgmt/AnnouncementService/QueryAnnBoardUser", reqModel);

                string mAnnBoardFolderPath = Path.Combine("."
                                                     , _apiAppConfig.RootFolderPath ?? "assets"
                                                     , _apiAppConfig.AnnBoardFolderPath ?? "AnnBoardImages");

                if (mResult.Result != null && mResult.Result.GridRespResult.Count() > 0)
                {
                    foreach (var item in mResult.Result.GridRespResult)
                    {
                        //string mAnnBoardFullpath = Path.Combine(item.ab_image_path, item.ab_image_name);
                        string? mFileUrl = Url.Action("GetAnnBoardImage", "Announcement", new { imageName = item.ab_image_name }, Request.Scheme);
                        if (!string.IsNullOrEmpty(mFileUrl))
                        {
                            item.ab_image_url = mFileUrl;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得公告失敗 {ex.Message}";
            }
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 後台-新增公告
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResultModel<bool> PostAnnouncement([FromForm] PostAnnouncementReqModel reqModel, IFormFile file)
        {
            string mActionLog = "PostAnnouncement";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();
            try
            {
                bool fileStage = false;

                if (file.Length > 0)
                {
                    var mMaxLength = 10 * 1024 * 1024;
                    var mSupportedTypes = new[]
                    {
                        "image/jpeg",
                        "image/png"
                    };

                    string mAnnFolderPath = Path.Combine("."
                                                        , _apiAppConfig.RootFolderPath ?? "assets"
                                                        , _apiAppConfig.AnnBoardFolderPath ?? "AnnBoardImages");

                    if (!Directory.Exists(mAnnFolderPath))
                    {
                        Directory.CreateDirectory(mAnnFolderPath);
                    }
                    string mGuidIamgeName = _apiAppConfig.AnnBoardGuid ?? @"d133fef2-4a53-43a8-874c-d66c9e7d7acb";

                    if (file.Length > mMaxLength)
                    {
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"修改首頁公告照片失敗，檔案太大";

                        return mResult;
                    }

                    if (!mSupportedTypes.Contains(file.ContentType))
                    {
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"修改公告照片失敗，檔案類型錯誤，只接受jpeg、png";

                        return mResult;
                    }

                    //定義圖片名稱
                    string mGuid = Guid.NewGuid().ToString();
                    string mFileName = $@"{mGuidIamgeName}_{mGuid}.{file.FileName.Split(".").Last()}";

                    //刪除該同檔名的檔案，因為可能JPEG、PNG
                    string[] mFilesInfo = Directory.GetFiles(mAnnFolderPath);
                    foreach (var itemFile in mFilesInfo)
                    {
                        if (Path.GetFileName(itemFile).Contains($@"{mGuidIamgeName}_{mGuid}"))
                        {
                            System.IO.File.Delete(itemFile);
                        }
                    }

                    //開啟要修改的圖片
                    using (var stream = new FileStream(Path.Combine(mAnnFolderPath, mFileName), FileMode.Create))
                    {
                        //這個方法會直接蓋掉原本圖片
                        file.CopyTo(stream);
                    }

                    fileStage = true;

                    reqModel.ab_image_path = mAnnFolderPath;
                    reqModel.ab_image_name = mFileName;
                }
                else
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"新增公告照片失敗，未收到照片";

                    return mResult;
                }

                mResult = _httpClientService.ProcessQuery<bool>("SystemMgmt/AnnouncementService/PostAnnouncement", reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增公告失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        [HttpPut]
        public ApiResultModel<bool> PutAnnouncement([FromForm] PutAnnouncementReqModel reqModel, IFormFile? file)
        {
            string mActionLog = "PutAnnouncement";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();
            try
            {
                bool fileStage = false;

                if (file != null && file.Length > 0)
                {
                    var mMaxLength = 10 * 1024 * 1024;
                    var mSupportedTypes = new[]
                    {
                        "image/jpeg",
                        "image/png"
                    };

                    string mAnnFolderPath = Path.Combine("."
                                                        , _apiAppConfig.RootFolderPath ?? "assets"
                                                        , _apiAppConfig.AnnBoardFolderPath ?? "AnnBoardImages");

                    if (!Directory.Exists(mAnnFolderPath))
                    {
                        Directory.CreateDirectory(mAnnFolderPath);
                    }
                    string mGuidIamgeName = _apiAppConfig.AnnBoardGuid ?? @"d133fef2-4a53-43a8-874c-d66c9e7d7acb";

                    if (file.Length > mMaxLength)
                    {
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"修改首頁公告照片失敗，檔案太大";

                        return mResult;
                    }

                    if (!mSupportedTypes.Contains(file.ContentType))
                    {
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"修改公告照片失敗，檔案類型錯誤，只接受jpeg、png";

                        return mResult;
                    }

                    //定義圖片名稱
                    string mFileName = $@"{mGuidIamgeName}_{reqModel.ab_id}.{file.FileName.Split(".").Last()}";

                    //刪除該同檔名的檔案，因為可能JPEG、PNG
                    string[] mFilesInfo = Directory.GetFiles(mAnnFolderPath);
                    foreach (var itemFile in mFilesInfo)
                    {
                        if (Path.GetFileName(itemFile).Contains($@"{mGuidIamgeName}_{reqModel.ab_id}"))
                        {
                            System.IO.File.Delete(itemFile);
                        }
                    }

                    //開啟要修改的圖片
                    using (var stream = new FileStream(Path.Combine(mAnnFolderPath, mFileName), FileMode.Create))
                    {
                        //這個方法會直接蓋掉原本圖片
                        file.CopyTo(stream);
                    }

                    fileStage = true;

                    reqModel.ab_image_path = mAnnFolderPath;
                    reqModel.ab_image_name = mFileName;
                }
                mResult = _httpClientService.ProcessQuery<bool>("SystemMgmt/AnnouncementService/PutAnnouncement", reqModel);

                if (fileStage) mResult.returnMsg += " 圖片修改成功";
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增公告失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 用來取得url圖片
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        [HttpGet("GetAnnBoardImage/{imageName}")]
        public async Task<IActionResult> GetAnnBoardImage(string imageName)
        {
            string mActionLog = "GetAnnBoardImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            try
            {
                var imagePath = Path.Combine("."
                                            , _apiAppConfig.RootFolderPath ?? "assets"
                                            , _apiAppConfig.AnnBoardFolderPath ?? "AnnBoardImages"
                                            , imageName);
                // 檢查檔案是否存在
                if (System.IO.File.Exists(imagePath))
                {
                    // 使用 FileStream 讀取檔案
                    using (FileStream imageFileStream = System.IO.File.OpenRead(imagePath))
                    {
                        // 確定檔案型別
                        FileInfo fileInfo = new FileInfo(imagePath);
                        string contentType = GetContentType(fileInfo.Extension); // 自定義方法來獲取內容型別

                        // 使用 MemoryStream 來讀取檔案至記憶體中的非同步作業
                        var memoryStream = new MemoryStream();
                        await imageFileStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0; // 重置流的位置

                        return File(memoryStream, contentType);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($@"{pBaseLog} {mActionLog} ，{ex.Message}");
                // 對於例外，您可以記錄錯誤或返回更具體的錯誤訊息
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private string GetContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                // 可以額外新增更多的格式
                default:
                    return "application/octet-stream";
            }
        }

        #region (圖片修改另一種思路)後台-修改公告圖片(不會進到服務裡面，直接在該位置依據所需位置儲存)
        /// <summary>
        /// (圖片修改另一種思路)後台-修改公告圖片(不會進到服務裡面，直接在該位置依據所需位置儲存)
        /// </summary>
        /// <returns></returns>
        //[HttpPut]
        //public ApiResultModel<bool> PutAnnouncement([FromForm] IFormFile[] fileArray)
        //{
        //    string mActionLog = "PutAnnouncement";
        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

        //    ApiResultModel<bool> mResult = new ApiResultModel<bool>();
        //    try
        //    {
        //        var mMaxLength = 5 * 1024 * 1024;
        //        var mSupportedTypes = new[]
        //        {
        //            "image/jpeg",
        //            "image/png"
        //        };

        //        string mAnnFolderPath = Path.Combine("..", _apiAppConfig.AnnFolderPath ?? "AnnImages");

        //        if (!Directory.Exists(mAnnFolderPath))
        //        {
        //            Directory.CreateDirectory(mAnnFolderPath);
        //        }
        //        string mGuidIamgeName = @"8e394045-1298-4610-9a2c-4bc9b08bc71a";

        //        for (int i = 0; i < 3; i++)
        //        {
        //            if (!System.IO.File.Exists(Path.Combine(mAnnFolderPath, fileArray[i].FileName)))
        //            {
        //                if (fileArray[i].Length == 0)
        //                {
        //                    mResult.Result = false;
        //                    mResult.returnStatus = 999;
        //                    mResult.returnMsg = $@"修改公告照片失敗，檔案未收到";

        //                    return mResult;
        //                }

        //                if (fileArray[i].Length > mMaxLength)
        //                {
        //                    mResult.Result = false;
        //                    mResult.returnStatus = 999;
        //                    mResult.returnMsg = $@"修改公告照片失敗，檔案太大";

        //                    return mResult;
        //                }

        //                if (!mSupportedTypes.Contains(fileArray[i].ContentType))
        //                {
        //                    mResult.Result = false;
        //                    mResult.returnStatus = 999;
        //                    mResult.returnMsg = $@"修改公告照片失敗，檔案類型錯誤，只接受jpeg、png";

        //                    return mResult;
        //                }

        //                // 檔案不存在表示照片修改過，則將新的照片覆蓋到舊的
        //                using (var stream = new FileStream(Path.Combine(mAnnFolderPath, $@"{mGuidIamgeName}_0{i}.{fileArray[i].FileName.Split(".").Last()}"), FileMode.Create))
        //                {
        //                    fileArray[i].CopyTo(stream);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mResult.Result = false;
        //        mResult.returnStatus = 999;
        //        mResult.returnMsg = $@"修改公告圖片失敗 {ex.Message}";
        //    }

        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

        //    return mResult;
        //}
        #endregion (圖片修改另一種思路)後台-修改公告圖片(不會進到服務裡面，直接在該位置依據所需位置儲存)
    }
}
