using AppAbstraction.FinancialMgmt;
using AppAbstraction.MemberMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.MemberBalance;
using DomainEntityDTO.Entity.FinancialMgmt.MemberBalance;
using DotNetBackEndService.Controllers.MemberMgmt;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndService.Controllers.FinancialMgmt
{
    [Route("api/FinancialMgmt/[controller]")]
    [ApiController]
    public class BalancesServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMemberBalanceService _memberBalanceSVC;

        private readonly string pBaseLog = "BalancesServiceController";
        public BalancesServiceController(
                ILogger<MembersServiceController> logger,
                IMapper mapper,
                IMemberBalanceService memberBalanceSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _memberBalanceSVC = memberBalanceSVC;
        }

        [HttpPost]
        [Route("QueryMemberBalanceAdmin")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceAdminRespModel>>> QueryMemberBalanceAdmin(QueryMemberBalanceAdminDTO reqModel)
        {
            string mActionLog = "QueryMemberBalanceAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceAdminRespModel>> m_result = new ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceAdminRespModel>>();

            try
            {
                m_result = _memberBalanceSVC.IMemberBalanceRepo.QueryMemberBalanceAdmin(reqModel); 
            }
            catch (Exception ex)
            {
                m_result.returnStatus = 0;
                m_result.returnMsg = ex.Message;

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return Ok(m_result);
        }

        [HttpPost]
        [Route("QueryMemberBalanceUser")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceUserRespModel>>> QueryMemberBalanceUser(QueryMemberBalanceDTO reqModel)
        {
            string mActionLog = "QueryMemberBalanceUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceUserRespModel>> m_result = new ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceUserRespModel>>();

            try
            {
                m_result = _memberBalanceSVC.IMemberBalanceRepo.QueryMemberBalanceUser(reqModel);
            }
            catch (Exception ex)
            {
                m_result.returnStatus = 0;
                m_result.returnMsg = ex.Message;

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            return Ok(m_result);
        }
    }
}
