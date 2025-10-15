using AutoMapper;
using InfraCommon.MapperServices.AutoMapperProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraCommon.MapperServices
{
    /// <summary>
    /// 暫時沒在用
    /// </summary>
    public class AutoMapperProfileService
    {
        private IMapper _mapper;

        public AutoMapperProfileService()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AuthenticationMgmtMappingProfile>());

            _mapper = config.CreateMapper();
        }
    }
}
