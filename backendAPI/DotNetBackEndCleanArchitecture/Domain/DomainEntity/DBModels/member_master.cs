using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("member_master")]
/// [Index("mm_account", Name = "INDEX_member_master_1", IsUnique = true)]
public partial class member_master
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long mm_id { get; set; }

    /// <summary>
    /// 帳號(現在為手機號碼)
    /// </summary>
    [StringLength(10)]
    public string mm_account { get; set; } = null!;

    /// <summary>
    /// 雜湊後密碼
    /// </summary>
    [StringLength(100)]
    public string mm_hash_pwd { get; set; } = null!;

    /// <summary>
    /// 第二組密碼，下單用
    /// </summary>
    [StringLength(100)]
    public string mm_2nd_hash_pwd { get; set; } = null!;

    /// <summary>
    /// 會員顯示名稱
    /// </summary>
    [StringLength(30)]
    public string mm_name { get; set; } = null!;

    [StringLength(10)]
    public string? mm_introduce_user { get; set; }

    /// <summary>
    /// 介紹人
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long mm_introduce_code { get; set; }

    [StringLength(10)]
    public string? mm_invite_user { get; set; }

    /// <summary>
    /// 邀請碼-Invitation code
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long mm_invite_code { get; set; }

    /// <summary>
    /// 性別(1:男生、2:女生)
    /// </summary>
    [StringLength(1)]
    public string? mm_gender { get; set; }

    /// <summary>
    /// 國碼
    /// </summary>
    [StringLength(10)]
    public string? mm_country_code { get; set; }

    /// <summary>
    /// 身分證字號
    /// </summary>
    [StringLength(10)]
    public string? mm_personal_id { get; set; }

    /// <summary>
    /// 電話號碼(目前手機號碼)
    /// </summary>
    [StringLength(15)]
    public string? mm_phone_number { get; set; }

    /// <summary>
    /// 通訊地址
    /// </summary>
    [StringLength(100)]
    public string? mm_address { get; set; }

    /// <summary>
    /// 信箱
    /// </summary>
    [StringLength(50)]
    public string? mm_email { get; set; }

    /// <summary>
    /// 銀行帳號
    /// </summary>
    [StringLength(25)]
    public string? mm_bank_account { get; set; } 

    /// <summary>
    /// 戶名
    /// </summary>
    [StringLength(20)]
    public string? mm_bank_account_name { get; set; } 

    /// <summary>
    /// 分行
    /// </summary>
    [StringLength(10)]
    public string? mm_branch { get; set; } 

    /// <summary>
    /// 會員等級(暫時沒用)
    /// </summary>
    [StringLength(1)]
    public string mm_level { get; set; } = null!;

    /// <summary>
    /// 角色權限:1-使用者、2-管理者
    /// </summary>
    [StringLength(1)]
    public string mm_role_type { get; set; } = null!;

    /// <summary>
    /// 帳號是否可正常使用
    /// </summary>
    [StringLength(1)]
    public string mm_status { get; set; } = null!;

    /// [Column(TypeName = "bigint(15)")]
    public long mm_kyc_id { get; set; }

    /// <summary>
    /// KYC認證，沒有認證不能提領
    /// </summary>
    [StringLength(1)]
    public string mm_kyc { get; set; } = null!;

    /// [Column(TypeName = "bigint(15)")]
    public long mm_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime mm_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long mm_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime mm_update_datetime { get; set; }
}
