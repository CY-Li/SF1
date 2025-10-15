using DomainEntity.Common;
using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyWithdraw;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntity.Entity.FinancialMgmt.ApplyWithdraw
{
    /// <summary>
    /// 後台-取得提領申請列表
    /// </summary>
    public class QueryApplyWithdrawAdminDTO : BaseGridReqDTO
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
    public class QueryApplyWithdrawUserDTO : BaseGridReqDTO
    {
        /// <summary>
        /// 提領人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "aw_mm_id is required")]
        [SwaggerSchema(Description = "提領人ID")]
        public long aw_mm_id { get; set; } = 0;

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
    public class PostApplyWithdrawDTO : PostApplyWithdrawReqModel
    {
        /// <summary>
        /// 申請狀態:10-待管理者付款、11-已匯入會員錢包、12-申請駁回、13-匯入失敗
        /// </summary>
        [StringLength(2)]
        public string aw_status { get; set; } = string.Empty;

        /// [Column(TypeName = "datetime")]
        public DateTime aw_create_datetime { get; set; }

        /// [Column(TypeName = "datetime")]
        public DateTime aw_update_datetime { get; set; }
    }

    /// <summary>
    /// 後台-申請提領覆核
    /// </summary>
    public class PutApplyWithdrawDTO : PutApplyWithdrawReqModel
    {
        /// [Column(TypeName = "datetime")]
        public DateTime create_datetime { get; set; }
    }
}
