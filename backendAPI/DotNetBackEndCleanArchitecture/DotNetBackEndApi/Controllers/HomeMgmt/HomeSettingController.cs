using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.HomeMgmt;
using DomainEntityDTO.Entity.SystemMgmt.Announcement;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetBackEndApi.Controllers.HomeMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeSettingController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;

        private readonly string pBaseLog = "HomeSettingController";

        public HomeSettingController(
            ILogger<HomeSettingController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _apiAppConfig = apiAppConfig.Value;
        }

        /// <summary>
        /// 後台、會員-取得公告圖片
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomeVideo")]
        [SwaggerOperation(Summary = "會員-取得公告影片")]
        public ApiResultModel<GetHomeVideoRespModel> GetHomeVideo()
        {
            string mActionLog = "GetHomeVideo";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ApiResultModel<GetHomeVideoRespModel> mResult = new ApiResultModel<GetHomeVideoRespModel>();

            try
            {
                mResult = _httpClientService.ProcessQuery<GetHomeVideoRespModel>("HomeMgmt/HomeSettingService/GetHomeVideo", null);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得首頁影片失敗 {ex.Message}";
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
