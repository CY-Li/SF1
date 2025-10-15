using DomainEntity.Common;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Entity.FinancialMgmt.MemberBalance
{
    public class QueryMemberBalanceAdminDTO : BaseGridReqDTO
    {
        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [RegularExpression("^09\\d{8}$", ErrorMessage = "帳號限定輸入10位數字，開頭為用09。")]
        //[SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 這裡會寫入點數類型代號(對應system_parameters:sp_code)
        /// </summary>
        /// [StringLength(2)]
        [RegularExpression("^(11|12|13|14|15|16)$", ErrorMessage = "點數類型限定為11~16數字字串，可不輸入撈取全部(1個月內的)")]
        //[SwaggerSchema(Description = "點數類型")]
        public string mb_points_type { get; set; } = string.Empty;
    }

    public class QueryMemberBalanceDTO : BaseGridReqDTO
    {
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "ID is required")]
        //[SwaggerSchema(Description = "會員ID")]
        public long mb_mm_id { get; set; }

        /// <summary>
        /// 這裡會寫入點數類型代號(對應system_parameters:sp_code)
        /// </summary>
        /// [StringLength(2)]
        [RegularExpression("^(11|12|13|14|15|16)$", ErrorMessage = "點數類型限定為11~16數字字串，可不輸入撈取全部(1個月內的)")]
        //[SwaggerSchema(Description = "點數類型")]
        public string mb_points_type { get; set; } = string.Empty;
    }
}
