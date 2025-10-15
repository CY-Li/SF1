using DomainAbstraction.Interface.AuthenticationMgmt;
using DomainEntity.Common;
using DomainEntity.Entity.AuthenticationMgmt.Login;
using DomainEntityDTO.Entity.AuthenticationMgmt.Login;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Impl.AuthenticationMgmt
{
    public class LoginRepository : ILoginRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        public LoginRepository(
                ILogger<LoginRepository> logger,
                IDBAccess dbAccess,
                IOptions<AppConfig> appConfig
            )
        {
            _logger = logger;
            _dbAccess = dbAccess;
            _dbAccess.SetConnectionString(appConfig.Value.ConnectionStrings.BackEndDatabase);
            _strConnection = appConfig.Value.ConnectionStrings.BackEndDatabase;
        }

        public ServiceResultDTO<LoginPO> Login(LoginReqModel reqModel)
        {
            ServiceResultDTO<LoginPO> mResult = new ServiceResultDTO<LoginPO>();

            try
            {
                string mSql = @"
SELECT A.mm_id
       ,A.mm_account
       ,A.mm_hash_pwd
       ,A.mm_name
       ,A.mm_introduce_code
       ,A.mm_invite_code
       ,A.mm_gender
       ,A.mm_personal_id
       ,A.mm_phone_number
       ,A.mm_mw_address
       ,A.mm_email
       ,A.mm_role_type
       ,A.mm_status
       ,A.mm_kyc
       ,B.mw_id
       ,B.mw_mm_id
       ,B.mw_currency         #貨幣種類
       ,B.mw_address          #錢包地址
       ,B.mw_subscripts_count #下標數
       ,B.mw_stored           #儲值點數
       ,B.mw_reward           #紅利點數
       ,B.mw_peace            #平安點數
       ,B.mw_mall             #商城點數
       ,B.mw_registration     #註冊點數
       ,B.mw_death            #死會點數
       ,B.mw_accumulation     #累積獎勵
FROM   rosca2.member_master AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.mm_id
WHERE  A.mm_account = @mm_account
       AND A.mm_status = 'Y' 
";

                List<LoginPO> mMMList = _dbAccess.GetObject<LoginPO>(mSql, reqModel);

                if (mMMList.Count == 1)
                {
                    LoginPO m_MemberMaster = mMMList.First();
                    //驗證密碼

                    if (!string.IsNullOrEmpty(reqModel.mm_pwd))
                    {
                        bool verify = HashProcess.VerifyHashPWD(reqModel.mm_pwd, m_MemberMaster.mm_hash_pwd);
                        if (verify)
                        {
                            mResult.Result = m_MemberMaster;
                            mResult.returnStatus = 1;
                            mResult.returnMsg = "登入成功";
                        }
                        else
                        {
                            mResult.returnStatus = 1;
                            mResult.returnMsg = "登入失敗、密碼錯誤";
                        }
                    }

                }
                else
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "登入失敗、資料不等於1";
                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"登入失敗、{ex.Message}";
            }

            return mResult;
        }
    }
}
