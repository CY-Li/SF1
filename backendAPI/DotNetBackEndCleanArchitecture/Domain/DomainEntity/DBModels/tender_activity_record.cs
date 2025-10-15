using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("tender_activity_record")]
/// [Index("tar_tr_code", Name = "INDEX_tender_activity_record_1", AllDescending = true)]
/// [Index("tar_status", Name = "INDEX_tender_activity_record_2")]
public partial class tender_activity_record
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long tar_id { get; set; }

    /// <summary>
    /// 操作人(下標人、得標人)
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long tar_mm_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long? tar_mm_introduce_code { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tar_tm_id { get; set; }

    /// <summary>
    /// 這裡指的不是總數量，而是一次下標的會數
    /// </summary>
    public int tar_tm_count { get; set; }

    /// <summary>
    /// 交易類別:11-下標扣除押金(儲值、註冊) 、12-下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利
    /// </summary>
    [StringLength(2)]
    public string tar_tr_type { get; set; } = null!;

    [StringLength(20)]
    public string tar_tr_code { get; set; } = null!;

    /// <summary>
    /// 標案活動紀錄狀態，0:未處理、1已經處理
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int tar_status { get; set; }

    /// <summary>
    /// 下標邀請組織列表(invitation_org)該欄位存放json值，供排成好處理、
    /// </summary>
    public string? tar_json { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tar_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime tar_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tar_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime tar_update_datetime { get; set; }
}
