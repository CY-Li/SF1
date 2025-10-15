using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.ScheduleEntity.SettlementRewardSchedule
{
    public class SettlementRewardPO
    {
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long tr_id { get; set; } = 0;

        /// <summary>
        /// 自製交易編號
        /// </summary>
        [StringLength(20)]
        public string tr_code { get; set; } = string.Empty;

        /// <summary>
        /// 下標人mm_id
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        public long tr_mm_id { get; set; } = 0;

        ///[Column(TypeName = "bigint(15)")]
        public long tr_tm_id { get; set; } = 0;

        /// <summary>
        /// 交易類別:11-下標扣除押金(儲值、註冊) 、12-下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)，tr_status:30)，下標紅利入儲值點數(紅利&gt;儲值:排程處理下層下標時給的紅利進儲值點數，tr_status:50) 、14-當期扣點(儲值點數(未死會):成組後扣除額) 、15-死會(紀錄10000，死會:紀錄10000(不進mb)) 、16-得標([紅利，包括押金:中標時新增紅利額度，tr_status:30]，[得標歸還押金(紅利&gt;儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄))、得標入點(紅利&gt;儲值、註冊、商城:中標後，排程處理中標後入的三種點數)，tr_status:50]) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
        /// </summary>
        [StringLength(2)]
        public string tr_type { get; set; } = null!;

        /// <summary>
        /// 交易狀態:30-待清算(入紅利點數欄位，tr_type:13、16) 、50-已清算(代表該交易最後狀態，tr_type:13、16) 、90-免清算(tr_type:11、12、14、15、17、18、19、20)
        /// </summary>
        [StringLength(2)]
        public string tr_status { get; set; } = string.Empty;

        /// <summary>
        /// tr_type:13、16會需要看時間來結算所以會由大於這個時間的直接做結算會比較方便
        /// </summary>
        ///[Column(TypeName = "datetime")]
        public DateTime? tr_settlement_time { get; set; }

        /// <summary>
        /// 儲值點數(每標8000在這邊錢會是固定的，死會後會補10000)
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal tr_mm_points { get; set; } = decimal.Zero;

        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_stored { get; set; } = decimal.Zero;

        /// <summary>
        /// 紅利點數，system_parameters:12
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_reward { get; set; } = decimal.Zero;

        /// <summary>
        /// 商城點數，system_parameters:14
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_mall { get; set; }
    }

    public class SettlementRewardInsertBalancePoint
    {
        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_stored { get; set; } = decimal.Zero;

        /// <summary>
        /// 儲值點數變動額
        /// </summary>
        public decimal mw_stored_chenge { get; set; } = decimal.Zero;

        /// <summary>
        /// 紅利點數，system_parameters:12
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_reward { get; set; } = decimal.Zero;

        /// <summary>
        /// 紅利點數變動額
        /// </summary>
        public decimal mw_reward_chenge { get; set; } = decimal.Zero;

        /// <summary>
        /// 商城點數，system_parameters:14
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_mall { get; set; } = decimal.Zero;

        /// <summary>
        /// 商城點數變動額
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_mall_change { get; set; } = decimal.Zero;
    }
}
