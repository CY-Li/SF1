using DomainAbstraction.Interface.MemberMgmt;

namespace AppAbstraction.MemberMgmt
{
    public interface IMemberService
    {
        IMemberRepository IMemberRepo { get; }
    }
}
