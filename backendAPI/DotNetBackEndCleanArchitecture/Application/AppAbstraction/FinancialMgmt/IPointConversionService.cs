using DomainAbstraction.Interface.FinancialMgmt;

namespace AppAbstraction.FinancialMgmt
{
    public interface IPointConversionService
    {
        IPointConversionRepository IPointConversionRepo { get; }
    }
}
