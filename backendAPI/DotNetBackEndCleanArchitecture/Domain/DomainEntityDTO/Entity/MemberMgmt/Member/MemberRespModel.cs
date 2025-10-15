using Swashbuckle.AspNetCore.Annotations;

namespace DomainEntityDTO.Entity.MemberMgmt.Member
{
    /// <summary>
    /// 會員、後台-取得該會員資料
    /// </summary>
    public class GetMemberRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        [SwaggerSchema(Description = "會員顯示名稱")]
        public string mm_name { get; set; } = string.Empty;

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
        /// 性別(1:男生、2:女生)
        /// </summary>
        /// [StringLength(1)]
        [SwaggerSchema(Description = "性別")]
        public string? mm_gender { get; set; } = string.Empty;

        /// <summary>
        /// 身分證字號
        /// </summary>
        /// [StringLength(10)]
        [SwaggerSchema(Description = "身分證字號")]
        public string? mm_personal_id { get; set; } = string.Empty;

        /// <summary>
        /// 通訊地址
        /// </summary>
        /// [StringLength(100)]
        [SwaggerSchema(Description = "錢包地址")]
        public string? mm_mw_address { get; set; } = string.Empty;

        /// <summary>
        /// 信箱
        /// </summary>
        /// [StringLength(50)]
        [SwaggerSchema(Description = "信箱")]
        public string? mm_email { get; set; } = string.Empty;

        /// <summary>
        /// 銀行代號
        /// </summary>
        /// [StringLength(10)]
        [SwaggerSchema(Description = "銀行代號")]
        public string? mm_bank_code { get; set; } = string.Empty;

        /// <summary>
        /// 銀行帳號
        /// </summary>
        /// [StringLength(25)]
        [SwaggerSchema(Description = "銀行帳號")]
        public string? mm_bank_account { get; set; } = string.Empty;

        /// <summary>
        /// 戶名
        /// </summary>
        /// [StringLength(20)]
        [SwaggerSchema(Description = "戶名")]
        public string? mm_bank_account_name { get; set; } = string.Empty;

        /// <summary>
        /// 分行
        /// </summary>
        /// [StringLength(50)]
        [SwaggerSchema(Description = "分行")]
        public string? mm_branch { get; set; } = string.Empty;

        /// <summary>
        /// 受益人名稱
        /// </summary>
        /// [StringLength(50)]
        [SwaggerSchema(Description = "受益人名稱")]
        public string? mm_beneficiary_name { get; set; } = string.Empty;

        /// <summary>
        /// 受益人電話
        /// </summary>
        /// [StringLength(50)]
        [SwaggerSchema(Description = "受益人電話")]
        public string? mm_beneficiary_phone { get; set; } = string.Empty;

        /// <summary>
        /// 受益人關係
        /// </summary>
        /// [StringLength(50)]
        [SwaggerSchema(Description = "受益人關係")]
        public string? mm_beneficiary_relationship { get; set; } = string.Empty;

        /// <summary>
        /// 角色權限:1-使用者、2-管理者
        /// </summary>
        /// [StringLength(1)]
        //[SwaggerSchema(Description = "角色權限")]
        //public string mm_role_type { get; set; } = string.Empty;

        /// <summary>
        /// 帳號是否可正常使用
        /// </summary>
        /// [StringLength(1)]
        [SwaggerSchema(Description = "帳號是否可正常使用")]
        public string mm_status { get; set; } = string.Empty;

        /// <summary>
        /// KYC認證，沒有認證不能提領
        /// </summary>
        /// [StringLength(1)]
        [SwaggerSchema(Description = "KYC認證")]
        public string mm_kyc { get; set; } = string.Empty;

        [SwaggerSchema(Description = "會員加入時間")]
        public DateTime mm_create_datetime { get; set; }
    }
}
