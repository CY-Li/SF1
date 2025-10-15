using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.SystemMgmt.Announcement
{
    /// <summary>
    /// 後台-取得公告圖片儲存位置
    /// </summary>
    public class GetAnnouncementPathRespModel
    {
        /// [StringLength(30)]
        public string? sps_parameter01 { get; set; }

        /// [StringLength(30)]
        public string? sps_parameter02 { get; set; }

        /// [StringLength(30)]
        public string? sps_parameter03 { get; set; }

        /// [StringLength(30)]
        public string? sps_parameter04 { get; set; }
    }
}
