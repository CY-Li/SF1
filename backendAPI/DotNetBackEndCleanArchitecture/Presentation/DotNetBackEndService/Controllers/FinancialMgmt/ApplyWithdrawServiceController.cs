using AppAbstraction.FinancialMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.ApplyWithdraw;
using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyWithdraw;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetBackEndService.Controllers.FinancialMgmt
{
    [Route("api/FinancialMgmt/[controller]")]
    [ApiController]
    public class ApplyWithdrawServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IApplyWithdrawService _applyWithdrawSVC;

        private readonly string pBaseLog = "ApplyWithdrawServiceController";

        public ApplyWithdrawServiceController(
                ILogger<ApplyWithdrawServiceController> logger,
                IMapper mapper,
                IApplyWithdrawService applyWithdrawSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _applyWithdrawSVC = applyWithdrawSVC;
        }

        /// <summary>
        /// 後台-取得儲值申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryApplyWithdrawAdmin")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawAdminRespModel>>> QueryApplyWithdrawAdmin(QueryApplyWithdrawAdminDTO reqModel)
        {
            string mActionLog = "QueryApplyWithdrawAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawAdminRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawAdminRespModel>>();

            try
            {
                mResult = _applyWithdrawSVC.IApplyWithdrawRepo.QueryApplyWithdrawAdmin(reqModel);
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
        /// 會員-取得提領申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryApplyWithdrawUser")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawUserRespModel>>> QueryApplyWithdrawUser(QueryApplyWithdrawUserDTO reqModel)
        {
            string mActionLog = "QueryApplyWithdrawUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawUserRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawUserRespModel>>();

            try
            {
                mResult = _applyWithdrawSVC.IApplyWithdrawRepo.QueryApplyWithdrawUser(reqModel);
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
        /// 會員-申請提領
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostApplyWithdraw")]
        public ActionResult<ServiceResultDTO<bool>> PostApplyWithdraw(PostApplyWithdrawDTO reqModel)
        {
            string mActionLog = "PostApplyWithdraw";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                reqModel.aw_status = "10";
                reqModel.aw_create_datetime = GetTimeZoneInfo.process();
                reqModel.aw_update_datetime = reqModel.aw_create_datetime;
                mResult = _applyWithdrawSVC.IApplyWithdrawRepo.PostApplyWithdraw(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增申請提領失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 後台-申請提領覆核
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PutApplyWithdraw")]
        public ActionResult<ServiceResultDTO<bool>> PutApplyWithdraw(PutApplyWithdrawDTO reqModel)
        {
            string mActionLog = "PutApplyWithdraw";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                reqModel.create_datetime = GetTimeZoneInfo.process();
                mResult = _applyWithdrawSVC.IApplyWithdrawRepo.PutApplyWithdraw(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"覆核申請提領失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }
    }
}
