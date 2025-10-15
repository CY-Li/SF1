using DomainEntity.Common;
using DomainEntity.Entity.TenderMgmt.Tender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainAbstraction.Interface.TenderMgmt
{
    public interface IBiddingRepository
    {
        ServiceResultDTO<bool> Bidding(BiddingDTO reqModel);
    }
}
