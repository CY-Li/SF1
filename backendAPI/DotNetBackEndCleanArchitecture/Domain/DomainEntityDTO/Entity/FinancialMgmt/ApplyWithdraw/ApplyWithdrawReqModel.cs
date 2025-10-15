using DomainEntityDTO.Common;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.FinancialMgmt.ApplyWithdraw
{
    /// <summary>
    /// 後台-取得提領申請列表
    /// </summary>
    public class QueryApplyWithdrawAdminReqModel : BaseGridReqModel
    {
        //[SwaggerSchema(Description = "申請狀態")]
        /// <summary>
        /// 申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗
        /// </summary>
        //[StringLength(2)]
        [RegularExpression("^(?:10|11|12|13)?$", ErrorMessage = "申請狀態限定為10~13數字字串，可不輸入撈取全部")]
        public string aw_status { get; set; } = null!;

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
    public class QueryApplyWithdrawUserReqModel : BaseGridReqModel
    {
        /// <summary>
        /// 儲值人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "提領人ID")]
        public long aw_mm_id { get; set; } = 0;

        //[SwaggerSchema(Description = "申請狀態")]
        /// <summary>
        /// 申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗
        /// </summary>
        //[StringLength(2)]
        [RegularExpression("^(?:10|11|12|13)?$", ErrorMessage = "申請狀態限定為10~13數字字串，可不輸入撈取全部")]
        [SwaggerSchema(Description = "申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗")]
        public string aw_status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 會員-申請提領
    /// </summary>
    public class PostApplyWithdrawReqModel
    {
        /// <summary>
        /// 提領人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "aw_mm_id is required")]
        [SwaggerSchema(Description = "儲值人ID")]
        public long aw_mm_id { get; set; } = 0;

        /// <summary>
        /// 儲值金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        public int aw_amount { get; set; }

        /// <summary>
        /// 提領錢包地址
        /// </summary>
        //[Required(ErrorMessage = "aw_wallet_address is required")]
        [StringLength(200)]
        public string? aw_wallet_address { get; set; }

        /// <summary>
        /// 儲值成功圖片
        /// </summary>
        //[StringLength(100)]
        //public string? aw_file_name { get; set; }

        public long aw_kyc_id { get; set; }

        //[SwaggerSchema(Description = "儲值成功圖片")]
        //public IFormFile? aw_file_image { get; set; }
        //public Guid ImageGuid = Guid.NewGuid();

        //[Required(ErrorMessage = "aw_type is required")]
        [SwaggerSchema(Description = "提領類別，10:虛擬幣、50:銀行轉帳")]
        public string aw_type { get; set; } = string.Empty;


    }

    /// <summary>
    /// 後台-申請提領覆核
    /// </summary>
    public class PutApplyWithdrawReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        /// 覆核人員
        [Required(ErrorMessage = "ID is required")]
        public long mm_id { get; set; } = 0;

        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "ID is required")]
        public long aw_id { get; set; } = 0;

        /// <summary>
        /// 儲值人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "ID is required")]
        public long aw_mm_id { get; set; } = 0;

        //[SwaggerSchema(Description = "申請狀態")]
        /// <summary>
        /// 申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗
        /// </summary>
        //[StringLength(2)]
        [RegularExpression("^(?:10|11|12|13)?$", ErrorMessage = "申請狀態限定為10~13數字字串，可不輸入撈取全部")]
        [SwaggerSchema(Description = "申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗")]
        public string aw_status { get; set; } = null!;
    }
}
