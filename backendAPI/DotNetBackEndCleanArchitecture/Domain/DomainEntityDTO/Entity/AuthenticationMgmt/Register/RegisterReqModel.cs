using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.AuthenticationMgmt.Register
{
    public class RegisterReqModel
    {
        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [Required(ErrorMessage = "Account is required"), MinLength(4), MaxLength(20)]
        //[RegularExpression("^09\\d{8}$", ErrorMessage = "帳號限定輸入10位數字，開頭為用09。")]
        [SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = null!;

        /// <summary>
        /// DB不存在該欄位
        /// </summary>
        [Required(ErrorMessage = "Password is required"), MinLength(4), MaxLength(15)]
        [SwaggerSchema(Description = "密碼")]
        public string mm_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 第二組密碼，不存在該欄位
        /// </summary>
        [Required(ErrorMessage = "2 Password is required"), MinLength(4), MaxLength(15)]
        [SwaggerSchema(Description = "密碼")]
        public string mm_2nd_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 介紹人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "介紹人")]
        public string mm_introduce_user { get; set; } = string.Empty;

        /// <summary>
        /// 邀請碼-Invitation code
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "邀請碼")]
        public string mm_invite_user { get; set; } = string.Empty;

        /// <summary>
        /// 國碼
        /// </summary>
        /// [StringLength(10)]
        [MaxLength(10)]
        [SwaggerSchema(Description = "國碼")]
        public string mm_country_code { get; set; } = string.Empty;
    }
}
