using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Entity.SystemMgmt.Announcement
{
    public class QueryAnnBoardAdminRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long ab_id { get; set; } = 0;

        /// <summary>
        /// 標題
        /// </summary>
        //[StringLength(30)]
        public string ab_title { get; set; } = string.Empty;

        /// <summary>
        /// 內容
        /// </summary>
        //[StringLength(300)]
        public string ab_content { get; set; } = string.Empty;

        /// <summary>
        /// 10:開啟，20:關閉
        /// </summary>
        //[StringLength(2)]
        public string ab_status { get; set; } = string.Empty;

        public string ab_image_url { get; set; } = string.Empty;
        public string ab_image_path { get; set; } = string.Empty;
        public string ab_image_name { get; set; } = string.Empty;

        /// [Column(TypeName = "bigint(15)")]
        public DateTime ab_datetime { get; set; }
    }

    public class QueryAnnBoardUserRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long ab_id { get; set; } = 0;

        /// <summary>
        /// 標題
        /// </summary>
        //[StringLength(30)]
        public string ab_title { get; set; } = string.Empty;

        /// <summary>
        /// 內容
        /// </summary>
        //[StringLength(300)]
        public string ab_content { get; set; } = string.Empty;

        /// <summary>
        /// 10:開啟，20:關閉
        /// </summary>
        //[StringLength(2)]
        //public string ab_status { get; set; } = string.Empty;

        public string ab_image_url { get; set; } = string.Empty;
        //public string ab_image_path { get; set; } = string.Empty;
        public string ab_image_name { get; set; } = string.Empty;

        /// [Column(TypeName = "bigint(15)")]
        public DateTime ab_datetime { get; set; }
    }
}
