using DomainAbstraction.Interface.TenderMgmt;
using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.Entity.FinancialMgmt.MemberBalance;
using DomainEntity.Entity.TenderMgmt.Tender;
using DomainEntity.Shared;
using DomainEntityDTO.Entity.FinancialMgmt.MemberBalance;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using DomainEntityDTO.Entity.TenderMgmt.Tender;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Persistence.Impl.TenderMgmt
{
    public class TenderRepository : ITenderRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "TenderRepository";

        public TenderRepository(
                ILogger<TenderRepository> logger,
                IDBAccess dbAccess,
                IOptions<AppConfig> appConfig
            )
        {
            _logger = logger;
            _dbAccess = dbAccess;
            _dbAccess.SetConnectionString(appConfig.Value.ConnectionStrings.BackEndDatabase);
            _strConnection = appConfig.Value.ConnectionStrings.BackEndDatabase;
        }

        public ServiceResultDTO<BaseGridRespDTO<QueryTenderRespModel>> QueryTenderAdmin(QueryTenderDTO reqModel)
        {
            string mActionLog = "QueryTenderAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryTenderRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryTenderRespModel>>
            {
                Result = new BaseGridRespDTO<QueryTenderRespModel>
                {
                    GridRespResult = new List<QueryTenderRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT A.tm_id
       ,A.tm_sn
       #,A.tm_initiator_mm_id
       ,A.tm_type
       ,CASE A.tm_type
          WHEN 'A' THEN 'A-散會'
          WHEN 'B' THEN 'B-6會'
          WHEN 'C' THEN 'C-12會'
          WHEN 'D' THEN 'D-24會'
          ELSE '錯誤'
        END AS tm_type_name
       #,A.tm_bidder
       ,SUBSTRING_INDEX(A.tm_winners , '|', -1) AS tm_winners
       #,A.tm_current_period
       ,A.tm_status
       #,A.tm_count
       ,A.tm_group_datetime
       ,A.tm_settlement_datetime 
       ,A.tm_win_first_datetime 
       ,A.tm_win_end_datetime 
       #,A.tm_create_datetime
       #,A.tm_update_datetime
       ,B.td_id
       #,B.td_participants
       ,B.td_sequence
       ,B.td_period
       #,B.td_status
       #,B.td_update_datetime
       ,C.mm_account 
FROM   rosca2.tender_master AS A
       INNER JOIN rosca2.tender_detail AS B
               ON B.td_tm_id = A.tm_id
       LEFT JOIN rosca2.member_master AS C
               ON C.mm_id = B.td_participants 
";

                //搜尋條件
                string mQueryWhereString = @" WHERE  1 = 1 ";
                if (!string.IsNullOrEmpty(reqModel.mm_account.Trim()))
                {
                    mQueryWhereString += @" AND A.td_participants = (SELECT A.mm_id FROM rosca.member_master AS A WHERE A.mm_account = @mm_account AND A.mm_status = 'Y' LIMIT 1) ";
                }
                if (reqModel.tm_id > 0)
                {
                    mQueryWhereString += @" AND A.tm_id = @tm_id ";
                }
                if (!string.IsNullOrEmpty(reqModel.tm_type.Trim()))
                {
                    mQueryWhereString += @" AND A.tm_type = @tm_type ";
                }
                if (!string.IsNullOrEmpty(reqModel.tm_status.Trim()))
                {
                    mQueryWhereString += @" AND A.tm_status = @tm_status ";
                }
                else
                {
                    mQueryWhereString += @" AND A.tm_status = 1 ";
                }

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER BY A.tm_sn , B.td_id ";

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
                    reqModel.specPageSize = 24;
                    //取得跳過、與提取多少筆資料
                    reqModel.skipCNT = reqModel.specPageSize * (reqModel.pageSize * (reqModel.pageIndex - 1));
                    reqModel.takeCNT = reqModel.specPageSize * reqModel.pageSize;
                    if (reqModel.takeCNT == 0)
                    {
                        reqModel.takeCNT = 10;
                    }
                }

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryOrderByString} {mQueryFetchString} ";
                var mData = _dbAccess.GetObject<QueryTenderGridPO>(mSql, reqModel);

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First() / 24;

                mResult.Result.GridRespResult = mData.GroupBy(g => new { g.tm_id })
                                                    .Select(item => new QueryTenderRespModel
                                                    {
                                                        tm_id = item.Key.tm_id,
                                                        tm_sn = item.First().tm_sn,
                                                        tm_type_name = item.First().tm_type_name,
                                                        //tm_initiator_mm_id = item.First().tm_initiator_mm_id,
                                                        //tm_type = item.First().tm_type,
                                                        //tm_bidder = item.First().tm_bidder,
                                                        tm_winners = item.First().tm_winners,
                                                        //tm_current_period = item.First().tm_current_period,
                                                        tm_status = item.First().tm_status,
                                                        tm_win_first_datetime = item.First().tm_win_first_datetime,
                                                        tm_win_end_datetime = item.First().tm_win_end_datetime,
                                                        //tm_count = item.First().tm_count,
                                                        TenderDetail = item.Select(s => new TenderDetailGridRespModel
                                                        {
                                                            td_id = s.td_id,
                                                            //td_participants = s.td_participants,
                                                            td_sequence = s.td_sequence,
                                                            td_period = s.td_period,
                                                            //td_status = s.td_status,
                                                            //td_update_datetime = s.td_update_datetime
                                                            mm_account = s.mm_account
                                                        }).ToList()
                                                    }).ToList();
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得標案成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        public ServiceResultDTO<BaseGridRespDTO<QueryTenderInProgressRespModel>> QueryTenderInProgressUser(QueryTenderInProgressDTO reqModel)
        {
            string mActionLog = "QueryTenderInProgressUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryTenderInProgressRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryTenderInProgressRespModel>>
            {
                Result = new BaseGridRespDTO<QueryTenderInProgressRespModel>
                {
                    GridRespResult = new List<QueryTenderInProgressRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT A.tm_id
       ,A.tm_sn
       #,CASE A.tm_type
       #   WHEN 'A' THEN 'A散會'
       #   WHEN 'B' THEN 'B6會'
       #   WHEN 'C' THEN 'C12會'
       #   WHEN 'D' THEN 'D24會'
       #   ELSE '錯誤'
       # END AS tm_type_name
       ,CASE A.tm_type
          WHEN 'A' THEN CONCAT_WS('-', 'A散會', A.tm_sn)
          WHEN 'B' THEN CONCAT_WS('-', 'B6會', A.tm_sn)
          WHEN 'C' THEN CONCAT_WS('-', 'C12會', A.tm_sn)
          WHEN 'D' THEN CONCAT_WS('-', 'D24會', A.tm_sn)
          ELSE '錯誤'
        END AS tm_type_name
       ,( LENGTH(A.tm_bidder) - LENGTH(REPLACE(A.tm_bidder, @mm_id, """")) ) / LENGTH(@mm_id) AS has_participants
FROM   rosca2.tender_master AS A
WHERE  A.tm_status = 1 
";

                //搜尋條件
                string mQueryWhereString = @"";
                //if (!string.IsNullOrEmpty(reqModel.mm_account.Trim()))
                //{

                //}

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER BY A.tm_sn ";

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
                    reqModel.specPageSize = 24;
                    //取得跳過、與提取多少筆資料
                    reqModel.skipCNT = reqModel.specPageSize * (reqModel.pageSize * (reqModel.pageIndex - 1));
                    reqModel.takeCNT = reqModel.specPageSize * reqModel.pageSize;
                    if (reqModel.takeCNT == 0)
                    {
                        reqModel.takeCNT = 10;
                    }
                }

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryOrderByString} {mQueryFetchString} ";
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryTenderInProgressRespModel>(mSql, reqModel);
                //mResult.Result.GridRespResult.ToList().ForEach(f => f.tm_type_name = $@"{f.tm_type_name}-{f.tm_sn}");

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First() / 24;

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得進行中標案成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        public ServiceResultDTO<BaseGridRespDTO<QueryTenderAllUserRespModel>> QueryTenderAllUser(QueryTenderInProgressDTO reqModel)
        {
            string mActionLog = "QueryTenderAllUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryTenderAllUserRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryTenderAllUserRespModel>>
            {
                Result = new BaseGridRespDTO<QueryTenderAllUserRespModel>
                {
                    GridRespResult = new List<QueryTenderAllUserRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
#會員資訊所有標案
SELECT A.tm_id
       ,A.tm_sn
       ,CASE A.tm_type
          WHEN 'A' THEN CONCAT('第', A.tm_sn, '組', '-散會')
          WHEN 'B' THEN CONCAT('第', A.tm_sn, '組', '-四人')
          WHEN 'C' THEN CONCAT('第', A.tm_sn, '組', '-雙人')
          WHEN 'D' THEN CONCAT('第', A.tm_sn, '組', '-單人')
          ELSE '錯誤'
        END      AS tm_type_name
       ,SUM(CASE
              WHEN B.td_period = 0 THEN 1
              ELSE 0
            END) AS self_available_quantity #自己、成組、活會數量，進行中: (參與中活會)
       ,SUM(1)   AS self_all_quantity #自己、成組、參與所有會數量，所有標會:(參與會數)
       #,( LENGTH(A.tm_bidder) - LENGTH(REPLACE(A.tm_bidder, @mm_id, """")) ) / LENGTH(@mm_id) AS has_participants
FROM   rosca2.tender_master AS A
       INNER JOIN rosca2.tender_detail AS B
               ON B.td_tm_id = A.tm_id
WHERE  B.td_participants = @mm_id
       AND A.tm_status = '1'
";

                //搜尋條件
                string mQueryWhereString = @"";
                if (reqModel.mm_id == 0)
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"取得標案失敗、錯誤ID";

                    return mResult;
                }

                //GROUP BY 寫的位置
                string mQueryGroupByString = @" GROUP  BY A.tm_sn ";

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER BY A.tm_sn ";

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
                    reqModel.specPageSize = 24;
                    //取得跳過、與提取多少筆資料
                    reqModel.skipCNT = reqModel.specPageSize * (reqModel.pageSize * (reqModel.pageIndex - 1));
                    reqModel.takeCNT = reqModel.specPageSize * reqModel.pageSize;
                    if (reqModel.takeCNT == 0)
                    {
                        reqModel.takeCNT = 10;
                    }
                }

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryGroupByString} {mQueryOrderByString} {mQueryFetchString} ";
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryTenderAllUserRespModel>(mSql, reqModel);
                //mResult.Result.GridRespResult.ToList().ForEach(f => f.tm_type_name = $@"{f.tm_type_name}-{f.tm_sn}");

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First() / 24;

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得進行中標案成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        public ServiceResultDTO<BaseGridRespDTO<QueryTenderParticipatedUserRespModel>> QueryTenderParticipatedUser(QueryTenderInProgressDTO reqModel)
        {
            string mActionLog = "QueryTenderParticipatedUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryTenderParticipatedUserRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryTenderParticipatedUserRespModel>>
            {
                Result = new BaseGridRespDTO<QueryTenderParticipatedUserRespModel>
                {
                    GridRespResult = new List<QueryTenderParticipatedUserRespModel>()
                }
            };

            try
            {
                //                string mQuerySql = @"
                //SELECT A.tm_id
                //       ,A.tm_sn
                //       #,CASE A.tm_type
                //       #   WHEN 'A' THEN 'A散會'
                //       #   WHEN 'B' THEN 'B6會'
                //       #   WHEN 'C' THEN 'C12會'
                //       #   WHEN 'D' THEN 'D24會'
                //       #   ELSE '錯誤'
                //       # END AS tm_type_name
                //       ,CASE A.tm_type
                //          WHEN 'A' THEN CONCAT_WS('-', A.tm_sn, 'A散會')
                //          WHEN 'B' THEN CONCAT_WS('-', A.tm_sn, 'B6會')
                //          WHEN 'C' THEN CONCAT_WS('-', A.tm_sn, 'C12會')
                //          WHEN 'D' THEN CONCAT_WS('-', A.tm_sn, 'D24會')
                //          ELSE '錯誤'
                //        END AS tm_type_name
                //       ,( LENGTH(A.tm_bidder) - LENGTH(REPLACE(A.tm_bidder, @mm_id, """")) ) / LENGTH(@mm_id) AS has_participants
                //FROM   rosca2.tender_master AS A
                //WHERE  A.tm_bidder LIKE '%@mm_id%' 
                //";

                string mQuerySql = @"
SELECT A.tm_id
       ,A.tm_sn
       ,CASE A.tm_type
          WHEN 'A' THEN CONCAT('第', A.tm_sn, '組', '-散會')
          WHEN 'B' THEN CONCAT('第', A.tm_sn, '組', '-四人')
          WHEN 'C' THEN CONCAT('第', A.tm_sn, '組', '-雙人')
          WHEN 'D' THEN CONCAT('第', A.tm_sn, '組', '-單人')
          ELSE '錯誤'
        END      AS tm_type_name
       ,SUM(CASE
              WHEN B.td_period = 0 THEN 1
              ELSE 0
            END) AS self_org_available_quantity #自己、傘下、成組、活會數量，進行中: (傘下參與中剩餘的活會)
       ,SUM(1)   AS self_org_all_quantity ##自己、傘下、參與所有會數量，參與總會數:(傘下參與的總會數)
FROM   rosca2.tender_master AS A
       INNER JOIN rosca2.tender_detail AS B
               ON B.td_tm_id = A.tm_id
WHERE  B.td_participants IN 
( 

#取得邀請人
(
WITH RECURSIVE GetSubordinateOrgCTE AS
(
        SELECT A.mm_id
               ,A.mm_invite_code
        FROM   rosca2.member_master AS A
        WHERE  A.mm_id = @mm_id # 找出第一個要請人
        
        UNION ALL
        
        SELECT B.mm_id
               ,B.mm_invite_code
        FROM   rosca2.member_master B
               INNER JOIN GetSubordinateOrgCTE C
                       ON B.mm_invite_code  = C.mm_id 
)

SELECT D.mm_id
FROM   GetSubordinateOrgCTE AS D
)
#取得邀請人結束

)
       AND A.tm_status = '1'
";

                //搜尋條件
                string mQueryWhereString = @"";
                if (reqModel.mm_id == 0)
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"取得標案失敗、錯誤ID";

                    return mResult;
                }

                //GROUP BY 寫的位置
                string mQueryGroupByString = @" GROUP  BY A.tm_sn ";

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER BY A.tm_sn ";

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
                    reqModel.specPageSize = 24;
                    //取得跳過、與提取多少筆資料
                    reqModel.skipCNT = reqModel.specPageSize * (reqModel.pageSize * (reqModel.pageIndex - 1));
                    reqModel.takeCNT = reqModel.specPageSize * reqModel.pageSize;
                    if (reqModel.takeCNT == 0)
                    {
                        reqModel.takeCNT = 10;
                    }
                }

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryGroupByString} {mQueryOrderByString} {mQueryFetchString} ";

                //原本要這樣取
                //mSql = string.Format(mSql, string.Join(',', mGetTenderConditionData.Select(s => s.td_id)));

                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryTenderParticipatedUserRespModel>(mSql, reqModel);
                //mResult.Result.GridRespResult.ToList().ForEach(f => f.tm_type_name = $@"{f.tm_type_name}-{f.tm_sn}");

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First() / 24;

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得進行中標案成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-取得標案
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<List<GetTenderRespModel>> GetTender(GetTenderReqModel reqModel)
        {
            string mActionLog = "GetTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<List<GetTenderRespModel>> mResult = new();

            try
            {
                string mSql = @"
SELECT A.tm_id
       ,A.tm_bidder
       ,A.tm_winners
       ,CASE A.tm_type
          WHEN 'A' THEN 24 - tm_count
          WHEN 'B' THEN (24 - tm_count) / 6
          WHEN 'C' THEN (24 - tm_count) / 12
          WHEN 'D' THEN 1
          ELSE 0
        END AS bid_count
       ,A.tm_count
       ,A.tm_type
       ,CASE A.tm_type
          WHEN 'A' THEN 'A-散會'
          WHEN 'B' THEN 'B-6會'
          WHEN 'C' THEN 'C-12會'
          WHEN 'D' THEN 'D-24會'
          ELSE '錯誤'
        END AS tm_type_name
       ,( LENGTH(A.tm_bidder) - LENGTH(REPLACE(A.tm_bidder, @mm_id, """")) ) / LENGTH(@mm_id) AS has_participants
FROM   rosca2.tender_master AS A
       INNER JOIN (SELECT MIN(B.tm_id) AS tm_id
                   FROM   rosca2.tender_master AS B
                   WHERE  B.tm_status = '0'
                   GROUP  BY B.tm_type) AS C
               ON C.tm_id = A.tm_id; 
";
                mResult.Result = _dbAccess.GetObject<GetTenderRespModel>(mSql, new { mm_id = reqModel.mm_id });

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得標案成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-取得參與紀錄
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<GetParticipationRecordRespModel> GetParticipationRecord(GetParticipationRecordReqModel reqModel)
        {
            string mActionLog = "GetParticipationRecord";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetParticipationRecordRespModel> mResult = new();

            try
            {
                string mSql = @"
SELECT SUM(CASE
             WHEN B.td_participants = @mm_id
                  AND B.td_period  = 0 
                  AND A.tm_status = '1' THEN 1
             ELSE 0
           END)  AS self_available_quantity #自己、成組、活會數量
       ,SUM(CASE
              WHEN B.td_participants = @mm_id THEN 1
              ELSE 0
            END) AS all_quantity #自己、參與所有會數量
       ,SUM(CASE
              WHEN B.td_period = 0 
                   AND A.tm_status = '1' THEN 1
              ELSE 0
            END) AS self_org_available_quantity #傘下包括自己、成組、活會數量
       ,SUM(1) AS self_org_quantity
FROM   rosca2.tender_master AS A
       INNER JOIN rosca2.tender_detail AS B
               ON B.td_tm_id = A.tm_id
WHERE  B.td_participants IN 
( 

#取得邀請人
(
WITH RECURSIVE GetSubordinateOrgCTE AS
(
        SELECT A.mm_id
               ,A.mm_invite_code
        FROM   rosca2.member_master AS A
        WHERE  A.mm_id = @mm_id # 找出第一個要請人
        
        UNION ALL
        
        SELECT B.mm_id
               ,B.mm_invite_code
        FROM   rosca2.member_master B
               INNER JOIN GetSubordinateOrgCTE C
                       ON B.mm_invite_code  = C.mm_id 
)

SELECT D.mm_id
FROM   GetSubordinateOrgCTE AS D
)
#取得邀請人結束

) 
";
                //GetInvitationOrg mGetInvitationOrgImpl = new GetInvitationOrg(_logger, _dbAccess);
                //List<GetInvitationOrgDTO> mGetInvitationOrg = mGetInvitationOrgImpl.GetInvitationOrgProcess(reqModel.mm_id);

                //string mSqlUpdateTarProcess = string.Format(mSql, string.Join(',', mGetInvitationOrg.Select(s => s.mm_id)));
                mResult.Result = _dbAccess.GetObject<GetParticipationRecordRespModel>(mSql, new { mm_id = reqModel.mm_id }).FirstOrDefault();

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得參與紀錄成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得參與紀錄失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-取得標案紀錄
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<GetTenderRecordRespModel> GetTenderRecord(GetTenderRecordReqModel reqModel)
        {
            string mActionLog = "GetTenderRecord";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetTenderRecordRespModel> mResult = new();

            try
            {
                string mSql = @"
SELECT  #A.ttr_sn
       A.ttr_tm_sn
       #A.ttr_tm_id
       ,A.ttr_title
       ,A.ttr_detail01
       ,A.ttr_detail02
       ,A.ttr_detail03
       ,A.ttr_detail04
       ,A.ttr_top
       ,A.ttr_owner
       ,A.ttr_phone
       ,A.ttr_address
       ,A.ttr_tm_group_datetime
       ,A.ttr_detail
       #,Attr_create_datetime
FROM   rosca2.temp_tender_record AS A  
WHERE  1 = 1
       AND A.ttr_tm_sn = @ttr_tm_sn 
";
                //GetInvitationOrg mGetInvitationOrgImpl = new GetInvitationOrg(_logger, _dbAccess);
                //List<GetInvitationOrgDTO> mGetInvitationOrg = mGetInvitationOrgImpl.GetInvitationOrgProcess(reqModel.mm_id);

                //string mSqlUpdateTarProcess = string.Format(mSql, string.Join(',', mGetInvitationOrg.Select(s => s.mm_id)));
                mResult.Result = _dbAccess.GetObject<GetTenderRecordRespModel>(mSql, reqModel).FirstOrDefault();

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得標案紀錄成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得標案紀錄失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-新增標案(初始化)
        /// </summary>
        /// <param name="nowDateTime"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PostTender(DateTime nowDateTime)
        {
            string mActionLog = "PostTender";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new();
            try
            {
                string[] mTmType = new string[] { "A", "B", "C", "D" };
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

                mTM.tm_name = "自動標案新增";
                mTM.tm_ticks = $@"TM{DateTime.Now.Ticks}";
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


                _dbAccess.BeginTrans();

                long mGetTMSeq = 0;
                int mProcCNT = 0;
                bool isOk = true;

                foreach (var item in mTmType)
                {
                    mGetTMSeq = _dbAccess.ExecuteTransactionGetObject<long>(mSqlGetTMSeq, null).First();
                    mTM.tm_id = mGetTMSeq;
                    mTM.tm_type = item;

                    mTdList.ForEach(f => f.td_tm_id = mGetTMSeq);

                    if (isOk)
                    {
                        mProcCNT = _dbAccess.ExecuteTransactionObject(m_sqlMaster, mTM);
                        isOk = mProcCNT == 1;

                        if (isOk)
                        {
                            mProcCNT = _dbAccess.ExecuteTransactionObject(m_sqlDetail, mTdList);
                            isOk = mProcCNT == 24;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (isOk)
                {
                    _dbAccess.Commit();
                    mResult.returnStatus = 1;
                    mResult.returnMsg = $@"新增標案(初始化)成功";
                }
                else
                {
                    _dbAccess.Rollback();
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"新增標案(初始化)失敗";
                }

            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增標案(初始化)失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}