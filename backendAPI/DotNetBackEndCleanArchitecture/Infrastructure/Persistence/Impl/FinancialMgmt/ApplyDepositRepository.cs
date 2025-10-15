using DomainAbstraction.Interface.FinancialMgmt;
using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.Entity.FinancialMgmt.ApplyDeposit;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Impl.FinancialMgmt
{
    public class ApplyDepositRepository : IApplyDepositRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "ApplyDepositRepository";

        public ApplyDepositRepository(
                ILogger<ApplyDepositRepository> logger,
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
        /// 後台-取得儲值申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositAdminRespModel>> QueryApplyDepositAdmin(QueryApplyDepositAdminDTO reqModel)
        {
            string mActionLog = "QueryApplyDepositAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositAdminRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositAdminRespModel>>
            {
                Result = new BaseGridRespDTO<QueryApplyDepositAdminRespModel>
                {
                    GridRespResult = new List<QueryApplyDepositAdminRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT ad_id
       ,ad_mm_id
       ,ad_amount
       ,ad_key #儲值後得到的KEY(要給我們才查的到)
       ,ad_url #儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
       ,ad_picture #儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
       ,ad_file_name #成功時貼上畫面所需儲存檔名
       ,ad_status #申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
       ,ad_type #儲值狀態:10-虛擬幣、50-銀行轉帳
       ,ad_kyc_id
       ,ad_create_member
       ,ad_create_datetime
       ,ad_update_member
       ,ad_update_datetime
       ,B.mm_account
FROM   rosca2.apply_deposit AS A
       INNER JOIN rosca2.member_master AS B
               ON B.mm_id = A.ad_mm_id 

";

                //搜尋條件
                string mQueryWhereString = " WHERE  1 = 1 ";

                if (!string.IsNullOrEmpty(reqModel.mm_account.Trim()))
                {
                    mQueryWhereString += @" AND B.mm_account = @mm_account ";
                }
                if (!string.IsNullOrEmpty(reqModel.mm_name.Trim()))
                {
                    mQueryWhereString += @" AND B.mm_name LIKE CONCAT('%', @mm_name, '%') ";
                }
                if (!string.IsNullOrEmpty(reqModel.ad_status.Trim()))
                {
                    mQueryWhereString += @" AND A.ad_status = @ad_status ";
                }

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER  BY A.ad_create_datetime   ";

                //取得筆數
                string mQueryFetchString = @"
 OFFSET  @skipCNT ROWS #跳過筆數、因標案筆數1頁10筆的話有240筆包含明細
 FETCH NEXT @takeCNT ROWS ONLY;  #取得筆數、因標案筆數1頁10筆的話有240筆包含明細
";
                //檢查是否要取全部資料
                if (reqModel.preGetIndex == 0)
                {
                    mQueryFetchString = "";
                }
                else
                {
                    //取得跳過、與提取多少筆資料
                    reqModel.skipCNT = reqModel.specPageSize * (reqModel.pageSize * (reqModel.pageIndex - 1));
                    reqModel.takeCNT = reqModel.specPageSize * reqModel.pageSize;
                    if (reqModel.takeCNT == 0)
                    {
                        reqModel.takeCNT = 10;
                    }
                }

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryOrderByString} {mQueryFetchString} ";
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryApplyDepositAdminRespModel>(mSql, reqModel);

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First();

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得申請儲值單程供";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得儲值申請列表失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-取得儲值申請列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositUserRespModel>> QueryApplyDepositUser(QueryApplyDepositUserDTO reqModel)
        {
            string mActionLog = "QueryApplyDepositUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositUserRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryApplyDepositUserRespModel>>
            {
                Result = new BaseGridRespDTO<QueryApplyDepositUserRespModel>
                {
                    GridRespResult = new List<QueryApplyDepositUserRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT ad_id
       ,ad_mm_id
       ,ad_amount
       ,ad_key #儲值後得到的KEY(要給我們才查的到)
       ,ad_url #儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
       ,ad_picture #儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
       ,ad_status #申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
       ,ad_create_member
       ,ad_create_datetime
       ,ad_update_member
       ,ad_update_datetime
FROM   rosca2.apply_deposit AS A
";

                //搜尋條件
                string mQueryWhereString = " WHERE  1 = 1 ";
                if (reqModel.ad_mm_id != 0)
                {
                    mQueryWhereString += @" AND A.ad_mm_id = @ad_mm_id ";
                }
                if (!string.IsNullOrEmpty(reqModel.ad_status.Trim()))
                {
                    mQueryWhereString += @" AND A.ad_status = @ad_status ";
                }

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER  BY A.ad_create_datetime   ";

                //取得筆數
                string mQueryFetchString = @"
 OFFSET  @skipCNT ROWS #跳過筆數、因標案筆數1頁10筆的話有240筆包含明細
 FETCH NEXT @takeCNT ROWS ONLY;  #取得筆數、因標案筆數1頁10筆的話有240筆包含明細
";
                //檢查是否要取全部資料
                if (reqModel.preGetIndex == 0)
                {
                    mQueryFetchString = "";
                }
                else
                {
                    //取得跳過、與提取多少筆資料
                    reqModel.skipCNT = reqModel.specPageSize * (reqModel.pageSize * (reqModel.pageIndex - 1));
                    reqModel.takeCNT = reqModel.specPageSize * reqModel.pageSize;
                    if (reqModel.takeCNT == 0)
                    {
                        reqModel.takeCNT = 10;
                    }
                }

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryOrderByString} {mQueryFetchString} ";
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryApplyDepositUserRespModel>(mSql, reqModel);

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First();
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得儲值申請列表失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-取得儲值資料
        /// </summary>
        /// <returns></returns>
        public ServiceResultDTO<GetDepositDataRespModel> GetDepositData()
        {
            string mActionLog = "GetDepositData";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetDepositDataRespModel> mResult = new ServiceResultDTO<GetDepositDataRespModel>();
            try
            {
                string m_sql = @"
SELECT A.sps_picture, A.sps_parameter01  FROM rosca2.system_parameter_setting AS A WHERE A.sps_code = 'DepositURL' LIMIT 1
";
                mResult.Result = _dbAccess.GetObject<GetDepositDataRespModel>(m_sql).FirstOrDefault();
                if (mResult.Result == null
                    || mResult.Result.sps_picture == null
                    || string.IsNullOrEmpty(mResult.Result.sps_parameter01))
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "取得儲值資料失敗";
                }
                else
                {
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "取得儲值資料成功";
                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得儲值資料失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-申請儲值(已付款要丟KEY)
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
//        public ServiceResultDTO<bool> PostApplyDeposit(PostApplyDepositDTO reqModel)
//        {
//            string mActionLog = "PostApplyDeposit";
//            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

//            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

//            try
//            {
//                string mSql = @"
//INSERT INTO rosca2.apply_deposit
//            (ad_mm_id
//             ,ad_amount
//             ,ad_key
//             ,ad_url
//             ,ad_picture
//             ,ad_status
//             ,ad_create_member
//             ,ad_create_datetime
//             ,ad_update_member
//             ,ad_update_datetime)
//VALUES      (@ad_mm_id
//             ,@ad_amount
//             ,@ad_key
//             ,@ad_url
//             ,@ad_picture
//             ,@ad_status
//             ,@ad_mm_id
//             ,@ad_create_datetime
//             ,@ad_mm_id
//             ,@ad_update_datetime); 
//";
//                _dbAccess.BeginTrans();

//                int mCNT = _dbAccess.ExecuteTransactionObject<PostApplyDepositDTO>(mSql, reqModel);
//                if (mCNT == 1)
//                {
//                    _dbAccess.Commit();

//                    mResult.Result = true;
//                    mResult.returnStatus = 1;
//                    mResult.returnMsg = "新增儲值申請成功";
//                }
//                else
//                {
//                    _dbAccess.Rollback();

//                    mResult.Result = false;
//                    mResult.returnStatus = 999;
//                    mResult.returnMsg = $@"新增儲值申請失敗、更新筆數;{mCNT}";
//                }
//            }
//            catch (Exception ex)
//            {
//                _dbAccess.Rollback();

//                mResult.Result = false;
//                mResult.returnStatus = 999;
//                mResult.returnMsg = $@"新增儲值申請失敗、{ex.Message}";

//                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

//            }

//            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

//            return mResult;
//        }

        /// <summary>
        /// 會員-申請儲值(已付款要丟KEY)
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PostApplyDeposit(PostApplyDepositDTO reqModel)
        {
            string mActionLog = "PostApplyDeposit";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                string mSql = @"
INSERT INTO rosca2.apply_deposit
            (ad_mm_id
             ,ad_amount
             ,ad_key
             ,ad_url
             ,ad_picture
             ,ad_akc_mw_address
             ,ad_file_name
             ,ad_status
             ,ad_type
             ,ad_kyc_id
             ,ad_create_member
             ,ad_create_datetime
             ,ad_update_member
             ,ad_update_datetime)
VALUES      (@ad_mm_id
             ,@ad_amount
             ,@ad_key
             ,@ad_url
             ,@ad_picture
             ,@ad_akc_mw_address
             ,@ad_file_name
             ,@ad_status
             ,@ad_type
             ,@ad_kyc_id
             ,@ad_mm_id
             ,@ad_create_datetime
             ,@ad_mm_id
             ,@ad_update_datetime); 
";
                _dbAccess.BeginTrans();

                int mCNT = _dbAccess.ExecuteTransactionObject<PostApplyDepositDTO>(mSql, reqModel);
                if (mCNT == 1)
                {
                    _dbAccess.Commit();

                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "新增儲值申請成功";
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"新增儲值申請失敗、更新筆數;{mCNT}";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增儲值申請失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-申請儲值覆核
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PutApplyDeposit(PutApplyDepositDTO reqModel)
        {
            string mActionLog = "PutApplyDeposit";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                //取得申請人member_master資料並鎖定，該部分取得的資料都是儲值人
                string mSqlGetMMConditionData = @"
SELECT A.mm_id
       ,B.mw_stored
       ,IFNULL((SELECT C.ad_amount
                FROM   rosca2.apply_deposit AS C
                WHERE  C.ad_id = @ad_id
                       AND C.ad_status IN( '10', '13' )
                       AND C.ad_mm_id = A.mm_id
                LIMIT  1), 0)                                AS ad_amount
       ,(SELECT NEXTVAL(rosca2.transaction_record_sequence)) AS tr_id
FROM   rosca2.member_master AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.mm_id
WHERE  A.mm_id = @ad_mm_id
       AND A.mm_status = 'Y' 
#FOR UPDATE 
";
                //                string mSqlGetMMConditionData = @"
                //SELECT A.mm_id
                //       ,B.mw_stored
                //       ,C.ad_amount
                //       ,(SELECT NEXTVAL(rosca2.transaction_record_sequence)) AS tr_id
                //FROM   rosca2.member_master AS A
                //       INNER JOIN rosca2.member_wallet AS B
                //               ON B.mw_mm_id = A.mm_id
                //       INNER JOIN rosca2.apply_deposit AS C
                //               ON C.ad_mm_id = A.mm_id
                //WHERE  A.mm_id = 1
                //       AND A.mm_status = 'Y'
                //       AND C.ad_id = 2
                //       AND C.ad_status IN( '10', '13' ) ;
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
SET    mw_stored = mw_stored + @ad_amount
       ,mw_update_member = @mm_id
       ,mw_update_datetime = @create_datetime
WHERE  mw_mm_id = @ad_mm_id; 
";

                string mSqlUpdateAD = @"
UPDATE rosca2.apply_deposit
SET    ad_status = @ad_status
       ,ad_update_member = @mm_id
       ,ad_update_datetime = @create_datetime
WHERE  ad_id = @ad_id;  
";

                if (reqModel.ad_status == "11")
                {
                    member_balance mInsertBalance = new member_balance();
                    transaction_record mInsertTR = new transaction_record();

                    _dbAccess.BeginTrans();
                    PutApplyDepositPO mGetMMConditionData = _dbAccess.ExecuteTransactionGetObject<PutApplyDepositPO>(mSqlGetMMConditionData, reqModel).First();
                    if (mGetMMConditionData.ad_amount == 0)
                    {
                        _dbAccess.Rollback();
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "覆核申請儲值失敗、找不到儲值申請單或是已經覆核過";

                        return mResult;
                    }

                    string mTrCode = GetFieldID.process("tr", mGetMMConditionData.tr_id, reqModel.create_datetime);

                    //新增帳務紀錄
                    mInsertBalance.mb_mm_id = mGetMMConditionData.mm_id;
                    mInsertBalance.mb_payee_mm_id = 0;
                    mInsertBalance.mb_tm_id = 0;
                    mInsertBalance.mb_td_id = 0;
                    mInsertBalance.mb_income_type = "I";
                    mInsertBalance.mb_tr_code = mTrCode;
                    mInsertBalance.mb_tr_type = "19";
                    mInsertBalance.mb_type = "22";
                    mInsertBalance.mb_points_type = "11";
                    mInsertBalance.mb_change_points = mGetMMConditionData.ad_amount;
                    mInsertBalance.mb_points = mGetMMConditionData.mw_stored + mGetMMConditionData.ad_amount;
                    mInsertBalance.mb_create_member = reqModel.mm_id;
                    mInsertBalance.mb_create_datetime = reqModel.create_datetime;
                    mInsertBalance.mb_update_member = reqModel.mm_id;
                    mInsertBalance.mb_update_datetime = reqModel.create_datetime;

                    //新增交易紀錄
                    mInsertTR.tr_code = mTrCode;
                    mInsertTR.tr_mm_id = mGetMMConditionData.mm_id;
                    mInsertTR.tr_payee_mm_id = 0;
                    mInsertTR.tr_tm_id = 0;
                    mInsertTR.tr_td_id = 0;
                    mInsertTR.tr_pp_id = 0;
                    mInsertTR.tr_type = "19";
                    mInsertTR.tr_status = "90";
                    mInsertTR.tr_mm_points = mGetMMConditionData.ad_amount;
                    mInsertTR.tr_income_type = "I";
                    mInsertTR.tr_create_member = reqModel.mm_id;
                    mInsertTR.tr_create_datetime = reqModel.create_datetime;
                    mInsertTR.tr_update_member = reqModel.mm_id;
                    mInsertTR.tr_update_datetime = reqModel.create_datetime;

                    mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlInsertBalance, mInsertBalance) == 1;
                    if (!mResult.Result)
                    {
                        _dbAccess.Rollback();
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "覆核申請儲值失敗、寫入帳務紀錄筆數不等於1";

                        return mResult;
                    }

                    mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlInsertTR, mInsertTR) == 1;
                    if (!mResult.Result)
                    {
                        _dbAccess.Rollback();
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "覆核申請儲值失敗、寫入交易紀錄筆數不等於1";

                        return mResult;
                    }

                    mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlUpdateMW, new
                    {
                        ad_amount = mGetMMConditionData.ad_amount,
                        mm_id = reqModel.mm_id,
                        create_datetime = reqModel.create_datetime,
                        ad_mm_id = reqModel.ad_mm_id
                    }) == 1;
                    if (!mResult.Result)
                    {
                        _dbAccess.Rollback();
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "覆核申請儲值失敗、更新會員錢包筆數不等於";

                        return mResult;
                    }

                    mResult.Result = _dbAccess.ExecuteTransactionObject(mSqlUpdateAD, reqModel) == 1;
                    if (mResult.Result)
                    {
                        //_dbAccess.Rollback();
                        _dbAccess.Commit();
                        mResult.returnStatus = 1;
                        mResult.returnMsg = "覆核申請儲值成功";
                    }
                    else
                    {
                        _dbAccess.Rollback();
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "覆核申請儲值失敗、更新儲值申請單筆數不等於1";
                    }
                }
                else if (reqModel.ad_status == "12")
                {
                    _dbAccess.BeginTrans();
                    mResult.Result = _dbAccess.ExecNonQuery<PutApplyDepositDTO>(mSqlUpdateAD, reqModel) == 1;
                    if (mResult.Result)
                    {
                        _dbAccess.Commit();
                        mResult.returnStatus = 1;
                        mResult.returnMsg = "覆核申請儲值成功，駁回成功";
                    }
                    else
                    {
                        _dbAccess.Rollback();
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "覆核申請儲值失敗，駁回失敗";
                    }
                }
                else
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"覆核申請儲值失敗，傳入非正確覆核狀態:{reqModel.ad_status}";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"覆核申請儲值失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
