using AppAbstraction.FinancialMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.ApplyDeposit;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndService.Controllers.FinancialMgmt
{
    [Route("api/FinancialMgmt/[controller]")]
    [ApiController]
    public class ApplyDepositServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IApplyDepositService _applyDepositSVC;

        private readonly string pBaseLog = "ApplyDepositServiceController";

        public ApplyDepositServiceController(
                ILogger<ApplyDepositServiceController> logger,
                IMapper mapper,
                IApplyDepositService applyDepositSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _applyDepositSVC = applyDepositSVC;
        }

        /// <summary>
        /// 後台-取得儲值申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryApplyDepositAdmin")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositAdminRespModel>>> QueryApplyDepositAdmin(QueryApplyDepositAdminDTO reqModel)
        {
            string mActionLog = "QueryApplyDepositAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositAdminRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositAdminRespModel>>();

            try
            {
                mResult = _applyDepositSVC.IApplyDepositRepo.QueryApplyDepositAdmin(reqModel);
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
        /// 會員-取得儲值申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryApplyDepositUser")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositUserRespModel>>> QueryApplyDepositUser(QueryApplyDepositUserDTO reqModel)
        {
            string mActionLog = "QueryApplyDepositUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositUserRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositUserRespModel>>();

            try
            {
                mResult = _applyDepositSVC.IApplyDepositRepo.QueryApplyDepositUser(reqModel);
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
        /// 會員-取得儲值資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDepositData")]
        //[SwaggerOperation(Summary = "會員-取得儲值資料")]
        public ActionResult<ServiceResultDTO<GetDepositDataRespModel>> GetDepositData()
        {
            string mActionLog = "GetDepositData";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetDepositDataRespModel> mResult = new ServiceResultDTO<GetDepositDataRespModel>();

            try
            {
                mResult = _applyDepositSVC.IApplyDepositRepo.GetDepositData();
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得儲值資料、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-申請儲值(已付款要丟KEY)
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("PostApplyDeposit")]
        //public ActionResult<ServiceResultDTO<bool>> PostApplyDeposit(PostApplyDepositDTO reqModel)
        //{
        //    string mActionLog = "PostApplyDeposit";
        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

        //    ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

        //    try
        //    {
        //        reqModel.ad_status = "10";
        //        reqModel.ad_create_datetime = GetTimeZoneInfo.process();
        //        reqModel.ad_update_datetime = reqModel.ad_create_datetime;
        //        mResult = _applyDepositSVC.IApplyDepositRepo.PostApplyDeposit(reqModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        mResult.Result = false;
        //        mResult.returnStatus = 999;
        //        mResult.returnMsg = $@"新增申請儲值失敗、{ex.Message}";

        //        _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
        //    }

        //    _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

        //    return Ok(mResult);
        //}

        /// <summary>
        /// 會員-申請儲值(已付款要丟KEY)
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostApplyDeposit")]
        public ActionResult<ServiceResultDTO<bool>> PostApplyDeposit(PostApplyDepositDTO reqModel)
        {
            string mActionLog = "PostApplyDeposit";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                reqModel.ad_status = "10";
                reqModel.ad_create_datetime = GetTimeZoneInfo.process();
                reqModel.ad_update_datetime = reqModel.ad_create_datetime;
                mResult = _applyDepositSVC.IApplyDepositRepo.PostApplyDeposit(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增申請儲值失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 後台-申請儲值覆核
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PutApplyDeposit")]
        public ActionResult<ServiceResultDTO<bool>> PutApplyDeposit(PutApplyDepositDTO reqModel)
        {
            string mActionLog = "PutApplyDeposit";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                reqModel.create_datetime = GetTimeZoneInfo.process();
                mResult = _applyDepositSVC.IApplyDepositRepo.PutApplyDeposit(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"覆核申請儲值失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }
    }
}
