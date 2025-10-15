using AppAbstraction.MemberMgmt;
using DomainAbstraction.Interface.MemberMgmt;

namespace AppService.MemberMgmt
{
    public class WalletService: IWalletService
    {
        public IWalletRepository IWalletRepo { get; }

        public WalletService(
            IWalletRepository _IWalletRepo
        )
        {
            IWalletRepo = _IWalletRepo;
        }
    }
}
