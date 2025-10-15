using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("pending_payment")]
/// [Index("pp_id", Name = "INDEX_pending_payment_1", AllDescending = true)]
/// [Index("pp_mm_id", "pp_status", Name = "INDEX_pending_payment_2")]
public partial class pending_payment
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long pp_sn { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long pp_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long pp_mm_id { get; set; }

    [StringLength(22)]
    public string pp_tr_code { get; set; } = null!;

    /// [Column(TypeName = "bigint(15)")]
    public long pp_tm_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long pp_td_id { get; set; }

    /// [Column(TypeName = "int(15)")]
    public int pp_amount { get; set; }

    /// <summary>
    /// 10:欠費、20:已補繳費、30:繳費需處罰
    /// </summary>
    [StringLength(2)]
    public string pp_status { get; set; } = null!;

    /// <summary>
    /// 超過時間繳費就要逞罰
    /// </summary>
    /// [Column(TypeName = "datetime")]
    public DateTime pp_penalty_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long pp_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime pp_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long pp_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime pp_update_datetime { get; set; }
}
