using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.AuthenticationMgmt.Login
{
    public class LoginReqModel
    {
        [Required(ErrorMessage = "Account is required"), MinLength(4), MaxLength(20)]
        [SwaggerSchema(Description = "帳號")]
        public string? mm_account { get; set; }

        [Required(ErrorMessage = "Password is required"), MinLength(4), MaxLength(15)]
        [SwaggerSchema(Description = "密碼")]
        public string? mm_pwd { get; set; }
    }
}
