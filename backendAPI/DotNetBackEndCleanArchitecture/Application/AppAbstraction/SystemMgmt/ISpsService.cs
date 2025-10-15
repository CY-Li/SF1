using DomainAbstraction.Interface.SystemMgmt;

namespace AppAbstraction.SystemMgmt
{
    public interface ISpsService
    {
        ISpsRepository ISpsRepo { get; }
    }
}
