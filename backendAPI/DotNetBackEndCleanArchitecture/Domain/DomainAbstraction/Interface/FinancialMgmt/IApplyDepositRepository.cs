using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.ApplyDeposit;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainAbstraction.Interface.FinancialMgmt
{
    public interface IApplyDepositRepository
    {
        ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositAdminRespModel>> QueryApplyDepositAdmin(QueryApplyDepositAdminDTO reqModel);
        ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositUserRespModel>> QueryApplyDepositUser(QueryApplyDepositUserDTO reqModel);
        ServiceResultDTO<GetDepositDataRespModel> GetDepositData();
        //ServiceResultDTO<bool> PostApplyDeposit(PostApplyDepositDTO reqModel);
        ServiceResultDTO<bool> PostApplyDeposit(PostApplyDepositDTO reqModel);
        ServiceResultDTO<bool> PutApplyDeposit(PutApplyDepositDTO reqModel);
    }
}
