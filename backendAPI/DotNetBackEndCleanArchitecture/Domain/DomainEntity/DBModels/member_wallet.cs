using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("member_wallet")]
/// [Index("mw_mm_id", Name = "INDEX_member_wallet_1", IsUnique = true)]
public partial class member_wallet
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long mw_id { get; set; }

    /// <summary>
    /// 會員ID
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long mw_mm_id { get; set; }

    /// <summary>
    /// 貨幣種類
    /// </summary>
    [StringLength(10)]
    public string? mw_currency { get; set; }

    /// <summary>
    /// 虛擬幣錢包地址
    /// </summary>
    [StringLength(150)]
    public string? mw_address { get; set; }

    /// <summary>
    /// 下標數(自己)，system_parameters:10
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int mw_subscripts_count { get; set; }

    /// <summary>
    /// 儲值點數(可用點數)，system_parameters:11
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mw_stored { get; set; }

    /// <summary>
    /// 紅利點數，system_parameters:12
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mw_reward { get; set; }

    /// <summary>
    /// 平安點數，system_parameters:13
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mw_peace { get; set; }

    /// <summary>
    /// 商城點數，system_parameters:14
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mw_mall { get; set; }

    /// <summary>
    /// 註冊點數，system_parameters:15
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mw_registration { get; set; }

    /// <summary>
    /// 死會點數，system_parameters:16
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mw_death { get; set; }

    /// <summary>
    /// 累積獎勵，system_parameters:17
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mw_accumulation { get; set; }

    /// <summary>
    /// 懲罰點數，system_parameters:18
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mw_punish { get; set; }

    [StringLength(1)]
    public string mw_level { get; set; } = null!;

    /// [Column(TypeName = "bigint(15)")]
    public long mw_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime mw_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long mw_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime mw_update_datetime { get; set; }
}
