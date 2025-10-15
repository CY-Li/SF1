using AppAbstraction.FinancialMgmt;
using DomainAbstraction.Interface.FinancialMgmt;

namespace AppService.FinancialMgmt
{
    public class ApplyDepositService : IApplyDepositService
    {
        public IApplyDepositRepository IApplyDepositRepo { get; }

        public ApplyDepositService(
            IApplyDepositRepository _IApplyDepositRepo
        )
        {
            IApplyDepositRepo = _IApplyDepositRepo;
        }
    }
}
