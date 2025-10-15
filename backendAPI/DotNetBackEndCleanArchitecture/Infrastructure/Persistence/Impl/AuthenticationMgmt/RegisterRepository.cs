using DomainAbstraction.Interface.AuthenticationMgmt;
using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.Entity.AuthenticationMgmt.Register;
using InfraCommon.DBA;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Impl.AuthenticationMgmt
{
    public class RegisterRepository : IRegisterRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        public RegisterRepository(
                ILogger<RegisterRepository> logger,
                IDBAccess dbAccess,
                IOptions<AppConfig> appConfig
            )
        {
            _logger = logger;
            _dbAccess = dbAccess;
            _dbAccess.SetConnectionString(appConfig.Value.ConnectionStrings.BackEndDatabase);
            _strConnection = appConfig.Value.ConnectionStrings.BackEndDatabase;
        }

        public int CheckAccount(string id)
        {
            int mResult = 0;

            string mSql = @"
SELECT Count(*)
FROM   member_master AS A
WHERE  A.mm_account = @mm_account
       AND A.mm_status = 'Y' 
";
            mResult = _dbAccess.GetObject<int>(mSql, new { mm_account = id }).First();
            return mResult;
        }

        /// <summary>
        /// 註冊會員
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PostMember(RegisterDTO reqModel)
        {
            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                string mSqlGetMMSeq = @"SELECT NEXTVAL(rosca2.member_master_sequence);";

                string mSqlMaster = @"
INSERT INTO rosca2.member_master
            (mm_id
             ,mm_account
             ,mm_hash_pwd
             ,mm_2nd_hash_pwd
             ,mm_name
             ,mm_introduce_user
             ,mm_introduce_code
             ,mm_invite_user
             ,mm_invite_code
             ,mm_gender
             ,mm_country_code
             ,mm_personal_id
             ,mm_phone_number
             ,mm_mw_address
             ,mm_email
             ,mm_bank_code
             ,mm_bank_account
             ,mm_bank_account_name
             ,mm_branch
             ,mm_beneficiary_name
             ,mm_beneficiary_phone
             ,mm_beneficiary_relationship
             ,mm_level
             ,mm_role_type
             ,mm_status
             ,mm_kyc_id
             ,mm_kyc
             ,mm_create_member
             ,mm_create_datetime
             ,mm_update_member
             ,mm_update_datetime)
VALUES     (@mm_id
            ,@mm_account
            ,@mm_hash_pwd
            ,@mm_2nd_hash_pwd
            ,@mm_name
            ,@mm_introduce_user
            ,@mm_introduce_code
            ,@mm_invite_user
            ,@mm_invite_code
            ,@mm_gender
            ,@mm_country_code
            ,@mm_personal_id
            ,@mm_phone_number
            ,@mm_mw_address
            ,@mm_email
            ,null #@mm_bank_code
            ,null #@mm_bank_account
            ,null #@mm_bank_account_name
            ,null #@mm_branch
            ,null #@mm_beneficiary_name
            ,null #@mm_beneficiary_phone
            ,null #@mm_beneficiary_relationship
            ,'0'
            ,@mm_role_type
            ,'Y'
            ,0
            ,'N'
            ,LAST_INSERT_ID() + 1
            ,@mm_create_datetime
            ,LAST_INSERT_ID() + 1
            ,@mm_update_datetime); 
";

                string mSqlWallet = @"
INSERT INTO rosca2.member_wallet
            (mw_mm_id
             ,mw_currency
             ,mw_address
             ,mw_subscripts_count
             ,mw_stored
             ,mw_reward
             ,mw_peace
             ,mw_mall
             ,mw_registration
             ,mw_death
             ,mw_accumulation
             ,mw_create_member
             ,mw_create_datetime
             ,mw_update_member
             ,mw_update_datetime)
VALUES     (@mw_mm_id
            ,null #@mw_currency
            ,null #@mw_address
            ,0
            ,0
            ,0
            ,0
            ,0
            ,0
            ,0
            ,0
            ,@mw_mm_id
            ,@mw_create_datetime
            ,@mw_mm_id
            ,@mw_update_datetime); 
";
                //string m_sqlReplace = string.Empty;

                //判斷使否有邀請碼有就用帳號去找mm_id
                //m_sqlReplace = e_REQModel.mm_invite_code == 0 ? "0"
                //                  : "(SELECT A.mm_id FROM rosca.member_master AS A WHERE A.mm_account = @mm_invite_code AND A.mm_status = 'Y' LIMIT 1)";

                //m_sql = string.Format(m_sql, m_sqlReplace);
                if (!string.IsNullOrEmpty(reqModel.mm_introduce_user))
                {
                    string mSqlGetIntroduceID = @"
SELECT IFNULL((SELECT A.mm_id
               FROM   rosca2.member_master AS A
               WHERE  A.mm_account = @mm_account), 0)  AS mm_introduce_code
";
                    reqModel.mm_introduce_code = _dbAccess.GetObject<long>(mSqlGetIntroduceID, new { mm_account = reqModel.mm_introduce_user }).FirstOrDefault();
                    if (reqModel.mm_introduce_code == 0)
                    {
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "註冊失敗、介紹碼帳號錯誤";

                        return mResult;
                    }
                }

                if (!string.IsNullOrEmpty(reqModel.mm_invite_user))
                {
                    string mSqlGetIntroducerID = @"
SELECT IFNULL((SELECT A.mm_id
               FROM   rosca2.member_master AS A
               WHERE  A.mm_account = @mm_account), 0)  AS mm_introduce_code
";
                    reqModel.mm_invite_code = _dbAccess.GetObject<long>(mSqlGetIntroducerID, new { mm_account = reqModel.mm_invite_user }).FirstOrDefault();
                    if (reqModel.mm_invite_code == 0)
                    {
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "註冊失敗、邀請碼錯誤";

                        return mResult;
                    }
                }
                else
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "註冊失敗、邀請碼未輸入";

                    return mResult;
                }

                long mGetMMSeq = _dbAccess.GetObject<long>(mSqlGetMMSeq).First();
                if (mGetMMSeq == 0)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "註冊失敗、取得ID錯誤";

                    return mResult;
                }

                reqModel.mm_id = mGetMMSeq;

                member_wallet mMWEntity = new member_wallet();
                mMWEntity.mw_mm_id = mGetMMSeq;
                mMWEntity.mw_create_datetime = reqModel.mm_create_datetime;
                mMWEntity.mw_update_datetime = reqModel.mm_create_datetime;

                _dbAccess.BeginTrans();
                int mCNT = _dbAccess.ExecuteTransactionObject<RegisterDTO>(mSqlMaster, reqModel);

                if (mCNT == 1)
                {
                    mCNT = _dbAccess.ExecuteTransactionObject<member_wallet>(mSqlWallet, mMWEntity);

                    if (mCNT == 1)
                    {
                        _dbAccess.Commit();

                        mResult.Result = true;
                        mResult.returnStatus = 1;
                        mResult.returnMsg = "註冊成功";
                    }
                    else
                    {
                        _dbAccess.Rollback();

                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "註冊失敗、會員錢包新增筆數不等於1";
                    }

                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "註冊失敗、會員新增筆數不等於1";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"註冊失敗、{ex.Message}";
            }

            return mResult;
        }
    }
}
