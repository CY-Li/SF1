using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.AuthenticationMgmt.Kyc;
using DomainEntityDTO.Entity.TenderMgmt.Tender;
using DotNetBackEndApi.Controllers.MemberMgmt;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.IO;
using System.Net.Mime;
using System.Reflection;

namespace DotNetBackEndApi.Controllers.AuthenticationMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class KycController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "KycController";

        public KycController(
            ILogger<KycController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台-取得KYC申請
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryKycAdmin")]
        public ApiResultModel<BaseGridRespModel<QueryKycAdminRespModel>> QueryKycAdmin([FromQuery] QueryKycAdminReqModel reqModel)
        {
            string mActionLog = "QueryKycAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            ApiResultModel<BaseGridRespModel<QueryKycAdminRespModel>> mResult = new();
            try
            {
                mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryKycAdminRespModel>>("AuthenticationMgmt/KycService/QueryKycAdmin", reqModel);

                if (mResult.Result?.GridRespResult.Count() > 0)
                {
                    mResult.Result.GridRespResult.ToList().ForEach(f =>
                    {
                        f.akc_front = Url.Action("GetKycImage", "Kyc", new { mId = f.akc_mm_id, id = f.akc_id, imageName = f.akc_front }, Request.Scheme) ?? f.akc_front;
                        f.akc_back = Url.Action("GetKycImage", "Kyc", new { mId = f.akc_mm_id, id = f.akc_id, imageName = f.akc_back }, Request.Scheme) ?? f.akc_front;
                    });
                }

            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得Kyc列表失敗 {ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog}，{ex.Message}");
            }

            return mResult;
        }

        [HttpGet("{mId}/{id}/{imageName}")]
        public async Task<IActionResult> GetKycImage(string mId, string id, string imageName)
        {
            string mActionLog = "GetKycImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            try
            {
                //string mKycFolderPath = $@"{_apiAppConfig.KycFolderPath}/{}";
                var imagePath = Path.Combine(".."
                                            , _apiAppConfig.KycFolderPath ?? "KycImages"
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
        /// 會員-申請KYC
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [SwaggerOperation("會員-申請KYC")]
        [HttpPost]
        public ApiResultModel<bool> PostKyc([FromForm] PostKycReqModel reqModel)
        {
            string mActionLog = "PostKyc";
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

                if (reqModel.akc_front_image.Length == 0 || reqModel.akc_back_image.Length == 0)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"Kyc申請失敗，檔案未收到";

                    return mResult;
                }

                if (reqModel.akc_front_image.Length > mMaxLength || reqModel.akc_back_image.Length > mMaxLength)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"Kyc申請失敗，檔案太大";

                    return mResult;
                }

                if (!mSupportedTypes.Contains(reqModel.akc_front_image.ContentType)
                    || !mSupportedTypes.Contains(reqModel.akc_back_image.ContentType)
                    )
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"Kyc申請失敗，檔案類型錯誤，只接受jpeg、png";

                    return mResult;
                }


                var mGetAkcId = _httpClientService.ProcessQuery<long>("AuthenticationMgmt/KycService/CheckKyc", reqModel.akc_mm_id);
                if (mGetAkcId.returnStatus != 1)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = mGetAkcId.returnMsg;

                    return mResult;
                }

                reqModel.akc_id = mGetAkcId.Result;
                //string mGuid = Guid.NewGuid().ToString();
                string mKycFolderPath = Path.Combine("..", _apiAppConfig.KycFolderPath ?? "KycImages");
                string mCreateKycFolderPath = Path.Combine(mKycFolderPath, reqModel.akc_mm_id.ToString(), reqModel.akc_id.ToString());
                if (!Directory.Exists(mCreateKycFolderPath))
                {
                    Directory.CreateDirectory(mCreateKycFolderPath);
                }
                string[] mKycFileName = new string[2];
                string[] mKycFilePath = new string[2];

                mKycFileName[0] = Path.Combine(@$"{reqModel.ImageGuid}_01.{reqModel.akc_front_image.FileName.Split(".").Last()}");
                mKycFileName[1] = Path.Combine(@$"{reqModel.ImageGuid}_02.{reqModel.akc_back_image.FileName.Split(".").Last()}");
                mKycFilePath[0] = Path.Combine(mCreateKycFolderPath, mKycFileName[0]);
                mKycFilePath[1] = Path.Combine(mCreateKycFolderPath, mKycFileName[1]);

                reqModel.akc_front = mKycFileName[0];
                reqModel.akc_back = mKycFileName[1];

                using (var stream = new FileStream(mKycFilePath[0], FileMode.Create))
                {
                    reqModel.akc_front_image.CopyTo(stream);
                }

                using (var stream = new FileStream(mKycFilePath[1], FileMode.Create))
                {
                    reqModel.akc_back_image.CopyTo(stream);
                }

                reqModel.akc_front_image = null;
                reqModel.akc_back_image = null;

                mResult = _httpClientService.ProcessQuery<bool>("AuthenticationMgmt/KycService/PostKyc", reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"申請Kyc失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-覆核KYC
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPut]
        public ApiResultModel<bool> PutKyc([FromBody] PutKycReqModel reqModel)
        {
            string mActionLog = "PutKyc";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();
            try
            {
                mResult = _httpClientService.ProcessQuery<bool>("AuthenticationMgmt/KycService/PutKyc", reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"覆核Kyc失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
