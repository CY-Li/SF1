using AppAbstraction.MemberMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.MemberMgmt.Wallet;
using DomainEntityDTO.Entity.MemberMgmt.Wallet;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndService.Controllers.MemberMgmt
{
    [Route("api/MemberMgmt/[controller]")]
    [ApiController]
    public class WalletServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IWalletService _walletSVC;

        private readonly string pBaseLog = "WalletServiceController";
        public WalletServiceController(
                ILogger<WalletServiceController> logger,
                IMapper mapper,
                IWalletService walletSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _walletSVC = walletSVC;
        }

        /// <summary>
        /// 後台-取得會員錢包列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryWallet")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryWalletRespModel>>> QueryWallet(QueryWalletDTO reqModel)
        {
            string mActionLog = "QueryWallet";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryWalletRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryWalletRespModel>>();

            try
            {
                mResult = _walletSVC.IWalletRepo.QueryWallet(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "取得會員列表失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 取得該會員錢包資料
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetWallet")]
        public ActionResult<ServiceResultDTO<GetWalletRespModel>> GetWallet([FromBody] long mm_id)
        {
            string mActionLog = "GetWallet";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetWalletRespModel> mResult = new ServiceResultDTO<GetWalletRespModel>();

            try
            {
                mResult = _walletSVC.IWalletRepo.GetWallet(mm_id);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "取得會員錢包失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }
    }
}
