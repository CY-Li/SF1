using AppAbstraction.FinancialMgmt;
using DomainAbstraction.Interface.FinancialMgmt;

namespace AppService.FinancialMgmt
{
    public class PointConversionService: IPointConversionService
    {
        public IPointConversionRepository IPointConversionRepo { get; }

        public PointConversionService(
            IPointConversionRepository _IPointConversionRepo
        )
        {
            IPointConversionRepo = _IPointConversionRepo;
        }
    }
}
