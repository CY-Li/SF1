using AutoMapper;
using DomainEntityDTO.Entity.AuthenticationMgmt.Login;
using DomainEntityDTO.Entity.AuthenticationMgmt.Register;

namespace InfraCommon.MapperServices.AutoMapperProfile
{
    public class AuthenticationMgmtMappingProfile: Profile
    {
        public AuthenticationMgmtMappingProfile()
        {
            //CreateMap<RegisterReqModel, LoginRESPModel>();
        }
    }
}
