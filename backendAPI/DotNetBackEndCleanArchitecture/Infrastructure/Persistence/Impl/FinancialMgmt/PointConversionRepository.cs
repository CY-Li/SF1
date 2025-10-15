using DomainAbstraction.Interface.FinancialMgmt;
using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.Entity.FinancialMgmt.PointConversion;
using DomainEntity.Entity.TenderMgmt.Tender;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Persistence.Impl.FinancialMgmt
{
    public class PointConversionRepository : IPointConversionRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "PointConversionRepository";

        public PointConversionRepository(
                ILogger<PointConversionRepository> logger,
                IDBAccess dbAccess,
                IOptions<AppConfig> appConfig
            )
        {
            _logger = logger;
            _dbAccess = dbAccess;
            _dbAccess.SetConnectionString(appConfig.Value.ConnectionStrings.BackEndDatabase);
            _strConnection = appConfig.Value.ConnectionStrings.BackEndDatabase;
        }

        /// <summary>
        /// 會員-取得交易資料
        /// </summary>
        /// <returns></returns>
        public ServiceResultDTO<GettransactionDataRespModel> GetTransactionData(long mm_id)
        {
            string mActionLog = "GetTransactionData";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GettransactionDataRespModel> mResult = new ServiceResultDTO<GettransactionDataRespModel>();
            try
            {
                string m_sql = @"
SELECT A.mm_kyc_id
       ,B.akc_mw_address
       ,B.akc_bank_code
       ,B.akc_bank_account
       ,B.akc_bank_account_name
       ,B.akc_branch
       ,C.sps_parameter01 AS company_bank_code
       ,C.sps_parameter02 AS company_bank_account
       ,C.sps_parameter03 AS company_bank_account_name
       ,C.sps_parameter04 AS company_branch
       ,C.sps_parameter05 AS deposit_rate
       ,C.sps_parameter06 AS withdraw_rate
FROM   rosca2.member_master AS A
       INNER JOIN rosca2.apply_kyc_certification AS B
               ON B.akc_id = A.mm_kyc_id
       INNER JOIN rosca2.system_parameter_setting AS C
               ON C.sps_code = 'TransactionSetting'
WHERE  A.mm_id = @mm_id 


";
                mResult.Result = _dbAccess.GetObject<GettransactionDataRespModel>(m_sql, new { mm_id = mm_id }).FirstOrDefault();
                if (mResult.Result == null
                    || string.IsNullOrEmpty(mResult.Result.akc_mw_address)
                    || string.IsNullOrEmpty(mResult.Result.deposit_rate))
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "取得交易資料失敗";
                }
                else
                {
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "取得交易資料成功";
                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得交易資料失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-點數轉換
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PostPointConversion(PostPointConversionDTO reqModel)
        {
            string mActionLog = "PostPointConversion";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                //取得申請人member_master資料並鎖定，該部分取得的資料都是儲值人
                string mSqlGetMMConditionData = @"
SELECT A.mm_id
       ,B.mw_stored
       ,B.mw_registration
       ,(SELECT NEXTVAL(rosca2.transaction_record_sequence)) AS tr_id
FROM   rosca2.member_master AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.mm_id
WHERE  A.mm_id = @mm_id
       AND A.mm_status = 'Y' 
FOR UPDATE 
";

                string mSqlInsertBalance = @"
INSERT INTO rosca2.member_balance
            (mb_mm_id
             ,mb_payee_mm_id
             ,mb_tm_id
             ,mb_td_id
             ,mb_income_type
             ,mb_tr_code
             ,mb_tr_type
             ,mb_type
             ,mb_points_type
             ,mb_change_points
             ,mb_points
             ,mb_create_member
             ,mb_create_datetime
             ,mb_update_member
             ,mb_update_datetime)
VALUES      (@mb_mm_id
             ,@mb_payee_mm_id
             ,@mb_tm_id
             ,@mb_td_id
             ,@mb_income_type
             ,@mb_tr_code
             ,@mb_tr_type
             ,@mb_type
             ,@mb_points_type
             ,@mb_change_points
             ,@mb_points
             ,@mb_create_member
             ,@mb_create_datetime
             ,@mb_update_member
             ,@mb_create_datetime); 
";

                string mSqlInsertTR = @"
INSERT INTO rosca2.transaction_record
            (tr_code
             ,tr_mm_id
             ,tr_payee_mm_id
             ,tr_tm_id
             ,tr_td_id
             ,tr_pp_id
             ,tr_type
             ,tr_status
             ,tr_mm_points
             ,tr_income_type
             ,tr_create_member
             ,tr_create_datetime
             ,tr_update_member
             ,tr_update_datetime)
VALUES      (@tr_code
             ,@tr_mm_id
             ,@tr_payee_mm_id
             ,@tr_tm_id
             ,@tr_td_id
             ,@tr_pp_id
             ,@tr_type
             ,@tr_status
             ,@tr_mm_points
             ,@tr_income_type
             ,@tr_create_member
             ,@tr_create_datetime
             ,@tr_update_member
             ,@tr_update_datetime); 
";

                string mSqlUpdateMW = @"
UPDATE rosca2.member_wallet
SET    mw_stored = mw_stored - @mw_stored
       ,mw_registration = mw_registration + @mw_registration
       ,mw_update_member = @mm_id
       ,mw_update_datetime = @update_datetime
WHERE  mw_mm_id = @mm_id; 
";

                _dbAccess.BeginTrans();

                List<member_balance> mInsertBalance = new List<member_balance>();
                transaction_record mInsertTR = new transaction_record();

                PostPointConversionPO mGetMMConditionData = _dbAccess.ExecuteTransactionGetObject<PostPointConversionPO>(mSqlGetMMConditionData, reqModel).First();
                if (mGetMMConditionData.mw_stored <= 0)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "轉換註冊點數失敗、錢包點數不夠";

                    return mResult;
                }
                else if (mGetMMConditionData.mw_stored < reqModel.amount)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "轉換註冊點數失敗、錢包點數少於轉換點數";

                    return mResult;
                }

                string mTrCode = GetFieldID.process("tr", mGetMMConditionData.tr_id, reqModel.update_datetime);

                //新增帳務紀錄
                mInsertBalance.Add(new member_balance()
                {
                    mb_mm_id = mGetMMConditionData.mm_id,
                    mb_payee_mm_id = 0,
                    mb_tm_id = 0,
                    mb_td_id = 0,
                    mb_income_type = "O",
                    mb_tr_code = mTrCode,
                    mb_tr_type = "21",
                    mb_type = "24",
                    mb_points_type = "11",
                    mb_change_points = reqModel.amount,
                    mb_points = mGetMMConditionData.mw_stored - reqModel.amount,
                    mb_create_member = reqModel.mm_id,
                    mb_create_datetime = reqModel.update_datetime,
                    mb_update_member = reqModel.mm_id,
                    mb_update_datetime = reqModel.update_datetime,
                });

                mInsertBalance.Add(new member_balance()
                {
                    mb_mm_id = mGetMMConditionData.mm_id,
                    mb_payee_mm_id = 0,
                    mb_tm_id = 0,
                    mb_td_id = 0,
                    mb_income_type = "I",
                    mb_tr_code = mTrCode,
                    mb_tr_type = "21",
                    mb_type = "24",
                    mb_points_type = "15",
                    mb_change_points = reqModel.amount,
                    mb_points = mGetMMConditionData.mw_registration + reqModel.amount,
                    mb_create_member = reqModel.mm_id,
                    mb_create_datetime = reqModel.update_datetime,
                    mb_update_member = reqModel.mm_id,
                    mb_update_datetime = reqModel.update_datetime,
                });

                //新增交易紀錄
                mInsertTR.tr_code = mTrCode;
                mInsertTR.tr_mm_id = mGetMMConditionData.mm_id;
                mInsertTR.tr_payee_mm_id = 0;
                mInsertTR.tr_tm_id = 0;
                mInsertTR.tr_td_id = 0;
                mInsertTR.tr_pp_id = 0;
                mInsertTR.tr_type = "21";
                mInsertTR.tr_status = "90";
                mInsertTR.tr_mm_points = reqModel.amount;
                mInsertTR.tr_income_type = "F";
                mInsertTR.tr_create_member = reqModel.mm_id;
                mInsertTR.tr_create_datetime = reqModel.update_datetime;
                mInsertTR.tr_update_member = reqModel.mm_id;
                mInsertTR.tr_update_datetime = reqModel.update_datetime;

                mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlInsertBalance, mInsertBalance) == 2;
                if (!mResult.Result)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "點數轉換失敗、寫入帳務紀錄筆數不等於2";

                    return mResult;
                }

                mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlInsertTR, mInsertTR) == 1;
                if (!mResult.Result)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "點數轉換失敗、寫入交易紀錄筆數不等於1";

                    return mResult;
                }

                mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlUpdateMW, new
                {
                    mw_stored = reqModel.amount,
                    mw_registration = reqModel.amount,
                    mm_id = reqModel.mm_id,
                    update_datetime = reqModel.update_datetime,
                }) == 1;
                if (!mResult.Result)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "點數轉換失敗、更新會員錢包筆數不等於1";

                    return mResult;
                }
                else
                {
                    //_dbAccess.Rollback();
                    _dbAccess.Commit();
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "點數轉換成功成功";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"點數轉換失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-點數贈與
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PostGiftPoint(PostGiftPointDTO reqModel)
        {
            string mActionLog = "PostGiftPoint";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                //取得申請人member_master資料並鎖定，該部分取得的資料都是儲值人
                string mSqlGetMMConditionData = @"
SELECT A.mm_id
       ,A.mm_invite_code
       ,A.mm_2nd_hash_pwd
       ,B.mw_stored
       ,B.mw_registration
       ,(SELECT NEXTVAL(rosca2.transaction_record_sequence)) AS tr_id
FROM   rosca2.member_master AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.mm_id
WHERE  A.mm_id = @mm_id
       AND A.mm_status = 'Y' 
FOR UPDATE 
";

                string mSqlGetRecipientMMConditionData = @"
WITH RECURSIVE GetSubordinateOrgCTE AS
(
        SELECT A.mm_id
               ,A.mm_invite_code
               ,1 AS class_level
        FROM   rosca2.member_master AS A
        WHERE  A.mm_id = @mm_id # 找出第一個要請人
        
        UNION ALL
        
        SELECT B.mm_id
               ,B.mm_invite_code
               ,C.class_level + 1
        FROM   rosca2.member_master B
               INNER JOIN GetSubordinateOrgCTE C
                       ON B.mm_invite_code  = C.mm_id 
),
GetSuperiorOrgCTE AS
(
        SELECT A.mm_id
               ,A.mm_invite_code
               ,1 AS class_level
        FROM   rosca2.member_master AS A
        WHERE  A.mm_id = @mm_id # 找出第一個要請人
        
        UNION ALL
        
        SELECT B.mm_id
               ,B.mm_invite_code
               ,C.class_level + 1
        FROM   rosca2.member_master B
               INNER JOIN GetSubordinateOrgCTE C
                       ON B.mm_id  = C.mm_invite_code 
)

SELECT D.mm_id
       ,F.mw_registration
FROM   (SELECT mm_id
        FROM   GetSubordinateOrgCTE
        UNION
        SELECT mm_id
        FROM   GetSuperiorOrgCTE) AS D
       INNER JOIN rosca2.member_master AS E
               ON E.mm_id = D.mm_id 
       INNER JOIN rosca2.member_wallet AS F
               ON F.mw_mm_id = D.mm_id
WHERE  E.mm_account = @recipient  
FOR UPDATE 
";
                //                string mSqlGetRecipientMMConditionData = @"
                //SELECT C.mm_id
                //       ,D.mw_registration
                //FROM   (SELECT A.mm_id
                //               ,A.mm_account
                //        FROM   rosca2.member_master AS A
                //        WHERE  A.mm_id = @mm_invite_code
                //               AND A.mm_status = 'Y'
                //        UNION ALL
                //        SELECT B.mm_id
                //               ,B.mm_account
                //        FROM   rosca2.member_master AS B
                //        WHERE  B.mm_invite_code = @mm_id
                //               AND B.mm_status = 'Y') AS C
                //       INNER JOIN rosca2.member_wallet AS D
                //               ON D.mw_mm_id = C.mm_id
                //WHERE  C.mm_account = @recipient 
                //FOR UPDATE 
                //";

                string mSqlInsertBalance = @"
INSERT INTO rosca2.member_balance
            (mb_mm_id
             ,mb_payee_mm_id
             ,mb_tm_id
             ,mb_td_id
             ,mb_income_type
             ,mb_tr_code
             ,mb_tr_type
             ,mb_type
             ,mb_points_type
             ,mb_change_points
             ,mb_points
             ,mb_create_member
             ,mb_create_datetime
             ,mb_update_member
             ,mb_update_datetime)
VALUES      (@mb_mm_id
             ,@mb_payee_mm_id
             ,@mb_tm_id
             ,@mb_td_id
             ,@mb_income_type
             ,@mb_tr_code
             ,@mb_tr_type
             ,@mb_type
             ,@mb_points_type
             ,@mb_change_points
             ,@mb_points
             ,@mb_create_member
             ,@mb_create_datetime
             ,@mb_update_member
             ,@mb_create_datetime); 
";

                string mSqlInsertTR = @"
INSERT INTO rosca2.transaction_record
            (tr_code
             ,tr_mm_id
             ,tr_payee_mm_id
             ,tr_tm_id
             ,tr_td_id
             ,tr_pp_id
             ,tr_type
             ,tr_status
             ,tr_mm_points
             ,tr_income_type
             ,tr_create_member
             ,tr_create_datetime
             ,tr_update_member
             ,tr_update_datetime)
VALUES      (@tr_code
             ,@tr_mm_id
             ,@tr_payee_mm_id
             ,@tr_tm_id
             ,@tr_td_id
             ,@tr_pp_id
             ,@tr_type
             ,@tr_status
             ,@tr_mm_points
             ,@tr_income_type
             ,@tr_create_member
             ,@tr_create_datetime
             ,@tr_update_member
             ,@tr_update_datetime); 
";

                string mSqlUpdateMW = @"
UPDATE rosca2.member_wallet
SET    mw_registration = mw_registration - @mw_registration
       ,mw_update_member = @mm_id
       ,mw_update_datetime = @update_datetime
WHERE  mw_mm_id = @mm_id; 
";

                string mSqlUpdateRecipientMW = @"
UPDATE rosca2.member_wallet
SET    mw_registration = mw_registration + @mw_registration
       ,mw_update_member = @mm_id
       ,mw_update_datetime = @update_datetime
WHERE  mw_mm_id = @recipient; 
";

                _dbAccess.BeginTrans();

                List<member_balance> mInsertBalance = new List<member_balance>();
                transaction_record mInsertTR = new transaction_record();

                PostGiftPointPO mGetMMConditionData = _dbAccess.ExecuteTransactionGetObject<PostGiftPointPO>(mSqlGetMMConditionData, reqModel).First();

                if (mGetMMConditionData.mw_registration <= 0)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "贈送註冊點數失敗、錢包點數不夠";

                    return mResult;
                }
                else if (mGetMMConditionData.mw_registration < reqModel.amount)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "贈送註冊點數失敗、錢包點數少於轉換點數";

                    return mResult;
                }

                bool verify = HashProcess.VerifyHashPWD(reqModel.mm_2nd_pwd, mGetMMConditionData.mm_2nd_hash_pwd);
                if (!verify)
                {
                    _dbAccess.Rollback();

                    mResult.returnStatus = 999;
                    mResult.returnMsg = "贈送註冊點數失敗、第2密碼錯誤";
                    return mResult;
                }

                PostGiftPointPO? mGetRecipientMMConditionData = _dbAccess.ExecuteTransactionGetObject<PostGiftPointPO>(mSqlGetRecipientMMConditionData
                    , new
                    {
                        mm_id = mGetMMConditionData.mm_id
                        ,recipient = reqModel.recipient
                    }).FirstOrDefault();

                if (mGetRecipientMMConditionData == null || mGetRecipientMMConditionData.mm_id == 0)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "贈送註冊點數失敗、找不到接受點數者";

                    return mResult;
                }

                string mTrCode = GetFieldID.process("tr", mGetMMConditionData.tr_id, reqModel.update_datetime);

                //新增帳務紀錄
                mInsertBalance.Add(new member_balance()
                {
                    mb_mm_id = mGetMMConditionData.mm_id,
                    mb_payee_mm_id = mGetRecipientMMConditionData.mm_id,
                    mb_tm_id = 0,
                    mb_td_id = 0,
                    mb_income_type = "O",
                    mb_tr_code = mTrCode,
                    mb_tr_type = "18",
                    mb_type = "21",
                    mb_points_type = "15",
                    mb_change_points = reqModel.amount,
                    mb_points = mGetMMConditionData.mw_registration - reqModel.amount,
                    mb_create_member = reqModel.mm_id,
                    mb_create_datetime = reqModel.update_datetime,
                    mb_update_member = reqModel.mm_id,
                    mb_update_datetime = reqModel.update_datetime,
                });

                mInsertBalance.Add(new member_balance()
                {
                    mb_mm_id = mGetRecipientMMConditionData.mm_id,
                    mb_payee_mm_id = mGetMMConditionData.mm_id,
                    mb_tm_id = 0,
                    mb_td_id = 0,
                    mb_income_type = "I",
                    mb_tr_code = mTrCode,
                    mb_tr_type = "18",
                    mb_type = "21",
                    mb_points_type = "15",
                    mb_change_points = reqModel.amount,
                    mb_points = mGetRecipientMMConditionData.mw_registration + reqModel.amount,
                    mb_create_member = reqModel.mm_id,
                    mb_create_datetime = reqModel.update_datetime,
                    mb_update_member = reqModel.mm_id,
                    mb_update_datetime = reqModel.update_datetime,
                });

                //新增交易紀錄
                mInsertTR.tr_code = mTrCode;
                mInsertTR.tr_mm_id = mGetMMConditionData.mm_id;
                mInsertTR.tr_payee_mm_id = mGetRecipientMMConditionData.mm_id;
                mInsertTR.tr_tm_id = 0;
                mInsertTR.tr_td_id = 0;
                mInsertTR.tr_pp_id = 0;
                mInsertTR.tr_type = "18";
                mInsertTR.tr_status = "90";
                mInsertTR.tr_mm_points = reqModel.amount;
                mInsertTR.tr_income_type = "F";
                mInsertTR.tr_create_member = reqModel.mm_id;
                mInsertTR.tr_create_datetime = reqModel.update_datetime;
                mInsertTR.tr_update_member = reqModel.mm_id;
                mInsertTR.tr_update_datetime = reqModel.update_datetime;

                mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlInsertBalance, mInsertBalance) == 2;
                if (!mResult.Result)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "贈送點數失敗、寫入帳務紀錄筆數不等於2";

                    return mResult;
                }

                mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlInsertTR, mInsertTR) == 1;
                if (!mResult.Result)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "贈送點數失敗、寫入交易紀錄筆數不等於1";

                    return mResult;
                }

                mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlUpdateMW, new
                {
                    mw_registration = reqModel.amount,
                    mm_id = reqModel.mm_id,
                    update_datetime = reqModel.update_datetime,
                }) == 1;
                if (!mResult.Result)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "贈送點數失敗、更新贈送會員錢包筆數不等於1";

                    return mResult;
                }

                mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlUpdateRecipientMW, new
                {
                    mw_registration = reqModel.amount,
                    mm_id = reqModel.mm_id,
                    recipient = mGetRecipientMMConditionData.mm_id,
                    update_datetime = reqModel.update_datetime,
                }) == 1;
                if (!mResult.Result)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "贈送點數失敗、更新點數接收會員錢包筆數不等於1";

                    return mResult;
                }
                else
                {
                    //_dbAccess.Rollback();
                    _dbAccess.Commit();
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "贈送點數成功";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"贈送點數失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
