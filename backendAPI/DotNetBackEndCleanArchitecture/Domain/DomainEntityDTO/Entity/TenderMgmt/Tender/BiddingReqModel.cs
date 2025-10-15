using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.TenderMgmt.Tender
{
    /// <summary>
    /// 會員-下標
    /// </summary>
    public class BiddingReqModel
    {
        /// <summary>
        /// 第二組密碼，不存在該欄位
        /// </summary>
        [Required(ErrorMessage = "2 Password is required"), MinLength(4), MaxLength(15)]
        [SwaggerSchema(Description = "下單密碼")]
        public string mm_2nd_pwd { get; set; } = string.Empty;

        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "會員ID")]
        public long mm_id { get; set; }

        /// <summary>
        /// RSOCA 主檔ID (Rotating Savings and Credit Association ID_民間互助會
        /// </summary>
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "tm_id is required")]
        [SwaggerSchema(Description = "標案代號")]
        public long tm_id { get; set; }

        /// <summary>
        /// 一搬玩法可以選擇要下幾標
        /// </summary>
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [SwaggerSchema(Description = "一搬玩法可以選擇要下幾標")]
        public int tm_count { get; set; } = 0;
    }
}
