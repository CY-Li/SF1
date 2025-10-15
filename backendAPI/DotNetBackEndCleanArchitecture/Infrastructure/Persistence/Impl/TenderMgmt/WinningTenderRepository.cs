using DomainAbstraction.Interface.TenderMgmt;
using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.Entity.TenderMgmt.Tender;
using DomainEntityDTO.Entity.TenderMgmt.Tender;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Impl.TenderMgmt
{
    public class WinningTenderRepository : IWinningTenderRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "WinningTenderRepository";

        public WinningTenderRepository(
                ILogger<WinningTenderRepository> logger,
                IDBAccess dbAccess,
                IOptions<AppConfig> appConfig
            )
        {
            _logger = logger;
            _dbAccess = dbAccess;
            _dbAccess.SetConnectionString(appConfig.Value.ConnectionStrings.BackEndDatabase);
            _strConnection = appConfig.Value.ConnectionStrings.BackEndDatabase;
        }

        public ServiceResultDTO<bool> WinningTender(WinningTenderReqModel reqModel)
        {
            string mActionLog = "WinningTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                string mSqlGetTMTD = @"
SELECT A.tm_id
       ,B.td_id
       ,A.tm_settlement_period
       ,A.tm_current_period
       ,A.tm_status
       ,B.td_participants
       ,B.td_period
       ,B.td_status
       ,C.mw_peace
FROM   rosca2.tender_master AS A
       INNER JOIN rosca2.tender_detail AS B
               ON B.td_tm_id = A.tm_id
       INNER JOIN rosca2.member_wallet AS C
               ON C.mw_mm_id = B.td_participants
WHERE  B.td_id = @td_id 
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
VALUES     (@mb_mm_id
            ,0
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
            ,@mb_update_datetime); 
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
             ,tr_settlement_time
             ,tr_mm_points
             ,tr_income_type
             ,tr_create_member
             ,tr_create_datetime
             ,tr_update_member
             ,tr_update_datetime)
VALUES     (@tr_code
            ,@tr_mm_id
            ,@tr_tm_id
            ,@tr_tm_id
            ,@tr_td_id
            ,@tr_pp_id
            ,@tr_type
            ,@tr_status
            ,@tr_settlement_time
            ,@tr_mm_points
            ,@tr_income_type
            ,@tr_create_member
            ,@tr_create_datetime
            ,@tr_update_member
            ,@tr_update_datetime); 
";

                string mSqlGetTrId = "SELECT NEXTVAL(rosca2.transaction_record_sequence)";

                string mSqlUpdateMW = @"
UPDATE rosca2.member_wallet
SET    mw_peace = mw_peace + @mw_peace
       ,mw_update_member = @mw_update_member
       ,mw_update_datetime = @mw_update_datetime
WHERE  mw_mm_id = @mw_mm_id 
";

                string mSqlUpdateTM = @"
UPDATE rosca2.tender_master
SET    tm_winners = CONCAT_WS('', tm_winners, CONCAT('|', @tm_winners))
       ,tm_current_period = tm_current_period + 1
       ,tm_update_member = @tm_update_member
       ,tm_update_datetime = @tm_update_datetime
WHERE  tm_id = @tm_id; 
";

                string mSqlUpdateTD = @"
UPDATE rosca2.tender_detail
SET    td_period = @td_period
       ,td_update_member = @td_update_member
       ,td_update_datetime = @td_update_datetime
WHERE  td_id = @td_id; 
";

                _dbAccess.BeginTrans();

                WinningTenderGetTMTD? mGetTMTDData = _dbAccess.ExecuteTransactionGetObject<WinningTenderGetTMTD>(mSqlGetTMTD, new { td_id = reqModel.td_id }).FirstOrDefault();

                if (mGetTMTDData == null)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "得標作業失敗，該標案明細找不到";
                }
                else if (mGetTMTDData.tm_settlement_period <= mGetTMTDData.tm_current_period)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "得標作業失敗，該標案還未執行該期扣款動作";
                }
                else if (mGetTMTDData.td_period != 0)
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "得標作業失敗，該標案明細已經得過標";
                }
                else if (mGetTMTDData.tm_status == "0")
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "得標作業失敗，該標案為程組";
                }
                else if (mGetTMTDData.td_status == "0")
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "得標作業失敗，該標案明細已得過標";
                }
                else
                {
                    decimal mPeace = 10000 * (mGetTMTDData.tm_current_period + 1) + 8000 - (200 * (mGetTMTDData.tm_current_period + 1));
                    member_balance mInsertBalance = new member_balance();
                    transaction_record mInsertTR = new transaction_record();
                    member_wallet mUpdateMW = new member_wallet();
                    DateTime mNowDateTime = GetTimeZoneInfo.process();
                    int mGetTrId = _dbAccess.ExecuteTransactionGetObject<int>(mSqlGetTrId, null).First();
                    string mTrCode = GetFieldID.process("tr", mGetTrId, mNowDateTime);
                    DateTime nextDays7 = new DateTime(mNowDateTime.Year, mNowDateTime.Month, mNowDateTime.Day).AddDays(7);
                    bool mIsOk = false;

                    mInsertBalance.mb_mm_id = mGetTMTDData.td_participants;
                    mInsertBalance.mb_payee_mm_id = 0;
                    mInsertBalance.mb_tm_id = mGetTMTDData.tm_id;
                    mInsertBalance.mb_td_id = mGetTMTDData.td_id;
                    mInsertBalance.mb_income_type = "I";
                    mInsertBalance.mb_tr_code = mTrCode;
                    mInsertBalance.mb_tr_type = "17";
                    mInsertBalance.mb_type = "18";
                    mInsertBalance.mb_points_type = "13";
                    mInsertBalance.mb_change_points = mPeace;
                    mInsertBalance.mb_points = mGetTMTDData.mw_peace + mPeace;
                    mInsertBalance.mb_create_member = reqModel.mm_id;
                    mInsertBalance.mb_create_datetime = mNowDateTime;
                    mInsertBalance.mb_update_member = reqModel.mm_id;
                    mInsertBalance.mb_update_datetime = mNowDateTime;

                    mInsertTR.tr_code = mTrCode;
                    mInsertTR.tr_mm_id = mGetTMTDData.td_participants;
                    mInsertTR.tr_payee_mm_id = 0;
                    mInsertTR.tr_tm_id = mGetTMTDData.tm_id;
                    mInsertTR.tr_td_id = mGetTMTDData.td_id;
                    mInsertTR.tr_pp_id = 0;
                    mInsertTR.tr_type = "17";
                    mInsertTR.tr_status = "30";
                    mInsertTR.tr_settlement_time = nextDays7;
                    mInsertTR.tr_mm_points = mPeace;
                    mInsertTR.tr_income_type = "I";
                    mInsertTR.tr_create_member = reqModel.mm_id;
                    mInsertTR.tr_create_datetime = mNowDateTime;
                    mInsertTR.tr_update_member = reqModel.mm_id;
                    mInsertTR.tr_update_datetime = mNowDateTime;

                    mIsOk = _dbAccess.ExecuteTransactionObject<member_balance>(mSqlInsertBalance, mInsertBalance) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"得標失敗、帳務紀錄更新筆數錯誤";

                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<transaction_record>(mSqlInsertTR, mInsertTR) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"得標失敗、交易紀錄更新筆數錯誤";

                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateMW, new
                    {
                        mw_peace = mPeace,
                        mw_update_member = reqModel.mm_id,
                        mw_mm_id = mGetTMTDData.td_participants,
                        mw_update_datetime = mNowDateTime
                    }) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"得標失敗、錢包更新筆數錯誤";

                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateTM, new
                    {
                        tm_winners = mGetTMTDData.td_participants,
                        tm_update_member = reqModel.mm_id,
                        tm_update_datetime = mNowDateTime,
                        tm_id = mGetTMTDData.tm_id
                    }) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"得標失敗、標案更新筆數錯誤";

                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateTD, new
                    {
                        td_period = mGetTMTDData.tm_current_period + 1,
                        td_update_member = reqModel.mm_id,
                        td_update_datetime = mNowDateTime,
                        td_id = reqModel.td_id
                    }) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"得標失敗、標案明細更新筆數錯誤";

                        return mResult;
                    }
                    else
                    {
                        //_dbAccess.Rollback();
                        _dbAccess.Commit();

                        mResult.Result = true;
                        mResult.returnStatus = 0;
                        mResult.returnMsg = $@"得標成功";
                    }
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"得標失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
