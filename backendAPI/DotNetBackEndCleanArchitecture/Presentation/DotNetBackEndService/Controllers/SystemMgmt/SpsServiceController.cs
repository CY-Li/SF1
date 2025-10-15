using AppAbstraction.SystemMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.MemberMgmt.Member;
using DomainEntity.Entity.MemberMgmt.Wallet;
using DomainEntity.Entity.SystemMgmt.SystemParameterSetting;
using DomainEntityDTO.Entity.MemberMgmt.Wallet;
using DomainEntityDTO.Entity.SystemMgmt.SystemParameterSetting;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndService.Controllers.SystemMgmt
{
    [Route("api/SystemMgmt/[controller]")]
    [ApiController]
    public class SpsServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ISpsService _spsSVC;

        private readonly string pBaseLog = "SpsServiceController";

        public SpsServiceController(
                ILogger<SpsServiceController> logger,
                IMapper mapper,
                ISpsService spsSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _spsSVC = spsSVC;
        }

        /// <summary>
        /// 後台-取得系統參數
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QuerySps")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QuerySpsRespModel>>> QuerySps(QuerySpsDTO reqModel)
        {
            string mActionLog = "QuerySps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QuerySpsRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QuerySpsRespModel>>();

            try
            {
                mResult = _spsSVC.ISpsRepo.QuerySps(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "取得取得系統參數失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }


        /// <summary>
        /// 後台-新增參數
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PostSps")]
        public ActionResult<ServiceResultDTO<bool>> PostSps(PostSpsDTO reqModel)
        {
            string mActionLog = "PostSps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                reqModel.sps_create_datetime = GetTimeZoneInfo.process();
                reqModel.sps_update_datetime = reqModel.sps_create_datetime;
                mResult = _spsSVC.ISpsRepo.PostSps(reqModel);
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
        /// 後台-更新參數資料
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PutSps")]
        public ActionResult<ServiceResultDTO<bool>> PutSps(PutSpsDTO reqModel)
        {
            string mActionLog = "PutSps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                if (reqModel.sps_id == 0)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "更新參數失敗，未傳入ID";

                    return mResult;
                }
                reqModel.update_datetime = GetTimeZoneInfo.process();
                mResult = _spsSVC.ISpsRepo.PutSps(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $"更新參數失敗，{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 後台-更新Kyc圖片
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PutKycImage")]
        public ActionResult<ServiceResultDTO<bool>> PutKycImage(byte[] reqModel)
        {
            string mActionLog = "PutKycImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                mResult = _spsSVC.ISpsRepo.PutKycImage(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $"更新Kyc圖片失敗，{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }
    }
}
