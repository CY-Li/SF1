using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("tender_detail")]
/// [Index("td_participants", Name = "INDEX_tender_detail_1")]
public partial class tender_detail
{
    /// <summary>
    /// RSOCA 明細檔ID (Rotating Savings and Credit Association ID_民間互助
    /// </summary>
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long td_id { get; set; }

    /// <summary>
    /// RSOCA 主檔ID (Rotating Savings and Credit Association ID_民間互助會
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long td_tm_id { get; set; }

    /// <summary>
    /// 參加者mm_id
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long? td_participants { get; set; }

    /// <summary>
    /// 標案明細序列(用來方便做查找)
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int td_sequence { get; set; }

    /// <summary>
    /// 該標中標期數
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int td_period { get; set; }

    /// <summary>
    /// 下標狀態
    /// </summary>
    [StringLength(1)]
    public string td_status { get; set; } = null!;

    /// <summary>
    /// 如果成組後每期扣錢沒扣到就會寫入pending_payment
    /// </summary>
    [StringLength(367)]
    public string? td_pp_id { get; set; }

    /// <summary>
    /// 逞罰次數
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int td_pp_penalty_count { get; set; }

    /// <summary>
    /// 是否都已繳費，如果有未繳費就不是0
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int td_pp_paid { get; set; }

    /// <summary>
    /// 發起者mm_id=tm_initiator_mm_id
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long td_tm_initiator_mm_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long td_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime td_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long td_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime td_update_datetime { get; set; }
}
