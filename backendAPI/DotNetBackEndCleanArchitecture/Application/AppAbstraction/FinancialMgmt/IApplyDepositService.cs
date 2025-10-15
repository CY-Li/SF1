using DomainAbstraction.Interface.FinancialMgmt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAbstraction.FinancialMgmt
{
    public interface IApplyDepositService
    {
        IApplyDepositRepository IApplyDepositRepo { get; }
    }
}
