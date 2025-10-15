using AppAbstraction.FinancialMgmt;
using DomainAbstraction.Interface.FinancialMgmt;

namespace AppService.FinancialMgmt
{
    public class MemberBalanceService: IMemberBalanceService
    {
        public IMemberBalanceRepository IMemberBalanceRepo { get; }

        public MemberBalanceService(
            IMemberBalanceRepository _IMemberBalanceRepo
        )
        {
            IMemberBalanceRepo = _IMemberBalanceRepo;
        }
    }
}
