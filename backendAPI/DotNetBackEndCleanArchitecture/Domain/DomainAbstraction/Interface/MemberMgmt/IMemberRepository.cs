using DomainEntity.Common;
using DomainEntity.Entity.MemberMgmt.Member;
using DomainEntityDTO.Entity.MemberMgmt.Member;

namespace DomainAbstraction.Interface.MemberMgmt
{
    public interface IMemberRepository
    {
        ServiceResultDTO<List<GetSubordinateOrgRespModel>> GetSubordinateOrg(long mm_id);
        ServiceResultDTO<BaseGridRespDTO<QueryMemberRespModel>> QueryMember(QueryMemberDTO reqModel);
        ServiceResultDTO<GetMemberRespModel> GetMember(long mm_id);
        ServiceResultDTO<bool> PutMember(PutMemberDTO reqModel);
        ServiceResultDTO<bool> UpdatePwd(UpdatePwdDTO reqModel);
    }
}
