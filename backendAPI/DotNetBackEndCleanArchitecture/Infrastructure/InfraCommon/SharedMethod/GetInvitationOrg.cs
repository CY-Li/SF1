using DomainEntity.Shared;
using InfraCommon.DBA;
using Microsoft.Extensions.Logging;

namespace InfraCommon.SharedMethod
{
    public class GetInvitationOrg
    {
        private readonly ILogger _logger;
        private readonly IDBAccess _dbAccess;

        private readonly string pBaseLog = "GetInvitationOrg";

        public GetInvitationOrg(
                ILogger logger,
                IDBAccess dbAccess)
        {
            _logger = logger;
            _dbAccess = dbAccess;
        }

        public List<GetInvitationOrgDTO> GetInvitationOrgProcess(long mm_id)
        {
            string mActionLog = "GetInvitationOrgProcess";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            List<GetInvitationOrgDTO> mResult = new();
            try
            {
                string mSqlGetInvitationOrg = @"
WITH RECURSIVE GetInvitationOrgCTE AS
(
        SELECT A.mm_id
               ,A.mm_invite_code
               ,1 AS class_level
               ,0 AS invite_level
        FROM   rosca2.member_master AS A
        WHERE  A.mm_id = (SELECT mm_invite_code FROM rosca2.member_master WHERE mm_id = @mm_id) # 找出第一個要請人
        
        UNION ALL
        
        SELECT B.mm_id
               ,B.mm_invite_code
               ,C.class_level + 1
               ,0 AS invite_level
        FROM   rosca2.member_master B
               INNER JOIN GetInvitationOrgCTE C
                       ON B.mm_id = C.mm_invite_code 
)

SELECT mm_id
       ,mm_invite_code
       ,class_level
       ,invite_level
FROM   GetInvitationOrgCTE 
";
                mResult = _dbAccess.GetObject<GetInvitationOrgDTO>(mSqlGetInvitationOrg, new { mm_id = mm_id }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return mResult;
        }
    }
}
