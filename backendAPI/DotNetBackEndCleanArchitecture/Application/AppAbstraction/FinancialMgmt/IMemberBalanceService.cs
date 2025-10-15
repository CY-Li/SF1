using DomainAbstraction.Interface.FinancialMgmt;

namespace AppAbstraction.FinancialMgmt
{
    public interface IMemberBalanceService
    {
        IMemberBalanceRepository IMemberBalanceRepo { get; }
    }
}
