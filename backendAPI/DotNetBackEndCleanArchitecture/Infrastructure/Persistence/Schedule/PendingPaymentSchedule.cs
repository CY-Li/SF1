using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.ScheduleEntity.PendingPaymentSchedule;
using DomainEntity.Shared;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Persistence.Schedule
{
    public class PendingPaymentSchedule
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "PendingPaymentSchedule";

        public PendingPaymentSchedule(
                ILogger<PendingPaymentSchedule> logger,
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
                string mSqlGetPP = @"
SELECT A.pp_sn
       ,A.pp_id
       ,A.pp_mm_id
       ,A.pp_tr_code
       ,A.pp_tm_id
       ,A.pp_td_id
       ,A.pp_amount
       ,A.pp_penalty_datetime
       ,B.mw_stored
FROM   rosca2.pending_payment AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.pp_mm_id
WHERE  A.pp_status = '10' 
#FOR UPDATE
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

                string mSqlInsertTar = @"
INSERT INTO rosca2.tender_activity_record
            (tar_mm_id
             ,tar_mm_introduce_code
             ,tar_tm_id
             ,tar_tm_count
             ,tar_tr_type
             ,tar_tr_code
             ,tar_status
             ,tar_json
             ,tar_create_member
             ,tar_create_datetime
             ,tar_update_member
             ,tar_update_datetime)
VALUES     (@tar_mm_id
            ,@tar_mm_introduce_code
            ,@tar_tm_id
            ,@tar_tm_count
            ,@tar_tr_type
            ,@tar_tr_code
            ,0
            ,@tar_json
            ,@tar_mm_id
            ,@tar_create_datetime
            ,@tar_mm_id
            ,@tar_update_datetime); 
";

                string mSqlUpdateMW = @"
UPDATE rosca2.member_wallet
SET    mw_stored = mw_stored - @mw_stored
       ,mw_accumulation = mw_accumulation + @mw_stored
       ,mw_update_member = @mw_update_member
       ,mw_update_datetime = @mw_update_datetime
WHERE  mw_mm_id = @mw_mm_id 
";

                string mSqlUpdateTD = @"
UPDATE rosca2.tender_detail
SET    td_pp_penalty_count = td_pp_penalty_count + @td_pp_penalty_count
       ,td_pp_paid = td_pp_paid - 1
       ,td_update_member = 0
       ,td_update_datetime = @td_update_datetime
WHERE  td_id = @td_id; 
";

                _dbAccess.BeginTrans();
                List<PendingPaymentSchedulePO> mGetPPData = _dbAccess.ExecuteTransactionGetObject<PendingPaymentSchedulePO>(mSqlGetPP, null).ToList();

                List<member_balance> mInsertBalance = new List<member_balance>();
                List<transaction_record> mInsertTR = new List<transaction_record>();
                List<tender_activity_record> mInsertTar = new List<tender_activity_record>();
                List<member_wallet> mUpdateMW = new List<member_wallet>();
                Dictionary<long, PendingPaymentSkdPoint> mOriginalWallet = new Dictionary<long, PendingPaymentSkdPoint>();//邀請人紅利
                Dictionary<long, int> mUpdateTD = new Dictionary<long, int>();
                DateTime mNowDateTime = GetTimeZoneInfo.process();
                int mMWCnt = 0;
                int mMBCnt = 0;
                //int mTDCnt = 0;
                bool mAmountFlage = false;
                bool mIsOk = false;

                if (mGetPPData.Count > 0)
                {
                    foreach (var item in mGetPPData)
                    {
                        PendingPaymentSkdPoint originalWalletItem = new PendingPaymentSkdPoint();
                        if (!mOriginalWallet.ContainsKey(item.pp_mm_id))
                        {
                            if (item.mw_stored > 8000)
                            {
                                mAmountFlage = true;
                                mOriginalWallet.Add(item.pp_mm_id, new PendingPaymentSkdPoint { mw_stored = item.mw_stored });
                                originalWalletItem = mOriginalWallet[item.pp_mm_id];
                                mMWCnt++;
                            }
                            else
                            {
                                mAmountFlage = false;
                            }
                        }
                        else
                        {
                            originalWalletItem = mOriginalWallet[item.pp_mm_id];
                            if (originalWalletItem.mw_stored > 8000)
                            {
                                mAmountFlage = true;
                            }
                            else
                            {
                                mAmountFlage = false;
                            }
                        }

                        if (mAmountFlage)
                        {
                            mMBCnt++;

                            mInsertBalance.Add(new member_balance()
                            {
                                mb_mm_id = item.pp_mm_id,
                                mb_payee_mm_id = 0,
                                mb_tm_id = item.pp_tm_id,
                                mb_td_id = item.pp_td_id,
                                mb_income_type = "O",
                                mb_tr_code = item.pp_tr_code,
                                mb_tr_type = "14",
                                mb_type = "15",
                                mb_points_type = "11",
                                mb_change_points = 8000,
                                mb_points = originalWalletItem.mw_stored,
                                mb_create_member = 0,
                                mb_create_datetime = mNowDateTime,
                                mb_update_member = 0,
                                mb_update_datetime = mNowDateTime
                            });


                            mInsertTR.Add(new transaction_record()
                            {
                                tr_code = item.pp_tr_code,
                                tr_mm_id = item.pp_mm_id,
                                tr_payee_mm_id = 0,
                                tr_tm_id = item.pp_tm_id,
                                tr_td_id = item.pp_td_id,
                                tr_pp_id = item.pp_id,
                                tr_type = "14",
                                tr_status = "90",
                                tr_mm_points = 8000,
                                tr_income_type = "O",
                                tr_create_member = 0,
                                tr_create_datetime = mNowDateTime,
                                tr_update_member = 0,
                                tr_update_datetime = mNowDateTime
                            });

                            GetInvitationOrg mGetInvitationOrgImpl = new GetInvitationOrg(_logger, _dbAccess);
                            List<GetInvitationOrgDTO> mGetInvitationOrg = mGetInvitationOrgImpl.GetInvitationOrgProcess(item.pp_mm_id);
                            mInsertTar.Add(new tender_activity_record()
                            {
                                tar_mm_id = item.pp_mm_id,
                                tar_mm_introduce_code = 0,
                                tar_tm_id = item.pp_tm_id,
                                tar_tm_count = 1,
                                tar_tr_type = "15",
                                tar_tr_code = item.pp_tr_code,
                                tar_json = Newtonsoft.Json.JsonConvert.SerializeObject(mGetInvitationOrg),
                                tar_create_datetime = mNowDateTime,
                                tar_update_datetime = mNowDateTime
                            });

                            if (item.pp_penalty_datetime > mNowDateTime)
                            {
                                if (!mUpdateTD.ContainsKey(item.pp_mm_id))
                                {
                                    mUpdateTD.Add(item.pp_td_id, 1);
                                }
                            }
                            else
                            {
                                if (!mUpdateTD.ContainsKey(item.pp_mm_id))
                                {
                                    mUpdateTD.Add(item.pp_td_id, 0);
                                }
                            }
                        }

                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<List<member_balance>>(mSqlInsertBalance, mInsertBalance) == mMBCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "待付款排程失敗、新增帳務紀錄筆數錯誤";
                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<List<transaction_record>>(mSqlInsertTR, mInsertTR) == mMBCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "待付款排程失敗、新增交易紀錄筆數錯誤";
                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<List<tender_activity_record>>(mSqlInsertTar, mInsertTar) == mMBCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "待付款排程失敗、新增標案活動紀錄筆數錯誤";
                        return mResult;
                    }

                    var mUpdateWallet = mOriginalWallet.Select(kv => new
                    {
                        mw_stored = kv.Value.mw_stored_chenge,
                        mw_update_member = kv.Key,
                        mw_update_datetime = mNowDateTime,
                        mw_mm_id = kv.Key
                    }).ToList();

                    mIsOk = _dbAccess.ExecuteTransactionObject<dynamic>(mSqlUpdateMW, mUpdateWallet) == mMWCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "待付款排程失敗、更新錢包筆數錯誤";
                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<dynamic>(mSqlUpdateTD, mUpdateTD.Select(kv => new
                    {
                        td_pp_penalty_count = kv.Value,
                        td_update_member = 0,
                        td_update_datetime = mNowDateTime,
                        td_id = kv.Key
                    }).ToList()
                        ) == mMBCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "待付款排程失敗、更新標案明細筆數錯誤";
                        return mResult;
                    }
                    else
                    {
                        //_dbAccess.Rollback();
                        _dbAccess.Commit();

                        mResult.returnStatus = 1;
                        mResult.returnMsg = "成組排程成功";
                    }
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.returnStatus = 999;
                    mResult.returnMsg = "待付款排程結束、沒有資料";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"待付款排程失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
