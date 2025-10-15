using AppAbstraction.SystemMgmt;
using DomainAbstraction.Interface.SystemMgmt;

namespace AppService.SystemMgmt
{
    public class AnnouncementService : IAnnouncementService
    {
        public IAnnouncementRepository IAnnouncementRepo { get; }

        public AnnouncementService(
            IAnnouncementRepository _IAnnouncementRepo
        )
        {
            IAnnouncementRepo = _IAnnouncementRepo;
        }
    }
}
