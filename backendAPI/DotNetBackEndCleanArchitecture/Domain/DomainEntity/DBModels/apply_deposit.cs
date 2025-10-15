using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("apply_deposit")]
/// [Index("ad_mm_id", Name = "INDEX_apply_deposit_1", AllDescending = true)]
/// [Index("ad_status", Name = "INDEX_apply_deposit_2")]
public partial class apply_deposit
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long ad_id { get; set; }

    /// <summary>
    /// 儲值人
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long ad_mm_id { get; set; }

    /// <summary>
    /// 儲值金額
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int ad_amount { get; set; }

    /// <summary>
    /// 儲值後得到的KEY(要給我們才查的到)
    /// </summary>
    [StringLength(200)]
    public string ad_key { get; set; } = null!;

    /// <summary>
    /// 儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
    /// </summary>
    [StringLength(200)]
    public string? ad_url { get; set; }

    /// <summary>
    /// 圖片(會員儲值會需要QR CODE故這邊新增該欄位)
    /// </summary>
    /// [Column(TypeName = "blob")]
    public byte[] ad_picture { get; set; } = null!;

    /// <summary>
    /// 申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
    /// </summary>
    [StringLength(2)]
    public string ad_status { get; set; } = null!;

    /// [Column(TypeName = "bigint(15)")]
    public long ad_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime ad_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long ad_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime ad_update_datetime { get; set; }
}
