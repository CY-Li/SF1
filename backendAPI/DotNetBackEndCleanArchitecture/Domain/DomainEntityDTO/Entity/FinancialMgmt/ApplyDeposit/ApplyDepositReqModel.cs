using DomainEntityDTO.Common;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit
{
    /// <summary>
    /// 後台-取得儲值申請列表
    /// </summary>
    public class QueryApplyDepositAdminReqModel : BaseGridReqModel
    {
        /// <summary>
        /// 申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
        /// </summary>
        /// [StringLength(1)]
        [RegularExpression("^(?:10|11|12|13)?$", ErrorMessage = "申請狀態限定為10~13數字字串，可不輸入撈取全部")]
        //[SwaggerSchema(Description = "申請狀態")]
        public string ad_status { get; set; } = string.Empty;

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [RegularExpression("^09\\d{8}$", ErrorMessage = "帳號限定輸入10位數字，開頭為用09。")]
        //[SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        [MaxLength(30)]
        [SwaggerSchema(Description = "會員顯示名稱")]
        public string mm_name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 會員-取得儲值申請列表
    /// </summary>
    public class QueryApplyDepositUserReqModel : BaseGridReqModel
    {
        /// <summary>
        /// 儲值人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "儲值人ID")]
        public long ad_mm_id { get; set; } = 0;

        /// <summary>
        /// 申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
        /// </summary>
        /// [StringLength(1)]
        [RegularExpression("^(?:10|11|12|13)?$", ErrorMessage = "申請狀態限定為10~13數字字串，可不輸入撈取全部")]
        [SwaggerSchema(Description = "申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗")]
        public string ad_status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 會員-申請儲值(已付款要丟KEY)
    /// </summary>
    public class PostApplyDepositReqModel
    {
        /// <summary>
        /// 儲值人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "ad_mm_id is required")]
        [SwaggerSchema(Description = "儲值人ID")]
        public long ad_mm_id { get; set; } = 0;

        /// <summary>
        /// 儲值金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        [Required(ErrorMessage = "amount is required")]
        [SwaggerSchema(Description = "儲值金額")]
        public int ad_amount { get; set; } = 0;

        /// <summary>
        /// 儲值後得到的KEY(要給我們才查的到)
        /// </summary>
        /// [StringLength(200)]
        [MaxLength(200)]
        [SwaggerSchema(Description = "儲值後得到的KEY(要給我們才查的到)")]
        public string? ad_key { get; set; } = string.Empty;

        /// <summary>
        /// 儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)
        /// </summary>
        /// [StringLength(200)]
        [MaxLength(200)]
        [SwaggerSchema(Description = "儲值網址(現在為流水號，利用QR CODE後需要輸入該流水號才能儲值)")]
        public string? ad_url { get; set; } = string.Empty;

        /// <summary>
        /// 圖片(會員儲值會需要QR CODE故這邊新增該欄位)
        /// </summary>
        /// [Column(TypeName = "blob")]
        [SwaggerSchema(Description = "QR CODE")]
        public byte[]? ad_picture { get; set; }


        [SwaggerSchema(Description = "虛擬幣錢包")]
        public string? ad_akc_mw_address { get; set; }

        [Required(ErrorMessage = "ad_type is required")]
        [SwaggerSchema(Description = "儲值類別，10:虛擬幣、50:銀行轉帳")]
        public string ad_type { get; set; } = string.Empty;

        /// <summary>
        /// 儲值成功圖片
        /// </summary>
        //[StringLength(100)]
        public string? ad_file_name { get; set; }

        public long ad_kyc_id { get; set; }

        [SwaggerSchema(Description = "儲值成功圖片")]
        public IFormFile? ad_file_image { get; set; }
        public Guid ImageGuid = Guid.NewGuid();
    }

    /// <summary>
    /// 後台-申請儲值覆核
    /// </summary>
    public class PutApplyDepositReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        /// 覆核人員
        [Required(ErrorMessage = "ID is required")]
        public long mm_id { get; set; } = 0;

        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "ID is required")]
        public long ad_id { get; set; } = 0;

        /// <summary>
        /// 儲值人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "ID is required")]
        public long ad_mm_id { get; set; } = 0;

        /// <summary>
        /// 申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
        /// </summary>
        /// [StringLength(1)]
        [Required(ErrorMessage = "ad_status is required")]
        [RegularExpression("^(11|12|13)$", ErrorMessage = "申請狀態限定為11~13數字字串")]
        public string ad_status { get; set; } = string.Empty;
    }
}
