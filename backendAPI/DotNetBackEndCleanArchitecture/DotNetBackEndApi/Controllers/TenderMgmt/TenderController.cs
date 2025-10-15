using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using DomainEntityDTO.Entity.TenderMgmt.Tender;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndApi.Controllers.TenderMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenderController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "TenderController";

        public TenderController(
            ILogger<TenderController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 會員-下標
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Bidding")]
        [SwaggerOperation(Summary = "會員-下標")]
        public ApiResultModel<bool> Bidding([FromBody] BiddingReqModel reqModel)
        {
            string mActionLog = "Bidding";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            mResult = _httpClientService.ProcessQuery<bool>("TenderMgmt/TenderService/Bidding", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-得標
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("WinningTender")]
        public ApiResultModel<bool> WinningTender([FromBody] WinningTenderReqModel reqModel)
        {
            string mActionLog = "WinningTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            mResult = _httpClientService.ProcessQuery<bool>("TenderMgmt/TenderService/WinningTender", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-取得標案列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>string mm_account, string mm_name, long mm_introducer, long mm_invite_code, int pageSize, int pageIndex, int preGetIndex
        [HttpGet]
        [Route("QueryTenderAdmin")]
        public ApiResultModel<BaseGridRespModel<QueryTenderRespModel>> QueryTenderAdmin([FromQuery] QueryTenderAdminReqModel reqModel)
        {
            string mActionLog = "QueryTenderAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryTenderRespModel>>("TenderMgmt/TenderService/QueryTenderAdmin", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 會員-取得進行中標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>string mm_account, string mm_name, long mm_introducer, long mm_invite_code, int pageSize, int pageIndex, int preGetIndex
        [HttpGet]
        [Route("QueryTenderInProgressUser")]
        [SwaggerOperation(Summary = "會員-取得進行中標案")]
        public ApiResultModel<BaseGridRespModel<QueryTenderInProgressRespModel>> QueryTenderInProgressUser([FromQuery] QueryTenderInProgressReqModel reqModel)
        {
            string mActionLog = "QueryTenderInProgressUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryTenderInProgressRespModel>>("TenderMgmt/TenderService/QueryTenderInProgressUser", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 會員-取得所有標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>string mm_account, string mm_name, long mm_introducer, long mm_invite_code, int pageSize, int pageIndex, int preGetIndex
        [HttpGet]
        [Route("QueryTenderAllUser")]
        [SwaggerOperation(Summary = "會員-取得所有標案")]
        public ApiResultModel<BaseGridRespModel<QueryTenderAllUserRespModel>> QueryTenderAllUser([FromQuery] QueryTenderInProgressReqModel reqModel)
        {
            string mActionLog = "QueryTenderAllUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryTenderAllUserRespModel>>("TenderMgmt/TenderService/QueryTenderAllUser", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 會員-取得已參與標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>string mm_account, string mm_name, long mm_introducer, long mm_invite_code, int pageSize, int pageIndex, int preGetIndex
        [HttpGet]
        [Route("QueryTenderParticipatedUser")]
        [SwaggerOperation(Summary = "會員-取得已參與標案")]
        public ApiResultModel<BaseGridRespModel<QueryTenderParticipatedUserRespModel>> QueryTenderParticipatedUser([FromQuery] QueryTenderInProgressReqModel reqModel)
        {
            string mActionLog = "QueryTenderParticipatedUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryTenderParticipatedUserRespModel>>("TenderMgmt/TenderService/QueryTenderParticipatedUser", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 會員-取得標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation(Summary = "會員-取得標案")]
        public ApiResultModel<List<GetTenderRespModel>> GetTender([FromQuery] GetTenderReqModel reqModel)
        {
            string mActionLog = "GetTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            
            ApiResultModel<List<GetTenderRespModel>> mResult = new();
            mResult = _httpClientService.ProcessQuery<List<GetTenderRespModel>>("TenderMgmt/TenderService/GetTender", reqModel);

            _logger.LogInformation("ROSCAController GetTenderList END");
            return mResult;
        }

        /// <summary>
        /// 會員-取得參與紀錄
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetParticipationRecord")]
        [SwaggerOperation(Summary = "會員-取得參與紀錄")]
        public ApiResultModel<GetParticipationRecordRespModel> GetParticipationRecord([FromQuery] GetParticipationRecordReqModel reqModel)
        {
            string mActionLog = "GetParticipationRecord";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<GetParticipationRecordRespModel> mResult = new();
            mResult = _httpClientService.ProcessQuery<GetParticipationRecordRespModel>("TenderMgmt/TenderService/GetParticipationRecord", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");
            return mResult;
        }

        /// <summary>
        /// 會員-取得標案紀錄
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTenderRecord")]
        [SwaggerOperation(Summary = "會員-取得標案紀錄")]
        public ApiResultModel<GetTenderRecordRespModel> GetTenderRecord([FromQuery] GetTenderRecordReqModel reqModel)
        {
            string mActionLog = "GetTenderRecord";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<GetTenderRecordRespModel> mResult = new();
            mResult = _httpClientService.ProcessQuery<GetTenderRecordRespModel>("TenderMgmt/TenderService/GetTenderRecord", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");
            return mResult;
        }

        /// <summary>
        /// 後台-新增標案(初始化)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResultModel<bool> PostTender()
        {
            string mActionLog = "PostTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();

            mResult = _httpClientService.ProcessQuery<bool>("TenderMgmt/TenderService/PostTender", null);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
