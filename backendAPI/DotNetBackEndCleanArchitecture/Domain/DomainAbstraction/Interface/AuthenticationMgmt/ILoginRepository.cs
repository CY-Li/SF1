using DomainEntity.Common;
using DomainEntity.Entity.AuthenticationMgmt.Login;
using DomainEntityDTO.Entity.AuthenticationMgmt.Login;


namespace DomainAbstraction.Interface.AuthenticationMgmt
{
    public interface ILoginRepository
    {
        ServiceResultDTO<LoginPO> Login(LoginReqModel reqModel);
    }
}
