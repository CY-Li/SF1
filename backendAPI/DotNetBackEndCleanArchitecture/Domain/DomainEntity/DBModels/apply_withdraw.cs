using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("apply_withdraw")]
/// [Index("aw_mm_id", Name = "INDEX_apply_withdraw_1", AllDescending = true)]
/// [Index("aw_status", Name = "INDEX_apply_withdraw_2")]
public partial class apply_withdraw
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long aw_id { get; set; }

    /// <summary>
    /// 儲值人
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long aw_mm_id { get; set; }

    /// <summary>
    /// 儲值金額
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int aw_amount { get; set; }

    /// <summary>
    /// 儲值後得到的KEY(要給我們才查的到)
    /// </summary>
    [StringLength(200)]
    public string aw_key { get; set; } = null!;

    /// <summary>
    /// 管理者錢包
    /// </summary>
    [StringLength(200)]
    public string? aw_url { get; set; }

    /// <summary>
    /// 提領錢包地址
    /// </summary>
    [StringLength(200)]
    public string? aw_wallet_address { get; set; }

    /// <summary>
    /// 申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗
    /// </summary>
    [StringLength(2)]
    public string aw_status { get; set; } = null!;

    /// [Column(TypeName = "bigint(15)")]
    public long aw_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime aw_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long aw_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime aw_update_datetime { get; set; }
}
