using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.TenderMgmt.Tender
{
    /// <summary>
    /// 會員-取得標案
    /// </summary>
    public class GetTenderRespModel
    {
        /// <summary>
        /// RSOCA 主檔ID (Rotating Savings and Credit Association ID_民間互助會
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long tm_id { get; set; } = 0;

        /// <summary>
        /// 標會種類:1-一般玩法(散會:可以選下標數)、2-1人玩法、3-2人玩法、3-4人玩法
        /// </summary>
        //[StringLength(1)]
        public string tm_type { get; set; } = string.Empty;

        /// <summary>
        /// 下標人會用|串接，EX:1|2|3
        /// </summary>
        //[StringLength(150)]
        //public string? tm_bidder { get; set; }

        /// <summary>
        /// 得獎者會用|串接，EX:1|2|3
        /// </summary>
        //[StringLength(150)]
        //public string? tm_winners { get; set; }

        /// <summary>
        /// 可以下的組數
        /// </summary>
        public int bid_count { get; set; } = 0;

        /// <summary>
        /// 用來簡單判斷是否成組
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int tm_count { get; set; }

        public string tm_type_name { get; set; }

        public int has_participants { get; set; } = 0;
    }
    /// <summary>
    /// 會員-取得參與紀錄
    /// </summary>
    public class GetParticipationRecordRespModel
    {
        [SwaggerSchema(Description = "進行中活會")]
        public int self_available_quantity { get; set; } = 0;

        [SwaggerSchema(Description = "已參與會數")]
        public int all_quantity { get; set; } = 0;

        [SwaggerSchema(Description = "組織總活會")]
        public int self_org_available_quantity { get; set; } = 0;

        [SwaggerSchema(Description = ".組織總會數")]
        public int self_org_quantity { get; set; } = 0;
    }

    /// <summary>
    /// 會員-取得標案紀錄
    /// </summary>
    public class GetTenderRecordRespModel
    {
        //[SwaggerSchema(Description = "標案紀錄sn")]
        //public long ttr_sn { get; set; } = 0;

        [SwaggerSchema(Description = "標案主檔成組順序")]
        public long ttr_tm_sn { get; set; } = 0;

        [SwaggerSchema(Description = "標案紀錄標題")]
        public string ttr_title { get; set; } = string.Empty;

        [SwaggerSchema(Description = "紀錄明細1")]
        public string ttr_detail01 { get; set; } = string.Empty;

        [SwaggerSchema(Description = "紀錄明細2")]
        public string ttr_detail02 { get; set; } = string.Empty;

        [SwaggerSchema(Description = "紀錄明細3")]
        public string ttr_detail03 { get; set; } = string.Empty;

        [SwaggerSchema(Description = "紀錄明細4")]
        public string ttr_detail04 { get; set; } = string.Empty;

        //[SwaggerSchema(Description = "標案主檔編號")]
        //public long ttr_tm_id { get; set; } = 0;

        [SwaggerSchema(Description = "會首")]
        public string ttr_top { get; set; } = string.Empty;

        [SwaggerSchema(Description = "標會擁有者")]
        public string ttr_owner { get; set; } = string.Empty;

        [SwaggerSchema(Description = "電話")]
        public string ttr_phone { get; set; } = string.Empty;

        [SwaggerSchema(Description = "地址")]
        public string ttr_address { get; set; } = string.Empty;

        [SwaggerSchema(Description = "標案成組時間")]
        public DateTime ttr_tm_group_datetime { get; set; }

        [SwaggerSchema(Description = "標案紀錄明細")]
        public string ttr_detail { get; set; } = string.Empty;
    }

}
