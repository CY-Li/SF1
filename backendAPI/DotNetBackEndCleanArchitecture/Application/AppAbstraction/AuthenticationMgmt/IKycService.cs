using DomainAbstraction.Interface.AuthenticationMgmt;

namespace AppAbstraction.AuthenticationMgmt
{
    public interface IKycService
    {
        IKycRepository IKycRepo { get; }
    }
}
