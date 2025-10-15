using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.AuthenticationMgmt.Register;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetBackEndApi.Controllers.AuthenticationMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly HttpClientService _httpClientService;


        public RegisterController(
            ILogger<RegisterController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            HttpClientService httpClientService
            )
        {
            _logger = logger;
            _apiAppConfig = apiAppConfig.Value;
            _httpClientService = httpClientService;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "會員-註冊")]
        public ApiResultModel<bool> Register([FromBody] RegisterReqModel reqModel)
        {
            _logger.LogInformation("RegisterController Register START");

            ApiResultModel<bool> mResult = new ApiResultModel<bool>();
            mResult = _httpClientService.ProcessQuery<bool>("AuthenticationMgmt/RegisterService/Register", reqModel);

            _logger.LogInformation("RegisterController Register END");
            return mResult;
        }
    }
}
