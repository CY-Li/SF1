using AppAbstraction.SystemMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.SystemMgmt.Announcement;
using DomainEntityDTO.Entity.SystemMgmt.Announcement;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBackEndService.Controllers.SystemMgmt
{
    [Route("api/SystemMgmt/[controller]")]
    [ApiController]
    public class AnnouncementServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IAnnouncementService _announcementSVC;

        private readonly string pBaseLog = "AnnouncementServiceController";
        public AnnouncementServiceController(
                ILogger<AnnouncementServiceController> logger,
                IMapper mapper,
                IAnnouncementService announcementSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _announcementSVC = announcementSVC;
        }

        /// <summary>
        /// 後台-取得公告圖片儲存位置
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //[Route("GetAnnouncementPath")]
        //public ActionResult<ServiceResultDTO<GetAnnouncementPathRespModel>> GetAnnouncementPath()
        //{
        //    string mActionLog = "GetAnnouncementPath";
        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

        //    ServiceResultDTO<GetAnnouncementPathRespModel> mResult = new ServiceResultDTO<GetAnnouncementPathRespModel>();

        //    try
        //    {
        //        //mResult = _announcementSVC.IAnnouncementRepo.GetAnnouncementPath();
        //    }
        //    catch (Exception ex)
        //    {
        //        mResult.returnStatus = 999;
        //        mResult.returnMsg = $@"取得儲值資料、{ex.Message}";

        //        _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
        //    }

        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

        //    return Ok(mResult);
        //}

        /// <summary>
        /// 後台-取得公告列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryAnnBoardAdmin")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardAdminRespModel>>> QueryAnnBoardAdmin(QueryAnnBoardAdminDTO reqModel)
        {
            string mActionLog = "QueryAnnBoardAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardAdminRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardAdminRespModel>>();

            try
            {
                mResult = _announcementSVC.IAnnouncementRepo.QueryAnnBoardAdmin(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 0;
                mResult.returnMsg = ex.Message;

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-取得公告列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryAnnBoardUser")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardUserRespModel>>> QueryAnnBoardUser(QueryAnnBoardUserDTO reqModel)
        {
            string mActionLog = "QueryAnnBoardUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardUserRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardUserRespModel>>();

            try
            {
                mResult = _announcementSVC.IAnnouncementRepo.QueryAnnBoardUser(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 0;
                mResult.returnMsg = ex.Message;

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 後台-新增公告
        /// </summary>
        /// <param name="reqModel"></param>
        [HttpPost]
        [Route("PostAnnouncement")]
        public ActionResult<ServiceResultDTO<bool>> PostAnnouncement([FromBody] PostAnnouncementDTO reqModel)
        {
            string mActionLog = "PostAnnouncement";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();

            try
            {
                reqModel.update_datetime = GetTimeZoneInfo.process();

                mResult = _announcementSVC.IAnnouncementRepo.PostAnnouncement(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增首頁公告失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-新增公告
        /// </summary>
        /// <param name="reqModel"></param>
        [HttpPost]
        [Route("PutAnnouncement")]
        public ActionResult<ServiceResultDTO<bool>> PutAnnouncement([FromBody] PutAnnouncementDTO reqModel)
        {
            string mActionLog = "PutAnnouncement";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();

            try
            {
                reqModel.update_datetime = GetTimeZoneInfo.process();

                mResult = _announcementSVC.IAnnouncementRepo.PutAnnouncement(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"更新首頁公告失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
