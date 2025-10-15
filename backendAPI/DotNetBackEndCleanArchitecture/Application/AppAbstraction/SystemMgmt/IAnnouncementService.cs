using DomainAbstraction.Interface.SystemMgmt;

namespace AppAbstraction.SystemMgmt
{
    public interface IAnnouncementService
    {
        IAnnouncementRepository IAnnouncementRepo { get; }
    }
}
