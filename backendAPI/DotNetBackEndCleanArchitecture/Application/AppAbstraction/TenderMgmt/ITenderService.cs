using DomainAbstraction.Interface.TenderMgmt;

namespace AppAbstraction.TenderMgmt
{
    public interface ITenderService
    {
        ITenderRepository ITenderRepo { get; }
        IBiddingRepository IBiddingRepo { get; }
        IWinningTenderRepository IWinningTenderRepo { get; }
    }
}
