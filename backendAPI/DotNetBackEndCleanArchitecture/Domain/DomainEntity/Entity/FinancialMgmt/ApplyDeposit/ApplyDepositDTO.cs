using DomainEntity.Common;
using DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntity.Entity.FinancialMgmt.ApplyDeposit
{
    /// <summary>
    /// 後台-取得儲值申請列表
    /// </summary>
    public class QueryApplyDepositAdminDTO : BaseGridReqDTO
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
    public class QueryApplyDepositUserDTO : BaseGridReqDTO
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
        [SwaggerSchema(Description = "申請狀態")]
        public string ad_status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 會員-申請儲值(已付款要丟KEY)
    /// </summary>
    public class PostApplyDepositDTO: PostApplyDepositReqModel
    {
        /// <summary>
        /// 申請狀態:10-待會員儲值、11-已匯入儲值金、12-申請駁回、13-匯入失敗
        /// </summary>
        /// [StringLength(1)]
        //[Required(ErrorMessage = "ad_status is required")]
        public string ad_status { get; set; } = string.Empty;
        /// [Column(TypeName = "datetime")]
        public DateTime ad_create_datetime { get; set; }

        /// [Column(TypeName = "datetime")]
        public DateTime ad_update_datetime { get; set; }
    }

    /// <summary>
    /// 後台-申請儲值覆核
    /// </summary>
    public class PutApplyDepositDTO : PutApplyDepositReqModel
    {
        /// [Column(TypeName = "datetime")]
        public DateTime create_datetime { get; set; }
    }
}
