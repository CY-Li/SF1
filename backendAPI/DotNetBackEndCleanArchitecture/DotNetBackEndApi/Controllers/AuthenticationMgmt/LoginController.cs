using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.AuthenticationMgmt.Login;
using DotNetBackEndApi.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotNetBackEndApi.Controllers.AuthenticationMgmt
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApiAppConfig _apiAppConfig;
        private readonly IConfiguration _configuration;


        public LoginController(
            ILogger<LoginController> logger,
            IOptions<ApiAppConfig> apiAppConfig,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _apiAppConfig = apiAppConfig.Value;
            _configuration = configuration;
        }

        // POST api/<LoginController>
        [HttpPost]
        [SwaggerOperation(Summary = "會員-登入")]
        public async Task<ApiResultModel<LoginRespModel>> Post([FromBody] LoginReqModel reqModel)
        {
            _logger.LogInformation("LoginController Login START");

            ApiResultModel<LoginRespModel> mResult = new ApiResultModel<LoginRespModel>();

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                // 根據帳號查詢會員
                var memberQuery = @"
                    SELECT mm.mm_id, mm.mm_account, mm.mm_hash_pwd, mm.mm_name, mm.mm_role_type, mm.mm_kyc,
                           mm.mm_introduce_code, mm.mm_invite_code,
                           mw.mw_subscripts_count, mw.mw_stored, mw.mw_reward, mw.mw_peace, 
                           mw.mw_mall, mw.mw_registration, mw.mw_death, mw.mw_accumulation
                    FROM member_master mm
                    LEFT JOIN member_wallet mw ON mm.mm_id = mw.mw_mm_id
                    WHERE mm.mm_account = @account AND mm.mm_status = 'Y'";
                
                using var command = new MySqlCommand(memberQuery, connection);
                command.Parameters.AddWithValue("@account", reqModel.mm_account);

                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    _logger.LogWarning($"會員帳號不存在: {reqModel.mm_account}");
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "帳號或密碼錯誤";
                    return mResult;
                }

                var memberId = reader.GetInt64(0);
                var memberAccount = reader.GetString(1);
                var memberHashPwd = reader.GetString(2);
                var memberName = reader.IsDBNull(3) ? "" : reader.GetString(3);
                var memberRoleType = reader.IsDBNull(4) ? "1" : reader.GetString(4);
                var memberKyc = reader.IsDBNull(5) ? "N" : reader.GetString(5);
                var introduceCode = reader.IsDBNull(6) ? 0L : reader.GetInt64(6);
                var inviteCode = reader.IsDBNull(7) ? 0L : reader.GetInt64(7);

                // 驗證密碼 - 支援 BCrypt 和 SHA256
                bool passwordValid = false;
                
                if (memberHashPwd.StartsWith("$2a$") || memberHashPwd.StartsWith("$2b$") || memberHashPwd.StartsWith("$2y$"))
                {
                    // BCrypt 雜湊 - 暫時跳過，使用明文比較
                    passwordValid = false;
                }
                else if (memberHashPwd.Length == 64)
                {
                    // SHA256 雜湊
                    var inputHash = ComputeSha256Hash(reqModel.mm_pwd);
                    passwordValid = inputHash.Equals(memberHashPwd, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    // 明文密碼
                    passwordValid = reqModel.mm_pwd == memberHashPwd;
                }
                
                if (!passwordValid)
                {
                    mResult.returnStatus = 999;
                    mResult.returnMsg = "帳號或密碼錯誤";
                    return mResult;
                }

                // 創建登入回應模型
                var loginResp = new LoginRespModel
                {
                    mm_id = memberId,
                    mm_account = memberAccount,
                    mm_name = memberName,
                    mm_role_type = memberRoleType,
                    mm_kyc = memberKyc,
                    mm_introduce_code = introduceCode,
                    mm_invite_code = inviteCode,
                    mw_subscripts_count = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    mw_stored = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9),
                    mw_reward = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10),
                    mw_peace = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11),
                    mw_mall = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12),
                    mw_registration = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13),
                    mw_death = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14),
                    mw_accumulation = reader.IsDBNull(15) ? 0 : reader.GetDecimal(15)
                };

                // 生成 JWT Token
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Sid, memberId.ToString()),
                    new Claim(ClaimTypes.Name, memberAccount),
                    new Claim(ClaimTypes.Role, "1"),
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var mToken = CreateToken(claims);
                loginResp.AccessToken = new JwtSecurityTokenHandler().WriteToken(mToken);
                loginResp.expiration = mToken.ValidTo;

                mResult.Result = loginResp;
                mResult.returnStatus = 1;
                mResult.returnMsg = "登入成功";
            }
            catch (Exception ex)
            {
                mResult.returnStatus = 999;
                mResult.returnMsg = $"登入失敗: {ex.Message}";
                _logger.LogError(ex, "LoginController Login Error");
            }

            _logger.LogInformation("LoginController Login END");
            return mResult;
        }

        /// <summary>
        /// 計算 SHA256 雜湊
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ComputeSha256Hash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToHexString(bytes);
            }
        }

        /// <summary>
        /// 建造Token
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        private JwtSecurityToken CreateToken(List<Claim> claims)
        {
            var mSecretkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiAppConfig.JwtSettings.SecurityKey));

            var mSigningCredentials = new SigningCredentials(mSecretkey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(   // 亦可使用　SecurityTokenDescriptor　來産生 Token
                issuer: _apiAppConfig.JwtSettings.Issuer,
                audience: _apiAppConfig.JwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_apiAppConfig.JwtSettings.Expires),
                signingCredentials: mSigningCredentials);

            return token;
        }
    }
}
