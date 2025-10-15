using AppAbstraction.FinancialMgmt;
using DomainAbstraction.Interface.FinancialMgmt;

namespace AppService.FinancialMgmt
{
    public class ApplyWithdrawService : IApplyWithdrawService
    {
        public IApplyWithdrawRepository IApplyWithdrawRepo { get; }

        public ApplyWithdrawService(
            IApplyWithdrawRepository _IApplyWithdrawRepo
        )
        {
            IApplyWithdrawRepo = _IApplyWithdrawRepo;
        }
    }
}
