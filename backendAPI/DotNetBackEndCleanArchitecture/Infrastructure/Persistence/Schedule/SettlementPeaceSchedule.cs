using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.ScheduleEntity.SettlementPeaceSchedule;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Schedule
{
    public class SettlementPeaceSchedule
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "SettlementPeaceSchedule";

        public SettlementPeaceSchedule(
                ILogger<SettlementPeaceSchedule> logger,
                IDBAccess dbAccess,
                IOptions<AppConfig> appConfig
            )
        {
            _logger = logger;
            _dbAccess = dbAccess;
            _dbAccess.SetConnectionString(appConfig.Value.ConnectionStrings.BackEndDatabase);
            _strConnection = appConfig.Value.ConnectionStrings.BackEndDatabase;
        }

        public ServiceResultDTO<bool> Process()
        {
            string mActionLog = "Process";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();
            try
            {
                string mSqlGetSettlementPeaceData = @"
SELECT A.tr_id
       ,A.tr_code
       ,A.tr_mm_id
       ,A.tr_tm_id
       ,A.tr_type
       ,A.tr_status
       ,A.tr_settlement_time
       ,A.tr_mm_points
       ,B.mw_stored
       ,B.mw_peace
       ,B.mw_mall
       ,B.mw_punish
       ,C.td_period
       ,C.td_pp_penalty_count
       ,C.td_pp_paid 
FROM   rosca2.transaction_record AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.tr_mm_id
       INNER JOIN rosca2.tender_detail AS C
               ON C.td_id = A.tr_td_id 
WHERE  A.tr_type = '17'
       AND A.tr_status = '30'
       AND C.td_pp_paid  = 0
       #AND @nowDateTime > A.tr_settlement_time 
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

                string mSqlUpdateMW = @"
UPDATE rosca2.member_wallet
SET    mw_peace = mw_peace - @mw_peace
       ,mw_stored = mw_stored + @mw_stored
       ,mw_mall = mw_mall + @mw_mall
       ,mw_accumulation = mw_accumulation + @mw_accumulation
       ,mw_update_member = @mw_update_member
       ,mw_update_datetime = @mw_update_datetime
WHERE  mw_mm_id = @mw_mm_id 
";

                string mSqlUpdateTR = @"
UPDATE rosca2.transaction_record
SET    tr_status = '50'
       ,tr_update_member = 0
       ,tr_update_datetime = @tr_update_datetime
WHERE  tr_id IN ({0}); 
";

                _dbAccess.BeginTrans();
                List<SettlementPeacePO> mGetTrData = _dbAccess.ExecuteTransactionGetObject<SettlementPeacePO>(mSqlGetSettlementPeaceData, null).ToList();

                List<member_balance> mInsertBalance = new List<member_balance>();
                Dictionary<long, SettlementPeaceInsertBalancePoint> mOriginalWallet = new Dictionary<long, SettlementPeaceInsertBalancePoint>();
                DateTime mNowDateTime = GetTimeZoneInfo.process();
                int mCheckCnt = 0;
                int mTRCnt = 0;
                int mMWCnt = 0;
                bool mIsOk = false;

                foreach (var item in mGetTrData)
                {
                    if (item.td_pp_paid == 0)
                    {
                        if (!mOriginalWallet.ContainsKey(item.tr_mm_id))
                        {
                            mOriginalWallet.Add(item.tr_mm_id, new SettlementPeaceInsertBalancePoint { mw_peace = item.mw_peace, mw_stored = item.mw_stored, mw_mall = item.mw_mall });
                            mMWCnt++;
                        }

                        SettlementPeaceInsertBalancePoint originalWalletItem = mOriginalWallet[item.tr_mm_id];

                        int mInterest = 1800 * item.td_period;
                        decimal mStored = item.tr_mm_points - (mInterest * 0.1m) - (2000 * item.td_pp_penalty_count);
                        decimal mMall = mInterest * 0.1m;
                        decimal mPunish = 2000 * item.td_pp_penalty_count;

                        originalWalletItem.mw_peace -= item.tr_mm_points;
                        originalWalletItem.mw_stored += mStored;
                        originalWalletItem.mw_mall += mMall;
                        originalWalletItem.mw_peace_chenge += item.tr_mm_points;
                        originalWalletItem.mw_stored_chenge += mStored;
                        originalWalletItem.mw_mall_chenge += mMall;
                        originalWalletItem.mw_accumulation_change += mInterest;
                        originalWalletItem.mw_punish += mPunish;
                        originalWalletItem.mw_punish_change += mPunish;

                        mInsertBalance.Add(new member_balance()
                        {
                            mb_mm_id = item.tr_mm_id,
                            mb_payee_mm_id = 0,
                            mb_tm_id = item.tr_tm_id,
                            mb_td_id = 0,
                            mb_income_type = "O",
                            mb_tr_code = item.tr_code,
                            mb_tr_type = item.tr_type,
                            mb_type = "18",
                            mb_points_type = "13",//平安點數
                            mb_change_points = item.tr_mm_points,
                            mb_points = originalWalletItem.mw_peace,
                            mb_create_member = item.tr_mm_id,
                            mb_create_datetime = mNowDateTime,
                            mb_update_member = item.tr_mm_id,
                            mb_update_datetime = mNowDateTime
                        });

                        mInsertBalance.Add(new member_balance()
                        {
                            mb_mm_id = item.tr_mm_id,
                            mb_payee_mm_id = 0,
                            mb_tm_id = item.tr_tm_id,
                            mb_td_id = 0,
                            mb_income_type = "I",
                            mb_tr_code = item.tr_code,
                            mb_tr_type = item.tr_type,
                            mb_type = "19",
                            mb_points_type = "11",
                            mb_change_points = 8000,//歸還押金
                            mb_points = originalWalletItem.mw_stored,
                            mb_create_member = item.tr_mm_id,
                            mb_create_datetime = mNowDateTime,
                            mb_update_member = item.tr_mm_id,
                            mb_update_datetime = mNowDateTime
                        });

                        mInsertBalance.Add(new member_balance()
                        {
                            mb_mm_id = item.tr_mm_id,
                            mb_payee_mm_id = 0,
                            mb_tm_id = item.tr_tm_id,
                            mb_td_id = 0,
                            mb_income_type = "I",
                            mb_tr_code = item.tr_code,
                            mb_tr_type = item.tr_type,
                            mb_type = "20",
                            mb_points_type = "11",
                            mb_change_points = mStored,//儲值點數
                            mb_points = originalWalletItem.mw_stored,
                            mb_create_member = item.tr_mm_id,
                            mb_create_datetime = mNowDateTime,
                            mb_update_member = item.tr_mm_id,
                            mb_update_datetime = mNowDateTime
                        });

                        mInsertBalance.Add(new member_balance()
                        {
                            mb_mm_id = item.tr_mm_id,
                            mb_payee_mm_id = 0,
                            mb_tm_id = item.tr_tm_id,
                            mb_td_id = 0,
                            mb_income_type = "I",
                            mb_tr_code = item.tr_code,
                            mb_tr_type = item.tr_type,
                            mb_type = "20",
                            mb_points_type = "14",//商城點數
                            mb_change_points = mMall,
                            mb_points = originalWalletItem.mw_mall,
                            mb_create_member = item.tr_mm_id,
                            mb_create_datetime = mNowDateTime,
                            mb_update_member = item.tr_mm_id,
                            mb_update_datetime = mNowDateTime
                        });

                        if (item.td_pp_penalty_count > 0)
                        {
                            mInsertBalance.Add(new member_balance()
                            {
                                mb_mm_id = item.tr_mm_id,
                                mb_payee_mm_id = 0,
                                mb_tm_id = item.tr_tm_id,
                                mb_td_id = 0,
                                mb_income_type = "I",
                                mb_tr_code = item.tr_code,
                                mb_tr_type = item.tr_type,
                                mb_type = "25",
                                mb_points_type = "18",
                                mb_change_points = mPunish,
                                mb_points = originalWalletItem.mw_punish,
                                mb_create_member = item.tr_mm_id,
                                mb_create_datetime = mNowDateTime,
                                mb_update_member = item.tr_mm_id,
                                mb_update_datetime = mNowDateTime
                            });
                            mCheckCnt++;
                        }

                        mCheckCnt += 4;
                        mTRCnt++;
                    }
                }

                var mUpdateWallet = mOriginalWallet.Select(kv => new
                {
                    mw_peace = kv.Value.mw_peace_chenge,
                    mw_stored = kv.Value.mw_stored_chenge,
                    mw_mall = kv.Value.mw_mall_chenge,
                    mw_accumulation = kv.Value.mw_accumulation_change,
                    mw_update_member = kv.Key,
                    mw_update_datetime = mNowDateTime,
                    mw_mm_id = kv.Key
                }).ToList();

                mIsOk = _dbAccess.ExecuteTransactionObject<List<member_balance>>(mSqlInsertBalance, mInsertBalance) == (mCheckCnt);
                if (!mIsOk)
                {
                    _dbAccess.Rollback();

                    mResult.returnStatus = 999;
                    mResult.returnMsg = "平安點數排程失敗、新增帳務紀錄筆數錯誤";
                    return mResult;
                }

                mIsOk = _dbAccess.ExecuteTransactionObject<dynamic>(mSqlUpdateMW, mUpdateWallet) == mMWCnt;
                if (!mIsOk)
                {
                    _dbAccess.Rollback();

                    mResult.returnStatus = 999;
                    mResult.returnMsg = "平安點數排程失敗、更新錢包筆數錯誤";
                    return mResult;
                }

                mSqlUpdateTR = string.Format(mSqlUpdateTR, string.Join(',', mGetTrData.Select(s => s.tr_id)));
                mIsOk = _dbAccess.ExecuteTransactionObject<dynamic>(mSqlUpdateTR, new { tr_update_datetime = mNowDateTime }) == mTRCnt;
                if (!mIsOk)
                {
                    _dbAccess.Rollback();

                    mResult.returnStatus = 999;
                    mResult.returnMsg = "平安點數排程失敗、更新交易紀錄筆數錯誤";
                    return mResult;
                }
                else
                {
                    //_dbAccess.Rollback();
                    _dbAccess.Commit();
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
                throw;
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
