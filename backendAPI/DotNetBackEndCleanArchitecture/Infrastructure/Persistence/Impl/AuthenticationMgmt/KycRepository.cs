using DomainAbstraction.Interface.AuthenticationMgmt;
using DomainEntity.Common;
using DomainEntity.Entity.AuthenticationMgmt.Kyc;
using DomainEntity.Entity.AuthenticationMgmt.Login;
using DomainEntityDTO.Entity.AuthenticationMgmt.Kyc;
using DomainEntityDTO.Entity.TenderMgmt.Tender;
using InfraCommon.DBA;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Impl.AuthenticationMgmt
{
    public class KycRepository : IKycRepository
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;
        private readonly string _strConnection;

        private readonly string pBaseLog = "KycRepository";

        public KycRepository(
                ILogger<KycRepository> logger,
                IDBAccess dbAccess,
                IOptions<AppConfig> appConfig
            )
        {
            _logger = logger;
            _dbAccess = dbAccess;
            _dbAccess.SetConnectionString(appConfig.Value.ConnectionStrings.BackEndDatabase);
            _strConnection = appConfig.Value.ConnectionStrings.BackEndDatabase;
        }


        public ServiceResultDTO<BaseGridRespDTO<QueryKycAdminRespModel>> QueryKycAdmin(QueryKycAdminDTO reqModel)
        {
            string mActionLog = "QueryKycAdmin";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryKycAdminRespModel>> mResult = new ServiceResultDTO<BaseGridRespDTO<QueryKycAdminRespModel>>
            {
                Result = new BaseGridRespDTO<QueryKycAdminRespModel>
                {
                    GridRespResult = new List<QueryKycAdminRespModel>()
                }
            };

            try
            {
                string mQuerySql = @"
SELECT A.akc_id
       ,A.akc_mm_id
       ,A.akc_front
       ,A.akc_back
       ,A.akc_gender
       ,A.akc_personal_id
       ,A.akc_mw_address
       ,A.akc_email
       ,A.akc_bank_account
       ,A.akc_bank_account_name
       ,A.akc_branch
       ,B.mm_account
       ,A.akc_status
       ,CASE A.akc_status
          WHEN '10' THEN '待審核'
          WHEN '20' THEN '通過'
          WHEN '30' THEN '駁回'
          ELSE '錯誤'
        END AS akc_status_name
       #,A.akc_create_member
       #,A.akc_create_datetime
       #,A.akc_update_member
       #,A.akc_update_datetime
FROM   rosca2.apply_kyc_certification AS A
       LEFT JOIN rosca2.member_master AS B
              ON B.mm_id = A.akc_mm_id
#WHERE  A.akc_status = '10'
WHERE  1 = 1 
";

                //搜尋條件
                string mQueryWhereString = @"";

                if (reqModel.akc_id != 0)
                {
                    mQueryWhereString += @" AND A.akc_id = @akc_id ";
                }

                if (!string.IsNullOrEmpty(reqModel.mm_account))
                {
                    mQueryWhereString += @" AND B.mm_account = @mm_account ";
                }

                if (!string.IsNullOrEmpty(reqModel.akc_status))
                {
                    mQueryWhereString += @" AND A.akc_status = @akc_status ";
                }

                //ORDER BY 寫的位置
                string mQueryOrderByString = @" ORDER BY A.akc_id ";

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
                    reqModel.specPageSize = 1;
                    //取得跳過、與提取多少筆資料
                    reqModel.skipCNT = reqModel.specPageSize * (reqModel.pageSize * (reqModel.pageIndex - 1));
                    reqModel.takeCNT = reqModel.specPageSize * reqModel.pageSize;
                    if (reqModel.takeCNT == 0)
                    {
                        reqModel.takeCNT = 10;
                    }
                }

                var mSql = $@"{mQuerySql} {mQueryWhereString} {mQueryOrderByString} {mQueryFetchString} ";
                mResult.Result.GridRespResult = _dbAccess.GetObject<QueryKycAdminRespModel>(mSql, reqModel);
                //mResult.Result.GridRespResult.ToList().ForEach(f => f.tm_type_name = $@"{f.tm_type_name}-{f.tm_sn}");

                mSql = $" SELECT COUNT(*) CNT FROM ( {mQuerySql} {mQueryWhereString} ) AS B";
                mResult.Result.GridTotalCount = _dbAccess.GetObject<int>(mSql, reqModel).First() / 24;

                mResult.returnStatus = 1;
                mResult.returnMsg = "取得KYC申請成功";

            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.returnStatus = 999;
                mResult.returnMsg = $@"取得KYC申請失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-檢查KYC資料
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        public ServiceResultDTO<long> CheckKyc(long mm_id)
        {
            string mActionLog = "CheckKyc";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<long> mResult = new ServiceResultDTO<long>();

            try
            {
                string mSqlChek = @"
SELECT COUNT(A.akc_id)
FROM   rosca2.apply_kyc_certification AS A
WHERE  A.akc_mm_id = @akc_mm_id
       AND A.akc_status = 10
UNION ALL
SELECT COUNT(A.akc_id)
FROM   rosca2.apply_kyc_certification AS A
WHERE  A.akc_mm_id = @akc_mm_id
       AND A.akc_status = 20 
";

                string mSqlGetAkcSeq = @"SELECT NEXTVAL(rosca2.apply_kyc_certification_sequence);";

                List<long> mMMList = _dbAccess.GetObject<long>(mSqlChek, new { akc_mm_id = mm_id });

                if (mMMList[0] > 0)
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"確認Kyc資料失敗、已有資料申請中";

                    return mResult;
                }

                long mGetAkcSeq = _dbAccess.GetObject<long>(mSqlGetAkcSeq, null).First();
                if (mGetAkcSeq > 0)
                {
                    mResult.Result = mGetAkcSeq;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = $@"確認Kyc資料成功";
                }
                else
                {
                    mResult.Result = 0;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"確認Kyc資料失敗，取得ID錯誤";
                }

            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"確認Kyc資料失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 會員-申請KYC
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PostKyc(PostKycDTO reqModel)
        {
            string mActionLog = "PostkKyc";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                bool mIsOk = false;

                string mSqlInsertAkc = @"
INSERT INTO rosca2.apply_kyc_certification
            (akc_id
             ,akc_mm_id
             ,akc_name
             ,akc_front
             ,akc_back
             ,akc_gender
             ,akc_personal_id
             ,akc_mw_address
             ,akc_email
             ,akc_bank_code
             ,akc_bank_account
             ,akc_bank_account_name
             ,akc_branch
             ,akc_beneficiary_name
             ,akc_beneficiary_phone
             ,akc_beneficiary_relationship
             ,akc_status
             ,akc_create_member
             ,akc_create_datetime
             ,akc_update_member
             ,akc_update_datetime)
VALUES     (@akc_id
            ,@akc_mm_id
            ,@akc_name
            ,@akc_front
            ,@akc_back
            ,@akc_gender
            ,@akc_personal_id
            ,@akc_mw_address
            ,@akc_email
            ,@akc_bank_code
            ,@akc_bank_account
            ,@akc_bank_account_name
            ,@akc_branch
            ,@akc_beneficiary_name
            ,@akc_beneficiary_phone
            ,@akc_beneficiary_relationship
            ,'10'
            ,@akc_create_member
            ,@akc_create_datetime
            ,@akc_update_member
            ,@akc_update_datetime); 
";

                _dbAccess.BeginTrans();
                mIsOk = _dbAccess.ExecuteTransactionObject<PostKycDTO>(mSqlInsertAkc, reqModel) == 1;

                if (mIsOk)
                {
                    _dbAccess.Commit();
                    mResult.Result = true;
                    mResult.returnStatus = 1;
                    mResult.returnMsg = $@"申請KYC成功";
                }
                else
                {
                    _dbAccess.Rollback();
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"申請KYC失敗";
                }

            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"申請KYC失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }

        /// <summary>
        /// 後台-覆核KYC申請
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        public ServiceResultDTO<bool> PutKyc(PutKycDTO reqModel)
        {
            string mActionLog = "PutKyc";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                bool mIsOk = false;

                string mSqlGetAkc = @"
SELECT A.akc_status
FROM   rosca2.apply_kyc_certification AS A
WHERE  A.akc_id = @akc_id 
";

                string mSqlUpdateMm = @"
UPDATE rosca2.member_master AS A
       INNER JOIN rosca2.apply_kyc_certification AS B
               ON B.akc_mm_id = A.mm_id
SET    A.mm_name = B.akc_name
       ,A.mm_gender = B.akc_gender
       ,A.mm_personal_id = B.akc_personal_id
       ,A.mm_mw_address = B.akc_mw_address
       ,A.mm_email = B.akc_email
       ,A.mm_bank_code = B.akc_bank_code
       ,A.mm_bank_account = B.akc_bank_account
       ,A.mm_bank_account_name = B.akc_bank_account_name
       ,A.mm_branch = B.akc_branch
       ,A.mm_beneficiary_name = B.akc_beneficiary_name
       ,A.mm_beneficiary_name = B.akc_beneficiary_name
       ,A.mm_beneficiary_relationship = B.akc_beneficiary_relationship
       ,A.mm_kyc_id = B.akc_id 
       ,A.mm_kyc = 'Y'
       ,A.mm_update_member = @mm_id
       ,A.mm_update_datetime = @update_datetime
WHERE  B.akc_id = @akc_id ;
";

                string mSqlUpdateAkc = @"
UPDATE rosca2.apply_kyc_certification
SET    akc_status = @akc_status
       ,akc_update_member = @mm_id
       ,akc_update_datetime = @update_datetime
WHERE  akc_id = @akc_id 
";

                List<string> mGetAkcStatus = _dbAccess.GetObject<string>(mSqlGetAkc, reqModel);
                if (mGetAkcStatus.Count == 0)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"覆核KYC失敗，查無此筆資料";

                    return mResult;
                }
                else if (mGetAkcStatus.First() != "10")
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"覆核KYC失敗，該筆KYC已審核";

                    return mResult;
                }
                else if (mGetAkcStatus.Count > 1)
                {
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"覆核KYC失敗，資料數量大於1";

                    return mResult;
                }

                _dbAccess.BeginTrans();

                if (reqModel.akc_status == "20")
                {
                    mIsOk = _dbAccess.ExecuteTransactionObject<PutKycDTO>(mSqlUpdateAkc, reqModel) == 1;
                    if (!mIsOk)
                    {
                        _dbAccess.Rollback();
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"覆核KYC失敗，更新Kyc筆數錯誤";

                        return mResult;
                    }

                    mIsOk = _dbAccess.ExecuteTransactionObject<PutKycDTO>(mSqlUpdateMm, reqModel) == 1;

                    if (mIsOk)
                    {
                        _dbAccess.Commit();
                        mResult.Result = true;
                        mResult.returnStatus = 1;
                        mResult.returnMsg = $@"覆核KYC成功";
                    }
                    else
                    {
                        _dbAccess.Rollback();
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"覆核KYC失敗，更新會員資料筆數錯誤";
                    }
                }
                else if (reqModel.akc_status == "30")
                {
                    mIsOk = _dbAccess.ExecuteTransactionObject<PutKycDTO>(mSqlUpdateAkc, reqModel) == 1;
                    if (mIsOk)
                    {
                        _dbAccess.Commit();
                        mResult.Result = true;
                        mResult.returnStatus = 1;
                        mResult.returnMsg = $@"覆核駁回KYC成功";
                    }
                    else
                    {
                        _dbAccess.Rollback();
                        mResult.Result = false;
                        mResult.returnStatus = 999;
                        mResult.returnMsg = $@"覆核KYC駁回失敗，更新Kyc筆數錯誤";
                    }
                }
                else
                {
                    _dbAccess.Rollback();
                    mResult.Result = false;
                    mResult.returnStatus = 999;
                    mResult.returnMsg = $@"覆核KYC失敗，狀態輸入錯誤";
                }
            }
            catch (Exception ex)
            {
                _dbAccess.Rollback();

                mResult.Result = false;
                mResult.returnStatus = 999;
                mResult.returnMsg = $@"申請KYC失敗、{ex.Message}";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
