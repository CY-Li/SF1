using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.AuthenticationMgmt.Kyc;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndApi.Controllers.FinancialMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplyDepositController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "ApplyDepositController";

        public ApplyDepositController(
            ILogger<ApplyDepositController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台-取得儲值申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryApplyDepositAdmin")]
        public ApiResultModel<BaseGridRespModel<QueryApplyDepositAdminRespModel>> QueryApplyDepositAdmin([FromQuery] QueryApplyDepositAdminReqModel reqModel)
        {
            string mActionLog = "QueryApplyDepositAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            ApiResultModel<BaseGridRespModel<QueryApplyDepositAdminRespModel>> mResult = new();
            try
            {
                mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryApplyDepositAdminRespModel>>("FinancialMgmt/ApplyDepositService/QueryApplyDepositAdmin", reqModel);

                if (mResult.Result?.GridRespResult.Count() > 0)
                {
                    mResult.Result.GridRespResult.ToList().ForEach(f =>
                    {
                        f.ad_file_name = Url.Action("GetDepositImage", "ApplyDeposit", new { mId = f.ad_mm_id, id = f.ad_kyc_id, imageName = f.ad_file_name }, Request.Scheme) ?? f.ad_file_name;
                    });
                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得儲值列表失敗 {ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog}，{ex.Message}");
            }

            return mResult;
        }

        [HttpGet("{mId}/{id}/{imageName}")]
        public async Task<IActionResult> GetDepositImage(string mId, string id, string imageName)
        {
            string mActionLog = "GetDepositImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            try
            {
                //string mKycFolderPath = $@"{_apiAppConfig.KycFolderPath}/{}";
                var imagePath = Path.Combine(".."
                                            , _apiAppConfig.DepositFolderPath ?? "DepositImages"
                                            , mId
                                            , id
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

        // 自定義方法來獲取內容型別
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
        /// 會員-取得儲值申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryApplyDepositUser")]
        [SwaggerOperation(Summary = "會員-取得儲值申請列表")]
        public ApiResultModel<BaseGridRespModel<QueryApplyDepositUserRespModel>> QueryApplyDepositUser([FromQuery] QueryApplyDepositUserReqModel reqModel)
        {
            string mActionLog = "QueryApplyDepositUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryApplyDepositUserRespModel>>("FinancialMgmt/ApplyDepositService/QueryApplyDepositUser", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 會員-取得儲值資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDepositData")]
        [SwaggerOperation(Summary = "會員-取得儲值資料")]
        public ApiResultModel<GetDepositDataRespModel> GetDepositData()
        {
            string mActionLog = "GetDepositData";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<GetDepositDataRespModel>("FinancialMgmt/ApplyDepositService/GetDepositData", null);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-申請儲值(已付款要丟KEY)
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //[SwaggerOperation(Summary = "會員-申請儲值(已付款要丟KEY)")]
        //public ApiResultModel<bool> PostApplyDeposit(PostApplyDepositReqModel reqModel)
        //{
        //    string mActionLog = "PostApplyDeposit";
        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

        //    var mResult = _httpClientService.ProcessQuery<bool>("FinancialMgmt/ApplyDepositService/PostApplyDeposit", reqModel);

        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

        //    return mResult;
        //}

        [SwaggerOperation("會員-申請儲值")]
        [HttpPost]
        public ApiResultModel<bool> PostApplyDeposit([FromForm] PostApplyDepositReqModel reqModel)
        {
            string mActionLog = "PostApplyDeposit";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();
            try
            {
                var mMaxLength = 5 * 1024 * 1024;
                var mSupportedTypes = new[]
                {
                    "image/jpeg",
                    "image/png"
                };

                if (reqModel.ad_file_image.Length == 0)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"儲值申請失敗，檔案未收到";

                    return mResult;
                }

                if (reqModel.ad_file_image.Length > mMaxLength)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"儲值申請失敗，檔案太大";

                    return mResult;
                }

                if (!mSupportedTypes.Contains(reqModel.ad_file_image.ContentType))
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"儲值申請失敗，檔案類型錯誤，只接受jpeg、png";

                    return mResult;
                }

                //string mGuid = Guid.NewGuid().ToString();
                string mFolderPath = Path.Combine("..", _apiAppConfig.DepositFolderPath ?? "DepositImages");
                string mCreateFolderPath = Path.Combine(mFolderPath, reqModel.ad_mm_id.ToString(), reqModel.ad_kyc_id.ToString());
                if (!Directory.Exists(mCreateFolderPath))
                {
                    Directory.CreateDirectory(mCreateFolderPath);
                }
                string mFileName = "";
                string mFilePath = "";

                mFileName = Path.Combine(@$"{reqModel.ImageGuid}_01.{reqModel.ad_file_image.FileName.Split(".").Last()}");
                mFilePath = Path.Combine(mCreateFolderPath, mFileName);

                reqModel.ad_file_name = mFileName;

                using (var stream = new FileStream(mFilePath, FileMode.Create))
                {
                    reqModel.ad_file_image.CopyTo(stream);
                }

                reqModel.ad_file_image = null;

                mResult = _httpClientService.ProcessQuery<bool>("FinancialMgmt/ApplyDepositService/PostApplyDeposit", reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"申請儲值失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-申請儲值覆核
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public ApiResultModel<bool> PutApplyDeposit(PutApplyDepositReqModel reqModel)
        {
            string mActionLog = "PutApplyDeposit";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<bool>("FinancialMgmt/ApplyDepositService/PutApplyDeposit", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
