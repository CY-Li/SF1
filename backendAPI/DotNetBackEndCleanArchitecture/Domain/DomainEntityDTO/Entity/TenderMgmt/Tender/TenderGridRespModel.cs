using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.TenderMgmt.Tender
{
    public class QueryTenderRespModel
    {
        /// <summary>
        /// RSOCA 主檔ID (Rotating Savings and Credit Association ID_民間互助會
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long tm_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        public long tm_sn { get; set; } = 0;

        /// <summary>
        /// 發起人mm_id
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        //public long tm_initiator_mm_id { get; set; } = 0;

        /// <summary>
        /// 標會種類:1-一般玩法(散會:可以選下標數)、2-1人玩法、3-2人玩法、3-4人玩法
        /// </summary>
        //[StringLength(1)]
        //public string tm_type { get; set; } = string.Empty;

        public string tm_type_name { get; set; } = string.Empty;

        /// <summary>
        /// 下標人會用|串接，EX:1|2|3
        /// </summary>
        //[StringLength(150)]
        //public string tm_bidder { get; set; } = string.Empty;

        /// <summary>
        /// 得獎者會用|串接，EX:1|2|3
        /// </summary>
        //[StringLength(150)]
        public string tm_winners { get; set; } = string.Empty;

        /// <summary>
        /// 目前期數(成組時，當下期數，剛寫入就是1)
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        //public int tm_current_period { get; set; } = 0;

        /// <summary>
        /// 是否成組:0-未成組,1-成組,2-結束
        /// </summary>
        //[StringLength(1)]
        public string tm_status { get; set; } = string.Empty;

        /// <summary>
        /// 用來簡單判斷是否成組
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        //public int tm_count { get; set; } = 0;

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

        public List<TenderDetailGridRespModel> TenderDetail { get; set; } = new();
    }

    public class TenderDetailGridRespModel
    {
        /// <summary>
        /// RSOCA 明細檔ID (Rotating Savings and Credit Association ID_民間互助
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long td_id { get; set; } = 0;

        /// <summary>
        /// 參加者mm_id
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        //public long td_participants { get; set; } = 0;

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
        /// 下標狀態
        /// </summary>
        //[StringLength(1)]
        //public string td_status { get; set; } = string.Empty;


        ///[Column(TypeName = "datetime")]
        //public DateTime td_update_datetime { get; set; }

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        //[StringLength(10)]
        public string mm_account { get; set; } = string.Empty;
    }

    public class QueryTenderInProgressRespModel
    {
        /// [Column(TypeName = "bigint(15)")]
        public long tm_sn { get; set; } = 0;

        public string tm_type_name { get; set; } = string.Empty;

        public int has_participants { get; set; } = 0;
    }

    public class QueryTenderAllUserRespModel
    {
        [SwaggerSchema(Description = "成組序號")]
        public long tm_sn { get; set; } = 0;

        [SwaggerSchema(Description = "成組顯示內容")]
        public string tm_type_name { get; set; } = string.Empty;

        [SwaggerSchema(Description = "所有標會:(參與會數)")] 
        public int self_available_quantity { get; set; } = 0;

        [SwaggerSchema(Description = "進行中: (參與中活會)")]
        public int self_all_quantity { get; set; } = 0;
    }

    public class QueryTenderParticipatedUserRespModel
    {
        [SwaggerSchema(Description = "成組序號")]
        public long tm_sn { get; set; } = 0;

        [SwaggerSchema(Description = "成組顯示內容")]
        public string tm_type_name { get; set; } = string.Empty;

        [SwaggerSchema(Description = "參與總會數:(傘下參與的總會數)")]
        public int self_org_available_quantity { get; set; } = 0;

        [SwaggerSchema(Description = "進行中: (傘下參與中剩餘的活會)")]
        public int self_org_all_quantity { get; set; } = 0;
    }
}
