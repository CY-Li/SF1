using DomainAbstraction.Interface.MemberMgmt;
using DomainEntity.Common;
using DomainEntity.Entity.MemberMgmt.Wallet;
using DomainEntityDTO.Entity.MemberMgmt.Wallet;
using InfraCommon.DBA;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Impl.MemberMgmt
{
    public class WalletRepository : IWalletRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "WalletRepository";

        public WalletRepository(
                ILogger<WalletRepository> logger,
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
        /// 後台-取得會員錢包列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<BaseGridRespDTO<QueryWalletRespModel>> QueryWallet(QueryWalletDTO reqModel)
        {
            string mActionLog = "QueryWallet";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryWalletRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryWalletRespModel>>
            {
                Result = new BaseGridRespDTO<QueryWalletRespModel>
                {
                    GridRespResult = new List<QueryWalletRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT A.mw_id
       ,A.mw_mm_id
       ,B.mm_account
       ,B.mm_name
       ,A.mw_currency #貨幣種類
       ,A.mw_address #虛擬幣錢包地址
       ,A.mw_subscripts_count #下標數(自己)，system_parameters:10
       ,A.mw_stored #儲值點數(可用點數)，system_parameters:11
       ,A.mw_reward #紅利點數，system_parameters:12
       ,A.mw_peace #平安點數，system_parameters:13
       ,A.mw_mall #商城點數，system_parameters:14
       ,A.mw_registration #註冊點數，system_parameters:15
       ,A.mw_death #死會點數，system_parameters:16
       ,A.mw_accumulation #累積獎勵，system_parameters:17
       ,A.mw_create_member
       ,A.mw_create_datetime
       ,A.mw_update_member
       ,A.mw_update_datetime
       #{0}
FROM   rosca2.member_wallet AS A
       INNER JOIN rosca2.member_master AS B
               ON B.mm_id = A.mw_mm_id 
 ";

//                string mSqlGetParticipateCount = @"
//,(SELECT COUNT(*)
//FROM   rosca2.tender_master AS A
//       INNER JOIN rosca2.tender_detail AS B
//               ON B.td_tm_id = A.tm_id
//WHERE  A.tm_status = 1
//       AND B.td_participants = @mm_id
//       AND B.td_period = 0  ) AS participate_count
//";
                //搜尋條件
                string mQueryWhereString = " WHERE  1 = 1 ";
                if (!string.IsNullOrEmpty(reqModel.mm_account))
                {
                    //mQuerySql = string.Format(mQuerySql, mSqlGetParticipateCount);
                    //mQueryWhereString += @" AND A.mm_id = (SELECT A.mm_id FROM rosca.member_master AS A WHERE A.mm_account = @mm_account AND A.mm_status = 'Y' LIMIT 1) ";
                    mQueryWhereString += @" AND B.mm_account = @mm_account ";
                }

                if (!string.IsNullOrEmpty(reqModel.mm_name))
                {
                    //mQuerySql = string.Format(mQuerySql, mSqlGetParticipateCount);
                    //mQueryWhereString += @" AND A.mm_id = (SELECT A.mm_id FROM rosca.member_master AS A WHERE A.mm_account = @mm_account AND A.mm_status = 'Y' LIMIT 1) ";
                    mQueryWhereString += @" AND B.mm_name LIKE CONCAT('%', @mm_name, '%') ";
                }
                //else
                //{
                //    mQuerySql = string.Format(mQuerySql, "");
                //}


                //ORDER BY 寫的位置
                string mQueryOrderByString = @"  ";
                if (string.IsNullOrEmpty(reqModel.pageSort))
                {
                    mQueryOrderByString = @" ORDER  BY A.mw_mm_id ";
                }
                else
                {
                    mQueryOrderByString = $@" ORDER  BY {reqModel.pageSort} {reqModel.pageSortDirection}  ";
                }


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

                var m_sql = $@"{mQuerySql} {mQueryWhereString} {mQueryOrderByString} {mQueryFetchString} ";
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryWalletRespModel>(m_sql, reqModel);

                m_sql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(m_sql, reqModel).First();
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得會員錢包列表成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得會員錢包列表失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");
            return mResult;
        }

        /// <summary>
        /// 取得該會員錢包資料
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        public ServiceResultDTO<GetWalletRespModel> GetWallet(long mm_id)
        {
            string mActionLog = "GetWallet";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetWalletRespModel> mResult = new ServiceResultDTO<GetWalletRespModel>();

            try
            {
                string mSql = @"
SELECT A.mw_id
       ,A.mw_mm_id
       ,A.mw_currency #貨幣種類
       ,A.mw_address #虛擬幣錢包地址
       ,A.mw_subscripts_count #下標數(自己)，system_parameters:10
       ,A.mw_stored #儲值點數(可用點數)，system_parameters:11
       ,A.mw_reward #紅利點數，system_parameters:12
       ,A.mw_peace #平安點數，system_parameters:13
       ,A.mw_mall #商城點數，system_parameters:14
       ,A.mw_registration #註冊點數，system_parameters:15
       ,A.mw_death #死會點數，system_parameters:16
       ,A.mw_accumulation #累積獎勵，system_parameters:17
       ,A.mw_create_member
       ,A.mw_create_datetime
       ,A.mw_update_member
       ,A.mw_update_datetime
       ,(SELECT SUM(8000)
        FROM   rosca2.tender_master AS A
               INNER JOIN rosca2.tender_detail AS B
                       ON B.td_tm_id = A.tm_id
        WHERE  B.td_participants = @mm_id
               AND B.td_period = 0)      AS current_bid_fee
       ,(SELECT SUM(A.group_amount)
         FROM   (SELECT CASE
                          WHEN COUNT(B.td_id) > 0 THEN 8000
                          ELSE 0
                        END AS group_amount
                 FROM   rosca2.tender_master AS A
                        INNER JOIN rosca2.tender_detail AS B
                                ON B.td_tm_id = A.tm_id
                 WHERE  B.td_participants = @mm_id
                        AND B.td_period = 0
                 GROUP  BY A.tm_id) AS A)AS next_bid_estimate 
FROM   rosca2.member_wallet AS A
WHERE  A.mw_mm_id = @mm_id ;
";
                var mGetWallet = _dbAccess.GetObject<GetWalletRespModel>(mSql, new { mm_id = mm_id });

                if (mGetWallet.Count == 1)
                {
                    mResult.Result = mGetWallet.First();
                    mResult.Result.next_bid_estimate = mResult.Result.current_bid_fee - mResult.Result.next_bid_estimate;

                    mResult.returnStatus = 1;
                    mResult.returnMsg = "取得會員錢包成功";
                }
                else
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "取得會員錢包失敗、沒有該會員";
                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得會員錢包失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
