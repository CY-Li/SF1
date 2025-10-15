using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.DBModels
{
    /// [Table("announcement_board")]
    /// [Index("ab_id", Name = "INDEX_announcement_board_1", AllDescending = true)]
    public partial class announcement_board
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long ab_id { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        [StringLength(30)]
        public string ab_title { get; set; } = null!;

        /// <summary>
        /// 內容
        /// </summary>
        [StringLength(300)]
        public string ab_content { get; set; } = null!;

        public byte[]? ab_image { get; set; }

        /// <summary>
        /// 10:開啟，20:關閉
        /// </summary>
        [StringLength(2)]
        public string ab_status { get; set; } = null!;

        /// [Column(TypeName = "bigint(15)")]
        public DateTime ab_datetime { get; set; }

        /// [Column(TypeName = "bigint(15)")]
        public long ab_create_member { get; set; }

        /// [Column(TypeName = "datetime")]
        public DateTime ab_create_datetime { get; set; }

        /// [Column(TypeName = "datetime")]
        public long ab_update_datetime { get; set; }

        /// [Column(TypeName = "bigint(15)")]
        public DateTime ab_update_member { get; set; }
    }
}
