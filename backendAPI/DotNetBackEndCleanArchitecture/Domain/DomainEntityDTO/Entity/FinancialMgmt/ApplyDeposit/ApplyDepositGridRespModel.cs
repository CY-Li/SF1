using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit
{
    /// <summary>
    /// 後台-取得儲值申請列表
    /// </summary>
    public class QueryApplyDepositAdminRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "儲值申請ID")]
        public long ad_id { get; set; } = 0;

        /// <summary>
        /// 儲值人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "儲值人")]
        public long ad_mm_id { get; set; } = 0;

        /// <summary>
        /// 儲值金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        //[SwaggerSchema(Description = "儲值金額")]
        public int ad_amount { get; set; } = 0;

        /// <summary>
        /// 儲值後得到的KEY(要給我們才查的到)
        /// </summary>
        /// [StringLength(200)]
        //[SwaggerSchema(Description = "儲值序號")]
        public string ad_key { get; set; } = string.Empty;

        /// <summary>
        /// 儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
        /// </summary>
        /// [StringLength(200)]
        //[SwaggerSchema(Description = "儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)")]
        public string? ad_url { get; set; }

        /// <summary>
        /// 成功時貼上畫面所需儲存檔名
        /// </summary>
        /// [StringLength(200)]
        //[SwaggerSchema(Description = "成功時貼上畫面所需儲存檔名")]
        public string ad_file_name { get; set; } = string.Empty;

        /// <summary>
        /// 申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
        /// </summary>
        /// [StringLength(1)]
        //[SwaggerSchema(Description = "申請狀態")]
        public string ad_status { get; set; } = string.Empty;

        /// <summary>
        /// 儲值狀態:10-虛擬幣、50-銀行轉帳
        /// </summary>
        /// [StringLength(1)]
        //[SwaggerSchema(Description = "儲值狀態")]
        public string ad_type { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// [StringLength(1)]
        //[SwaggerSchema(Description = "")]
        public long ad_kyc_id { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        //[SwaggerSchema(Description = "申請時間")]
        public DateTime ad_create_datetime { get; set; }

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        //[StringLength(10)]
        public string mm_account { get; set; } = string.Empty;
    }

    /// <summary>
    /// 後台-取得儲值申請列表
    /// </summary>
    public class QueryApplyDepositUserRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "申請單ID")]
        public long ad_id { get; set; } = 0;

        /// <summary>
        /// 儲值金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        [SwaggerSchema(Description = "儲值金額")]
        public int ad_amount { get; set; } = 0;

        /// <summary>
        /// 儲值後得到的KEY(要給我們才查的到)
        /// </summary>
        /// [StringLength(200)]
        [SwaggerSchema(Description = "儲值序號")]
        public string ad_key { get; set; } = string.Empty;

        /// <summary>
        /// 申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
        /// </summary>
        /// [StringLength(1)]
        [SwaggerSchema(Description = "申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗")]
        public string ad_status { get; set; } = string.Empty;

        /// <summary>
        /// 儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
        /// </summary>
        /// [StringLength(200)]
        [SwaggerSchema(Description = "儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)")]
        public string? ad_url { get; set; }

        /// [Column(TypeName = "datetime")]
        [SwaggerSchema(Description = "申請時間")]
        public DateTime ad_create_datetime { get; set; }
    }
}
