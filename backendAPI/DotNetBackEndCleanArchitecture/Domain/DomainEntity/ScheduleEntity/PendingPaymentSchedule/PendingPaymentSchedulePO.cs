namespace DomainEntity.ScheduleEntity.PendingPaymentSchedule
{
    public class PendingPaymentSchedulePO
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long pp_sn { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        public long pp_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        public long pp_mm_id { get; set; } = 0;

        //[StringLength(22)]
        public string pp_tr_code { get; set; } = string.Empty;

        /// [Column(TypeName = "bigint(15)")]
        public long pp_tm_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        public long pp_td_id { get; set; }= 0;

        /// [Column(TypeName = "int(15)")]
        public decimal pp_amount { get; set; } = 0;

        /// <summary>
        /// 超過時間繳費就要逞罰
        /// </summary>
        /// [Column(TypeName = "datetime")]
        public DateTime pp_penalty_datetime { get; set; }

        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        /// [Precision(15, 5)]
        public decimal mw_stored { get; set; } = decimal.Zero;
    }

    public class PendingPaymentSkdPoint
    {
        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        /// [Precision(15, 5)]
        public decimal mw_stored { get; set; } = decimal.Zero;

        /// <summary>
        /// 儲值點數變動額
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_stored_chenge { get; set; } = decimal.Zero;
    }
}
