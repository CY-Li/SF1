using System.ComponentModel.DataAnnotations;

namespace DomainEntity.ScheduleEntity.GroupDebitSchedule
{
    public class GroupDebitGetTM
    {
        /// <summary>
        /// RSOCA 主檔ID (Rotating Savings and Credit Association ID_民間互助會
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long tm_id { get; set; } = 0;

        /// <summary>
        /// RSOCA 明細檔ID (Rotating Savings and Credit Association ID_民間互助
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long td_id { get; set; } = 0;

        /// <summary>
        /// 排程已經處理的期數
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int tm_settlement_period { get; set; } = 0;

        /// <summary>
        /// 目前期數(成組時，當下期數，剛寫入就是1)
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int tm_current_period { get; set; } = 0;

        /// <summary>
        /// 排程查資料查過這個時間就要處理
        /// </summary>
        ///[Column(TypeName = "datetime")]
        public DateTime tm_settlement_datetime { get; set; }

        /// <summary>
        /// 參加者mm_id
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        public long td_participants { get; set; } = 0;

        /// <summary>
        /// 標案明細序列(用來方便做查找)
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int td_sequence { get; set; } = 0;

        /// <summary>
        /// 該標中標期數
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int td_period { get; set; } = 0;

        /// <summary>
        /// 如果成組後每期扣錢沒扣到就會寫入pending_payment，有可能重複欠款|分隔
        /// </summary>
        [StringLength(367)]
        public string td_pp_id { get; set; } = string.Empty;

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
        /// 死會點數，system_parameters:16
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_death { get; set; } = decimal.Zero;
    }

    public class GroupDebitInsertPoint
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
        /// 死會點數，system_parameters:16
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_death { get; set; } = decimal.Zero;

        /// <summary>
        /// 死會點數變動額
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_death_chenge { get; set; } = decimal.Zero;
    }
}
