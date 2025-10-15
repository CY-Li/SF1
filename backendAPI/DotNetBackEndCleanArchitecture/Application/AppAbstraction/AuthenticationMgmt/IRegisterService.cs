using DomainAbstraction.Interface.AuthenticationMgmt;

namespace AppAbstraction.AuthenticationMgmt
{
    public interface IRegisterService
    {
        IRegisterRepository IRegisterRepo { get; }
    }
}
