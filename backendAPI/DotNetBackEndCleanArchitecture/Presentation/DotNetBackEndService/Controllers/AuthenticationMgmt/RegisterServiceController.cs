using AppAbstraction.AuthenticationMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.AuthenticationMgmt.Register;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBackEndService.Controllers.AuthenticationMGMT
{
    [Route("api/AuthenticationMgmt/[controller]")]
    [ApiController]
    public class RegisterServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRegisterService _registerSVC;

        public RegisterServiceController(
                ILogger<RegisterServiceController> logger,
                IMapper mapper,
                IRegisterService registerSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _registerSVC = registerSVC;
        }

        /// <summary>
        /// 註冊會員
        /// </summary>
        /// <param name="e_registerREQModel"></param>
        [HttpPost]
        [Route("Register")]
        public ActionResult<ServiceResultDTO<bool>> Register([FromBody] RegisterDTO reqModel)
        {
            _logger.LogInformation("RegisterServiceController Register START");
            ServiceResultDTO<bool> mResult = new();

            try
            {
                //取得該帳號狀態為Y的筆數
                bool mCheckAcc = _registerSVC.IRegisterRepo.CheckAccount(reqModel.mm_account) == 0 ? true : false;

                //檢查該帳號存在0筆才註冊
                if (mCheckAcc)
                {
                    //將密碼跟Salt雜湊演算
                    reqModel.mm_hash_pwd = HashProcess.GetHashPWD(reqModel.mm_pwd);
                    reqModel.mm_2nd_hash_pwd = HashProcess.GetHashPWD(reqModel.mm_2nd_pwd);
                    //reqModel.mm_introducer = reqModel.mm_introducer != 0 ? reqModel.mm_introducer : 0;
                    //reqModel.mm_invite_code = reqModel.mm_invite_code != 0 ? reqModel.mm_invite_code : 0;
                    reqModel.mm_phone_number = reqModel.mm_account;
                    reqModel.mm_role_type = "1";
                    reqModel.mm_create_datetime = GetTimeZoneInfo.process();
                    reqModel.mm_update_datetime = reqModel.mm_create_datetime;

                    if (reqModel.mm_create_datetime == DateTime.MinValue)
                    {
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"註冊失敗、時間格式轉換錯誤，{reqModel.mm_create_datetime}";
                    }
                    else
                    {
                        //最後執行位置
                        mResult = _registerSVC.IRegisterRepo.PostMember(reqModel);
                    }
                }
                else
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "註冊失敗、帳號已存在";
                }
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = ex.Message.ToString();

                _logger.LogInformation($@"RegisterServiceController Register {ex.Message}");
            }

            _logger.LogInformation("RegisterServiceController Register END");

            return mResult;
        }
    }
}
