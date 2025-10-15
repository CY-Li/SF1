using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Entity.TenderMgmt.Tender
{
    public class WinningTenderReqModel
    {
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// RSOCA 明細檔ID (Rotating Savings and Credit Association ID_民間互助
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long td_id { get; set; } = 0;
    }
}
