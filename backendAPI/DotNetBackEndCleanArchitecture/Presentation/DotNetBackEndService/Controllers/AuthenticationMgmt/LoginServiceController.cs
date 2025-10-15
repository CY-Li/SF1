using AppAbstraction.AuthenticationMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.AuthenticationMgmt.Login;
using DomainEntityDTO.Entity.AuthenticationMgmt.Login;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBackEndService.Controllers.AuthenticationMgmt
{
    [Route("api/AuthenticationMgmt/[controller]")]
    [ApiController]
    public class LoginServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ILoginService _loginSVC;

        public LoginServiceController(
                ILogger<LoginServiceController> logger,
                IMapper mapper,
                ILoginService loginSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _loginSVC = loginSVC;
        }

        /// <summary>
        /// 在輸入時需要驗證，驗證會員是否已存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        public ActionResult<ServiceResultDTO<LoginPO>> Login([FromBody] LoginReqModel reqModel)
        {
            _logger.LogInformation("LoginServiceController Login START");
            ServiceResultDTO<LoginPO> mResult = new();

            try
            {
                mResult = _loginSVC.ILoginRepo.Login(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 1;
                mResult.returnMsg = ex.Message;

                _logger.LogInformation($@"LoginServiceController Login {ex.Message}");
            }

            _logger.LogInformation("LoginServiceController Login END");

            return Ok(mResult);
        }
    }
}
