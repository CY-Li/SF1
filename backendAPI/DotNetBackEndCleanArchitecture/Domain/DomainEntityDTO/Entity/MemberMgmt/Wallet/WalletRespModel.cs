using Swashbuckle.AspNetCore.Annotations;

namespace DomainEntityDTO.Entity.MemberMgmt.Wallet
{
    /// <summary>
    /// 取得該會員錢包資料
    /// </summary>
    public class GetWalletRespModel
    {
        /// <summary>
        /// 貨幣種類
        /// </summary>
        /// [StringLength(10)]
        [SwaggerSchema(Description = "貨幣種類")]
        public string? mw_currency { get; set; } = string.Empty;

        /// <summary>
        /// 虛擬幣錢包地址
        /// </summary>
        /// [StringLength(150)]
        [SwaggerSchema(Description = "虛擬幣錢包地址")]
        public string? mw_address { get; set; } = string.Empty;

        /// <summary>
        /// 下標數(自己)，system_parameters:10
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        [SwaggerSchema(Description = "下標數")]
        public int mw_subscripts_count { get; set; } = 0;

        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "儲值點數:11")]
        public decimal mw_stored { get; set; } = 0;

        /// <summary>
        /// 紅利點數，system_parameters:12
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "紅利點數:12")]
        public decimal mw_reward { get; set; } = 0;

        /// <summary>
        /// 平安點數，system_parameters:13
        /// </summary>
        /// [Column("mw_ peace")]
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "平安點數:13")]
        public decimal mw_peace { get; set; } = 0;

        /// <summary>
        /// 商城點數，system_parameters:14
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "商城點數:14")]
        public decimal mw_mall { get; set; } = 0;

        /// <summary>
        /// 註冊點數，system_parameters:15
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "註冊點數:15")]
        public decimal mw_registration { get; set; } = 0;

        /// <summary>
        /// 死會點數，system_parameters:16
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "死會點數:16")]
        public decimal mw_death { get; set; } = 0;

        /// <summary>
        /// 累積獎勵，system_parameters:17
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "累積獎勵")]
        public decimal mw_accumulation { get; set; } = 0;

        /// <summary>
        /// 當期繳交會費(不存在該欄位)
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "當期繳交會費")]
        public decimal current_bid_fee { get; set; } = 0;

        /// <summary>
        /// 下期預估會費(不存在該欄位)
        /// </summary>
        ////// [Precision(15, 5)]
        [SwaggerSchema(Description = "下期預估會費")]
        public decimal next_bid_estimate { get; set; } = 0;
    }
}
