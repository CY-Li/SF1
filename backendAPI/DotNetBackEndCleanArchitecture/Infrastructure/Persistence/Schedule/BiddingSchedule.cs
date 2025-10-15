using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.Entity.TenderMgmt.Tender;
using DomainEntity.ScheduleEntity.BiddingSchedule;
using DomainEntity.ScheduleEntity.SettlementPeaceSchedule;
using DomainEntity.Shared;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Persistence.Schedule
{
    public class BiddingSchedule
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "BiddingSchedule";

        public BiddingSchedule(
                ILogger<BiddingSchedule> logger,
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
                //流程1. 取得需要派發邀請人紅利所需活動紀錄
                //流程2. 資料群組加速邀請人紅利計算，因為每位會員直系邀請人不變，目前算法又是撈取當下下標數量做計算紅利派發，所以可以這樣做
                //流程3. new所需判斷邀請人等級所需class
                //流程4. 依據流程3每個group foreach
                //流程5. 派發每個group下的每個下單資料
                //流程6. 因group過一個下單資料可能會有多筆邀請人需要計算等級
                //       所以一個錯就全部錯，並且該group所有檢查等級全跳過
                //       雖然該group的所有下標資料都會foreach過，但都會先檢查第一筆是否檢查過如果檢查過就以第一筆計算的等級為準
                //流程7. 取得該會員邀請列表
                //流程8. 取得該會員邀請列表等級
                //流程9. 預先處理該下標所派發紅利
                //流程10. 鎖資料，取得該邀請人紅利
                //流程11. 帳務紀錄資料預處理
                //流程12. 交易紀錄資料預處理
                //流程13. 錢包變動額度處理
                //流程14. 介紹人帳務紀錄、交易紀錄、錢包變動額預處理
                //流程15. 預處理錢包資料
                //流程16. 預處理活動紀錄檔資料
                //流程17. 新增帳務紀錄
                //流程18. 新增交易紀錄
                //流程19. 更新錢包
                //流程20. 更新下標活動紀錄

                //流程1. 取得需要派發邀請人紅利所需活動紀錄
                string mSqlGetTar = @"
SELECT A.tar_id
       ,A.tar_mm_id
       ,A.tar_mm_introduce_code
       ,A.tar_tm_id
       ,A.tar_tm_count
       ,A.tar_tr_type
       ,A.tar_tr_code
       ,A.tar_status
       ,A.tar_json
       ,A.tar_create_datetime
FROM   rosca2.tender_activity_record AS A
WHERE  A.tar_status = 0
       AND A.tar_tr_type IN('13', '15')
ORDER  BY A.tar_mm_id 
";

                //流程10. 鎖資料，取得該邀請人紅利
                string mSqlGetRewardCondition = @"
SELECT B.mw_reward
FROM   rosca2.member_master AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.mm_id
WHERE  A.mm_status = 'Y'
       AND A.mm_id = @mm_id 
FOR UPDATE 
";

                //流程17. 新增帳務紀錄
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

                //流程18. 新增交易紀錄
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

                //流程19. 更新錢包
                string mSqlUpdateMW = @"
UPDATE rosca2.member_wallet
SET    mw_reward = mw_reward + @mw_reward
       ,mw_accumulation = mw_accumulation + @mw_reward
       ,mw_update_member = @mw_mm_id
       ,mw_update_datetime = @mw_update_datetime
WHERE  mw_mm_id = @mw_mm_id 
";

                string mSqlUpdateMM = @"
UPDATE rosca2.member_master
SET    mm_level = @mm_level
       ,mm_update_member = 0
       ,mm_update_datetime = @mm_update_datetime
WHERE  mm_id = @mm_id; 
";

                //流程20. 更新下標活動紀錄
                string mSqlUpdateTar = @"
UPDATE rosca2.tender_activity_record
SET    tar_status = 1 ,
       tar_update_member = 0 ,
       tar_update_datetime = @tar_update_datetime
WHERE  tar_id IN ({0});
";


                //流程1. 取得需要派發邀請人紅利所需活動紀錄
                List<GetTarData> mGetTenderData = _dbAccess.GetObject<GetTarData>(mSqlGetTar, null).ToList();

                if (mGetTenderData.Count > 0)
                {
                    //流程2. 資料群組加速邀請人紅利計算，因為每位會員直系邀請人不變，目前算法又是撈取當下下標數量做計算紅利派發，所以可以這樣做
                    var mTenderDataGroup = mGetTenderData.GroupBy(g => new { g.tar_mm_id }).ToList();

                    //流程3. new所需判斷邀請人等級所需class
                    GetLevleData mGetLevelData = new GetLevleData(_dbAccess);
                    //流程4. 依據流程3每個group foreach
                    foreach (var itemGroup in mTenderDataGroup)
                    {
                        int mGroupCnt = 0;//判斷是否為group下的第一筆資料
                        int mCheckInviteCnt = 0;
                        int mCheckIntroduceCnt = 0;
                        int mCheckLevelCnt = 0;
                        bool mIsOk = true;
                        Dictionary<long, int> mMemberLevel = new Dictionary<long, int>();//儲存邀請人等級用
                        Dictionary<long, BiddingSkdPoint> mOriginalWallet = new Dictionary<long, BiddingSkdPoint>();//邀請人紅利
                        //Dictionary<long, decimal> mInviteBonus = new Dictionary<long, decimal>();//邀請人紅利
                        DateTime mNowDateTime = GetTimeZoneInfo.process();
                        List<member_balance> mInsertBalance = new List<member_balance>();
                        List<transaction_record> mInsertTR = new List<transaction_record>();
                        List<member_wallet> mUpdateMW = new List<member_wallet>();

                        _dbAccess.BeginTrans();
                        //流程5. 派發每個group下的每個下單資料
                        foreach (var item in itemGroup)
                        {
                            //流程. 預處理結算該紅利時間，方便後續排程取得資料
                            DateTime nextMonth15 = new DateTime(item.tar_create_datetime.Year, item.tar_create_datetime.Month, 15).AddMonths(1);
                            try
                            {
                                if (string.IsNullOrEmpty(item.tar_json))
                                {
                                    throw new Exception("json空值所以拋出錯誤讓該group結束");
                                }

                                //流程7. 取得該會員邀請列表
                                List<GetInvitationOrgDTO>? mInvitationOrgList = JsonConvert.DeserializeObject<List<GetInvitationOrgDTO>>(item.tar_json);

                                //流程8. 取得該會員邀請列表等級
                                foreach (var itemOrg in mInvitationOrgList)
                                {
                                    //[{ "mm_id":9,"mm_invite_code":6,"class_level":1,"invite_level":"0"}
                                    //,{ "mm_id":6,"mm_invite_code":1,"class_level":2,"invite_level":"0"}
                                    //,{ "mm_id":1,"mm_invite_code":0,"class_level":3,"invite_level":"0"}]
                                    if (!mMemberLevel.TryGetValue(itemOrg.mm_id, out int value))
                                    {
                                        itemOrg.invite_level = mGetLevelData.GetLevel(itemOrg.mm_id);
                                        mMemberLevel.Add(itemOrg.mm_id, itemOrg.invite_level);
                                        mCheckLevelCnt++;
                                    }
                                    else
                                    {
                                        itemOrg.invite_level = value;
                                    }
                                }

                                if (item.tar_tr_type == "13")
                                {
                                    //流程14. 介紹人帳務紀錄、交易紀錄、錢包變動額預處理
                                    if (item.tar_mm_introduce_code != 0)
                                    {
                                        mCheckIntroduceCnt++;

                                        //流程13. 錢包變動額度處理
                                        if (mOriginalWallet.ContainsKey(item.tar_mm_introduce_code))
                                        {
                                            mOriginalWallet[item.tar_mm_introduce_code].mw_reward += 2000 * item.tar_tm_count;
                                            mOriginalWallet[item.tar_mm_introduce_code].mw_reward_chenge += 2000 * item.tar_tm_count;
                                        }
                                        else
                                        {
                                            //流程10. 鎖資料，取得該邀請人紅利
                                            decimal mintroducerReward = _dbAccess.ExecuteTransactionGetObject<decimal>(mSqlGetRewardCondition, new { mm_id = item.tar_mm_introduce_code }).First();
                                            mOriginalWallet.Add(item.tar_mm_introduce_code, new BiddingSkdPoint { mw_reward = mintroducerReward + 2000 * item.tar_tm_count, mw_reward_chenge = 2000 * item.tar_tm_count });
                                        }

                                        //流程11. 帳務紀錄資料預處理
                                        mInsertBalance.Add(new member_balance()
                                        {
                                            mb_mm_id = item.tar_mm_introduce_code,
                                            mb_payee_mm_id = 0,
                                            mb_tm_id = item.tar_tm_id,
                                            mb_td_id = 0,
                                            mb_income_type = "I",
                                            mb_tr_code = item.tar_tr_code,
                                            mb_tr_type = item.tar_tr_type,
                                            mb_type = "13",
                                            mb_points_type = "12",
                                            mb_change_points = 2000 * item.tar_tm_count,
                                            mb_points = mOriginalWallet[item.tar_mm_introduce_code].mw_reward,
                                            mb_create_member = item.tar_mm_introduce_code,
                                            mb_create_datetime = mNowDateTime,
                                            mb_update_member = item.tar_mm_introduce_code,
                                            mb_update_datetime = mNowDateTime
                                        });

                                        //流程12. 交易紀錄資料預處理
                                        mInsertTR.Add(new transaction_record()
                                        {
                                            tr_code = item.tar_tr_code,
                                            tr_mm_id = item.tar_mm_introduce_code,
                                            tr_payee_mm_id = 0,
                                            tr_tm_id = item.tar_tm_id,
                                            tr_td_id = 0,
                                            tr_pp_id = 0,
                                            tr_type = item.tar_tr_type,
                                            tr_status = "30",
                                            tr_settlement_time = nextMonth15,
                                            tr_mm_points = 2000 * item.tar_tm_count,
                                            tr_income_type = "I",
                                            tr_create_member = item.tar_mm_introduce_code,
                                            tr_create_datetime = mNowDateTime,
                                            tr_update_member = item.tar_mm_introduce_code,
                                            tr_update_datetime = mNowDateTime
                                        });
                                    }

                                    //流程9. 預先處理該下標所派發紅利
                                    int currentLevel = 1;
                                    int totalBonus = 2000;
                                    int payBonus = 0;
                                    bool payFlage = false;

                                    foreach (var invitation in mInvitationOrgList)
                                    {
                                        //2024/11/02 更新只算到等級4
                                        if (invitation.invite_level > 4)
                                        {
                                            continue;
                                        }

                                        //if (item.tar_mm_introduce_code != invitation.mm_id)
                                        if (payBonus < totalBonus && invitation.invite_level == currentLevel)
                                        {
                                            payFlage = true;
                                            invitation.invite_bonus = 500;
                                            payBonus += invitation.invite_bonus;
                                            currentLevel++;
                                        }
                                        else if (payBonus < totalBonus && invitation.invite_level > currentLevel)
                                        {
                                            payFlage = true;
                                            //因如果當下currentLevel是2但下一個邀請人等級是3，級距來講應該要拿到2+3的錢
                                            //但原本(invitation.invite_level - currentLevel)會少500，所以應該 + 1
                                            invitation.invite_bonus = 500 * (invitation.invite_level - currentLevel + 1);
                                            payBonus += invitation.invite_bonus;
                                            currentLevel = invitation.invite_level;
                                        }
                                        else // invitation.invite_level < currentLevel
                                        {
                                            payFlage = false;
                                            invitation.invite_bonus = 0;
                                        }

                                        if (payFlage)
                                        {
                                            mCheckInviteCnt++;

                                            //流程13. 錢包變動額度處理
                                            if (mOriginalWallet.ContainsKey(invitation.mm_id))
                                            {
                                                mOriginalWallet[invitation.mm_id].mw_reward += invitation.invite_bonus * item.tar_tm_count;
                                                mOriginalWallet[invitation.mm_id].mw_reward_chenge += invitation.invite_bonus * item.tar_tm_count;
                                            }
                                            else
                                            {
                                                //流程10. 鎖資料，取得該邀請人紅利
                                                decimal mInviterReward = _dbAccess.ExecuteTransactionGetObject<decimal>(mSqlGetRewardCondition, new { mm_id = invitation.mm_id }).First();
                                                mOriginalWallet.Add(invitation.mm_id, new BiddingSkdPoint { mw_reward = mInviterReward + invitation.invite_bonus * item.tar_tm_count, mw_reward_chenge = invitation.invite_bonus * item.tar_tm_count });
                                            }

                                            //流程11. 帳務紀錄資料預處理
                                            mInsertBalance.Add(new member_balance()
                                            {
                                                mb_mm_id = invitation.mm_id,
                                                mb_payee_mm_id = 0,
                                                mb_tm_id = item.tar_tm_id,
                                                mb_td_id = 0,
                                                mb_income_type = "I",
                                                mb_tr_code = item.tar_tr_code,
                                                mb_tr_type = item.tar_tr_type,
                                                mb_type = "13",
                                                mb_points_type = "12",
                                                mb_change_points = invitation.invite_bonus * item.tar_tm_count,
                                                mb_points = mOriginalWallet[invitation.mm_id].mw_reward,
                                                mb_create_member = invitation.mm_id,
                                                mb_create_datetime = mNowDateTime,
                                                mb_update_member = invitation.mm_id,
                                                mb_update_datetime = mNowDateTime
                                            });

                                            //流程12. 交易紀錄資料預處理
                                            mInsertTR.Add(new transaction_record()
                                            {
                                                tr_code = item.tar_tr_code,
                                                tr_mm_id = invitation.mm_id,
                                                tr_payee_mm_id = 0,
                                                tr_tm_id = item.tar_tm_id,
                                                tr_td_id = 0,
                                                tr_pp_id = 0,
                                                tr_type = item.tar_tr_type,
                                                tr_settlement_time = nextMonth15,
                                                tr_status = "30",
                                                tr_mm_points = invitation.invite_bonus * item.tar_tm_count,
                                                tr_income_type = "I",
                                                tr_create_member = invitation.mm_id,
                                                tr_create_datetime = mNowDateTime,
                                                tr_update_member = invitation.mm_id,
                                                tr_update_datetime = mNowDateTime
                                            });

                                            payFlage = false;
                                        }
                                    }

                                    //item.tar_status = 1;
                                }
                                else if (item.tar_tr_type == "15")
                                {
                                    //流程9. 預先處理該下標所派發紅利
                                    int currentLevel = 4;
                                    int totalBonus = 50;
                                    int payBonus = 0;
                                    bool payFlage = false;

                                    foreach (var invitation in mInvitationOrgList)
                                    {
                                        if (payBonus < 50 && invitation.invite_level >= currentLevel)
                                        {
                                            payFlage = true;
                                            if (payBonus == 0)
                                            {
                                                invitation.invite_bonus = 30;
                                                payBonus += invitation.invite_bonus;
                                            }else if (payBonus == 30)
                                            {
                                                invitation.invite_bonus = 20;
                                                payBonus += invitation.invite_bonus;
                                            }
                                            else
                                            {
                                                payFlage = false;
                                                invitation.invite_bonus = 0;
                                            }
                                        }
                                        else // level < currentLevel
                                        {
                                            payFlage = false;
                                            invitation.invite_bonus = 0;
                                        }


                                        if (payFlage)
                                        {
                                            mCheckInviteCnt++;

                                            //流程13. 錢包變動額度處理
                                            if (mOriginalWallet.ContainsKey(invitation.mm_id))
                                            {
                                                mOriginalWallet[invitation.mm_id].mw_reward += invitation.invite_bonus * item.tar_tm_count;
                                                mOriginalWallet[invitation.mm_id].mw_reward_chenge += invitation.invite_bonus * item.tar_tm_count;
                                            }
                                            else
                                            {
                                                //流程10. 鎖資料，取得該邀請人紅利
                                                decimal mInviterReward = _dbAccess.ExecuteTransactionGetObject<decimal>(mSqlGetRewardCondition, new { mm_id = invitation.mm_id }).First();
                                                mOriginalWallet.Add(invitation.mm_id, new BiddingSkdPoint { mw_reward = mInviterReward + invitation.invite_bonus * item.tar_tm_count, mw_reward_chenge = invitation.invite_bonus * item.tar_tm_count });
                                            }

                                            //流程11. 帳務紀錄資料預處理
                                            mInsertBalance.Add(new member_balance()
                                            {
                                                mb_mm_id = invitation.mm_id,
                                                mb_payee_mm_id = 0,
                                                mb_tm_id = item.tar_tm_id,
                                                mb_td_id = 0,
                                                mb_income_type = "I",
                                                mb_tr_code = item.tar_tr_code,
                                                mb_tr_type = item.tar_tr_type,
                                                mb_type = "16",
                                                mb_points_type = "12",
                                                mb_change_points = invitation.invite_bonus,
                                                mb_points = mOriginalWallet[invitation.mm_id].mw_reward,
                                                mb_create_member = invitation.mm_id,
                                                mb_create_datetime = mNowDateTime,
                                                mb_update_member = invitation.mm_id,
                                                mb_update_datetime = mNowDateTime
                                            });

                                            //流程12. 交易紀錄資料預處理
                                            mInsertTR.Add(new transaction_record()
                                            {
                                                tr_code = item.tar_tr_code,
                                                tr_mm_id = invitation.mm_id,
                                                tr_payee_mm_id = 0,
                                                tr_tm_id = item.tar_tm_id,
                                                tr_td_id = 0,
                                                tr_pp_id = 0,
                                                tr_type = item.tar_tr_type,
                                                tr_settlement_time = nextMonth15,
                                                tr_status = "30",
                                                tr_mm_points = invitation.invite_bonus,
                                                tr_income_type = "I",
                                                tr_create_member = invitation.mm_id,
                                                tr_create_datetime = mNowDateTime,
                                                tr_update_member = invitation.mm_id,
                                                tr_update_datetime = mNowDateTime
                                            });

                                            payFlage = false;
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation($@"{pBaseLog} {mActionLog} 預處理 {ex.Message}");
                                _dbAccess.Rollback();
                                //流程6. 因group過一個下單資料可能會有多筆邀請人需要計算等級
                                //       所以一個錯就全部錯，並且該group所有檢查等級全跳過
                                //       雖然該group的所有下標資料都會foreach過，但都會先檢查第一筆是否檢查過如果檢查過就以第一筆計算的等級為準
                                break;
                            }
                        }
                        //流程15. 預處理錢包資料
                        mUpdateMW = mOriginalWallet.Select(s => new member_wallet
                        {
                            mw_mm_id = s.Key,
                            mw_reward = s.Value.mw_reward_chenge,
                            mw_update_datetime = mNowDateTime
                        }).ToList();

                        //流程16. 預處理活動紀錄檔資料
                        string mSqlUpdateTarProcess = string.Format(mSqlUpdateTar, string.Join(',', itemGroup.Select(s => s.tar_id)));
                        _logger.LogInformation($@"{pBaseLog} {mActionLog} 新增帳務紀錄");
                        //流程17. 新增帳務紀錄
                        if (mIsOk)
                        {
                            mIsOk = _dbAccess.ExecuteTransactionObject<List<member_balance>>(mSqlInsertBalance, mInsertBalance) == (mCheckInviteCnt + mCheckIntroduceCnt);
                            if (!mIsOk)
                            {
                                _dbAccess.Rollback();

                                return mResult;
                            }
                        }
                        _logger.LogInformation($@"{pBaseLog} {mActionLog} 新增交易紀錄");
                        if (mIsOk)
                        {
                            //流程18. 新增交易紀錄
                            mIsOk = _dbAccess.ExecuteTransactionObject<List<transaction_record>>(mSqlInsertTR, mInsertTR) == (mCheckInviteCnt + mCheckIntroduceCnt);
                            if (!mIsOk)
                            {
                                _dbAccess.Rollback();
                                return mResult;
                            }
                        }
                        _logger.LogInformation($@"{pBaseLog} {mActionLog} 更新錢包");

                        if (mIsOk)
                        {
                            //流程19. 更新錢包
                            mIsOk = _dbAccess.ExecuteTransactionObject<List<member_wallet>>(mSqlUpdateMW, mUpdateMW) == mOriginalWallet.Count();
                            if (!mIsOk)
                            {
                                _dbAccess.Rollback();
                                return mResult;
                            }
                        }

                        _logger.LogInformation($@"{pBaseLog} {mActionLog} 更新會員等級");
                        var mUpdateMM = mMemberLevel.Select(s => new
                        {
                            mm_id = s.Key,
                            mm_level = s.Value,
                            mm_update_datetime = mNowDateTime
                        }).ToList();
                        if (mIsOk)
                        {
                            //流程. 更新會員等級
                            mIsOk = _dbAccess.ExecuteTransactionObject<dynamic>(mSqlUpdateMM, mUpdateMM) == mCheckLevelCnt;
                            if (!mIsOk)
                            {
                                _dbAccess.Rollback();
                                return mResult;
                            }
                        }

                        _logger.LogInformation($@"{pBaseLog} {mActionLog} 更新下標活動紀錄");
                        if (mIsOk)
                        {
                            //流程20. 更新下標活動紀錄
                            mIsOk = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateTarProcess, new { tar_update_datetime = mNowDateTime }) == itemGroup.Count();
                        }

                        if (!mIsOk)
                        {
                            _dbAccess.Rollback();
                            return mResult;
                        }
                        else
                        {
                            _dbAccess.Commit();
                            //_dbAccess.Commit();
                        }

                        //mInsertBalance = new List<member_balance>();
                        //mInsertTR = new List<transaction_record>();
                        //mUpdateMW = new List<member_wallet>();
                        //mMemberLevel.Clear();
                        //mInviteBonus.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"下標排程失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
