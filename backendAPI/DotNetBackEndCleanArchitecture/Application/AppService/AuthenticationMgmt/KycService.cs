using AppAbstraction.AuthenticationMgmt;
using DomainAbstraction.Interface.AuthenticationMgmt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppService.AuthenticationMgmt
{
    public class KycService: IKycService
    {
        public IKycRepository IKycRepo { get; }

        public KycService(
                IKycRepository _IKycRepo
            )
        {
            IKycRepo = _IKycRepo;
        }
    }
}
