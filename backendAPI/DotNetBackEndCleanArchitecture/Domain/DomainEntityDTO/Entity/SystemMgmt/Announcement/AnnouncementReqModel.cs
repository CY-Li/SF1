using DomainEntityDTO.Common;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.SystemMgmt.Announcement
{
    /// <summary>
    /// 後台-取得公告列表
    /// </summary>
    public class QueryAnnBoardAdminReqModel : BaseGridReqModel
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

    /// <summary>
    /// 會員-取得公告列表
    /// </summary>
    public class QueryAnnBoardUserReqModel : BaseGridReqModel
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

    public class PostAnnouncementReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        public long mm_id { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        [Required(ErrorMessage = "ab_title is required")]
        [StringLength(30)]
        public string ab_title { get; set; } = null!;

        /// <summary>
        /// 內容
        /// </summary>
        [StringLength(300)]
        public string ab_content { get; set; } = string.Empty;

        public string ab_image_path { get; set; } = string.Empty;
        public string ab_image_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "ab_status is required")]
        [RegularExpression("^(?:10|20)?$", ErrorMessage = "狀態限定為10、20數字字串")]
        public string ab_status { get; set; } = string.Empty;

        public DateTime ab_datetime { get; set; }
    }

    public class PutAnnouncementReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        [Required(ErrorMessage = "mm_id is required")]
        public long mm_id { get; set; }

        [Required(ErrorMessage = "ab_id is required")]
        public long ab_id { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        [Required(ErrorMessage = "ab_title is required")]
        [StringLength(30)]
        public string ab_title { get; set; } = null!;

        /// <summary>
        /// 內容
        /// </summary>
        [StringLength(300)]
        public string ab_content { get; set; } = string.Empty;

        public string ab_image_path { get; set; } = string.Empty;
        public string ab_image_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "ab_status is required")]
        [RegularExpression("^(?:10|20)?$", ErrorMessage = "狀態限定為10、20數字字串")]
        public string ab_status { get; set; } = string.Empty;

        public DateTime ab_datetime { get; set; }
    }
}
