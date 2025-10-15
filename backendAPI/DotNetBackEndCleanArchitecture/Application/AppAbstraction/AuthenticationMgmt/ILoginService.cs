using DomainAbstraction.Interface.AuthenticationMgmt;

namespace AppAbstraction.AuthenticationMgmt
{
    public interface ILoginService
    {
        ILoginRepository ILoginRepo { get; }
    }
}
