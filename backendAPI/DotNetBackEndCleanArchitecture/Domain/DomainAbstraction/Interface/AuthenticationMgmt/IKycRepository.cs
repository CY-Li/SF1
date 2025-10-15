using DomainEntity.Common;
using DomainEntity.Entity.AuthenticationMgmt.Kyc;
using DomainEntityDTO.Entity.AuthenticationMgmt.Kyc;

namespace DomainAbstraction.Interface.AuthenticationMgmt
{
    public interface IKycRepository
    {
        ServiceResultDTO<BaseGridRespDTO<QueryKycAdminRespModel>> QueryKycAdmin(QueryKycAdminDTO reqModel);
        ServiceResultDTO<long> CheckKyc(long mm_id);
        ServiceResultDTO<bool> PostKyc(PostKycDTO reqModel);
        ServiceResultDTO<bool> PutKyc(PutKycDTO reqModel);
    }
}
