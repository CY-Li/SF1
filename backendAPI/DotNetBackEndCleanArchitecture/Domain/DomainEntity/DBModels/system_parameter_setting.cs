using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("system_parameter_setting")]
/// [Index("sps_code", Name = "INDEX_system_parameter_setting_1")]
public partial class system_parameter_setting
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long sps_id { get; set; }

    /// <summary>
    /// 設定編號
    /// </summary>
    [StringLength(25)]
    public string sps_code { get; set; } = null!;

    /// <summary>
    /// 設定名稱
    /// </summary>
    [StringLength(25)]
    public string sps_name { get; set; } = null!;

    /// <summary>
    /// 參數描述
    /// </summary>
    [StringLength(100)]
    public string? sps_description { get; set; }

    /// <summary>
    /// 圖片(會員儲值會需要QR CODE故這邊新增該欄位)
    /// </summary>
    /// [Column(TypeName = "blob")]
    public byte[]? sps_picture { get; set; }

    [StringLength(100)]
    public string? sps_parameter01 { get; set; }

    [StringLength(100)]
    public string? sps_parameter02 { get; set; }

    [StringLength(100)]
    public string? sps_parameter03 { get; set; }

    [StringLength(100)]
    public string? sps_parameter04 { get; set; }

    [StringLength(100)]
    public string? sps_parameter05 { get; set; }

    [StringLength(100)]
    public string? sps_parameter06 { get; set; }

    [StringLength(100)]
    public string? sps_parameter07 { get; set; }

    [StringLength(100)]
    public string? sps_parameter08 { get; set; }

    [StringLength(100)]
    public string? sps_parameter09 { get; set; }

    [StringLength(100)]
    public string? sps_parameter10 { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long sps_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime sps_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long sps_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime sps_update_datetime { get; set; }
}
