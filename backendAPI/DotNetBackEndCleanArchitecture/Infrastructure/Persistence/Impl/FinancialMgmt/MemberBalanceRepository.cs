using DomainAbstraction.Interface.FinancialMgmt;
using DomainEntity.Common;
using DomainEntity.Entity.FinancialMgmt.MemberBalance;
using DomainEntityDTO.Entity.FinancialMgmt.MemberBalance;
using InfraCommon.DBA;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Persistence.Impl.FinancialMgmt
{
    public class MemberBalanceRepository : IMemberBalanceRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "MemberBalanceRepository";

        public MemberBalanceRepository(
                ILogger<MemberBalanceRepository> logger,
                IDBAccess dbAccess,
                IOptions<AppConfig> appConfig
            )
        {
            _logger = logger;
            _dbAccess = dbAccess;
            _dbAccess.SetConnectionString(appConfig.Value.ConnectionStrings.BackEndDatabase);
            _strConnection = appConfig.Value.ConnectionStrings.BackEndDatabase;
        }

        public ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceAdminRespModel>> QueryMemberBalanceAdmin(QueryMemberBalanceAdminDTO reqModel)
        {
            string mActionLog = "QueryMemberBalanceAdimin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceAdminRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceAdminRespModel>>
            {
                Result = new BaseGridRespDTO<QueryMemberBalanceAdminRespModel>
                {
                    GridRespResult = new List<QueryMemberBalanceAdminRespModel>()
                }
            };

            try
            {
                //                string mQuerySql = @"
                //SELECT A.mb_id
                //       ,A.mb_mm_id           #會員ID
                //       ,A.mb_payee_mm_id     #收款人
                //       ,A.mb_tm_id       #標案ID
                //       ,A.mb_td_id       #標案明細ID
                //       ,A.mb_income_type     #點數流向方向
                //       ,A.mb_tr_type         #交易類別
                //       ,A.mb_type            #帳務類別
                //       ,A.mb_points_type     #點數類型
                //       ,A.mb_change_points   #點數異動額
                //       ,A.mb_points          #點數異動後額度
                //       ,A.mb_create_datetime #新增時間
                //FROM   rosca2.member_balance AS A
                //WHERE  A.mb_mm_id = @mb_mm_id
                //";

                string mQuerySql = @"
SELECT B.mb_id
       ,A.mb_mm_id
       ,A.mb_payee_mm_id
       ,A.mb_tm_id
       ,A.mb_td_id
       ,A.mb_income_type
       ,CASE A.mb_income_type
          WHEN 'I' THEN '流入'
          WHEN 'O' THEN '流出'
          ELSE '錯誤'
        END AS mb_income_type_name
       ,A.mb_tr_type
       ,B.mb_tr_code
       ,B.mb_type
       ,B.mb_points_type
       ,B.mb_change_points
       ,A.mb_points
       ,A.mb_create_datetime
       ,C.sp_name AS mb_tr_type_name
       ,D.sp_name AS mb_type_name
       ,E.sp_name AS mb_points_type_name
FROM   rosca2.member_balance AS A
       INNER JOIN(SELECT MAX(AA.mb_id)             AS mb_id
                         ,AA.mb_tr_code
                         ,AA.mb_type               #帳務類別
                         ,AA.mb_points_type        #點數類型
                         ,SUM(AA.mb_change_points) AS mb_change_points
                  FROM   rosca2.member_balance AS AA
						 INNER JOIN rosca2.member_master AS BB
								 ON BB.mm_id = AA.mb_mm_id
                  WHERE  BB.mm_account = @mm_account
                  GROUP  BY AA.mb_tr_code
                            ,AA.mb_type
                            ,AA.mb_points_type) AS B
               ON B.mb_id = A.mb_id
       LEFT JOIN rosca2.parameter_category AS C
              ON C.sp_code = A.mb_tr_type
                 AND C.sp_key = 'TRType'
       LEFT JOIN rosca2.parameter_category AS D
              ON D.sp_code = A.mb_type
                 AND D.sp_key = 'MBType'
       LEFT JOIN rosca2.parameter_category AS E
              ON E.sp_code = B.mb_points_type
                 AND E.sp_key = 'MBPointsType'
WHERE 1 = 1
";

                //搜尋條件
                string mQueryWhereString = @"
       #AND A.mb_create_datetime >= DATE_FORMAT(CURDATE(), '%Y-%m-01')
       #AND A.mb_create_datetime < DATE_ADD(DATE_FORMAT(CURDATE(), '%Y-%m-01'), INTERVAL 1 MONTH) 
";
                if (!string.IsNullOrEmpty(reqModel.mb_points_type)
                    //&& Regex.Match(reqModel.mb_points_type, "^\\d{11,16}$").Success
                    && Regex.Match(reqModel.mb_points_type, "^(?:11|12|13|14|15|16)?$").Success
                    )
                {
                    mQueryWhereString += @" AND B.mb_points_type = @mb_points_type ";
                }

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER BY A.mb_id    ";

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
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryMemberBalanceAdminRespModel>(mSql, reqModel);

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First();
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得帳務紀錄成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得帳務紀錄失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        public ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceUserRespModel>> QueryMemberBalanceUser(QueryMemberBalanceDTO reqModel)
        {
            string mActionLog = "QueryMemberBalanceUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceUserRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryMemberBalanceUserRespModel>>
            {
                Result = new BaseGridRespDTO<QueryMemberBalanceUserRespModel>
                {
                    GridRespResult = new List<QueryMemberBalanceUserRespModel>()
                }
            };

            try
            {
                //                string mQuerySql = @"
                //SELECT A.mb_id
                //       ,A.mb_mm_id           #會員ID
                //       ,A.mb_payee_mm_id     #收款人
                //       ,A.mb_tm_id       #標案ID
                //       ,A.mb_td_id       #標案明細ID
                //       ,A.mb_income_type     #點數流向方向
                //       ,A.mb_tr_type         #交易類別
                //       ,A.mb_type            #帳務類別
                //       ,A.mb_points_type     #點數類型
                //       ,A.mb_change_points   #點數異動額
                //       ,A.mb_points          #點數異動後額度
                //       ,A.mb_create_datetime #新增時間
                //FROM   rosca2.member_balance AS A
                //WHERE  A.mb_mm_id = @mb_mm_id
                //";

                string mQuerySql = @"
SELECT B.mb_id
       ,A.mb_mm_id
       ,A.mb_payee_mm_id
       ,A.mb_tm_id
       ,A.mb_td_id
       ,A.mb_income_type
       ,CASE A.mb_income_type
          WHEN 'I' THEN '流入'
          WHEN 'O' THEN '流出'
          ELSE '錯誤'
        END AS mb_income_type_name
       ,A.mb_tr_type
       ,B.mb_tr_code
       ,B.mb_type
       ,B.mb_points_type
       ,B.mb_change_points
       ,A.mb_points
       ,A.mb_create_datetime
       ,C.sp_name AS mb_tr_type_name
       ,D.sp_name AS mb_type_name
       ,E.sp_name AS mb_points_type_name
FROM   rosca2.member_balance AS A
       INNER JOIN(SELECT MAX(A.mb_id)             AS mb_id
                         ,A.mb_tr_code
                         ,A.mb_type               #帳務類別
                         ,A.mb_points_type        #點數類型
                         ,SUM(A.mb_change_points) AS mb_change_points
                  FROM   rosca2.member_balance AS A
                  WHERE  A.mb_mm_id = @mb_mm_id
                  GROUP  BY A.mb_tr_code
                            ,A.mb_type
                            ,A.mb_points_type) AS B
               ON B.mb_id = A.mb_id
       LEFT JOIN rosca2.parameter_category AS C
              ON C.sp_code = A.mb_tr_type
                 AND C.sp_key = 'TRType'
       LEFT JOIN rosca2.parameter_category AS D
              ON D.sp_code = A.mb_type
                 AND D.sp_key = 'MBType'
       LEFT JOIN rosca2.parameter_category AS E
              ON E.sp_code = B.mb_points_type
                 AND E.sp_key = 'MBPointsType'
WHERE  A.mb_mm_id = @mb_mm_id 
";

                //搜尋條件
                string mQueryWhereString = @"
       #AND A.mb_create_datetime >= DATE_FORMAT(CURDATE(), '%Y-%m-01')
       #AND A.mb_create_datetime < DATE_ADD(DATE_FORMAT(CURDATE(), '%Y-%m-01'), INTERVAL 1 MONTH) 
";
                if (!string.IsNullOrEmpty(reqModel.mb_points_type) 
                    //&& Regex.Match(reqModel.mb_points_type, "^\\d{11,16}$").Success
                    && Regex.Match(reqModel.mb_points_type, "^(?:11|12|13|14|15|16)?$").Success
                    )
                {
                    mQueryWhereString += @" AND B.mb_points_type = @mb_points_type ";
                }

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER BY A.mb_id    ";

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
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryMemberBalanceUserRespModel>(mSql, reqModel);

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First();
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得帳務紀錄成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得帳務紀錄失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
