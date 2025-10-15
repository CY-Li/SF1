using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyWithdraw;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetBackEndApi.Controllers.FinancialMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplyWithdrawController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "ApplyWithdrawController";

        public ApplyWithdrawController(
            ILogger<ApplyWithdrawController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台-取得提領申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryApplyWithdrawAdmin")]
        public ApiResultModel<BaseGridRespModel<QueryApplyWithdrawAdminRespModel>> QueryApplyWithdrawAdmin([FromQuery] QueryApplyWithdrawAdminReqModel reqModel)
        {
            string mActionLog = "QueryApplyWithdrawAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryApplyWithdrawAdminRespModel>>("FinancialMgmt/ApplyWithdrawService/QueryApplyWithdrawAdmin", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        [HttpGet("{mId}/{id}/{imageName}")]
        public async Task<IActionResult> GetWithdrawImage(string mId, string id, string imageName)
        {
            string mActionLog = "GetWithdrawImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            try
            {
                //string mKycFolderPath = $@"{_apiAppConfig.KycFolderPath}/{}";
                var imagePath = Path.Combine(".."
                                            , _apiAppConfig.WithdrawrFolderPath ?? "WithdrawImages"
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
        /// 會員-取得提領申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryApplyWithdrawUser")]
        [SwaggerOperation(Summary = "會員-取得儲值申請列表")]
        public ApiResultModel<BaseGridRespModel<QueryApplyWithdrawUserRespModel>> QueryApplyWithdrawUser([FromQuery] QueryApplyWithdrawUserReqModel reqModel)
        {
            string mActionLog = "QueryApplyWithdrawUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryApplyWithdrawUserRespModel>>("FinancialMgmt/ApplyWithdrawService/QueryApplyWithdrawUser", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 會員-申請提領
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //[SwaggerOperation(Summary = "會員-申請提領")]
        //public ApiResultModel<bool> PostApplyWithdraw(PostApplyWithdrawReqModel reqModel)
        //{
        //    string mActionLog = "PostApplyWithdraw";
        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

        //    var mResult = _httpClientService.ProcessQuery<bool>("FinancialMgmt/ApplyWithdrawService/PostApplyWithdraw", reqModel);

        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

        //    return mResult;
        //}

        /// <summary>
        /// 會員-申請提領
        /// </summary>
        /// <returns></returns>
        [SwaggerOperation("會員-申請提領")]
        [HttpPost]
        public ApiResultModel<bool> PostApplyWithdraw([FromBody] PostApplyWithdrawReqModel reqModel)
        {
            string mActionLog = "PostApplyWithdraw";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();
            try
            {
                //var mMaxLength = 5 * 1024 * 1024;
                //var mSupportedTypes = new[]
                //{
                //    "image/jpeg",
                //    "image/png"
                //};

                //if (reqModel.aw_file_image.Length == 0)
                //{
                //    mResult.Result = false;
                //    mResult.returnStatus = 999;
                //    mResult.returnMsg = $@"儲值提領失敗，檔案未收到";

                //    return mResult;
                //}

                //if (reqModel.aw_file_image.Length > mMaxLength)
                //{
                //    mResult.Result = false;
                //    mResult.returnStatus = 999;
                //    mResult.returnMsg = $@"儲值提領失敗，檔案太大";

                //    return mResult;
                //}

                //if (!mSupportedTypes.Contains(reqModel.aw_file_image.ContentType))
                //{
                //    mResult.Result = false;
                //    mResult.returnStatus = 999;
                //    mResult.returnMsg = $@"儲值提領失敗，檔案類型錯誤，只接受jpeg、png";

                //    return mResult;
                //}

                ////string mGuid = Guid.NewGuid().ToString();
                //string mFolderPath = Path.Combine("..", _apiAppConfig.WithdrawrFolderPath ?? "WithdrawImages");
                //string mCreateFolderPath = Path.Combine(mFolderPath, reqModel.aw_mm_id.ToString(), reqModel.aw_kyc_id.ToString());
                //if (!Directory.Exists(mCreateFolderPath))
                //{
                //    Directory.CreateDirectory(mCreateFolderPath);
                //}
                //string mFileName = "";
                //string mFilePath = "";

                //mFileName = Path.Combine(@$"{reqModel.ImageGuid}_01.{reqModel.aw_file_image.FileName.Split(".").Last()}");
                //mFilePath = Path.Combine(mCreateFolderPath, mFileName);

                //reqModel.aw_file_name = mFileName;

                //using (var stream = new FileStream(mFilePath, FileMode.Create))
                //{
                //    reqModel.aw_file_image.CopyTo(stream);
                //}

                //reqModel.aw_file_image = null;

                mResult = _httpClientService.ProcessQuery<bool>("FinancialMgmt/ApplyWithdrawService/PostApplyWithdraw", reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"儲值提領失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-申請提領覆核
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public ApiResultModel<bool> PutApplyWithdraw(PutApplyWithdrawReqModel reqModel)
        {
            string mActionLog = "PutApplyWithdraw";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<bool>("FinancialMgmt/ApplyWithdrawService/PutApplyWithdraw", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
