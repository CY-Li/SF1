using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.MemberBalance;
using DomainEntity.Entity.MemberMgmt.Member;
using DomainEntityDTO.Entity.FinancialMgmt.MemberBalance;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainAbstraction.Interface.FinancialMgmt
{
    public interface IMemberBalanceRepository
    {
        ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceAdminRespModel>> QueryMemberBalanceAdmin(QueryMemberBalanceAdminDTO reqModel);
        ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceUserRespModel>> QueryMemberBalanceUser(QueryMemberBalanceDTO reqModel);
    }
}
