using AppAbstraction.SystemMgmt;
using DomainAbstraction.Interface.SystemMgmt;

namespace AppService.SystemMgmt
{
    public class SpsService : ISpsService
    {
        public ISpsRepository ISpsRepo { get; }

        public SpsService(
            ISpsRepository _ISpsRepo
        )
        {
            ISpsRepo = _ISpsRepo;
        }
    }
}
