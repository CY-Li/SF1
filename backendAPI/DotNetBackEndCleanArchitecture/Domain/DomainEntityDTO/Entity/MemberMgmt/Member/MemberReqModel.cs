using DomainEntityDTO.Common;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.MemberMgmt.Member
{
    /// <summary>
    /// 後台-取得會員列表
    /// </summary>
    public class QueryMemberReqModel : BaseGridReqModel
    {
        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [RegularExpression("^09\\d{8}$", ErrorMessage = "帳號限定輸入10位數字，開頭為用09。")]
        //[SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        [MaxLength(30)]
        //[SwaggerSchema(Description = "會員名稱")]
        public string mm_name { get; set; } = string.Empty;

        /// <summary>
        /// 介紹人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "介紹人")]
        public string mm_introduce_user { get; set; } = string.Empty;

        /// <summary>
        /// 邀請碼-Invitation code
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "邀請碼")]
        //public long mm_invite_code { get; set; } = 0;
        public string mm_invite_user { get; set; } = string.Empty;
    }

    /// <summary>
    /// 會員、後台-更新會員資料
    /// </summary>
    public class PutMemberReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [Required(ErrorMessage = "mm_account is required")]
        [RegularExpression("^09\\d{8}$", ErrorMessage = "mm_account限定輸入10位數字，開頭為用09。")]
        [SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// DB不存在該欄位
        /// </summary>
        //[RegularExpression(@"^[a-zA-Z0-9]{4,15}$", ErrorMessage = "密碼必須為4-15個英文字母和數字組成")]
        //[SwaggerSchema(Description = "密碼")]
        //public string mm_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 下單密碼
        /// </summary>
        //[RegularExpression(@"^[a-zA-Z0-9]{4,15}$", ErrorMessage = "密碼必須為4-15個英文字母和數字組成")]
        //[SwaggerSchema(Description = "下單密碼")]
        //public string mm_2nd_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        //[MinLength(4), MaxLength(30)]
        //[SwaggerSchema(Description = "會員顯示名稱")]
        //public string mm_name { get; set; } = string.Empty;

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
        //[SwaggerSchema(Description = "邀請碼")]
        //public long mm_invite_code { get; set; } = 0;

        /// <summary>
        /// 性別(1:男生、2:女生)
        /// </summary>
        /// [StringLength(1)]
        //[RegularExpression(@"^[1-2]$", ErrorMessage = "性別只能輸入1:男、2:女")]
        //[SwaggerSchema(Description = "性別")]
        //public string mm_gender { get; set; } = string.Empty;

        /// <summary>
        /// 身分證字號
        /// </summary>
        /// [StringLength(10)]
        //[StringLength(10)]
        //[RegularExpression(@"^[A-Z]{1}[1-2]{1}[0-9]{8}$$", ErrorMessage = "身分證字號錯誤")]
        //[SwaggerSchema(Description = "身分證字號")]
        //public string mm_personal_id { get; set; } = string.Empty;

        /// <summary>
        /// 通訊地址
        /// </summary>
        /// [StringLength(100)]
        //[MaxLength(100)]
        //[SwaggerSchema(Description = "通訊地址")]
        //public string mm_address { get; set; } = string.Empty;

        /// <summary>
        /// 信箱
        /// </summary>
        /// [StringLength(50)]
        //[MaxLength(50)]
        //[SwaggerSchema(Description = "信箱")]
        //public string mm_email { get; set; } = string.Empty;
    }

    public class UpdatePwdReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [Required(ErrorMessage = "mm_account is required")]
        [RegularExpression("^09\\d{8}$", ErrorMessage = "mm_account限定輸入10位數字，開頭為用09。")]
        [SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// DB不存在該欄位
        /// </summary>
        [RegularExpression(@"^[a-zA-Z0-9]{4,15}$", ErrorMessage = "密碼必須為4-15個英文字母和數字組成")]
        [SwaggerSchema(Description = "密碼")]
        public string mm_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 下單密碼
        /// </summary>
        [RegularExpression(@"^[a-zA-Z0-9]{4,15}$", ErrorMessage = "密碼必須為4-15個英文字母和數字組成")]
        [SwaggerSchema(Description = "下單密碼")]
        public string mm_2nd_pwd { get; set; } = string.Empty;
    }
}
