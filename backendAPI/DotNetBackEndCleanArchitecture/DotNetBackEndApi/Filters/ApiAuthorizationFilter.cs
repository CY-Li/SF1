using DomainEntityDTO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DotNetBackEndApi.Filters
{
    public class ApiAuthorizationFilter : Attribute, IAuthorizationFilter
    {
        void IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context)
        {
            var _apiAppConfig = (ApiAppConfig)context.HttpContext.RequestServices.GetService<IOptions<ApiAppConfig>>().Value;

            bool m_tokenFlag = context.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues m_jwtToken);

            var m_ignore = (from a in context.ActionDescriptor.EndpointMetadata
                            where a.GetType() == typeof(AllowAnonymousAttribute)
                            select a).FirstOrDefault();

            if (m_ignore == null)
            {
                if (m_tokenFlag)
                {
                    //var valiresult = ValidateToken(jwtValue, validateParameters);
                    ClaimsPrincipal valiresult = ValidateToken(m_jwtToken, _apiAppConfig);

                    if (valiresult != null)
                    {
                        context.HttpContext.User = valiresult;
                    }
                    else
                    {
                        context.Result = new JsonResult(new ApiResultModel<bool>()
                        {
                            Result = false,
                            returnStatus = 401,
                            returnMsg = "驗證-沒有登入"
                        });
                    }
                }
                else
                {
                    context.Result = new JsonResult(new ApiResultModel<bool>()
                    {
                        Result = false,
                        returnStatus = 401,
                        returnMsg = "沒有登入"
                    });
                }
            }
        }

        private ClaimsPrincipal ValidateToken(string p_jwtToken, ApiAppConfig p_apiAppConfig)
        {
            ClaimsPrincipal? m_claimsPrincipal = null;
            try
            {
                string m_jwtToken = p_jwtToken.ToString().Replace("bearer ", "");
                var m_validateParameters = new TokenValidationParameters()
                {
                    // 驗證 Issuer(發行者) (一般都會)
                    ValidateIssuer = p_apiAppConfig.JwtSettings.ValidateIssuer,
                    ValidIssuer = p_apiAppConfig.JwtSettings.Issuer,

                    // 驗證 Audience (通常不太需要)
                    ValidateAudience = p_apiAppConfig.JwtSettings.ValidateAudience,
                    ValidAudience = p_apiAppConfig.JwtSettings.Audience,

                    // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
                    ValidateIssuerSigningKey = p_apiAppConfig.JwtSettings.ValidateIssuerSigningKey,
                    // 應該從 IConfiguration 取得
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(p_apiAppConfig.JwtSettings.SecurityKey)),

                    // 驗證 Token 的有效期間 (一般都會)
                    ValidateLifetime = p_apiAppConfig.JwtSettings.ValidateLifetime,
                    ClockSkew = TimeSpan.Zero // 時間到，就過期
                };

                var m_tokenHandler = new JwtSecurityTokenHandler();
                m_claimsPrincipal = m_tokenHandler.ValidateToken(m_jwtToken, m_validateParameters, out var m_validatedToken);

                return m_claimsPrincipal;
            }
            catch (SecurityTokenValidationException)
            {
                //return m_claimsPrincipal;
            }
            catch (Exception)
            {
                //return m_claimsPrincipal;
            }

            return m_claimsPrincipal;
        }
    }
}
