using DomainAbstraction.Interface.MemberMgmt;
using DomainEntity.Common;
using DomainEntity.Entity.MemberMgmt.Member;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using InfraCommon.DBA;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static DomainEntityDTO.Entity.MemberMgmt.Member.GetSubordinateOrgRespModel;

namespace Persistence.Impl.MemberMgmt
{
    public class MemberRepository : IMemberRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "MemberRepository";

        public MemberRepository(
                ILogger<MemberRepository> logger,
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
        /// 會員-取得下屬組織
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        public ServiceResultDTO<List<GetSubordinateOrgRespModel>> GetSubordinateOrg(long mm_id)
        {
            string mActionLog = "GetSubordinateOrg";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<List<GetSubordinateOrgRespModel>> mResult = new ServiceResultDTO<List<GetSubordinateOrgRespModel>>
            {
                Result = new List<GetSubordinateOrgRespModel>()
            };

            try
            {
                string mGetSql = @"
WITH RECURSIVE GetSubordinateOrgCTE AS
(
        SELECT A.mm_id
               ,A.mm_invite_code
               ,1 AS class_level
               ,0 AS invite_level
        FROM   rosca2.member_master AS A
        WHERE  A.mm_id = @mm_id # 找出第一個要請人
        
        UNION ALL
        
        SELECT B.mm_id
               ,B.mm_invite_code
               ,C.class_level + 1
               ,0 AS invite_level
        FROM   rosca2.member_master B
               INNER JOIN GetSubordinateOrgCTE C
                       ON B.mm_invite_code  = C.mm_id 
)

#SELECT mm_id
#       ,mm_invite_code
#       ,class_level
#       ,invite_level
#FROM   GetSubordinateOrgCTE 

SELECT #D.mm_id
       E.mm_account
       #,D.mm_invite_code
       ,E.mm_name
       ,E.mm_level
       ,G.mw_subscripts_count 
       ,F.mm_account AS mm_invite_account
       ,D.class_level
       ,D.invite_level
       ,pserson_subscripts_count(D.mm_id) AS total_qty
FROM   GetSubordinateOrgCTE AS D
       INNER JOIN rosca2.member_master AS E
               ON E.mm_id = D.mm_id
       LEFT JOIN rosca2.member_master AS F
               ON F.mm_id = D.mm_invite_code 
       INNER JOIN rosca2.member_wallet AS G
               ON G.mw_mm_id = D.mm_id
 ";

                mResult.Result = _dbAccess.GetObject<GetSubordinateOrgRespModel>(mGetSql, new {mm_id = mm_id}).ToList();
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得下屬組織成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得下屬組織失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");
            return mResult;
        }

        /// <summary>
        /// 後台-取得會員列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<BaseGridRespDTO<QueryMemberRespModel>> QueryMember(QueryMemberDTO reqModel)
        {
            string mActionLog = "QueryMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryMemberRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryMemberRespModel>>
            {
                Result = new BaseGridRespDTO<QueryMemberRespModel>
                {
                    GridRespResult = new List<QueryMemberRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT A.mm_id
       ,A.mm_account
       #,A.mm_hash_pwd
       ,A.mm_name
       ,A.mm_introduce_user
       ,A.mm_introduce_code
       ,A.mm_invite_user
       ,A.mm_invite_code
       ,A.mm_gender
       ,A.mm_personal_id
       ,A.mm_phone_number
       ,A.mm_mw_address
       ,A.mm_email
       ,A.mm_level
       ,A.mm_role_type
       ,A.mm_status
       ,A.mm_kyc
       ,A.mm_create_member
       ,A.mm_create_datetime
       ,A.mm_update_member
       ,A.mm_update_datetime
       ,B.mw_subscripts_count
       ,B.mw_stored
       ,B.mw_reward
FROM   rosca2.member_master AS A
       INNER JOIN rosca2.member_wallet AS B
               ON B.mw_mm_id = A.mm_id
 ";

                //搜尋條件
                string mQueryWhereString = " WHERE  1 = 1 ";
                if (!string.IsNullOrEmpty(reqModel.mm_account.Trim()))
                {
                    //mQueryWhereString += @" AND A.mm_id = (SELECT A.mm_id FROM rosca.member_master AS A WHERE A.mm_account = @mm_account AND A.mm_status = 'Y' LIMIT 1) ";
                    mQueryWhereString += @" AND A.mm_account = @mm_account ";
                }
                if (!string.IsNullOrEmpty(reqModel.mm_name.Trim()))
                {
                    mQueryWhereString += @" AND A.mm_name LIKE CONCAT('%', @mm_name, '%') ";
                }
                if (!string.IsNullOrEmpty(reqModel.mm_introduce_user))
                {
                    mQueryWhereString += @" AND A.mm_introduce_user = @mm_introduce_code ";
                }
                if (!string.IsNullOrEmpty(reqModel.mm_invite_user))
                {
                    mQueryWhereString += @" AND A.mm_invite_user = @mm_invite_code ";
                }


                //ORDER BY 寫的位置
                string mQueryOrderByString = @"  ";
                if (string.IsNullOrEmpty(reqModel.pageSort))
                {
                    mQueryOrderByString = @" ORDER  BY A.mm_id ";
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
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryMemberRespModel>(m_sql, reqModel);

                m_sql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(m_sql, reqModel).First();
                mResult.returnStatus = 1;
                mResult.returnMsg = "取得會員列表成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得會員列表失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");
            return mResult;
        }

        /// <summary>
        /// 會員、後台-取得該會員資料
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        public ServiceResultDTO<GetMemberRespModel> GetMember(long mm_id)
        {
            string mActionLog = "GetMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetMemberRespModel> mResult = new ServiceResultDTO<GetMemberRespModel>();

            try
            {
                string mSql = @"
SELECT A.mm_id
       ,A.mm_account
       ,A.mm_name
       ,A.mm_introduce_user
       #,A.mm_introduce_code
       ,A.mm_invite_user
       #,A.mm_invite_code
       ,A.mm_gender
       ,A.mm_country_code
       ,A.mm_personal_id
       ,A.mm_phone_number
       ,A.mm_mw_address
       #,A.mm_email
       ,A.mm_bank_code
       ,A.mm_bank_account
       ,A.mm_bank_account_name
       ,A.mm_branch
       ,A.mm_beneficiary_name
       ,A.mm_beneficiary_phone
       ,A.mm_beneficiary_relationship
       ,A.mm_level
       ,A.mm_role_type
       ,A.mm_status
       ,A.mm_kyc_id
       ,A.mm_kyc
       ,A.mm_create_datetime
       #,(SELECT COUNT(*)
       #FROM   rosca2.tender_master AS A
       #       INNER JOIN rosca2.tender_detail AS B
       #               ON B.td_tm_id = A.tm_id
       #WHERE  A.tm_status = 1
       #       AND B.td_participants = @mm_id
       #       AND B.td_period = 0  ) AS participate_count
FROM   rosca2.member_master AS A
WHERE  A.mm_id = @mm_id
       AND A.mm_status = 'Y'; 
";
                var mMember_master_list = _dbAccess.GetObject<GetMemberRespModel>(mSql, new { mm_id = mm_id });

                if (mMember_master_list.Count == 1)
                {
                    var m_member_master = mMember_master_list.First();
                    mResult.Result = m_member_master;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "取得會員成功";
                }
                else
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "取得會員失敗、沒有該會員";
                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得會員失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員、後台-更新會員資料
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PutMember(PutMemberDTO reqModel)
        {
            string mActionLog = "PutMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                string mSql = @"
UPDATE  rosca2.member_master
SET     mm_update_member = @mm_id
        ,mm_update_datetime = mm_update_datetime
        #,mm_name = @mm_name
        #,mm_phone_number = @mm_account
        ,mm_introduce_user = @mm_introduce_user
WHERE   mm_id = @mm_id
        AND mm_account = @mm_account
        AND mm_status = 'Y'; 
";
                string p_updateWhereString = @"";

                //if (!string.IsNullOrEmpty(e_putMemberDAOModel.mm_name))
                //{
                //    p_updateWhereString += " ,mm_name = @mm_name ";
                //}

                //if (!string.IsNullOrEmpty(reqModel.mm_hash_pwd))
                //{
                //    p_updateWhereString += " ,mm_hash_pwd = @mm_hash_pwd ";
                //}

                //if (!string.IsNullOrEmpty(reqModel.mm_2nd_pwd))
                //{
                //    p_updateWhereString += " ,mm_2nd_hash_pwd = @mm_2nd_hash_pwd ";
                //}

                if (!string.IsNullOrEmpty(reqModel.mm_introduce_user))
                {
                    string mSqlGetIntroduceID = @"
SELECT IFNULL((SELECT A.mm_id
               FROM   rosca2.member_master AS A
               WHERE  A.mm_account = @mm_account), 0)  AS mm_introduce_code
";
                    reqModel.mm_introduce_code = _dbAccess.GetObject<long>(mSqlGetIntroduceID, new { mm_account = reqModel.mm_introduce_user }).FirstOrDefault();
                    if (reqModel.mm_introduce_code == 0)
                    {
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = "註冊失敗、介紹碼帳號錯誤";

                        return mResult;
                    }
                }

                mSql = string.Format(mSql, p_updateWhereString);

                _dbAccess.BeginTrans();
                int mCNT = _dbAccess.ExecuteTransactionObject<PutMemberDTO>(mSql, reqModel);
                if (mCNT == 1)
                {
                    _dbAccess.Commit();

                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "更新會員成功";
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"更新會員失敗、更新筆數;{mCNT}";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"更新會員失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        public ServiceResultDTO<bool> UpdatePwd(UpdatePwdDTO reqModel)
        {
            string mActionLog = "UpdatePwd";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                string mSql = @"
UPDATE  rosca2.member_master
SET     mm_update_member = @mm_id
        ,mm_update_datetime = mm_update_datetime
        {0}
WHERE   mm_id = @mm_id
        AND mm_account = @mm_account
        AND mm_status = 'Y'; 
";
                string p_updateWhereString = @"";

                //if (!string.IsNullOrEmpty(e_putMemberDAOModel.mm_name))
                //{
                //    p_updateWhereString += " ,mm_name = @mm_name ";
                //}

                if (!string.IsNullOrEmpty(reqModel.mm_hash_pwd))
                {
                    p_updateWhereString += " ,mm_hash_pwd = @mm_hash_pwd ";
                }

                if (!string.IsNullOrEmpty(reqModel.mm_2nd_pwd))
                {
                    p_updateWhereString += " ,mm_2nd_hash_pwd = @mm_2nd_hash_pwd ";
                }

                mSql = string.Format(mSql, p_updateWhereString);

                _dbAccess.BeginTrans();
                int mCNT = _dbAccess.ExecuteTransactionObject<UpdatePwdDTO>(mSql, reqModel);
                if (mCNT == 1)
                {
                    _dbAccess.Commit();

                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = "更新密碼成功";
                }
                else
                {
                    _dbAccess.Rollback();

                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"更新密碼失敗、更新筆數;{mCNT}";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"更新密碼失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
