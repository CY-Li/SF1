using DomainEntity.Common;
using DomainEntity.Entity.SystemMgmt.SystemParameterSetting;
using DomainEntityDTO.Entity.HomeMgmt;
using DomainEntityDTO.Entity.SystemMgmt.SystemParameterSetting;

namespace DomainAbstraction.Interface.SystemMgmt
{
    public interface ISpsRepository
    {
        ServiceResultDTO<BaseGridRespDTO<QuerySpsRespModel>> QuerySps(QuerySpsDTO reqModel);
        ServiceResultDTO<bool> PostSps(PostSpsDTO reqModel);
        ServiceResultDTO<bool> PutSps(PutSpsDTO reqModel);
        ServiceResultDTO<bool> PutKycImage(byte[] reqModel);


        public ServiceResultDTO<GetHomeVideoRespModel> GetHomeVideo();
    }
}
