using DomainAbstraction.Interface.MemberMgmt;

namespace AppAbstraction.MemberMgmt
{
    public interface IWalletService
    {
        IWalletRepository IWalletRepo { get; }
    }
}
