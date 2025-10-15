using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("parameter_category")]
/// [Index("sp_key", Name = "INDEX_parameter_category_1")]
public partial class parameter_category
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long sp_id { get; set; }

    /// <summary>
    /// 查詢參數設定群組用
    /// </summary>
    [StringLength(25)]
    public string sp_key { get; set; } = null!;

    /// <summary>
    /// 參數類別
    /// </summary>
    [StringLength(25)]
    public string sp_code { get; set; } = null!;

    /// <summary>
    /// 參數名稱
    /// </summary>
    [StringLength(25)]
    public string sp_name { get; set; } = null!;

    /// <summary>
    /// 參數解釋
    /// </summary>
    [StringLength(50)]
    public string? sp_description { get; set; } 

    /// [Column(TypeName = "bigint(15)")]
    public long sp_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime sp_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long sp_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime sp_update_datetime { get; set; }
}
