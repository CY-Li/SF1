using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Entity.HomeMgmt
{
    public class GetHomeVideoRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long sps_id { get; set; } = 0;

        public string video_url1 { get; set; } = string.Empty;

        public string video_url2 { get; set; } = string.Empty;

        public string video_url3 { get; set; } = string.Empty;

        public string video_url4 { get; set; } = string.Empty;

        public string video_url5 { get; set; } = string.Empty;
    }
}
