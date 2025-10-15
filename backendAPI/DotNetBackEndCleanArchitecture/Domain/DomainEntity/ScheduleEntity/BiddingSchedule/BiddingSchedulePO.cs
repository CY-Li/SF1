using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.ScheduleEntity.BiddingSchedule
{
    public class GetTarData
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long tar_id { get; set; } = 0;
        public long tar_mm_introduce_code { get; set; } = 0;

        /// <summary>
        /// 操作人(下標人、得標人)
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        public long tar_mm_id { get; set; } = 0;

        /// <summary>
        /// 這裡指的不是總數量，而是一次下標的會數
        /// </summary>
        public int tar_tm_count { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        public long tar_tm_id { get; set; } = 0;

        /// <summary>
        /// 交易類別:11-下標扣除押金(儲值、註冊) 、12-下標扣除額(儲值、註冊) 、13-下標紅利(紅利:下層下標時給的紅利(下標當下會先查找所有上層邀請人，透過排程寫入紅利)，tr_status:30)，下標紅利入儲值點數(紅利&gt;儲值:排程處理下層下標時給的紅利進儲值點數，tr_status:50) 、14-當期扣點(儲值點數(未死會):成組後扣除額) 、15-死會(紀錄10000，死會:紀錄10000(不進mb)) 、16-得標([紅利，包括押金:中標時新增紅利額度，tr_status:30]，[得標歸還押金(紅利&gt;儲值:中標後，排程歸還下標時押金(這部分不進交易紀錄))、得標入點(紅利&gt;儲值、註冊、商城:中標後，排程處理中標後入的三種點數)，tr_status:50]) 、17-贈與點數 、18-儲值點數 、19-提領點數 、20-轉換點數(儲值轉註冊)
        /// </summary>
        //[StringLength(2)]
        public string tar_tr_type { get; set; } = string.Empty;

        //[StringLength(20)]
        public string tar_tr_code { get; set; } = string.Empty;

        /// <summary>
        /// 標案活動紀錄狀態，0:未處理、1已經處理
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int tar_status { get; set; } = 0;

        /// <summary>
        /// 下標邀請組織列表(invitation_org)該欄位存放json值，供排成好處理、
        /// </summary>
        public string? tar_json { get; set; } = string.Empty;

        /// [Column(TypeName = "datetime")]
        public DateTime tar_create_datetime { get; set; }

        /// <summary>
        /// tr_type:13、16會需要看時間來結算所以會由大於這個時間的直接做結算會比較方便
        /// </summary>
        ///[Column(TypeName = "datetime")]
        public DateTime? tr_settlement_time { get; set; }
    }

    public class BiddingSkdGetMMConditionData
    {
        /// <summary>
        /// 紅利點數，system_parameters:12
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_reward { get; set; } = decimal.Zero;
    }

    public class BiddingSkdPoint
    {
        /// <summary>
        /// 紅利點數，system_parameters:12
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_reward { get; set; } = decimal.Zero;

        /// <summary>
        /// 紅利點數變動額
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_reward_chenge { get; set; } = decimal.Zero;
    }
}
