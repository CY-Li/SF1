using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Shared
{
    public class GetLevleDataDTO
    {
        public string mm_account { get; set; } = string.Empty;
        public string mm_invite_code { get; set; } = string.Empty;
        public int mw_subscripts_count { get; set; } = 0;
        public int class_level { get; set; } = 0;
        public int total_qty { get; set; } = 0;
        public string class2_mm_account { get; set; } = string.Empty;
        public int class2_qty { get; set; } = 0;
        public string class3_mm_account { get; set; } = string.Empty;
        public int class3_qty { get; set; } = 0;
        public string class4_mm_account { get; set; } = string.Empty;
        public int class4_qty { get; set; } = 0;
        public string class5_mm_account { get; set; } = string.Empty;
        public int class5_qty { get; set; } = 0;
        public string class6_mm_account { get; set; } = string.Empty;
        public int class6_qty { get; set; } = 0;
    }
}
