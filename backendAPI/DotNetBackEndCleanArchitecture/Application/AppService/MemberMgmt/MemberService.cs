using AppAbstraction.MemberMgmt;
using DomainAbstraction.Interface.MemberMgmt;

namespace AppService.MemberMgmt
{
    public class MemberService : IMemberService
    {
        public IMemberRepository IMemberRepo { get; }

        public MemberService(
            IMemberRepository _IMemberRepo
        )
        {
            IMemberRepo = _IMemberRepo;
        }
    }
}
