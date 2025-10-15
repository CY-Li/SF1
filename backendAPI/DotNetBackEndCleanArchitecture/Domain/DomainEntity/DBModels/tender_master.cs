using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("tender_master")]
/// [Index("tm_group_datetime", Name = "INDEX_tender_master_1", AllDescending = true)]
public partial class tender_master
{
    /// <summary>
    /// RSOCA 主檔ID (Rotating Savings and Credit Association ID_民間互助會
    /// </summary>
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long tm_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tm_sn { get; set; }

    /// <summary>
    /// 標會名稱
    /// </summary>
    [StringLength(25)]
    public string tm_name { get; set; } = null!;

    /// <summary>
    /// 取得時間加字串(暫時沒用，用來記錄時間點)
    /// </summary>
    [StringLength(20)]
    public string tm_ticks { get; set; } = null!;

    /// <summary>
    /// 發起人mm_id
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long tm_initiator_mm_id { get; set; }

    /// <summary>
    /// 標會種類:1-一般玩法(散會:可以選下標數)、2-1人玩法、3-2人玩法、4-4人玩法
    /// </summary>
    [StringLength(1)]
    public string tm_type { get; set; } = null!;

    /// <summary>
    /// 下標人會用|串接，EX:1|2|3
    /// </summary>
    [StringLength(150)]
    public string? tm_bidder { get; set; }

    /// <summary>
    /// 得獎者會用|串接，EX:1|2|3
    /// </summary>
    [StringLength(150)]
    public string? tm_winners { get; set; }

    /// <summary>
    /// 排程已經處理的期數
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int tm_settlement_period { get; set; }

    /// <summary>
    /// 目前期數(成組時，當下期數，剛寫入就是1)，得標會依據該期數算錢
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int tm_current_period { get; set; }

    /// <summary>
    /// 是否成組:0-未成組,1-成組,2-結束
    /// </summary>
    [StringLength(1)]
    public string tm_status { get; set; } = null!;

    /// <summary>
    /// 用來簡單判斷是否成組
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int tm_count { get; set; }

    /// <summary>
    /// 排程查資料查過這個時間就要處理
    /// </summary>
    /// [Column(TypeName = "datetime")]
    public DateTime? tm_settlement_datetime { get; set; }

    /// <summary>
    /// 第一次開標時間
    /// </summary>
    /// [Column(TypeName = "datetime")]
    public DateTime? tm_win_first_datetime { get; set; }

    /// <summary>
    /// 此組最後一次開標時間
    /// </summary>
    /// [Column(TypeName = "datetime")]
    public DateTime? tm_win_end_datetime { get; set; }

    /// <summary>
    /// 成組時間
    /// </summary>
    /// [Column(TypeName = "datetime")]
    public DateTime? tm_group_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tm_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime tm_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tm_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime tm_update_datetime { get; set; }
}
