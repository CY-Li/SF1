using AppAbstraction.AuthenticationMgmt;
using DomainAbstraction.Interface.AuthenticationMgmt;

namespace AppService.AuthenticationMgmt
{
    public class LoginService : ILoginService
    {
        public ILoginRepository ILoginRepo { get; }

        public LoginService(
                ILoginRepository _ILoginRepo
            )
        {
            ILoginRepo = _ILoginRepo;
        }
    }
}
