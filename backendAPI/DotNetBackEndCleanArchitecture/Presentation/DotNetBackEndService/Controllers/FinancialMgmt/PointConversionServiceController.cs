using AppAbstraction.FinancialMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.ApplyWithdraw;
using DomainEntity.Entity.FinancialMgmt.PointConversion;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBackEndService.Controllers.FinancialMgmt
{
    [Route("api/FinancialMgmt/[controller]")]
    [ApiController]
    public class PointConversionServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPointConversionService _pointConversionSVC;

        private readonly string pBaseLog = "PointConversionServiceController";

        public PointConversionServiceController(
                ILogger<PointConversionServiceController> logger,
                IMapper mapper,
                IPointConversionService pointConversionSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _pointConversionSVC = pointConversionSVC;
        }

        /// <summary>
        /// 會員-取得交易資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTransactionData")]
        //[SwaggerOperation(Summary = "會員-取得儲值資料")]
        public ActionResult<ServiceResultDTO<GettransactionDataRespModel>> GetTransactionData([FromBody]long mm_id)
        {
            string mActionLog = "GetTransactionData";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GettransactionDataRespModel> mResult = new ServiceResultDTO<GettransactionDataRespModel>();

            try
            {
                mResult = _pointConversionSVC.IPointConversionRepo.GetTransactionData(mm_id);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得交易資料失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-點數轉換
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostPointConversion")]
        public ActionResult<ServiceResultDTO<bool>> PostPointConversion(PostPointConversionDTO reqModel)
        {
            string mActionLog = "PostPointConversion";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                reqModel.update_datetime = GetTimeZoneInfo.process();
                mResult = _pointConversionSVC.IPointConversionRepo.PostPointConversion(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"點數轉換失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員-點數贈送
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostGiftPoint")]
        public ActionResult<ServiceResultDTO<bool>> PostGiftPoint(PostGiftPointDTO reqModel)
        {
            string mActionLog = "PostGiftPoint";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                reqModel.update_datetime = GetTimeZoneInfo.process();
                mResult = _pointConversionSVC.IPointConversionRepo.PostGiftPoint(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"點數贈送失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }
    }
}
