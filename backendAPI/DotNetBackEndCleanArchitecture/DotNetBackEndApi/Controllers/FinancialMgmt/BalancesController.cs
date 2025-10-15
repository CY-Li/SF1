using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.FinancialMgmt.MemberBalance;
using DotNetBackEndApi.Controllers.MemberMgmt;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndApi.Controllers.FinancialMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalancesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "BalancesController";

        public BalancesController(
            ILogger<BalancesController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台-取得帳務紀錄列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryMemberBalanceAdmin")]
        [SwaggerOperation(Summary = "後台-取得帳務紀錄列表")]
        public ApiResultModel<BaseGridRespModel<QueryMemberBalanceAdminRespModel>> QueryMemberBalanceAdmin([FromQuery] QueryMemberBalanceAdminReqModel reqModel)
        {
            string mActionLog = "QueryMemberBalanceAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var m_result = _httpClientService.ProcessQuery<BaseGridRespModel<QueryMemberBalanceAdminRespModel>>("FinancialMgmt/BalancesService/QueryMemberBalanceAdmin", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return m_result;
        }

        /// <summary>
        /// 會員、後台-取得帳務紀錄列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryMemberBalanceUser")]
        [SwaggerOperation(Summary = "會員-取得帳務紀錄列表")]
        public ApiResultModel<BaseGridRespModel<QueryMemberBalanceUserRespModel>> QueryMemberBalanceUser([FromQuery] QueryMemberBalanceUserReqModel reqModel)
        {
            string mActionLog = "QueryMemberBalanceUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var m_result = _httpClientService.ProcessQuery<BaseGridRespModel<QueryMemberBalanceUserRespModel>>("FinancialMgmt/BalancesService/QueryMemberBalanceUser", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return m_result;
        }
    }
}
