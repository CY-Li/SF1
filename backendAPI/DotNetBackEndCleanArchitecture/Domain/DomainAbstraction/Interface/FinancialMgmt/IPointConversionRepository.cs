using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.PointConversion;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;

namespace DomainAbstraction.Interface.FinancialMgmt
{
    public interface IPointConversionRepository
    {
        ServiceResultDTO<GettransactionDataRespModel> GetTransactionData(long mm_id);
        ServiceResultDTO<bool> PostPointConversion(PostPointConversionDTO reqModel);
        ServiceResultDTO<bool> PostGiftPoint(PostGiftPointDTO reqModel);
    }
}
