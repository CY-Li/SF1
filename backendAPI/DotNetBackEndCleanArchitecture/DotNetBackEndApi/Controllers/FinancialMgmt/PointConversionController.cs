using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyWithdraw;
using DomainEntityDTO.Entity.FinancialMgmt.PointConversion;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetBackEndApi.Controllers.FinancialMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointConversionController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "PointConversionController";

        public PointConversionController(
            ILogger<PointConversionController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 會員-取得交易資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{mm_id}")]
        [SwaggerOperation(Summary = "會員-取得交易資料")]
        public ActionResult<ApiResultModel<GettransactionDataRespModel>> GetTransactionData(long mm_id)
        {
            string mActionLog = "GetTransactionData";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<GettransactionDataRespModel> m_result = new ApiResultModel<GettransactionDataRespModel>();

            m_result = _httpClientService.ProcessQuery<GettransactionDataRespModel>("FinancialMgmt/PointConversionService/GetTransactionData", mm_id);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            return m_result;
        }


        /// <summary>
        /// 會員-轉換點數
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation(Summary = "會員-轉換點數")]
        public ApiResultModel<bool> PostPointConversion(PostPointConversionReqModel reqModel)
        {
            string mActionLog = "PostPointConversion";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<bool>("FinancialMgmt/PointConversionService/PostPointConversion", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-贈送點數
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GiftPoint")]
        [SwaggerOperation(Summary = "會員-贈送點數")]
        public ApiResultModel<bool> PsotGiftPoint(PostGiftPointReqModel reqModel)
        {
            string mActionLog = "PsotGiftPoint";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<bool>("FinancialMgmt/PointConversionService/PostGiftPoint", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
