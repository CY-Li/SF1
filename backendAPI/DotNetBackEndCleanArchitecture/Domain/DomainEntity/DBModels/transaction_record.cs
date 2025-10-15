using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("transaction_record")]
/// [Index("tr_mm_id", Name = "INDEX_transaction_record_1")]
/// [Index("tr_mm_id", "tr_type", Name = "INDEX_transaction_record_2", IsDescending = new[] { false, true })]
public partial class transaction_record
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long tr_id { get; set; }

    /// <summary>
    /// 自製交易編號
    /// </summary>
    [StringLength(22)]
    public string tr_code { get; set; } = null!;

    /// <summary>
    /// 下標人mm_id
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long tr_mm_id { get; set; }

    /// <summary>
    /// 收款人帳號(註冊點數可以轉給其他人)
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long tr_payee_mm_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tr_tm_id { get; set; }

    /// <summary>
    /// RSOCA 明細檔ID
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long tr_td_id { get; set; }

    /// <summary>
    /// 如果成組後每期扣錢沒扣到就會寫入pending_payment
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long tr_pp_id { get; set; }

    /// <summary>
    /// 交易類別:11-下標扣除押金(儲值、註冊) 、12-下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)，tr_status:30)，下標紅利入儲值點數(紅利&gt;儲值:排程處理下層下標時給的紅利進儲值點數，tr_status:50) 、14-當期扣點(儲值點數(未死會):成組後扣除額) 、15-死會(紀錄10000，死會:紀錄10000(不進mb)) 、16-得標([紅利，包括押金:中標時新增紅利額度，tr_status:30]，[得標歸還押金(紅利&gt;儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄))、得標入點(紅利&gt;儲值、註冊、商城:中標後，排程處理中標後入的三種點數)，tr_status:50]) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
    /// </summary>
    [StringLength(2)]
    public string tr_type { get; set; } = null!;

    /// <summary>
    /// 交易狀態:30-待清算(入紅利點數欄位，tr_type:13、16) 、50-已清算(代表該交易最後狀態，tr_type:13、16) 、90-免清算(tr_type:11、12、14、15、17、18、19、20)
    /// </summary>
    [StringLength(2)]
    public string tr_status { get; set; } = null!;

    /// <summary>
    /// tr_type:13、16會需要看時間來結算所以會由大於這個時間的直接做結算會比較方便
    /// </summary>
    /// [Column(TypeName = "datetime")]
    public DateTime? tr_settlement_time { get; set; }

    /// <summary>
    /// 儲值點數(每標8000在這邊錢會是固定的，死會後會補10000)
    /// </summary>
    /// [Precision(15, 5)]
    public decimal tr_mm_points { get; set; }

    /// <summary>
    /// 點數流向:I-收入，O支出(寫入時全大寫，依照mm_id角度)
    /// </summary>
    [StringLength(1)]
    public string tr_income_type { get; set; } = null!;

    /// <summary>
    /// 排程已經處理的期數，對應當下tm_settlement_period
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int tr_settlement_period { get; set; }

    /// <summary>
    /// 目前期數(成組時，當下期數，剛寫入就是1)，得標會依據該期數算錢，對應當下tm_current_period
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int tr_current_period { get; set; }

    [StringLength(100)]
    public string? tr_remark { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tr_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime tr_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long tr_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime tr_update_datetime { get; set; }
}
