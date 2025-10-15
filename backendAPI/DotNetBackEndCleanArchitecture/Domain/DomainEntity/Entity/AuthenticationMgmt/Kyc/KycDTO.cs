using DomainEntity.Common;
using DomainEntityDTO.Entity.AuthenticationMgmt.Kyc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Entity.AuthenticationMgmt.Kyc
{
    public class QueryKycAdminDTO : BaseGridReqDTO
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long akc_id { get; set; } = 0;

        /// <summary>
        /// 帳號
        /// </summary>
        [MaxLength(10)]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 申請狀態:10-待認證、20-認證通過、30-認證駁回、40-認證失敗
        /// </summary>
        [RegularExpression("^(?:10|20|30)?$", ErrorMessage = "申請狀態限定為10、20、30數字字串，可不輸入撈取全部")]
        public string akc_status { get; set; } = string.Empty;
    }

    public class PostKycDTO : PostKycReqModel
    {
        /// [Column(TypeName = "bigint(15)")]
        public long akc_create_member { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        public DateTime akc_create_datetime { get; set; }

        /// [Column(TypeName = "bigint(15)")]
        public long akc_update_member { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        public DateTime akc_update_datetime { get; set; }
    }

    public class PutKycDTO : PutKycReqModel
    {
        /// [Column(TypeName = "datetime")]
        public DateTime update_datetime { get; set; }
    }
}
