using System.ComponentModel.DataAnnotations;

namespace DomainEntity.DBModels;

/// [Table("member_balance")]
/// [Index("mb_mm_id", Name = "INDEX_member_balance_1")]
/// [Index("mb_mm_id", "mb_points_type", Name = "INDEX_member_balance_2")]
public partial class member_balance
{
    /// [Key]
    /// [Column(TypeName = "bigint(15)")]
    public long mb_id { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long mb_mm_id { get; set; }

    /// <summary>
    /// 收款人帳號(註冊點數可以轉給其他人)
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long mb_payee_mm_id { get; set; }

    /// <summary>
    /// 標案主檔ID
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long mb_tm_id { get; set; }

    /// <summary>
    /// RSOCA 明細檔ID
    /// </summary>
    /// [Column(TypeName = "bigint(15)")]
    public long mb_td_id { get; set; }

    /// <summary>
    /// 點數流向:I-收入，O支出(寫入時全大寫，依照mm_id角度)
    /// </summary>
    [StringLength(1)]
    public string mb_income_type { get; set; } = null!;

    /// <summary>
    /// 自製交易編號
    /// </summary>
    [StringLength(20)]
    public string mb_tr_code { get; set; } = null!;

    /// <summary>
    /// 交易類別:11-下標扣除押金(儲值、註冊) 、12-下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)，tr_status:30)，下標紅利入儲值點數(紅利&gt;儲值:排程處理下層下標時給的紅利進儲值點數，tr_status:50) 、14-當期扣點(儲值點數(未死會):成組後扣除額) 、15-死會(紀錄10000，死會:紀錄10000(不進mb)) 、16-得標([紅利，包括押金:中標時新增紅利額度，tr_status:30]，[得標歸還押金(紅利&gt;儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄))、得標入點(紅利&gt;儲值、註冊、商城:中標後，排程處理中標後入的三種點數)，tr_status:50]) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
    /// </summary>
    [StringLength(2)]
    public string mb_tr_type { get; set; } = null!;

    /// <summary>
    /// 帳務類別:11-下標扣除押金(儲值、註冊) 、12下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)) 、14-下標紅利入儲值點數(紅利&gt;儲值:排程處理下層下標時給的紅利進儲值點數) 、15-當期扣點(儲值點數(未死會):成組後扣除額，死會:紀錄10000(不進mb)) 、16-得標紅利(紅利，包括押金:中標時新增紅利額度) 、17-得標歸還押金(紅利&gt;儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄)) 、18-得標入點(紅利&gt;儲值、註冊、商城:中標後，排程處理中標後入的三種點數) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
    /// </summary>
    [StringLength(2)]
    public string mb_type { get; set; } = null!;

    /// <summary>
    /// 這裡會寫入點數類型代號(對應system_parameters:sp_code)，11-儲值點數、12-紅利點數、13-平安點數、14-商城點數、15-註冊點數、16-死會點數
    /// </summary>
    [StringLength(2)]
    public string mb_points_type { get; set; } = null!;

    /// <summary>
    /// 點數異動額(點數種類多所以該筆資料會寫入點數類型)
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mb_change_points { get; set; }

    /// <summary>
    /// 點數異動後額度(點數種類多所以該筆資料會寫入點數類型))
    /// </summary>
    /// [Precision(15, 5)]
    public decimal mb_points { get; set; }

    /// <summary>
    /// 排程已經處理的期數，對應當下tm_settlement_period
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int mb_settlement_period { get; set; }

    /// <summary>
    /// 目前期數(成組時，當下期數，剛寫入就是1)，得標會依據該期數算錢，對應當下tm_current_period
    /// </summary>
    /// [Column(TypeName = "int(15)")]
    public int mb_current_period { get; set; }

    [StringLength(100)]
    public string? mb_remark { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long mb_create_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime mb_create_datetime { get; set; }

    /// [Column(TypeName = "bigint(15)")]
    public long mb_update_member { get; set; }

    /// [Column(TypeName = "datetime")]
    public DateTime mb_update_datetime { get; set; }
}
