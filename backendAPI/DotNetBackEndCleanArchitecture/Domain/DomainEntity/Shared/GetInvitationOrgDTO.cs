using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Shared
{
    public class GetInvitationOrgDTO
    {
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 邀請碼-Invitation code
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        public long mm_invite_code { get; set; } = 0;
        public int class_level { get; set; } = 0;
        public int invite_level { get; set; } = 0;

        /// <summary>
        /// 這是為了算出前後給紅利
        /// </summary>
        public int invite_bonus { get; set; } = 0;
    }
}
