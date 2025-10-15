using DomainEntity.Common;
using DomainEntity.Entity.MemberMgmt.Wallet;
using DomainEntityDTO.Entity.MemberMgmt.Wallet;

namespace DomainAbstraction.Interface.MemberMgmt
{
    public interface IWalletRepository
    {
        ServiceResultDTO<BaseGridRespDTO<QueryWalletRespModel>> QueryWallet(QueryWalletDTO reqModel);
        ServiceResultDTO<GetWalletRespModel> GetWallet(long mm_id);
    }
}
