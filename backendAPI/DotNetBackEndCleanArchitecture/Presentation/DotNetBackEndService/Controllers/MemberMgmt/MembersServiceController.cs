using AppAbstraction.MemberMgmt;
using AutoMapper;
using DomainEntity.Common;
using DomainEntity.Entity.MemberMgmt.Member;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using InfraCommon.SharedMethod;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndService.Controllers.MemberMgmt
{
    [Route("api/MemberMgmt/[controller]")]
    [ApiController]
    public class MembersServiceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMemberService _memberSVC;

        private readonly string pBaseLog = "MembersServiceController";
        public MembersServiceController(
                ILogger<MembersServiceController> logger,
                IMapper mapper,
                IMemberService memberSVC
            )
        {
            _logger = logger;
            _mapper = mapper;
            _memberSVC = memberSVC;
        }

        /// <summary>
        /// 會員-取得下屬組織
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSubordinateOrg")]
        public ActionResult<ServiceResultDTO<List<GetSubordinateOrgRespModel>>> GetSubordinateOrg([FromBody] long mm_id)
        {
            string mActionLog = "GetSubordinateOrg";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<List<GetSubordinateOrgRespModel>> mResult = new ServiceResultDTO<List<GetSubordinateOrgRespModel>>();

            try
            {
                mResult = _memberSVC.IMemberRepo.GetSubordinateOrg(mm_id);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "取得下屬組織失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 後台-取得會員列表
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryMember")]
        public ActionResult<ServiceResultDTO<BaseGridRespDTO<QueryMemberRespModel>>> QueryMember(QueryMemberDTO reqModel)
        {
            string mActionLog = "QueryMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<BaseGridRespDTO<QueryMemberRespModel>> mResult = new();

            try
            {
                mResult = _memberSVC.IMemberRepo.QueryMember(reqModel);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "取得會員列表失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員、後台-取得該會員資料
        /// </summary>
        /// <param name="mm_id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMember")]
        public ActionResult<ServiceResultDTO<GetMemberRespModel>> GetMember([FromBody] long mm_id)
        {
            string mActionLog = "GetMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<GetMemberRespModel> mResult = new ServiceResultDTO<GetMemberRespModel>();

            try
            {
                mResult = _memberSVC.IMemberRepo.GetMember(mm_id);
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "取得會員失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員、後台-更新會員資料
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PutMember")]
        public ActionResult<ServiceResultDTO<bool>> PutMember(PutMemberDTO reqModel)
        {
            string mActionLog = "PutMember";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                if (reqModel.mm_id > 0 && !string.IsNullOrEmpty(reqModel.mm_account))
                {
                    //PutMemberDAOModel m_putMemberDAOModel = _Mapper.Map<PutMemberREQModel, PutMemberDAOModel>(reqModel);
                    //if (!string.IsNullOrEmpty(reqModel.mm_pwd))
                    //{
                    //    reqModel.mm_hash_pwd = HashProcess.GetHashPWD(reqModel.mm_pwd);
                    //}

                    //if (!string.IsNullOrEmpty(reqModel.mm_2nd_pwd))
                    //{
                    //    reqModel.mm_2nd_hash_pwd = HashProcess.GetHashPWD(reqModel.mm_2nd_pwd);
                    //}
                    reqModel.mm_update_datetime = GetTimeZoneInfo.process();
                    mResult = _memberSVC.IMemberRepo.PutMember(reqModel);
                }
                else
                {
                    mResult.returnStatus = 0;
                    mResult.returnMsg = "更新會員失敗、參數錯誤";

                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "更新會員失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }

        /// <summary>
        /// 會員、後台-更新會員資料
        /// </summary>
        /// <param name="reqModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatePwd")]
        public ActionResult<ServiceResultDTO<bool>> UpdatePwd(UpdatePwdDTO reqModel)
        {
            string mActionLog = "UpdatePwd";
            _logger.LogInformation($@"{pBaseLog} {mActionLog} START");

            ServiceResultDTO<bool> mResult = new ServiceResultDTO<bool>();

            try
            {
                if (reqModel.mm_id > 0 && !string.IsNullOrEmpty(reqModel.mm_account))
                {
                    //PutMemberDAOModel m_putMemberDAOModel = _Mapper.Map<PutMemberREQModel, PutMemberDAOModel>(reqModel);
                    if (!string.IsNullOrEmpty(reqModel.mm_pwd))
                    {
                        reqModel.mm_hash_pwd = HashProcess.GetHashPWD(reqModel.mm_pwd);
                    }

                    if (!string.IsNullOrEmpty(reqModel.mm_2nd_pwd))
                    {
                        reqModel.mm_2nd_hash_pwd = HashProcess.GetHashPWD(reqModel.mm_2nd_pwd);
                    }
                    reqModel.mm_update_datetime = GetTimeZoneInfo.process();
                    mResult = _memberSVC.IMemberRepo.UpdatePwd(reqModel);
                }
                else
                {
                    mResult.returnStatus = 0;
                    mResult.returnMsg = "更新會員失敗、參數錯誤";

                }
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = "更新會員失敗";

                _logger.LogInformation($@"{pBaseLog} {mActionLog} {ex.Message}");
            }

            _logger.LogInformation($@"{pBaseLog} {mActionLog} END");

            return Ok(mResult);
        }
    }
}
