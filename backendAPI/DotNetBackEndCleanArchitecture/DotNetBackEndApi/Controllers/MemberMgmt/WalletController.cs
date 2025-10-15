using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using DomainEntityDTO.Entity.MemberMgmt.Wallet;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndApi.Controllers.MemberMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "WalletController";

        public WalletController(
            ILogger<WalletController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台-取得會員錢包列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryWallet")]
        public ApiResultModel<BaseGridRespModel<QueryWalletRespModel>> QueryWallet([FromQuery] QueryWalletReqModel reqModel)
        {
            string mActionLog = "QueryWallet";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryWalletRespModel>>("MemberMgmt/WalletService/QueryWallet", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 後台-取得該會員錢包資料
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{mm_id}")]
        [SwaggerOperation(Summary = "會員-取得該會員錢包資料")]
        public ApiResultModel<GetWalletRespModel> GetWallet(long mm_id)
        {
            string mActionLog = "GetWallet";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<GetWalletRespModel> m_result = new ApiResultModel<GetWalletRespModel>();

            m_result = _httpClientService.ProcessQuery<GetWalletRespModel>("MemberMgmt/WalletService/GetWallet", mm_id);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            return m_result;
        }
    }
}
