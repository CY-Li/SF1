using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.ApplyWithdraw;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyWithdraw;

namespace DomainAbstraction.Interface.FinancialMgmt
{
    public interface IApplyWithdrawRepository
    {
        //ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawAdminRespModel>> QueryApplyWithdrawAdmin(QueryApplyWithdrawAdminDTO reqModel);
        //ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawUserRespModel>> QueryApplyWithdrawUser(QueryApplyWithdrawUserDTO reqModel);
        ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawAdminRespModel>> QueryApplyWithdrawAdmin(QueryApplyWithdrawAdminDTO reqModel);
        ServiceResultDTO<BaseGridRespDTO<QueryApplyWithdrawUserRespModel>> QueryApplyWithdrawUser(QueryApplyWithdrawUserDTO reqModel);
        ServiceResultDTO<bool> PostApplyWithdraw(PostApplyWithdrawDTO reqModel);
        ServiceResultDTO<bool> PutApplyWithdraw(PutApplyWithdrawDTO reqModel);
    }
}
