using DomainEntity.Common;
using DomainEntity.Entity.SystemMgmt.Announcement;
using DomainEntityDTO.Entity.SystemMgmt.Announcement;

namespace DomainAbstraction.Interface.SystemMgmt
{
    public interface IAnnouncementRepository
    {
        ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardAdminRespModel>> QueryAnnBoardAdmin(QueryAnnBoardAdminDTO reqModel);
        ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardUserRespModel>> QueryAnnBoardUser(QueryAnnBoardUserDTO reqModel);
        ServiceResultDTO<bool> PostAnnouncement(PostAnnouncementDTO reqModel);
        ServiceResultDTO<bool> PutAnnouncement(PutAnnouncementDTO reqModel);
    }
}
