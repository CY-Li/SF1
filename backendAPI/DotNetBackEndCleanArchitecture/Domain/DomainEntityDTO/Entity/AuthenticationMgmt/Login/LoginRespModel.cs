using Swashbuckle.AspNetCore.Annotations;

namespace DomainEntityDTO.Entity.AuthenticationMgmt.Login
{
    public class LoginRespModel
    {
        [SwaggerSchema(Description = "Token")]
        public string AccessToken { get; set; } = string.Empty;
        //public string RefreshToken { get; set; } = string.Empty;
        [SwaggerSchema(Description = "到期時間")]
        public DateTime expiration { get; set; }

        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = null!;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        [SwaggerSchema(Description = "會員顯示名稱")]
        public string mm_name { get; set; } = null!;

        /// <summary>
        /// 介紹人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "介紹人")]
        public long mm_introduce_code { get; set; }

        /// <summary>
        /// 邀請碼-Invitation code
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "邀請碼")]
        public long mm_invite_code { get; set; }

        /// <summary>
        /// 角色權限:1-使用者、2-管理者
        /// </summary>
        /// [StringLength(1)]
        [SwaggerSchema(Description = "角色權限-使用者")]
        public string mm_role_type { get; set; } = null!;

        /// <summary>
        /// KYC認證，沒有認證不能提領
        /// </summary>
        /// [StringLength(1)]
        [SwaggerSchema(Description = "KYC驗證狀態")]
        public string mm_kyc { get; set; } = null!;

        /// <summary>
        /// 下標數(自己)，system_parameters:10
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        [SwaggerSchema(Description = "下標數")]
        public int mw_subscripts_count { get; set; }

        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "儲值點數")]
        public decimal mw_stored { get; set; }

        /// <summary>
        /// 紅利點數，system_parameters:12
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "紅利點數")]
        public decimal mw_reward { get; set; }

        /// <summary>
        /// 平安點數，system_parameters:13
        /// </summary>
        /// [Column("mw_ peace")]
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "平安點數")]
        public decimal mw_peace { get; set; }

        /// <summary>
        /// 商城點數，system_parameters:14
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "商城點數")]
        public decimal mw_mall { get; set; }

        /// <summary>
        /// 註冊點數，system_parameters:15
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "註冊點數")]
        public decimal mw_registration { get; set; }

        /// <summary>
        /// 死會點數，system_parameters:16
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "死會點數")]
        public decimal mw_death { get; set; }

        /// <summary>
        /// 累積獎勵，system_parameters:17
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "累積獎勵")]
        public decimal mw_accumulation { get; set; }
    }
}
