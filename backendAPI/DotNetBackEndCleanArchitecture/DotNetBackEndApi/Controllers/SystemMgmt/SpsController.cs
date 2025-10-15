using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.SystemMgmt.SystemParameterSetting;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndApi.Controllers.SystemMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "SpsController";

        public SpsController(
            ILogger<SpsController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台-取得系統參數
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QuerySps")]
        public ApiResultModel<BaseGridRespModel<QuerySpsRespModel>> QuerySps([FromQuery] QuerySpsReqModel reqModel)
        {
            string mActionLog = "QuerySps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QuerySpsRespModel>>("SystemMgmt/SpsService/QuerySps", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 後台-新增參數
        /// </summary>
        /// <param name="reqModel"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        //public ApiResultModel<bool> PostSps([FromForm] PostSpsReqModel reqModel, IFormFile? file)
        public ApiResultModel<bool> PostSps([FromBody] PostSpsReqModel reqModel)
        {
            string mActionLog = "PostSps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> m_result = new ApiResultModel<bool>();

            //if (file != null && file.Length > 0)
            //{
            //    using (MemoryStream memoryStream = new MemoryStream())
            //    {
            //        // 將 IFormFile 轉換為 byte[]
            //        file.CopyTo(memoryStream);
            //        reqModel.sps_picture = memoryStream.ToArray();
            //    }
            //}
            m_result = _httpClientService.ProcessQuery<bool>("SystemMgmt/SpsService/PostSps", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");
            return m_result;
        }

        /// <summary>
        /// 後台-更新參數資料
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPut]
        public ApiResultModel<bool> PutSps([FromBody] PutSpsReqModel reqModel)
        {
            string mActionLog = "PutSps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<bool>("SystemMgmt/SpsService/PutSps", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 後台-更新Kyc圖片
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("PutKycImage")]
        public ApiResultModel<bool> PutKycImage([FromForm] IFormFile FileKyc)
        {
            string mActionLog = "PutKycImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new();

            try
            {
                byte[] mKycFileBuffer = new byte[1024];
                if (FileKyc != null && FileKyc.Length > 0)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        // 將 IFormFile 轉換為 byte[]
                        FileKyc.CopyTo(memoryStream);
                        mKycFileBuffer = memoryStream.ToArray();
                    }
                }

                mResult = _httpClientService.ProcessQuery<bool>("SystemMgmt/SpsService/PutKycImage", mKycFileBuffer);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"修改KYC圖片失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
