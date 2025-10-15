using DomainEntityDTO.Common;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.TenderMgmt.Tender
{
    public class QueryTenderAdminReqModel : BaseGridReqModel
    {
        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        [StringLength(10)]
        public string? mm_account { get; set; } = string.Empty;

        /// <summary>
        /// RSOCA 主檔ID (Rotating Savings and Credit Association ID_民間互助會
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long tm_id { get; set; } = 0;

        /// <summary>
        /// 標會種類:1-一般玩法(散會:可以選下標數)、2-1人玩法、3-2人玩法、3-4人玩法
        /// </summary>
        [StringLength(1)]
        public string tm_type { get; set; } = string.Empty;

        /// <summary>
        /// 是否成組:0-未成組,1-成組,2-結束
        /// </summary>
        [StringLength(1)]
        public string tm_status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 會員-取得進行中標案
    /// </summary>
    public class QueryTenderInProgressReqModel : BaseGridReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; }
    }
    /// <summary>
    /// 會員-取得標案
    /// </summary>
    public class GetTenderReqModel
    {
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; }
    }

    /// <summary>
    /// 會員-取得參與紀錄
    /// </summary>
    public class GetParticipationRecordReqModel
    {
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; }
    }

    /// <summary>
    /// 會員-取得標案紀錄
    /// </summary>
    public class GetTenderRecordReqModel
    {
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "ttr_tm_sn is required")]
        [SwaggerSchema(Description = "標案紀錄sn")]
        public long ttr_tm_sn { get; set; }
    }
}
