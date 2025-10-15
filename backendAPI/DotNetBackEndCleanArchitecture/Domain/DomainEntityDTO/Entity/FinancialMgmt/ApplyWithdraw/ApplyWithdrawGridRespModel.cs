using Swashbuckle.AspNetCore.Annotations;

namespace DomainEntityDTO.Entity.FinancialMgmt.ApplyWithdraw
{
    /// <summary>
    /// 後台-取得儲值申請列表
    /// </summary>
    public class QueryApplyWithdrawAdminRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "儲值申請ID")]
        public long aw_id { get; set; } = 0;

        /// <summary>
        /// 提領人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "儲值人")]
        public long aw_mm_id { get; set; } = 0;

        /// <summary>
        /// 提領金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        //[SwaggerSchema(Description = "儲值金額")]
        public int aw_amount { get; set; } = 0;

        /// <summary>
        /// 儲值後得到的KEY(要給我們才查的到)
        /// </summary>
        /// [StringLength(200)]
        //[SwaggerSchema(Description = "儲值序號")]
        public string aw_key { get; set; } = string.Empty;

        /// <summary>
        /// 申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗
        /// </summary>
        //[StringLength(2)]
        public string aw_status { get; set; } = string.Empty;

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        //[StringLength(10)]
        public string mm_account { get; set; } = string.Empty;
    }

    /// <summary>
    /// 後台-取得儲值申請列表
    /// </summary>
    public class QueryApplyWithdrawUserRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "申請單ID")]
        public long aw_id { get; set; } = 0;

        /// <summary>
        /// 提領金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        [SwaggerSchema(Description = "儲值金額")]
        public decimal aw_amount { get; set; } = decimal.Zero;

        /// <summary>
        /// 儲值後得到的KEY(要給我們才查的到)
        /// </summary>
        /// [StringLength(200)]
        [SwaggerSchema(Description = "儲值序號")]
        public string aw_key { get; set; } = string.Empty;

        /// <summary>
        /// 申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗
        /// </summary>
        //[StringLength(2)]
        [SwaggerSchema(Description = "申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗")]
        public string aw_status { get; set; } = string.Empty;

        /// [Column(TypeName = "datetime")]
        [SwaggerSchema(Description = "申請時間")]
        public DateTime aw_create_datetime { get; set; }
    }
}
