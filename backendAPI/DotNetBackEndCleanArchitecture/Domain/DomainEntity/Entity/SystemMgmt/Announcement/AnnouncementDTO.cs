using DomainEntity.Common;
using DomainEntityDTO.Common;
using DomainEntityDTO.Entity.SystemMgmt.Announcement;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntity.Entity.SystemMgmt.Announcement
{
    /// <summary>
    /// 後台-取得公告列表
    /// </summary>
    public class QueryAnnBoardAdminDTO : BaseGridReqDTO
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long ab_id { get; set; } = 0;
        /// <summary>
        /// 標題
        /// </summary>
        //[StringLength(30)]
        [MaxLength(30)]
        //[SwaggerSchema(Description = "公告標題")]
        public string ab_title { get; set; } = string.Empty;
    }

    /// <summary>
    /// 會員-取得公告列表
    /// </summary>
    public class QueryAnnBoardUserDTO : BaseGridReqDTO
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long ab_id { get; set; } = 0;
        /// <summary>
        /// 標題
        /// </summary>
        //[StringLength(30)]
        [MaxLength(30)]
        [SwaggerSchema(Description = "公告標題")]
        public string ab_title { get; set; } = string.Empty;
    }

    public class PostAnnouncementDTO : PostAnnouncementReqModel
    {
        public long ab_id { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        public DateTime update_datetime { get; set; }
    }

    public class PutAnnouncementDTO : PutAnnouncementReqModel
    {
        /// [Column(TypeName = "datetime")]
        public DateTime update_datetime { get; set; }
    }
}
