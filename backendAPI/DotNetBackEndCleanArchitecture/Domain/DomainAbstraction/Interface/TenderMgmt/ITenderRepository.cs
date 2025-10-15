using DomainEntity.Common;
using DomainEntity.Entity.TenderMgmt.Tender;
using DomainEntityDTO.Entity.TenderMgmt.Tender;

namespace DomainAbstraction.Interface.TenderMgmt
{
    public interface ITenderRepository
    {
        ServiceResultDTO<BaseGridRespDTO<QueryTenderRespModel>> QueryTenderAdmin(QueryTenderDTO reqModel);
        ServiceResultDTO<BaseGridRespDTO<QueryTenderInProgressRespModel>> QueryTenderInProgressUser(QueryTenderInProgressDTO reqModel);
        ServiceResultDTO<BaseGridRespDTO<QueryTenderAllUserRespModel>> QueryTenderAllUser(QueryTenderInProgressDTO reqModel);
        ServiceResultDTO<BaseGridRespDTO<QueryTenderParticipatedUserRespModel>> QueryTenderParticipatedUser(QueryTenderInProgressDTO reqModel);
        ServiceResultDTO<List<GetTenderRespModel>> GetTender(GetTenderReqModel reqModel);
        ServiceResultDTO<GetParticipationRecordRespModel> GetParticipationRecord(GetParticipationRecordReqModel reqModel);
        ServiceResultDTO<GetTenderRecordRespModel> GetTenderRecord(GetTenderRecordReqModel reqModel);
        ServiceResultDTO<bool> PostTender(DateTime nowDateTime);
    }
}
