using AppAbstraction.AuthenticationMgmt;
using DomainAbstraction.Interface.AuthenticationMgmt;

namespace AppService.AuthenticationMgmt
{
    public class RegisterService : IRegisterService
    {
        public IRegisterRepository IRegisterRepo { get; }

        public RegisterService(
                IRegisterRepository _IRegisterRepo
            )
        {
            IRegisterRepo = _IRegisterRepo;
        }
    }
}
