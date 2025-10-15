using DomainAbstraction.Interface.FinancialMgmt;

namespace AppAbstraction.FinancialMgmt
{
    public interface IApplyWithdrawService
    {
        IApplyWithdrawRepository IApplyWithdrawRepo { get; }
    }
}
