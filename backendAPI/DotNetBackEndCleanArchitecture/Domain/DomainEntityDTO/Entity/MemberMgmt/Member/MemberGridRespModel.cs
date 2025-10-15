using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.MemberMgmt.Member
{
    /// <summary>
    /// 會員-取得下屬組織
    /// </summary>
    public class GetSubordinateOrgRespModel
    {
        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        //[StringLength(10)]
        [SwaggerSchema(Description = "會員帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        //[StringLength(30)]
        [SwaggerSchema(Description = "會員名稱")]
        public string mm_name { get; set; } = string.Empty;

        /// <summary>
        /// 會員等級(暫時沒用)
        /// </summary>
        //[StringLength(1)]
        [SwaggerSchema(Description = "會員等級")]
        public string mm_level { get; set; } = string.Empty;

        /// <summary>
        /// 下標數(自己)，system_parameters:10
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        [SwaggerSchema(Description = "會員下標數量")]
        public int mw_subscripts_count { get; set; }

        [SwaggerSchema(Description = "會員上層帳號")]
        public string mm_invite_account { get; set; } = string.Empty;

        [SwaggerSchema(Description = "會員階層(1等於是自己也只會有一筆)")]
        public int class_level { get; set; } = 0;

        [SwaggerSchema(Description = "會員下層總數包刮自己")]
        public int total_qty { get; set; } = 0;
    }
    /// <summary>
    /// 後台-取得會員列表
    /// </summary>
    public class QueryMemberRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        //[SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        //[SwaggerSchema(Description = "會員顯示名稱")]
        public string mm_name { get; set; } = string.Empty;

        /// <summary>
        /// 介紹人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "介紹人")]
        public long mm_introduce_code { get; set; } = 0;

        public string mm_introduce_user { get; set; } = string.Empty;

        /// <summary>
        /// 邀請碼-Invitation code
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "邀請碼")]
        public long mm_invite_code { get; set; } = 0;


        public string mm_invite_user { get; set; } = string.Empty;

        /// <summary>
        /// 性別(1:男生、2:女生)
        /// </summary>
        /// [StringLength(1)]
        //[SwaggerSchema(Description = "性別")]
        public string? mm_gender { get; set; } = string.Empty;

        /// <summary>
        /// 身分證字號
        /// </summary>
        /// [StringLength(10)]
        //[SwaggerSchema(Description = "身分證字號")]
        public string? mm_personal_id { get; set; } = string.Empty;

        /// <summary>
        /// 通訊地址
        /// </summary>
        /// [StringLength(100)]
        //[SwaggerSchema(Description = "通訊地址")]
        public string? mm_mw_address { get; set; } = string.Empty;

        /// <summary>
        /// 信箱
        /// </summary>
        /// [StringLength(50)]
        //[SwaggerSchema(Description = "信箱")]
        public string? mm_email { get; set; } = string.Empty;

        /// <summary>
        /// 會員等級(暫時沒用)
        /// </summary>
        //[StringLength(1)]
        //[SwaggerSchema(Description = "會員等級")]
        public string mm_level { get; set; } = string.Empty;

        /// <summary>
        /// 角色權限:1-使用者、2-管理者
        /// </summary>
        /// [StringLength(1)]
        //[SwaggerSchema(Description = "角色權限")]
        public string mm_role_type { get; set; } = string.Empty;

        /// <summary>
        /// 帳號是否可正常使用
        /// </summary>
        /// [StringLength(1)]
        //[SwaggerSchema(Description = "帳號是否可正常使用")]
        public string mm_status { get; set; } = string.Empty;

        /// <summary>
        /// KYC認證，沒有認證不能提領
        /// </summary>
        /// [StringLength(1)]
        //[SwaggerSchema(Description = "KYC認證")]
        public string mm_kyc { get; set; } = string.Empty;

        /// <summary>
        /// 下標數(自己)，system_parameters:10
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        //[SwaggerSchema(Description = "下標數")]
        public int mw_subscripts_count { get; set; } = 0;

        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        ////// [Precision(15, 5)]
        //[SwaggerSchema(Description = "儲值點數")]
        public decimal mw_stored { get; set; } = 0;

        /// <summary>
        /// 紅利點數，system_parameters:12
        /// </summary>
        ////// [Precision(15, 5)]
        //[SwaggerSchema(Description = "紅利點數")]
        public decimal mw_reward { get; set; } = 0;

        public DateTime mm_create_datetime { get; set; }
    }
}
