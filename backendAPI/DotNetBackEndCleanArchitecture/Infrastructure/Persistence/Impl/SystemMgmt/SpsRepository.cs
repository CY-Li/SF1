using DomainAbstraction.Interface.SystemMgmt;
using DomainEntity.Common;
using DomainEntity.DBModels;
using DomainEntity.Entity.FinancialMgmt.ApplyDeposit;
using DomainEntity.Entity.MemberMgmt.Wallet;
using DomainEntity.Entity.SystemMgmt.SystemParameterSetting;
using DomainEntityDTO.Entity.HomeMgmt;
using DomainEntityDTO.Entity.MemberMgmt.Wallet;
using DomainEntityDTO.Entity.SystemMgmt.SystemParameterSetting;
using InfraCommon.DBA;
using InfraCommon.SharedMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Impl.FinancialMgmt;

namespace Persistence.Impl.SystemMgmt
{
    public class SpsRepository : ISpsRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "SpsRepository";

        public SpsRepository(
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
        /// 後台-取得系統參數
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<BaseGridRespDTO<QuerySpsRespModel>> QuerySps(QuerySpsDTO reqModel)
        {
            string mActionLog = "QuerySps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QuerySpsRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QuerySpsRespModel>>
            {
                Result = new BaseGridRespDTO<QuerySpsRespModel>
                {
                    GridRespResult = new List<QuerySpsRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT A.sps_id
       ,A.sps_code
       ,A.sps_name
       ,A.sps_description
       ,A.sps_parameter01
       ,A.sps_parameter02
       ,A.sps_parameter03
       ,A.sps_parameter04
       ,A.sps_parameter05
       ,A.sps_parameter06
       ,A.sps_parameter07
       ,A.sps_parameter08
       ,A.sps_parameter09
       ,A.sps_parameter10
FROM   rosca2.system_parameter_setting AS A 
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
                if (!string.IsNullOrEmpty(reqModel.sps_code))
                {
                    //mQuerySql = string.Format(mQuerySql, mSqlGetParticipateCount);
                    //mQueryWhereString += @" AND A.mm_id = (SELECT A.mm_id FROM rosca.member_master AS A WHERE A.mm_account = @mm_account AND A.mm_status = 'Y' LIMIT 1) ";
                    mQueryWhereString += @" AND A.sps_code LIKE '%@sps_code%' ";
                }
                if (!string.IsNullOrEmpty(reqModel.sps_name))
                {
                    //mQuerySql = string.Format(mQuerySql, mSqlGetParticipateCount);
                    //mQueryWhereString += @" AND A.mm_id = (SELECT A.mm_id FROM rosca.member_master AS A WHERE A.mm_account = @mm_account AND A.mm_status = 'Y' LIMIT 1) ";
                    mQueryWhereString += @" AND A.sps_name LIKE '%@sps_name%' ";
                }
                //else
                //{
                //    mQuerySql = string.Format(mQuerySql, "");
                //}


                //ORDER BY 寫的位置
                string mQueryOrderByString = @"  ";
                if (string.IsNullOrEmpty(reqModel.pageSort))
                {
                    mQueryOrderByString = @" ORDER  BY A.sps_id  ";
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
                mResult.Result.GridRespResult = _dbAccess.GetObject<QuerySpsRespModel>(m_sql, reqModel);

                m_sql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(m_sql, reqModel).First();
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得系統參數成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得系統參數失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");
            return mResult;
        }

        /// <summary>
        /// 後台-新增參數
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PostSps(PostSpsDTO reqModel)
        {
            string mActionLog = "PostSps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                string mSql = @"
INSERT INTO rosca2.system_parameter_setting
            (sps_code
             ,sps_name
             ,sps_description
             ,sps_picture
             ,sps_parameter01
             ,sps_parameter02
             ,sps_parameter03
             ,sps_parameter04
             ,sps_parameter05
             ,sps_parameter06
             ,sps_parameter07
             ,sps_parameter08
             ,sps_parameter09
             ,sps_parameter10
             ,sps_create_member
             ,sps_create_datetime
             ,sps_update_member
             ,sps_update_datetime)
VALUES      (@sps_code
             ,@sps_name
             ,@sps_description
             ,@sps_picture
             ,@sps_parameter01
             ,@sps_parameter02
             ,@sps_parameter03
             ,@sps_parameter04
             ,@sps_parameter05
             ,@sps_parameter06
             ,@sps_parameter07
             ,@sps_parameter08
             ,@sps_parameter09
             ,@sps_parameter10
             ,@mm_id
             ,@sps_create_datetime
             ,@mm_id
             ,@sps_update_datetime); 
";

                var mCNT = _dbAccess.ExecNonQuery<PostSpsDTO>(mSql, reqModel);
                if (mCNT == 1)
                {
                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "新增參數成功";
                }
                else
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"新增參數失敗、更新筆數;{mCNT}";
                }
            }
            catch (Exception ex)
            {
                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"新增參數失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            return mResult;
        }

        /// <summary>
        /// 後台-更新參數
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PutSps(PutSpsDTO reqModel)
        {
            string mActionLog = "PutSps";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                bool mIsOk = false;

                //取得申請人member_master資料並鎖定，該部分取得的資料都是儲值人
                string mSqlUpdateSps = @"
UPDATE rosca2.system_parameter_setting
SET    sps_code = @sps_code
       ,sps_name = @sps_name
       ,sps_description = @sps_description
       ,sps_parameter01 = @sps_parameter01
       ,sps_parameter02 = @sps_parameter02
       ,sps_parameter03 = @sps_parameter03
       ,sps_parameter04 = @sps_parameter04
       ,sps_parameter05 = @sps_parameter05
       ,sps_parameter06 = @sps_parameter06
       ,sps_parameter07 = @sps_parameter07
       ,sps_parameter08 = @sps_parameter08
       ,sps_parameter09 = @sps_parameter09
       ,sps_parameter10 = @sps_parameter10
       ,sps_update_member = 0
       ,sps_update_datetime = @update_datetime
WHERE  sps_id = @sps_id; 
";

                _dbAccess.BeginTrans();
                mIsOk = _dbAccess.ExecuteTransactionObject<PutSpsDTO>(mSqlUpdateSps, reqModel) == 1;

                if (mIsOk)
                {
                    _dbAccess.Commit();

                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "更新參數成功";
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "更新參數失敗，更新筆數錯誤";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"更新參數失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-更新Kyc圖片
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PutKycImage(byte[] reqModel)
        {
            string mActionLog = "PutKycImage";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                bool mIsOk = false;

                //取得申請人member_master資料並鎖定，該部分取得的資料都是儲值人
                string mSqlUpdateSps = @"
UPDATE rosca2.system_parameter_setting
SET    sps_picture = @sps_picture
       ,sps_update_member = 0
       ,sps_update_datetime = @update_datetime
WHERE  sps_code = 'DepositURL'; 
";

                _dbAccess.BeginTrans();
                mIsOk = _dbAccess.ExecuteTransactionObject<dynamic>(mSqlUpdateSps, new
                {
                    sps_picture = reqModel,
                    update_datetime = GetTimeZoneInfo.process()
                }) == 1;

                if (mIsOk)
                {
                    _dbAccess.Commit();

                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "更新Kyc圖片成功";
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "更新Kyc圖片失敗，更新筆數錯誤";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"更新Kyc圖片失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// HomeSetting使用
        /// </summary>
        /// <returns></returns>
        public ServiceResultDTO<GetHomeVideoRespModel> GetHomeVideo()
        {
            string mActionLog = "GetHomeVideo";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetHomeVideoRespModel> mResult = new ServiceResultDTO<GetHomeVideoRespModel>();
            try
            {
                string m_sql = @"
SELECT A.sps_id
       #,A.sps_code
       #,A.sps_name
       #,A.sps_description 
       ,A.sps_parameter01 AS video_url1 
       ,A.sps_parameter02 AS video_url2
       ,A.sps_parameter03 AS video_url3
       ,A.sps_parameter04 AS video_url4
       ,A.sps_parameter05 AS video_url5
FROM   rosca2.system_parameter_setting AS A
WHERE  A.sps_code = 'HomeVideoSetting' 
";
                mResult.Result = _dbAccess.GetObject<GetHomeVideoRespModel>(m_sql).FirstOrDefault();
                if (mResult.Result == null)
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "取得首頁影片失敗";
                }
                else
                {
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "取得首頁影片成功";
                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得首頁影片失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");

            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

    }
}
