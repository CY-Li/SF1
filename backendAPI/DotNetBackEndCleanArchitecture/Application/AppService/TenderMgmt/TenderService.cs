using AppAbstraction.TenderMgmt;
using DomainAbstraction.Interface.TenderMgmt;

namespace AppService.TenderMgmt
{
    public class TenderService: ITenderService
    {
        public ITenderRepository ITenderRepo { get; }
        public IBiddingRepository IBiddingRepo { get; }
        public IWinningTenderRepository IWinningTenderRepo { get; }

        public TenderService(
            ITenderRepository _ITenderRepo,
            IBiddingRepository _IBiddingRepo,
            IWinningTenderRepository _IWinningTenderRepo
        )
        {
            ITenderRepo = _ITenderRepo;
            IBiddingRepo = _IBiddingRepo;
            IWinningTenderRepo = _IWinningTenderRepo;
        }
    }
}
