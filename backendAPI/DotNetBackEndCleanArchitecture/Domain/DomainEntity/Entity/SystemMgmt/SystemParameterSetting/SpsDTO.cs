using DomainEntity.Common;
using DomainEntityDTO.Entity.SystemMgmt.SystemParameterSetting;
using System.ComponentModel.DataAnnotations;

namespace DomainEntity.Entity.SystemMgmt.SystemParameterSetting
{
    public class QuerySpsDTO : BaseGridReqDTO
    {
        /// <summary>
        /// 設定編號
        /// </summary>
        [StringLength(25)]
        public string sps_code { get; set; } = string.Empty;

        /// <summary>
        /// 設定名稱
        /// </summary>
        [StringLength(25)]
        public string sps_name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 後台-新增參數
    /// </summary>
    public class PostSpsDTO : PostSpsReqModel
    {
        /// [Column(TypeName = "bigint(15)")]
        public long sps_create_member { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        public DateTime sps_create_datetime { get; set; }

        /// [Column(TypeName = "bigint(15)")]
        public long sps_update_member { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        public DateTime sps_update_datetime { get; set; }
    }

    public class PutSpsDTO: PutSpsReqModel
    {
        public DateTime update_datetime { get; set; }
    }
}
