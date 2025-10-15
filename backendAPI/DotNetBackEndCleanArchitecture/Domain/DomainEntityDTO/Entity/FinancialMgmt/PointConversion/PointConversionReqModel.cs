using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Entity.FinancialMgmt.PointConversion
{
    public class PostPointConversionReqModel
    {
        /// <summary>
        /// 轉換人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "轉換人ID")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 轉換金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        public int amount { get; set; }
    }

    public class PostGiftPointReqModel
    {
        /// <summary>
        /// 轉換人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        [SwaggerSchema(Description = "贈與人ID")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 第二組密碼，不存在該欄位
        /// </summary>
        [Required(ErrorMessage = "2 Password is required"), MinLength(4), MaxLength(15)]
        [SwaggerSchema(Description = "下單密碼")]
        public string mm_2nd_pwd { get; set; } = string.Empty;

        [Required(ErrorMessage = "recipient is required")]
        [SwaggerSchema(Description = "接收人ID")]
        public string recipient { get; set; } = null!;
        /// <summary>
        /// 轉換金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        public int amount { get; set; }
    }
}
