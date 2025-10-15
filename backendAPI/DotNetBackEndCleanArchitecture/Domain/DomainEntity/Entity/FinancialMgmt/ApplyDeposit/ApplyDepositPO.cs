using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Entity.FinancialMgmt.ApplyDeposit
{
    /// <summary>
    /// 後台-申請儲值覆核
    /// </summary>
    public class PutApplyDepositPO
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_stored { get; set; } = 0;

        /// <summary>
        /// 儲值金額
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        public int ad_amount { get; set; } = 0;

        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long tr_id { get; set; } = 0;
    }
}
