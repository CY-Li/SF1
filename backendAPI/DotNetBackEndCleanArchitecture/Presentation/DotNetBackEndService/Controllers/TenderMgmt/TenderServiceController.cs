using AppAbstraction.TenderMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.MemberMgmt.Member;
using DomainEntity.Entity.TenderMgmt.Tender;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using DomainEntityDTO.Entity.TenderMgmt.Tender;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndService.Controllers.TenderMgmt
{
    [Route("api/TenderMgmt/[controller]")]
    [ApiController]
    public class TenderServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ITenderService _tenderSVC;

        private readonly string pBaseLog = "TenderServiceController";
        public TenderServiceController(
                ILogger<TenderServiceController> logger,
                IMapper mapper,
                ITenderService tenderSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _tenderSVC = tenderSVC;
        }

        /// <summary>
        /// 會員-下標
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Bidding")]
        public ActionResult<ServiceResultDTO<bool>> Bidding([FromBody] BiddingDTO reqModel)
        {
            string mActionLog = "Bidding";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();
            try
            {
                reqModel.update_datetime = GetTimeZoneInfo.process();
                mResult = _tenderSVC.IBiddingRepo.Bidding(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "下標失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 後台-得標
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("WinningTender")]
        public ActionResult<ServiceResultDTO<bool>> WinningTender([FromBody] WinningTenderReqModel reqModel)
        {
            string mActionLog = "WinningTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();
            try
            {
                mResult = _tenderSVC.IWinningTenderRepo.WinningTender(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"得標失敗，{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 後台-取得標案列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryTenderAdmin")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryTenderRespModel>>> QueryTenderAdmin(QueryTenderDTO reqModel)
        {
            string mActionLog = "QueryMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryTenderRespModel>> mResult = new();

            try
            {
                mResult = _tenderSVC.ITenderRepo.QueryTenderAdmin(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案列表失敗，{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-取得進行中標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryTenderInProgressUser")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryTenderInProgressRespModel>>> QueryTenderInProgressUser(QueryTenderInProgressDTO reqModel)
        {
            string mActionLog = "QueryTenderInProgressUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryTenderInProgressRespModel>> mResult = new();

            try
            {
                mResult = _tenderSVC.ITenderRepo.QueryTenderInProgressUser(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得進行中標案失敗，{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-取得所有標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryTenderAllUser")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryTenderAllUserRespModel>>> QueryTenderAllUser(QueryTenderInProgressDTO reqModel)
        {
            string mActionLog = "QueryTenderAllUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryTenderAllUserRespModel>> mResult = new();

            try
            {
                mResult = _tenderSVC.ITenderRepo.QueryTenderAllUser(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得所有標案失敗，{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-取得已參與標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryTenderParticipatedUser")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryTenderParticipatedUserRespModel>>> QueryTenderParticipatedUser(QueryTenderInProgressDTO reqModel)
        {
            string mActionLog = "QueryTenderParticipatedUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryTenderParticipatedUserRespModel>> mResult = new();

            try
            {
                mResult = _tenderSVC.ITenderRepo.QueryTenderParticipatedUser(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得已參與標案失敗，{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-取得標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTender")]
        public ActionResult<ServiceResultDTO<List<GetTenderRespModel>>> GetTender(GetTenderReqModel reqModel)
        {
            string mActionLog = "GetTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<List<GetTenderRespModel>> mResult = new();
            try
            {
                mResult = _tenderSVC.ITenderRepo.GetTender(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} ENDs");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-取得參與紀錄
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetParticipationRecord")]
        public ActionResult<ServiceResultDTO<GetParticipationRecordRespModel>> GetParticipationRecord(GetParticipationRecordReqModel reqModel)
        {
            string mActionLog = "GetParticipationRecord";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetParticipationRecordRespModel> mResult = new();
            try
            {
                mResult = _tenderSVC.ITenderRepo.GetParticipationRecord(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得參與紀錄失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} ENDs");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-取得標案紀錄
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTenderRecord")]
        public ActionResult<ServiceResultDTO<GetTenderRecordRespModel>> GetTenderRecord(GetTenderRecordReqModel reqModel)
        {
            string mActionLog = "GetTenderRecord";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetTenderRecordRespModel> mResult = new();
            try
            {
                mResult = _tenderSVC.ITenderRepo.GetTenderRecord(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案紀錄失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} ENDs");

            return Ok(mResult);
        }

        /// <summary>
        /// 後台-新增標案(初始化)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("PostTender")]
        public ActionResult<ServiceResultDTO<bool>> PostTender()
        {
            string mActionLog = "PostTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();

            try
            {
                DateTime mNowDateTime = GetTimeZoneInfo.process();
                mResult = _tenderSVC.ITenderRepo.PostTender(mNowDateTime);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增標案(初始化)失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} ENDs");

            return Ok(mResult);
        }
    }
}
