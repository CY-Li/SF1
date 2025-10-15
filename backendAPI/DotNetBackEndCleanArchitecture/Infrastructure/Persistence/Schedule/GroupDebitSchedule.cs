using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.ScheduleEntity.GroupDebitSchedule;
using DomainEntity.Shared;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Schedule
{
    public class GroupDebitSchedule
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "GroupDebitSchedule";

        public GroupDebitSchedule(
                ILogger<GroupDebitSchedule> logger,
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
                string mSqlGetTM = @"
SELECT A.tm_id
       ,B.td_id
       ,A.tm_settlement_period
       ,A.tm_current_period
       ,A.tm_settlement_datetime
       ,B.td_participants
       ,B.td_sequence
       ,B.td_period
       ,B.td_pp_id
       ,C.mw_stored
       ,C.mw_reward
       ,C.mw_death
FROM   rosca2.tender_master AS A
       INNER JOIN rosca2.tender_detail AS B
               ON B.td_tm_id = A.tm_id
       INNER JOIN rosca2.member_wallet AS C
               ON C.mw_mm_id = B.td_participants
WHERE  A.tm_status = 1 
       AND A.tm_current_period = A.tm_settlement_period
#FOR UPDATE
";

                string mSqlGetTrId = "SELECT NEXTVAL(rosca2.transaction_record_sequence)";

                string mSqlGetPpId = "SELECT NEXTVAL(rosca2.pending_payment_sequence)";

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
             ,mb_settlement_period
             ,mb_current_period
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
            ,@mb_settlement_period
            ,@mb_current_period
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
             ,tr_settlement_period
             ,tr_current_period
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
            ,@tr_settlement_period
            ,@tr_current_period
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

                string mSqlInsertPP = @"
INSERT INTO rosca2.pending_payment
            (pp_id
             ,pp_mm_id
             ,pp_tr_code
             ,pp_tm_id
             ,pp_td_id
             ,pp_amount
             ,pp_status
             ,pp_penalty_datetime
             ,pp_create_member
             ,pp_create_datetime
             ,pp_update_member
             ,pp_update_datetime)
VALUES     (@pp_id
            ,@pp_mm_id
            ,@pp_tr_code
            ,@pp_tm_id
            ,@pp_td_id
            ,@pp_amount
            ,'10'
            ,@pp_penalty_datetime
            ,@pp_create_member
            ,@pp_create_datetime
            ,@pp_update_member
            ,@pp_update_datetime); 
";

                string mSqlUpdateMW = @"
UPDATE rosca2.member_wallet
SET    mw_stored = mw_stored - @mw_stored
       ,mw_death = mw_death + @mw_death
       ,mw_accumulation = mw_accumulation + @mw_stored
       ,mw_update_member = @mw_update_member
       ,mw_update_datetime = @mw_update_datetime
WHERE  mw_mm_id = @mw_mm_id 
";

                string mSqlUpdateTM = @"
UPDATE rosca2.tender_master
SET    tm_settlement_period = tm_settlement_period + 1
       ,tm_settlement_datetime = DATE_ADD(tm_settlement_datetime,INTERVAL 1 MONTH)
       ,tm_update_member = 0
       ,tm_update_datetime = @tm_update_datetime
WHERE  tm_id IN({0}); 
";

                string mSqlUpdateTD = @"
UPDATE rosca2.tender_detail
SET    td_pp_id = CONCAT_WS('|', td_pp_id, @td_pp_id)
       ,td_pp_paid = td_pp_paid + 1
       ,td_update_member = @td_update_member
       ,td_update_datetime = @td_update_datetime
WHERE  td_id = @td_id; 
";

                _dbAccess.BeginTrans();

                List<GroupDebitGetTM> mGetTenderData = _dbAccess.ExecuteTransactionGetObject<GroupDebitGetTM>(mSqlGetTM, null).ToList();

                List<member_balance> mInsertBalance = new List<member_balance>();
                List<transaction_record> mInsertTR = new List<transaction_record>();
                List<tender_activity_record> mInsertTar = new List<tender_activity_record>();
                List<pending_payment> mInsertPP = new List<pending_payment>();
                List<member_wallet> mUpdateMW = new List<member_wallet>();
                Dictionary<long, GroupDebitInsertPoint> mOriginalWallet = new Dictionary<long, GroupDebitInsertPoint>();
                HashSet<long> mUpdateTM = new HashSet<long>();
                Dictionary<long, string> mUpdateTD = new Dictionary<long, string>();
                int mMWCnt = 0;
                int mMBCnt = 0;
                int mPPCnt = 0;
                int mDeathCnt = 0;
                int mTMCnt = 0;
                DateTime mNowDateTime = GetTimeZoneInfo.process();
                bool mIsOk = false;

                if (mGetTenderData.Count % 24 != 0)
                {
                    _logger.LogInformation($@"{pBaseLog} {mActionLog} 取得程組排程筆數不對，必須24倍數");

                    mResult.returnStatus = 999;
                    mResult.returnMsg = "取得程組排程筆數不對，必須24倍數";
                    return mResult;
                }

                if (mGetTenderData.Count > 0)
                {
                    foreach (var item in mGetTenderData)
                    {
                        if (!mUpdateTM.Contains(item.tm_id))
                        {
                            mUpdateTM.Add(item.tm_id);
                            mTMCnt++;
                        }

                        int mGetTrId = _dbAccess.ExecuteTransactionGetObject<int>(mSqlGetTrId, null).First();
                        string mTrCode = GetFieldID.process("tr", mGetTrId, mNowDateTime);

                        if (!mOriginalWallet.ContainsKey(item.td_participants))
                        {
                            mOriginalWallet.Add(item.td_participants, new GroupDebitInsertPoint { mw_stored = item.mw_stored, mw_reward = item.mw_reward, mw_death = item.mw_death });
                            mMWCnt++;
                        }

                        GroupDebitInsertPoint originalWalletItem = mOriginalWallet[item.td_participants];

                        if (item.td_period == 0)
                        {
                            if (originalWalletItem.mw_stored >= 8000)
                            {
                                mMBCnt++;
                                originalWalletItem.mw_stored -= 8000;
                                originalWalletItem.mw_stored_chenge += 8000;

                                //扣除該期標案金額
                                mInsertBalance.Add(new member_balance()
                                {
                                    mb_mm_id = item.td_participants,
                                    mb_payee_mm_id = 0,
                                    mb_tm_id = item.tm_id,
                                    mb_td_id = item.td_id,
                                    mb_income_type = "O",
                                    mb_tr_code = mTrCode,
                                    mb_tr_type = "14",
                                    mb_type = "15",//下標時扣除押金
                                    mb_points_type = "11",
                                    mb_change_points = 8000,
                                    mb_points = originalWalletItem.mw_stored,
                                    mb_settlement_period = item.tm_settlement_period + 1,
                                    mb_current_period = item.tm_current_period,
                                    mb_create_member = item.td_participants,
                                    mb_create_datetime = mNowDateTime,
                                    mb_update_member = item.td_participants,
                                    mb_update_datetime = mNowDateTime
                                });

                                //扣除標案金額交易紀錄
                                mInsertTR.Add(new transaction_record()
                                {
                                    tr_code = mTrCode,
                                    tr_mm_id = item.td_participants,
                                    tr_payee_mm_id = 0,
                                    tr_tm_id = item.tm_id,
                                    tr_td_id = item.td_id,
                                    tr_pp_id = 0,
                                    tr_type = "14",
                                    tr_status = "90",
                                    tr_mm_points = 8000,
                                    tr_income_type = "O",
                                    tr_settlement_period = item.tm_settlement_period + 1,
                                    tr_current_period = item.tm_current_period,
                                    tr_create_member = item.td_participants,
                                    tr_create_datetime = mNowDateTime,
                                    tr_update_member = item.td_participants,
                                    tr_update_datetime = mNowDateTime
                                });

                                GetInvitationOrg mGetInvitationOrgImpl = new GetInvitationOrg(_logger, _dbAccess);
                                List<GetInvitationOrgDTO> mGetInvitationOrg = mGetInvitationOrgImpl.GetInvitationOrgProcess(item.td_participants);
                                mInsertTar.Add(new tender_activity_record()
                                {
                                    tar_mm_id = item.td_participants,
                                    tar_mm_introduce_code = 0,
                                    tar_tm_id = item.tm_id,
                                    tar_tm_count = 1,
                                    tar_tr_type = "15",
                                    tar_tr_code = mTrCode,
                                    tar_json = Newtonsoft.Json.JsonConvert.SerializeObject(mGetInvitationOrg),
                                    tar_create_datetime = mNowDateTime,
                                    tar_update_datetime = mNowDateTime
                                });
                            }
                            else
                            {
                                mPPCnt++;
                                int mGetPpId = _dbAccess.ExecuteTransactionGetObject<int>(mSqlGetPpId, null).First();

                                if (!mUpdateTD.ContainsKey(item.td_id))
                                {
                                    mUpdateTD.Add(item.td_id, mGetPpId.ToString());
                                    mMWCnt++;
                                }

                                mInsertPP.Add(new pending_payment()
                                {
                                    pp_id = mGetPpId,
                                    pp_mm_id = item.td_participants,
                                    pp_tr_code = mTrCode,
                                    pp_tm_id = item.tm_id,
                                    pp_td_id = item.td_id,
                                    pp_amount = 8000,
                                    pp_status = "10",
                                    pp_penalty_datetime = item.tm_settlement_datetime.AddDays(5),
                                    pp_create_member = 0,
                                    pp_create_datetime = mNowDateTime,
                                    pp_update_member = 0,
                                    pp_update_datetime = mNowDateTime
                                });
                            }
                        }
                        else
                        {
                            mDeathCnt++;

                            originalWalletItem.mw_death += 10000;
                            originalWalletItem.mw_death_chenge += 10000;

                            //扣除該期標案金額
                            mInsertBalance.Add(new member_balance()
                            {
                                mb_mm_id = item.td_participants,
                                mb_payee_mm_id = 0,
                                mb_tm_id = item.tm_id,
                                mb_td_id = item.td_id,
                                mb_income_type = "O",
                                mb_tr_code = mTrCode,
                                mb_tr_type = "16",
                                mb_type = "15",
                                mb_points_type = "16",
                                mb_change_points = 10000,
                                mb_settlement_period = item.tm_settlement_period + 1,
                                mb_current_period = item.tm_current_period,
                                mb_points = originalWalletItem.mw_death,
                                mb_create_member = item.td_participants,
                                mb_create_datetime = mNowDateTime,
                                mb_update_member = item.td_participants,
                                mb_update_datetime = mNowDateTime
                            });

                            //扣除標案金額交易紀錄
                            mInsertTR.Add(new transaction_record()
                            {
                                tr_code = mTrCode,
                                tr_mm_id = item.td_participants,
                                tr_payee_mm_id = 0,
                                tr_tm_id = item.tm_id,
                                tr_td_id = item.td_id,
                                tr_pp_id = 0,
                                tr_type = "16",
                                tr_status = "90",
                                tr_mm_points = 10000,
                                tr_income_type = "O",
                                tr_settlement_period = item.tm_settlement_period + 1,
                                tr_current_period = item.tm_current_period,
                                tr_create_member = item.td_participants,
                                tr_create_datetime = mNowDateTime,
                                tr_update_member = item.td_participants,
                                tr_update_datetime = mNowDateTime
                            });
                        }
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<List<member_balance>>(mSqlInsertBalance, mInsertBalance) == (mMBCnt + mDeathCnt);
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "成組排程失敗、新增帳務紀錄筆數錯誤";
                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<List<transaction_record>>(mSqlInsertTR, mInsertTR) == (mMBCnt + mDeathCnt);
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "成組排程失敗、新增交易紀錄筆數錯誤";
                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<List<tender_activity_record>>(mSqlInsertTar, mInsertTar) == mMBCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "成組排程失敗、新增標案活動紀錄筆數錯誤";
                        return mResult;
                    }

                    if (mInsertPP.Count > 0)
                    {
                        mIsOk = _dbAccess.ExecuteTransactionObject<List<pending_payment>>(mSqlInsertPP, mInsertPP) == mPPCnt;
                        if (!mIsOk)
                        {
                            _dbAccess.Rollback();

                            mResult.returnStatus = 999;
                            mResult.returnMsg = "成組排程失敗、新增待付款筆數錯誤";
                            return mResult;
                        }
                    }

                    var mUpdateWallet = mOriginalWallet.Select(kv => new
                    {
                        mw_stored = kv.Value.mw_stored_chenge,
                        mw_death = kv.Value.mw_death_chenge,
                        mw_update_member = kv.Key,
                        mw_update_datetime = mNowDateTime,
                        mw_mm_id = kv.Key
                    }).ToList();

                    mIsOk = _dbAccess.ExecuteTransactionObject<dynamic>(mSqlUpdateMW, mUpdateWallet) == mMWCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "成組排程失敗、更新錢包筆數錯誤";
                        return mResult;
                    }

                    mSqlUpdateTM = string.Format(mSqlUpdateTM, string.Join(',', mUpdateTM.ToList()));
                    mIsOk = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateTM, new { tm_update_datetime = mNowDateTime }) == mTMCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "成組排程失敗、更新標案主檔筆數錯誤";
                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<dynamic>(mSqlUpdateTD, mUpdateTD.Select(kv => new
                    {
                        td_pp_id = kv.Value,
                        td_update_member = 0,
                        td_update_datetime = mNowDateTime,
                        td_id = kv.Key
                    }).ToList()
                        ) == mPPCnt;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "成組排程失敗、更新標案明細筆數錯誤";
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
                    mResult.returnMsg = "成組排程結束、沒有資料";
                }

            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"成組後排程失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
