using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.TenderMgmt.Tender;
using DotNetBackEndApi.Controllers.TenderMgmt;
using DotNetBackEndApi.Shared;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DotNetBackEndApi.Controllers.ScheduleMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "ScheduleController";

        public ScheduleController(
            ILogger<ScheduleController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台-下標排程
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BiddingSchedule")]
        public ApiResultModel<bool> BiddingSchedule()
        {
            string mActionLog = "BiddingSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            mResult = _httpClientService.ProcessQuery<bool>("ScheduleMgmt/ScheduleService/BiddingSchedule", null);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-清算紅利排程
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SettlementRewardSchedule")]
        public ApiResultModel<bool> SettlementRewardSchedule()
        {
            string mActionLog = "SettlementRewardSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            mResult = _httpClientService.ProcessQuery<bool>("ScheduleMgmt/ScheduleService/SettlementRewardSchedule", null);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        //[HttpGet]
        //[Route("delay")]
        //public IActionResult DelayJob()
        //{
        //    BackgroundJob.Enqueue(
        //        () =>_httpClientService.ProcessQuery<bool>("ScheduleMgmt/ScheduleService/SettlementRewardSchedule",null));

        //    return Ok();
        //}

        /// <summary>
        /// 後台-清算得標排程
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SettlementPeaceSchedule")]
        public ApiResultModel<bool> SettlementPeaceSchedule()
        {
            string mActionLog = "SettlementPeaceSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            mResult = _httpClientService.ProcessQuery<bool>("ScheduleMgmt/ScheduleService/SettlementPeaceSchedule", null);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-成組排程
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GroupDebitSchedule")]
        public ApiResultModel<bool> GroupDebitSchedule()
        {
            string mActionLog = "GroupDebitSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            mResult = _httpClientService.ProcessQuery<bool>("ScheduleMgmt/ScheduleService/GroupDebitSchedule", null);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-待付款排程
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PendingPaymentSchedule")]
        public ApiResultModel<bool> PendingPaymentSchedule()
        {
            string mActionLog = "PendingPaymentSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            mResult = _httpClientService.ProcessQuery<bool>("ScheduleMgmt/ScheduleService/PendingPaymentSchedule", null);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
