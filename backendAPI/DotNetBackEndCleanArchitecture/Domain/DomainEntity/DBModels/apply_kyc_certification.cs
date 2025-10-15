using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("apply_kyc_certification")]
/// [Index("akc_mm_id", Name = "INDEX_apply_kyc_certification_1")]
/// [Index("akc_status", Name = "INDEX_apply_kyc_certification_2")]
public partial class apply_kyc_certification
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long akc_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long akc_mm_id { get; set; }

    /// <summary>
    /// 身分證正面
    /// </summary>
    [StringLength(100)]
    public string akc_front { get; set; } = null!;

    /// <summary>
    /// 身分證反面
    /// </summary>
    [StringLength(100)]
    public string akc_back { get; set; } = null!;

    /// <summary>
    /// 性別(1:男生、2:女生)
    /// </summary>
    [StringLength(1)]
    public string akc_gender { get; set; } = null!;

    /// <summary>
    /// 身分證字號
    /// </summary>
    [StringLength(10)]
    public string akc_personal_id { get; set; } = null!;

    /// <summary>
    /// 通訊地址
    /// </summary>
    [StringLength(100)]
    public string akc_address { get; set; } = null!;

    /// <summary>
    /// 信箱
    /// </summary>
    [StringLength(50)]
    public string akc_email { get; set; } = null!;

    /// <summary>
    /// 銀行帳號
    /// </summary>
    [StringLength(25)]
    public string akc_bank_account { get; set; } = null!;

    /// <summary>
    /// 戶名
    /// </summary>
    [StringLength(20)]
    public string akc_bank_account_name { get; set; } = null!;

    /// <summary>
    /// 分行
    /// </summary>
    [StringLength(10)]
    public string akc_branch { get; set; } = null!;

    /// <summary>
    /// 申請狀態:10-待認證、20-認證通過、30-認證駁回、40-認證失敗
    /// </summary>
    [StringLength(2)]
    public string akc_status { get; set; } = null!;

    /// [Column(TypeName = "bigint(15)")]
    public long akc_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime akc_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long akc_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime akc_update_datetime { get; set; }
}
