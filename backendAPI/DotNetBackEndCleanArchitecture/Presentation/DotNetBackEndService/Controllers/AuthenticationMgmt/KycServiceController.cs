using AppAbstraction.AuthenticationMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.AuthenticationMgmt.Kyc;
using DomainEntity.Entity.AuthenticationMgmt.Register;
using DomainEntity.Entity.TenderMgmt.Tender;
using DomainEntityDTO.Entity.AuthenticationMgmt.Kyc;
using DomainEntityDTO.Entity.TenderMgmt.Tender;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBackEndService.Controllers.AuthenticationMgmt
{
    [Route("api/AuthenticationMgmt/[controller]")]
    [ApiController]
    public class KycServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IKycService _kycSVC;

        private readonly string pBaseLog = "KycServiceController";

        public KycServiceController(
                ILogger<KycServiceController> logger,
                IMapper mapper,
                IKycService kycSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _kycSVC = kycSVC;
        }

        /// <summary>
        /// 後台-取得KYC申請
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryKycAdmin")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryKycAdminRespModel>>> QueryKycAdmin(QueryKycAdminDTO reqModel)
        {
            string mActionLog = "QueryKycAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryKycAdminRespModel>> mResult = new();

            try
            {
                mResult = _kycSVC.IKycRepo.QueryKycAdmin(reqModel);
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
        /// 會員-確認Kyc資料
        /// </summary>
        /// <param name="reqModel"></param>
        [HttpPost]
        [Route("CheckKyc")]
        public ActionResult<ServiceResultDTO<long>> CheckKyc([FromBody] long reqModel)
        {
            string mActionLog = "CheckKyc";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<long> mResult = new();

            try
            {
                mResult = _kycSVC.IKycRepo.CheckKyc(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = 0;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"確認Kyc資料失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-申請KYC
        /// </summary>
        /// <param name="reqModel"></param>
        [HttpPost]
        [Route("PostKyc")]
        public ActionResult<ServiceResultDTO<bool>> PostKyc([FromBody] PostKycDTO reqModel)
        {
            string mActionLog = "PostKyc";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();

            try
            {
                reqModel.akc_create_member = reqModel.akc_mm_id;
                reqModel.akc_create_datetime = GetTimeZoneInfo.process();
                reqModel.akc_update_member = reqModel.akc_mm_id;
                reqModel.akc_update_datetime = GetTimeZoneInfo.process();

                mResult = _kycSVC.IKycRepo.PostKyc(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"確認Kyc資料失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-覆核KYC
        /// </summary>
        /// <param name="reqModel"></param>
        [HttpPost]
        [Route("PutKyc")]
        public ActionResult<ServiceResultDTO<bool>> PutKyc([FromBody] PutKycDTO reqModel)
        {
            string mActionLog = "Postkyc";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();

            try
            {
                reqModel.update_datetime = GetTimeZoneInfo.process();

                mResult = _kycSVC.IKycRepo.PutKyc(reqModel);
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"覆核Kyc失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
