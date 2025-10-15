using DomainEntityDTO.Common;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetBackEndApi.Controllers.HomeMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomePictureController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "HomePictureController";

        public HomePictureController(
            ILogger<HomePictureController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台、會員-取得首頁輪播圖片
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAnnImages")]
        [SwaggerOperation(Summary = "會員-取得首頁輪播圖片")]
        public ApiResultModel<List<string>> GetAnnImages()
        {
            string mActionLog = "GetAnnImages";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<List<string>> mResult = new ApiResultModel<List<string>>();

            try
            {
                string mFolderPath = Path.Combine("."
                                                  , _apiAppConfig.RootFolderPath ?? "assets"
                                                  , _apiAppConfig.AnnFolderPath ?? "AnnImages");

                List<string> mAnnPath = new List<string>();

                DirectoryInfo mDirectoryInfo = new DirectoryInfo(mFolderPath);

                // 獲取所有檔案的資訊
                FileInfo[] files = mDirectoryInfo.GetFiles();
                foreach (var item in files)
                {
                    string? mFileUrl = Url.Action("GetAnnImage", "HomePicture", new { imageName = item.Name }, Request.Scheme);

                    if (!string.IsNullOrEmpty(mFileUrl))
                    {
                        mAnnPath.Add(mFileUrl);
                    }
                }
                mAnnPath.Sort();

                mResult.Result = mAnnPath;
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得首頁輪播圖片成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得首頁輪播圖片失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台、會員-取得首頁花絮圖片
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBlooperImages")]
        [SwaggerOperation(Summary = "會員-取得首頁花絮圖片")]
        public ApiResultModel<List<string>> GetBlooperImages()
        {
            string mActionLog = "GetBlooperImages";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<List<string>> mResult = new ApiResultModel<List<string>>();

            try
            {
                string mFolderPath = Path.Combine("."
                                                  , _apiAppConfig.RootFolderPath ?? "assets"
                                                  , _apiAppConfig.BlooperFolderPath ?? "BlooperImages");

                List<string> mAnnPath = new List<string>();

                DirectoryInfo mDirectoryInfo = new DirectoryInfo(mFolderPath);

                // 獲取所有檔案的資訊
                FileInfo[] files = mDirectoryInfo.GetFiles();
                foreach (var item in files)
                {
                    string? mFileUrl = Url.Action("GetBlooperImage", "HomePicture", new { imageName = item.Name }, Request.Scheme);

                    if (!string.IsNullOrEmpty(mFileUrl))
                    {
                        mAnnPath.Add(mFileUrl);
                    }
                }
                mAnnPath.Sort();

                mResult.Result = mAnnPath;
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得首頁花絮圖片成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得首頁花絮圖片失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 用來取得url圖片
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        [HttpGet("GetAnnImage/{imageName}")]
        public async Task<IActionResult> GetAnnImage(string imageName)
        {
            string mActionLog = "GetAnnImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            try
            {
                string imagePath = Path.Combine("."
                                                  , _apiAppConfig.RootFolderPath ?? "assets"
                                                  , _apiAppConfig.AnnFolderPath ?? "AnnImages"
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

        /// <summary>
        /// 用來取得url圖片
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        [HttpGet("GetBlooperImage/{imageName}")]
        public async Task<IActionResult> GetBlooperImage(string imageName)
        {
            string mActionLog = "GetBlooperImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            try
            {
                string imagePath = Path.Combine("."
                                                  , _apiAppConfig.RootFolderPath ?? "assets"
                                                  , _apiAppConfig.BlooperFolderPath ?? "BlooperImages"
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

        /// <summary>
        /// GetAnnImage取圖片時會透過這個來篩選圖片類型
        /// 自定義方法來獲取內容型別
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 後台-修改首頁輪播圖片(不會進到服務裡面，直接在該位置依據所需位置儲存)
        /// </summary>
        /// <param name="fileArray"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("PutAnnFrontPage")]
        public ApiResultModel<bool> PutAnnFrontPage([FromForm] IFormCollection fileArray)
        {
            string mActionLog = "PutAnnFrontPage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            try
            {
                if (fileArray.Files.Count > 0)
                {

                    var mMaxLength = 5 * 1024 * 1024;
                    var mSupportedTypes = new[]
                    {
                        "image/jpeg",
                        "image/png"
                    };

                    string mFolderPath = Path.Combine("."
                                                      , _apiAppConfig.RootFolderPath ?? "assets"
                                                      , _apiAppConfig.AnnFolderPath ?? "AnnImages");

                    if (!Directory.Exists(mFolderPath))
                    {
                        Directory.CreateDirectory(mFolderPath);
                    }
                    string mGuidIamgeName = @"a39d806f-7463-4fd4-947f-7f81295b4213";

                    foreach (var item in fileArray.Files)
                    {
                        //判斷圖片是否為預設圖片名稱，不是代表要改圖片
                        if (!System.IO.File.Exists(Path.Combine(mFolderPath, item.FileName)))
                        {
                            if (item.Length == 0)
                            {
                                mResult.Result = false;
                                mResult.returnStatus = 999;
                                mResult.returnMsg = $@"修改首頁輪播照片失敗，檔案未收到";

                                return mResult;
                            }

                            if (item.Length > mMaxLength)
                            {
                                mResult.Result = false;
                                mResult.returnStatus = 999;
                                mResult.returnMsg = $@"修改首頁輪播照片失敗，檔案太大";

                                return mResult;
                            }

                            if (!mSupportedTypes.Contains(item.ContentType))
                            {
                                mResult.Result = false;
                                mResult.returnStatus = 999;
                                mResult.returnMsg = $@"修改首頁輪播照片失敗，檔案類型錯誤，只接受jpeg、png";

                                return mResult;
                            }
                            //取得修改第幾張圖
                            string mFileIndex = item.Name.Split('[')[1][0].ToString();
                            //重定義圖片名稱
                            string mFileName = $@"{mGuidIamgeName}_0{mFileIndex}.{item.FileName.Split(".").Last()}";

                            //刪除該同檔名的檔案，因為可能JPEG、PNG
                            string[] mFilesInfo = Directory.GetFiles(mFolderPath);
                            foreach (var itemFile in mFilesInfo)
                            {
                                if (Path.GetFileName(itemFile).Contains($@"{mGuidIamgeName}_0{mFileIndex}"))
                                {
                                    System.IO.File.Delete(itemFile);
                                }
                            }

                            //開啟要修改的圖片
                            using (var stream = new FileStream(Path.Combine(mFolderPath, mFileName), FileMode.Create))
                            {
                                //這個方法會直接蓋掉原本圖片
                                item.CopyTo(stream);
                            }

                            mResult.Result = true;
                            mResult.returnStatus = 1;
                            mResult.returnMsg = $@"修改首頁輪播照片成功";
                        }
                    }

                }
                else
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"修改首頁輪播圖片失敗，未傳送圖片";
                }
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"修改首頁輪播圖片失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-修改花絮圖片(不會進到服務裡面，直接在該位置依據所需位置儲存)
        /// </summary>
        /// <param name="fileArray"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("PutBlooperFrontPage")]
        public ApiResultModel<bool> PutBlooperFrontPage([FromForm] IFormCollection fileArray)
        {
            string mActionLog = "PutBlooperFrontPage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            try
            {
                if (fileArray.Files.Count > 0)
                {

                    var mMaxLength = 5 * 1024 * 1024;
                    var mSupportedTypes = new[]
                    {
                        "image/jpeg",
                        "image/png"
                    };

                    string mFolderPath = Path.Combine("."
                                                      , _apiAppConfig.RootFolderPath ?? "assets"
                                                      , _apiAppConfig.BlooperFolderPath ?? "BlooperImages");

                    if (!Directory.Exists(mFolderPath))
                    {
                        Directory.CreateDirectory(mFolderPath);
                    }
                    string mGuidIamgeName = @"3dc70f6d-d41a-44a6-a460-aa16b37662fe";

                    foreach (var item in fileArray.Files)
                    {
                        //判斷圖片是否為預設圖片名稱，不是代表要改圖片
                        if (!System.IO.File.Exists(Path.Combine(mFolderPath, item.FileName)))
                        {
                            if (item.Length == 0)
                            {
                                mResult.Result = false;
                                mResult.returnStatus = 999;
                                mResult.returnMsg = $@"修改首頁花絮照片失敗，檔案未收到";

                                return mResult;
                            }

                            if (item.Length > mMaxLength)
                            {
                                mResult.Result = false;
                                mResult.returnStatus = 999;
                                mResult.returnMsg = $@"修改首頁花絮照片失敗，檔案太大";

                                return mResult;
                            }

                            if (!mSupportedTypes.Contains(item.ContentType))
                            {
                                mResult.Result = false;
                                mResult.returnStatus = 999;
                                mResult.returnMsg = $@"修改公告照片失敗，檔案類型錯誤，只接受jpeg、png";

                                return mResult;
                            }
                            //取得修改第幾張圖
                            string mFileIndex = item.Name.Split('[')[1][0].ToString();
                            //重定義圖片名稱
                            string mFileName = $@"{mGuidIamgeName}_0{mFileIndex}.{item.FileName.Split(".").Last()}";

                            //刪除該同檔名的檔案，因為可能JPEG、PNG
                            string[] mFilesInfo = Directory.GetFiles(mFolderPath);
                            foreach (var itemFile in mFilesInfo)
                            {
                                if (Path.GetFileName(itemFile).Contains($@"{mGuidIamgeName}_0{mFileIndex}"))
                                {
                                    System.IO.File.Delete(itemFile);
                                }
                            }

                            //開啟要修改的圖片
                            using (var stream = new FileStream(Path.Combine(mFolderPath, mFileName), FileMode.Create))
                            {
                                //這個方法會直接蓋掉原本圖片
                                item.CopyTo(stream);
                            }

                            mResult.Result = true;
                            mResult.returnStatus = 1;
                            mResult.returnMsg = $@"修改首頁花絮照片成功";
                        }
                    }

                }
                else
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"修改首頁花絮圖片失敗，未傳送圖片";
                }
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"修改首頁花絮圖片失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
