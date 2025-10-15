using DomainAbstraction.Interface.SystemMgmt;
using DomainEntity.Common;
using DomainEntity.Entity.SystemMgmt.Announcement;
using DomainEntityDTO.Entity.SystemMgmt.Announcement;
using InfraCommon.DBA;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.SqlTypes;

namespace Persistence.Impl.SystemMgmt
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "AnnouncementRepository";

        public AnnouncementRepository(
                ILogger<AnnouncementRepository> logger,
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
        /// 後台-取得公告列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardAdminRespModel>> QueryAnnBoardAdmin(QueryAnnBoardAdminDTO reqModel)
        {
            string mActionLog = "QueryAnnBoardAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardAdminRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardAdminRespModel>>
            {
                Result = new BaseGridRespDTO<QueryAnnBoardAdminRespModel>
                {
                    GridRespResult = new List<QueryAnnBoardAdminRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT A.ab_id
       ,A.ab_title
       ,A.ab_content
       ,A.ab_image
       ,A.ab_image_name
       ,A.ab_image_path
       ,A.ab_status
       ,A.ab_datetime
FROM   rosca2.announcement_board AS A
";

                //搜尋條件
                string mQueryWhereString = " WHERE  1 = 1 ";

                if (reqModel.ab_id > 0)
                {
                    mQueryWhereString += @" AND A.ab_id = @ab_id ";
                }
                if (!string.IsNullOrEmpty(reqModel.ab_title.Trim()))
                {
                    mQueryWhereString += @" AND A.ab_title LIKE CONCAT('%', @ab_title, '%') ";
                }

                //ORDER BY 寫的位置
                string mQueryOrderByString = @"  ";
                if (string.IsNullOrEmpty(reqModel.pageSort))
                {
                    mQueryOrderByString = @" ORDER  BY A.ab_datetime DESC, A.ab_id desc ";
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

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryOrderByString} {mQueryFetchString} ";
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryAnnBoardAdminRespModel>(mSql, reqModel);

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First();

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得申請提領單成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得提領申請列表失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-取得公告列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardUserRespModel>> QueryAnnBoardUser(QueryAnnBoardUserDTO reqModel)
        {
            string mActionLog = "QueryAnnBoardUser";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardUserRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryAnnBoardUserRespModel>>
            {
                Result = new BaseGridRespDTO<QueryAnnBoardUserRespModel>
                {
                    GridRespResult = new List<QueryAnnBoardUserRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT A.ab_id
       ,A.ab_title
       ,A.ab_content
       ,A.ab_image
       ,A.ab_image_name
       ,A.ab_image_path
       ,A.ab_status
       ,A.ab_datetime
FROM   rosca2.announcement_board AS A
";

                //搜尋條件
                string mQueryWhereString = " WHERE  1 = 1 AND A.ab_status = '10'";

                if (reqModel.ab_id > 0)
                {
                    mQueryWhereString += @" AND A.ab_id = @ab_id ";
                }
                if (!string.IsNullOrEmpty(reqModel.ab_title.Trim()))
                {
                    mQueryWhereString += @" AND A.ab_title LIKE CONCAT('%', @ab_title, '%') ";
                }

                //ORDER BY 寫的位置
                string mQueryOrderByString = @"  ";
                if (string.IsNullOrEmpty(reqModel.pageSort))
                {
                    mQueryOrderByString = @" ORDER  BY A.ab_datetime DESC, A.ab_id desc ";
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

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryOrderByString} {mQueryFetchString} ";
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryAnnBoardUserRespModel>(mSql, reqModel);

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First();

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得申請提領單成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得提領申請列表失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} START");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-新增首頁公告
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PostAnnouncement(PostAnnouncementDTO reqModel)
        {
            string mActionLog = "PostAnnouncement";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                string mSql = @"
INSERT INTO rosca2.announcement_board
            (ab_title
             ,ab_content
             ,ab_image
             ,ab_image_path
             ,ab_image_name
             ,ab_status
             ,ab_datetime
             ,ab_create_member
             ,ab_create_datetime
             ,ab_update_datetime
             ,ab_update_member)
VALUES     (@ab_title
            ,@ab_content
            ,NULL
            ,@ab_image_path
            ,@ab_image_name
            ,@ab_status
            ,@ab_datetime
            ,@mm_id
            ,@update_datetime
            ,@mm_id
            ,@update_datetime); 
";
                _dbAccess.BeginTrans();

                int mCNT = _dbAccess.ExecuteTransactionObject<PostAnnouncementDTO>(mSql, reqModel);
                if (mCNT == 1)
                {
                    _dbAccess.Commit();

                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "新增公告成功";
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"新增公告失敗、更新筆數;{mCNT}";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增公告失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-更新公告
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PutAnnouncement(PutAnnouncementDTO reqModel)
        {
            string mActionLog = "PutAnnouncement";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                string mSql = @"
UPDATE rosca2.announcement_board
SET    ab_title = @ab_title
       ,ab_content = @ab_content
       {0}
       ,ab_status = @ab_status
       ,ab_datetime = @ab_datetime
       ,ab_update_datetime = @update_datetime
       ,ab_update_member = @mm_id
WHERE  ab_id = @ab_id;
";
                string mPlusSql = string.Empty;
                if (!string.IsNullOrEmpty(reqModel.ab_image_path))
                {
                    mPlusSql += " ,ab_image_path = @ab_image_path ";
                }

                if (!string.IsNullOrEmpty(reqModel.ab_image_name))
                {
                    mPlusSql += " ,ab_image_name = @ab_image_name ";
                }

                mSql = string.Format(mSql, mPlusSql);
                _dbAccess.BeginTrans();

                int mCNT = _dbAccess.ExecuteTransactionObject<PutAnnouncementDTO>(mSql, reqModel);
                if (mCNT == 1)
                {
                    _dbAccess.Commit();

                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "更新公告成功";
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"更新公告失敗、更新筆數;{mCNT}";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"更新公告失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
