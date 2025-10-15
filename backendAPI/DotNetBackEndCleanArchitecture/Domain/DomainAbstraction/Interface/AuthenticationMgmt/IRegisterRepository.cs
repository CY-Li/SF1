using DomainEntity.Common;
using DomainEntity.Entity.AuthenticationMgmt.Register;

namespace DomainAbstraction.Interface.AuthenticationMgmt
{
    public interface IRegisterRepository
    {
        /// <summary>
        /// 在輸入時需要驗證，驗證會員是否已存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int CheckAccount(string id);

        /// <summary>
        /// 註冊會員
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        ServiceResultDTO<bool> PostMember(RegisterDTO reqModel);
    }
}
