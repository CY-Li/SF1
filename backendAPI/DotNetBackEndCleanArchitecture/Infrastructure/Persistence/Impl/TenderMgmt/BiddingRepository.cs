using DomainAbstraction.Interface.TenderMgmt;
using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.Entity.TenderMgmt.Tender;
using DomainEntity.Shared;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Impl.TenderMgmt
{
    public class BiddingRepository : IBiddingRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "BiddingRepository";

        public BiddingRepository(
                ILogger<BiddingRepository> logger,
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
        /// 會員-下標
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> Bidding(BiddingDTO reqModel)
        {
            string mActionLog = "Bidding";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                //流程1.取標案種類、該mm_id是否下標過、已經下標數量、介紹人(在這以子查詢方法取得是因為不想做鎖行)
                //流程2. tender_master、tender_detail 資料並鎖定，需要透過流程1決定判斷鎖幾筆
                //流程3. 鎖資料，取錢包餘額與紅利餘額
                //流程4. 取transaction_record_sequence
                //流程5. 判斷餘額是否足夠與下標數量是否>0
                //流程6. 組tar資料方便上層紅利派發排程
                //流程7. 下標押金與下標所需帳務紀錄、交易紀錄資料處理
                //流程8. 判斷比數1，新增活動紀錄(tender_activity_record)
                //流程9. 判斷新增帳務紀錄筆數標數總量 + m_CNTCheck才對(因為 + m_CNTCheck是押金部分)
                //流程10. 判斷新增交易紀錄筆數標數總量 + m_CNTCheck才對(因為 + m_CNTCheck是押金部分)
                //流程11. 判斷筆數1，更新標案主檔(tender_master)，下標人、已經下標數量
                //流程12. 判斷筆數，更新標案明細(tender_detail)，下標人、該名系狀態
                //流程13. 判斷筆數1，更新帳戶錢包，下標數量、錢包餘額
                //流程14. 判斷筆數1，更新標案主檔(tender_master)狀態，因如果標案如果滿24就要自動新增一個該類型標案
                //流程15. 判斷是否正確，下標後，標案數量如果滿24就要自動新增一個該類型標案

                //流程1.取標案種類、該mm_id是否下標過、已經下標數量、介紹人(在這以子查詢方法取得是因為不想做鎖行)
                string mSqlGetTenderData = @"
SELECT A.tm_type
       ,SUM(( CASE
                WHEN B.td_participants = @td_participants THEN 1
                ELSE 0
              END )) AS has_participants
       ,tm_count
       ,(SELECT A.mm_introduce_code FROM rosca2.member_master AS A WHERE A.mm_id = @mm_id) AS tar_mm_introduce_code
       ,(SELECT A.mm_invite_code FROM rosca2.member_master AS A WHERE A.mm_id = @mm_id) AS tar_mm_invite_code
FROM   rosca2.tender_master AS A
       LEFT JOIN (SELECT td_tm_id
                         ,td_participants
                  FROM   rosca2.tender_detail) AS B
              ON B.td_tm_id = A.tm_id
WHERE  A.tm_id = @tm_id ;
";

                //流程2. tender_master、tender_detail 資料並鎖定，需要透過流程1決定判斷鎖幾筆
                string mSqlGetTenderConditionData = @"
SELECT A.tm_id 
       ,B.td_id
       ,B.td_participants
       ,B.td_sequence
FROM   rosca2.tender_master  AS A
       INNER JOIN rosca2.tender_detail  AS B
               ON B.td_tm_id  = A.tm_id 
WHERE  A.tm_id  = @tm_id
       AND A.tm_status  = '0'
       AND B.td_status  = '0'
ORDER  BY B.td_sequence 
LIMIT  @mCNT
FOR UPDATE 
";

                //流程3. 鎖資料，取錢包餘額與紅利餘額
                string mSqlGetMMConditionData = @"
SELECT B.mw_stored
       ,B.mw_reward
       ,A.mm_2nd_hash_pwd
FROM   rosca2.member_master AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.mm_id
WHERE  A.mm_status = 'Y'
       AND A.mm_id = @mm_id 
FOR UPDATE 
";

                //流程4. 取transaction_record_sequence
                //流程4. 自製tr_code為了方便整合一筆交易得內容
                string mSqlGetTrId = "SELECT NEXTVAL(rosca2.transaction_record_sequence)";

                //流程8. 判斷比數1，新增活動紀錄(tender_activity_record)//流程8. 判斷比數1，新增活動紀錄(tender_activity_record)
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

                //流程9. 判斷新增帳務紀錄筆數標數總量 + m_CNTCheck才對(因為 + m_CNTCheck是押金部分)
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

                //流程10. 判斷新增交易紀錄筆數標數總量 + m_CNTCheck才對(因為 + m_CNTCheck是押金部分)
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
VALUES     (@tr_code
            ,@tr_mm_id
            ,@tr_tm_id
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

                //流程11. 判斷筆數1，更新標案主檔(tender_master)，下標人、已經下標數量
                string mSqlUpdateTenderMaster = @"
UPDATE rosca2.tender_master
SET    tm_bidder = CONCAT_WS('', tm_bidder, REPEAT(CONCAT('|', @tm_bidder), @cnt))
       ,tm_count = tm_count + @tm_count
       ,tm_update_member = @mm_id
       ,tm_update_datetime = @tm_update_datetime
WHERE  tm_id = @tm_id 
";

                //流程12. 判斷筆數，更新標案明細(tender_detail)，下標人、該名系狀態
                string mSqlUpdateTenderDetail = @"
UPDATE rosca2.tender_detail
SET    td_participants = @mm_id,
       td_status = '1',
       td_update_member = @mm_id,
       td_update_datetime = @td_update_datetime
WHERE  td_tm_id = @td_tm_id
AND    td_id IN( {0} )
";

                //流程13. 判斷筆數1，更新帳戶錢包，下標數量、錢包餘額
                string mSqlUpdateMW = @"
UPDATE rosca2.member_wallet
SET    mw_subscripts_count = mw_subscripts_count + @mw_subscripts_count
       ,mw_stored = mw_stored - @mw_stored
       ,mw_update_member = @mm_id
       ,mw_update_datetime = @mw_update_datetime
WHERE  mw_mm_id = @mm_id 
";

                //流程14. 判斷筆數1，更新標案主檔(tender_master)狀態，因如果標案如果滿24就要自動新增一個該類型標案
                string mSqlUpdateTM = @"
UPDATE rosca2.tender_master
SET    tm_status = 1
       ,tm_sn = (SELECT NEXTVAL(rosca2.tender_master_group_sequence))
       ,tm_settlement_datetime = @tm_settlement_datetime
       ,tm_win_first_datetime = @tm_win_first_datetime
       ,tm_win_end_datetime = @tm_win_end_datetime
       ,tm_group_datetime = @tm_update_datetime
       ,tm_update_member = @mm_id
       ,tm_update_datetime = @tm_update_datetime
WHERE  tm_id = @tm_id
       AND tm_count = 24 
";

                //該註解為以後可能效能問題可參考方案
                #region 流程14該註解為以後可能效能問題可參考方案
                //                string mSqlUpdateRM = @"
                //UPDATE rosca2.tender_master
                //SET    tm_status = '1'
                //       ,tm_create_datetime = @mm_id
                //       ,tm_update_member = NOW()
                //WHERE  roscam_id = (SELECT CASE (SELECT Count(*)
                //                                 FROM   rosca.rosca_master AS A
                //                                        LEFT JOIN rosca.rosca_detail AS B
                //                                               ON B.roscad_roscam_id =
                //                                                  A.roscam_id
                //                                 WHERE  A.roscam_id = @roscam_id
                //                                        AND B.roscad_status = '1')
                //                             WHEN 24 THEN @roscam_id
                //                             ELSE 0
                //                           END AS roscam_id); 
                //";
                #endregion 流程14該註解為以後可能效能問題可參考方案

                int mCNT = 0; //m_sqlGetROSCAConditionData用與算錢用
                int mCNTCheck = 0;//用來判斷寫入帳務紀錄與交易紀錄筆數是否正確
                decimal mNeedAMT = decimal.Zero;//下標會員該扣的錢
                bool mIsOk = false;

                //流程1.取標案種類、該mm_id是否下標過、已經下標數量、介紹人(在這以子查詢方法取得是因為不想做鎖行)
                BiddingGetTenderDataPO? mGetTenderData = _dbAccess.GetObject<BiddingGetTenderDataPO>(mSqlGetTenderData, new { tm_id = reqModel.tm_id, td_participants = reqModel.mm_id, mm_id = reqModel.mm_id }).FirstOrDefault();
                if (mGetTenderData != null
                    && !string.IsNullOrEmpty(mGetTenderData.tm_type)
                    //&& mGetTenderData.has_participants == 0
                    && mGetTenderData.tm_count < 24)
                {
                    switch (mGetTenderData.tm_type)
                    {
                        case "A":
                            mCNT = reqModel.tm_count == 0 ? 1 : reqModel.tm_count;
                            break;
                        case "B":
                            mCNT = reqModel.tm_count == 0 ? 6 : 6 * reqModel.tm_count;
                            break;
                        case "C":
                            mCNT = reqModel.tm_count == 0 ? 12 : 12 * reqModel.tm_count;
                            break;
                        case "D":
                            mCNT = 24;
                            break;
                        default:
                            break;
                    }

                    if ((mGetTenderData.tm_count + mCNT) > 24)
                    {
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"下標失敗，該標案下標後超過可下標數量，玩法:{mGetTenderData.tm_type}、下標後數量:{mGetTenderData.tm_count + mCNT}";

                        return mResult;
                    }
                }
                else
                {
                    //流程1.取標案種類、該mm_id是否下標過、已經下標數量
                    mResult.returnStatus = 999;
                    //mResult.returnMsg = $@"下標失敗，該標案不接受重複下標或玩法取得錯誤，玩法:{mGetTenderData.tm_type}、下標數:{mGetTenderData.has_participants}";
                    mResult.returnMsg = $@"下標失敗，該下標或玩法取得錯誤，玩法:{mGetTenderData.tm_type}、您的下標數:{mGetTenderData.has_participants}";

                    return mResult;
                }

                //該玩法所需扣的錢
                mNeedAMT = 8000 * mCNT + 8000 * mCNT;

                //流程2. tender_master、tender_detail 資料並鎖定，需要透過流程1決定判斷鎖幾筆
                mSqlGetTenderConditionData = mSqlGetTenderConditionData.Replace("@mCNT", mCNT.ToString());

                List<member_balance> mInsertBalance = new List<member_balance>();
                List<transaction_record> mInsertTR = new List<transaction_record>();

                GetInvitationOrg mGetInvitationOrgImpl = new GetInvitationOrg(_logger, _dbAccess);
                List<GetInvitationOrgDTO> mGetInvitationOrg = mGetInvitationOrgImpl.GetInvitationOrgProcess(reqModel.mm_id);

                _dbAccess.BeginTrans();

                //流程2.tender_master、tender_detail 資料並鎖定，需要透過流程1決定判斷鎖幾筆
                List<BiddingGetTenderConditionData> mGetTenderConditionData = _dbAccess.ExecuteTransactionGetObject<BiddingGetTenderConditionData>(mSqlGetTenderConditionData, reqModel).ToList();
                //流程3. 鎖資料，取錢包餘額與紅利餘額
                BiddingGetMMConditionData mGetMMConditionData = _dbAccess.ExecuteTransactionGetObject<BiddingGetMMConditionData>(mSqlGetMMConditionData, reqModel).First();
                bool verify = HashProcess.VerifyHashPWD(reqModel.mm_2nd_pwd, mGetMMConditionData.mm_2nd_hash_pwd);
                if (!verify)
                {
                    _dbAccess.Rollback();

                    mResult.returnStatus = 999;
                    mResult.returnMsg = "下標失敗、下單密碼錯誤";
                    return mResult;
                }

                //流程4. 取transaction_record_sequence
                int mGetTrId = _dbAccess.ExecuteTransactionGetObject<int>(mSqlGetTrId, null).First();
                //流程4. 自製tr_code為了方便整合一筆交易得內容
                string mTrCode = GetFieldID.process("tr", mGetTrId, reqModel.update_datetime);

                //流程5. 判斷餘額是否足夠與下標數量是否>0
                if (mGetMMConditionData.mw_stored >= mNeedAMT && mGetTenderConditionData.Count > 0)
                {
                    //流程6. 組tar資料方便上層紅利派發排程
                    _logger.LogInformation($@"{pBaseLog} {mActionLog}查看 {reqModel.mm_id}、{mGetInvitationOrg.Count()}{string.Join(",", mGetInvitationOrg.Select(s => s.mm_invite_code))}");
                    _logger.LogInformation($@"{pBaseLog} {mActionLog}查看 ");
                    _logger.LogInformation($@"{pBaseLog} {mActionLog}查看 {Newtonsoft.Json.JsonConvert.SerializeObject(mGetInvitationOrg)}");
                    tender_activity_record mInsertTar = new()
                    {
                        tar_mm_id = reqModel.mm_id,
                        tar_mm_introduce_code = mGetTenderData.tar_mm_introduce_code,
                        tar_tm_id = reqModel.tm_id,
                        tar_tm_count = mCNT,
                        tar_tr_type = "13",
                        tar_tr_code = mTrCode,
                        tar_json = Newtonsoft.Json.JsonConvert.SerializeObject(mGetInvitationOrg),
                        tar_create_datetime = reqModel.update_datetime,
                        tar_update_datetime = reqModel.update_datetime
                    };

                    //該錢包的餘額是因為需要做到帳務紀錄有連續性，所以用一個變數單純存著
                    decimal mNowPoints = mGetMMConditionData.mw_stored;

                    //流程7. 下標押金與下標所需帳務紀錄、交易紀錄資料處理
                    for (int i = 0; i < mGetTenderConditionData.Count; i++)
                    {
                        #region 押金只有第一筆才做處理
                        if (i == 0)
                        {
                            mCNTCheck++;
                            mNowPoints = mNowPoints - 8000 * mCNT;
                            //下標時扣除押金
                            mInsertBalance.Add(new member_balance()
                            {
                                mb_mm_id = reqModel.mm_id,
                                mb_payee_mm_id = 0,
                                mb_tm_id = mGetTenderConditionData[i].tm_id,
                                mb_td_id = 0,
                                mb_income_type = "O",
                                mb_tr_code = mTrCode,
                                mb_tr_type = "11",
                                mb_type = "11",//下標時扣除押金
                                mb_points_type = "11",
                                mb_change_points = 8000 * mCNT,
                                mb_points = mNowPoints,
                                mb_create_member = reqModel.mm_id,
                                mb_create_datetime = reqModel.update_datetime,
                                mb_update_member = reqModel.mm_id,
                                mb_update_datetime = reqModel.update_datetime
                            });

                            //扣除押金交易紀錄
                            mInsertTR.Add(new transaction_record()
                            {
                                tr_code = mTrCode,
                                tr_mm_id = reqModel.mm_id,
                                tr_payee_mm_id = 0,
                                tr_tm_id = mGetTenderConditionData[i].tm_id,
                                tr_td_id = 0,
                                tr_pp_id = 0,
                                tr_type = "11",
                                tr_status = "90",
                                tr_mm_points = 8000,
                                tr_income_type = "O",
                                tr_create_member = reqModel.mm_id,
                                tr_create_datetime = reqModel.update_datetime,
                                tr_update_member = reqModel.mm_id,
                                tr_update_datetime = reqModel.update_datetime
                            });
                        }
                        #endregion 押金只有第一筆才做處理 END

                        #region 處理該標案下標紀錄
                        mNowPoints = mNowPoints - 8000;
                        //扣除該期標案金額
                        mInsertBalance.Add(new member_balance()
                        {
                            mb_mm_id = reqModel.mm_id,
                            mb_payee_mm_id = 0,
                            mb_tm_id = mGetTenderConditionData[i].tm_id,
                            mb_td_id = mGetTenderConditionData[i].td_id,
                            mb_income_type = "O",
                            mb_tr_code = mTrCode,
                            mb_tr_type = "12",
                            mb_type = "12",//下標時扣除押金
                            mb_points_type = "11",
                            mb_change_points = 8000,
                            mb_points = mNowPoints,
                            mb_create_member = reqModel.mm_id,
                            mb_create_datetime = reqModel.update_datetime,
                            mb_update_member = reqModel.mm_id,
                            mb_update_datetime = reqModel.update_datetime
                        });

                        //扣除標案金額交易紀錄
                        mInsertTR.Add(new transaction_record()
                        {
                            tr_code = mTrCode,
                            tr_mm_id = reqModel.mm_id,
                            tr_payee_mm_id = 0,
                            tr_tm_id = mGetTenderConditionData[i].tm_id,
                            tr_td_id = mGetTenderConditionData[i].td_id,
                            tr_pp_id = 0,
                            tr_type = "12",
                            tr_status = "90",
                            tr_mm_points = 8000,
                            tr_income_type = "O",
                            tr_create_member = reqModel.mm_id,
                            tr_create_datetime = reqModel.update_datetime,
                            tr_update_member = reqModel.mm_id,
                            tr_update_datetime = reqModel.update_datetime
                        });
                        #endregion 處理該標案下標紀錄
                    }


                    //流程8. 判斷比數1，新增活動紀錄(tender_activity_record)
                    mIsOk = _dbAccess.ExecuteTransactionObject<tender_activity_record>(mSqlInsertTar, mInsertTar) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "下標失敗、新增標案活動紀錄筆數錯誤";
                        return mResult;
                    }

                    //流程9. 判斷新增帳務紀錄筆數標數總量 + m_CNTCheck才對(因為 + m_CNTCheck是押金部分)
                    mIsOk = _dbAccess.ExecuteTransactionObject<List<member_balance>>(mSqlInsertBalance, mInsertBalance) == (mCNT + mCNTCheck);
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "下標失敗、新增帳務紀錄筆數錯誤";
                        return mResult;
                    }

                    //流程10. 判斷新增交易紀錄筆數標數總量 + m_CNTCheck才對(因為 + m_CNTCheck是押金部分)
                    mIsOk = _dbAccess.ExecuteTransactionObject<List<transaction_record>>(mSqlInsertTR, mInsertTR) == (mCNT + mCNTCheck);
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "下標失敗、新增交易紀錄筆數錯誤";
                        return mResult;
                    }

                    //流程11. 判斷筆數1，更新標案主檔(tender_master)，下標人、已經下標數量
                    mIsOk = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateTenderMaster
                                , new
                                {
                                    tm_bidder = reqModel.mm_id,
                                    cnt = mCNT,
                                    tm_count = mGetTenderConditionData.Count,
                                    mm_id = reqModel.mm_id,
                                    tm_update_datetime = reqModel.update_datetime,
                                    tm_id = reqModel.tm_id
                                }) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "下標失敗、更新標案主檔筆數錯誤";
                        return mResult;
                    }

                    //流程12. 判斷筆數，更新標案明細(tender_detail)，下標人、該名系狀態
                    mSqlUpdateTenderDetail = string.Format(mSqlUpdateTenderDetail, string.Join(',', mGetTenderConditionData.Select(s => s.td_id)));

                    mIsOk = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateTenderDetail
                                , new
                                {
                                    mm_id = reqModel.mm_id,
                                    td_tm_id = reqModel.tm_id,
                                    td_update_datetime = reqModel.update_datetime
                                }) == mGetTenderConditionData.Count;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "下標失敗、更新標案明細筆數錯誤";
                        return mResult;
                    }

                    //流程13. 判斷筆數1，更新帳戶錢包，下標數量、錢包餘額
                    mIsOk = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateMW
                                , new
                                {
                                    mw_subscripts_count = mCNT,
                                    mm_id = reqModel.mm_id,
                                    mw_stored = mNeedAMT,
                                    mw_update_datetime = reqModel.update_datetime
                                }) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "下標失敗、更新下標人錢包筆數錯誤";
                        return mResult;
                    }

                    //流程14. 判斷筆數1，更新標案主檔(tender_master)狀態，因如果標案如果滿24就要自動新增一個該類型標案
                    DateTime mSettlementDatetime;
                    int mDayOfMonth = reqModel.update_datetime.Day;
                    if (mDayOfMonth >= 2 || mDayOfMonth <= 16)
                    {
                        mSettlementDatetime = new DateTime(reqModel.update_datetime.Year, reqModel.update_datetime.Month, 16).AddMonths(1);
                    }
                    else if (mDayOfMonth >= 17)
                    {
                        mSettlementDatetime = new DateTime(reqModel.update_datetime.Year, reqModel.update_datetime.Month, 1).AddMonths(2);
                    }
                    else if (mDayOfMonth == 1)
                    {
                        mSettlementDatetime = new DateTime(reqModel.update_datetime.Year, reqModel.update_datetime.Month, 1).AddMonths(1);
                    }
                    else
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "下標失敗、成組後更新標案主檔錯誤";
                        return mResult;
                    }

                    DateTime mWinEndDatetime = mSettlementDatetime.AddMonths(24);

                    var m_adCNT = _dbAccess.ExecuteTransactionObject<object>(mSqlUpdateTM
                                        , new
                                        {
                                            mm_id = reqModel.mm_id
                                            ,tm_settlement_datetime = mSettlementDatetime
                                            ,tm_win_first_datetime = mSettlementDatetime
                                            ,tm_win_end_datetime = mWinEndDatetime
                                            ,tm_update_datetime = reqModel.update_datetime
                                            ,tm_id = reqModel.tm_id
                                        });

                    if (m_adCNT > 1)
                    {
                        _dbAccess.Rollback();

                        mResult.returnStatus = 999;
                        mResult.returnMsg = "下標失敗、更新標案成組資料失敗";
                        return mResult;
                    }

                    //流程15. 判斷是否正確，下標後，標案數量如果滿24就要自動新增一個該類型標案
                    if ((mGetTenderData.tm_count + mCNT) == 24)
                    {
                        mResult = PostTender(_dbAccess, mGetTenderData.tm_type, reqModel.update_datetime);

                        if (!mResult.Result)
                        {
                            _dbAccess.Rollback();

                            return mResult;
                        }
                        else
                        {
                            //如果沒錯就初始化該回傳資料
                            mResult = new();
                        }
                    }
                }
                else
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "下標失敗、餘額不足";
                    return mResult;
                }

                if (mIsOk)
                {
                    _dbAccess.Commit();
                    //_dbAccess.Rollback();
                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "下標成功";
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.returnStatus = 999;
                    mResult.returnMsg = "下標失敗";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"下標失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        private ServiceResultDTO<bool> PostTender(IDBAccess dbAccess, string tm_type, DateTime nowDateTime)
        {
            string mActionLog = "PostTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();

            try
            {
                string mSqlGetTMSeq = @"SELECT NEXTVAL(rosca2.tender_master_sequence);";

                string m_sqlMaster = @"
INSERT INTO rosca2.tender_master
            (tm_id #主檔ID
             ,tm_name #標會名稱
             ,tm_ticks #取得時間加字串(暫時沒用，用來記錄時間點)
             ,tm_initiator_mm_id #發起人
             ,tm_type #標會種類:1-一般玩法(散會:可以選下標數)、2-1人玩法、3-2人玩法、3-4人玩法
             ,tm_bidder #下標人會用|串接，EX:1|2|3
             ,tm_winners #得獎者會用|串接，EX:1|2|3
             ,tm_current_period #目前期數(成組時，當下期數，剛寫入就是1)
             ,tm_status #是否成組:0-未成組,1-成組,2-結束
             ,tm_group_datetime #是否成組:0-未成組,1-成組,2-結束
             ,tm_create_member
             ,tm_create_datetime
             ,tm_update_member
             ,tm_update_datetime)
VALUES     (@tm_id
            ,@tm_name
            ,@tm_ticks
            ,0
            ,@tm_type
            ,''
            ,''
            ,0
            ,'0'
            ,NULL
            ,0
            ,@tm_create_datetime
            ,0
            ,@tm_update_datetime); 
";

                string m_sqlDetail = @"
INSERT INTO rosca2.tender_detail
            (td_tm_id #主檔ID
             ,td_participants #參加者mm_id
             ,td_sequence #標案明細序列(用來方便做查找)
             ,td_period #該標中標期數
             ,td_status #下標狀態
             ,td_tm_initiator_mm_id #發起者mm_id=tm_initiator_mm_id
             ,td_create_member
             ,td_create_datetime
             ,td_update_member
             ,td_update_datetime)
VALUES     (@td_tm_id
            ,NULL
            ,@td_sequence
            ,0
            ,'0'
            ,0
            ,0
            ,@td_create_datetime
            ,0
            ,@td_update_datetime); 
";
                tender_master mTM = new();
                List<tender_detail> mTdList = new();

                mTM.tm_name = "下標時自動標案新增";
                mTM.tm_ticks = $@"TM{DateTime.Now.Ticks}";
                mTM.tm_type = tm_type;
                //mTM.tm_bidder = string.Empty;
                //mTM.tm_winners = string.Empty;
                //mTM.tm_current_period = 0;
                mTM.tm_create_datetime = nowDateTime;
                mTM.tm_update_datetime = nowDateTime;

                for (int i = 1; i < 25; i++)
                {
                    mTdList.Add(new tender_detail()
                    {
                        td_sequence = i,
                        td_create_datetime = nowDateTime,
                        td_update_datetime = nowDateTime
                    });
                }

                long mGetTMSeq = dbAccess.ExecuteTransactionGetObject<long>(mSqlGetTMSeq, null).First();
                int mProcCNT = 0;
                bool isOk = true;

                mTM.tm_id = mGetTMSeq;
                mTdList.ForEach(f => f.td_tm_id = mGetTMSeq);

                mProcCNT = dbAccess.ExecuteTransactionObject(m_sqlMaster, mTM);
                isOk = mProcCNT == 1;

                if (isOk)
                {
                    mProcCNT = dbAccess.ExecuteTransactionObject(m_sqlDetail, mTdList);
                    isOk = mProcCNT == 24;
                }

                if (isOk)
                {
                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = $@"下標時新增標案成功";
                }
                else
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"下標時新增標案失敗";
                }
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"下標時新增標案失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
