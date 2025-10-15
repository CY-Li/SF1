using AutoMapper;
using DomainEntity.Common;
using Microsoft.AspNetCore.Mvc;
using Persistence.Schedule;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndService.Controllers.ScheduleMgmt
{
    [Route("api/ScheduleMgmt/[controller]")]
    [ApiController]
    public class ScheduleServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly BiddingSchedule _biddingSchedule;
        private readonly SettlementRewardSchedule _settlementRewardSchedule;
        private readonly SettlementPeaceSchedule _settlementPeaceSchedule;
        private readonly GroupDebitSchedule _groupDebitSchedule;
        private readonly PendingPaymentSchedule _pendingPaymentSchedule;

        private readonly string pBaseLog = "ScheduleServiceController";
        public ScheduleServiceController(
                ILogger<ScheduleServiceController> logger,
                IMapper mapper,
                BiddingSchedule biddingSchedule,
                SettlementRewardSchedule settlementRewardSchedule,
                SettlementPeaceSchedule settlementPeaceSchedule,
                GroupDebitSchedule groupDebitSchedule,
                PendingPaymentSchedule pendingPaymentSchedule
            )
        {
            _logger = logger;
            _mapper = mapper;
            _biddingSchedule = biddingSchedule;
            _settlementRewardSchedule = settlementRewardSchedule;
            _settlementPeaceSchedule = settlementPeaceSchedule;
            _groupDebitSchedule = groupDebitSchedule;
            _pendingPaymentSchedule = pendingPaymentSchedule;
        }

        [HttpPost]
        [Route("BiddingSchedule")]
        public ActionResult<ServiceResultDTO<bool>> BiddingSchedule()
        {
            string mActionLog = "BiddingSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();
            try
            {
                _biddingSchedule.Process();
                //reqModel.update_datetime = GetTimeZoneInfo.process();
                //mResult = _tenderSVC.ITenderRepo.Bidding(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "下標排程失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        [HttpPost]
        [Route("SettlementRewardSchedule")]
        public ActionResult<ServiceResultDTO<bool>> SettlementRewardSchedule()
        {
            string mActionLog = "SettlementRewardSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();
            try
            {
                _settlementRewardSchedule.Process();
                //reqModel.update_datetime = GetTimeZoneInfo.process();
                //mResult = _tenderSVC.ITenderRepo.Bidding(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "清算紅利排程失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        [HttpPost]
        [Route("SettlementPeaceSchedule")]
        public ActionResult<ServiceResultDTO<bool>> SettlementPeaceSchedule()
        {
            string mActionLog = "SettlementPeaceSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();
            try
            {
                _settlementPeaceSchedule.Process();
                //reqModel.update_datetime = GetTimeZoneInfo.process();
                //mResult = _tenderSVC.ITenderRepo.Bidding(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "清算得標排程失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        [HttpPost]
        [Route("GroupDebitSchedule")]
        public ActionResult<ServiceResultDTO<bool>> GroupDebitSchedule()
        {
            string mActionLog = "GroupDebitSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();
            try
            {
                _groupDebitSchedule.Process();
                //reqModel.update_datetime = GetTimeZoneInfo.process();
                //mResult = _tenderSVC.ITenderRepo.Bidding(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "成組排程失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        [HttpPost]
        [Route("PendingPaymentSchedule")]
        public ActionResult<ServiceResultDTO<bool>> PendingPaymentSchedule()
        {
            string mActionLog = "PendingPaymentSchedule";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();
            try
            {
                _pendingPaymentSchedule.Process();
                //reqModel.update_datetime = GetTimeZoneInfo.process();
                //mResult = _tenderSVC.ITenderRepo.Bidding(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "待付款排程失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }
    }
}
