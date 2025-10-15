using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndApi.Controllers.MemberMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "MembersController";

        public MembersController(
            ILogger<MembersController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 會員-取得下屬組織
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSubordinateOrg/{mm_id}")]
        [SwaggerOperation(Summary = "會員-取得下屬組織")]
        public ApiResultModel<List<GetSubordinateOrgRespModel>> GetSubordinateOrg(long mm_id)
        {
            string mActionLog = "GetSubordinateOrg";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<List<GetSubordinateOrgRespModel>> mResult = new ApiResultModel<List<GetSubordinateOrgRespModel>>();

            if (mm_id > 0)
            {
                mResult = _httpClientService.ProcessQuery<List<GetSubordinateOrgRespModel>>("MemberMgmt/MembersService/GetSubordinateOrg", mm_id);
            }
            else
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "取得下屬組織，mm_id必須輸入正確";
            }
            

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            return mResult;
        }

        /// <summary>
        /// 後台-取得會員列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>string mm_account, string mm_name, long mm_introducer, long mm_invite_code, int pageSize, int pageIndex, int preGetIndex
        [HttpGet]
        [Route("QueryMember")]
        public ApiResultModel<BaseGridRespModel<QueryMemberRespModel>> QueryMember([FromQuery] QueryMemberReqModel reqModel)
        {
            string mActionLog = "QueryMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<BaseGridRespModel<QueryMemberRespModel>>("MemberMgmt/MembersService/QueryMember", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 會員、後台-取得該會員資料
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{mm_id}")]
        [SwaggerOperation(Summary = "會員-取得該會員資料")]
        public ApiResultModel<GetMemberRespModel> GetMember(long mm_id)
        {
            string mActionLog = "GetMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<GetMemberRespModel> m_result = new ApiResultModel<GetMemberRespModel>();

            m_result = _httpClientService.ProcessQuery<GetMemberRespModel>("MemberMgmt/MembersService/GetMember", mm_id);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            return m_result;
        }

        /// <summary>
        /// 會員、後台-更新會員資料
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Member")]
        [SwaggerOperation(Summary = "會員-更新會員資料")]
        public ApiResultModel<bool> PutMember([FromBody] PutMemberReqModel reqModel)
        {
            string mActionLog = "PutMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<bool>("MemberMgmt/MembersService/PutMember", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }

        /// <summary>
        /// 會員-更新密碼
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Password")]
        [SwaggerOperation(Summary = "會員-更新密碼")]
        public ApiResultModel<bool> UpdatePwd([FromBody] UpdatePwdReqModel reqModel)
        {
            string mActionLog = "UpdatePwd";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            var mResult = _httpClientService.ProcessQuery<bool>("MemberMgmt/MembersService/UpdatePwd", reqModel);

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return mResult;
        }
    }
}
