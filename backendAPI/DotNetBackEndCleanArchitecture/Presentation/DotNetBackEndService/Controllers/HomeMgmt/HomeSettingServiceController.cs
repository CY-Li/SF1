using AppAbstraction.MemberMgmt;
using AppAbstraction.SystemMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntityDTO.Entity.HomeMgmt;
using DomainEntityDTO.Entity.SystemMgmt.Announcement;
using DotNetBackEndService.Controllers.MemberMgmt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBackEndService.Controllers.HomeMgmt
{
    [Route("api/HomeMgmt/[controller]")]
    [ApiController]
    public class HomeSettingServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ISpsService _spsSVC;

        private readonly string pBaseLog = "HomeSettingService";
        public HomeSettingServiceController(
                ILogger<HomeSettingServiceController> logger,
                IMapper mapper,
                ISpsService spsSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _spsSVC = spsSVC;
        }

        /// <summary>
        /// 後台、會員-取得公告、影片(youtube URL)
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetHomeVideo")]
        public ActionResult<ServiceResultDTO<GetHomeVideoRespModel>> GetHomeVideo()
        {
            string mActionLog = "GetHomeVideo";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetHomeVideoRespModel> mResult = new ServiceResultDTO<GetHomeVideoRespModel>();

            try
            {
                mResult = _spsSVC.ISpsRepo.GetHomeVideo();
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得首頁影片資訊失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
