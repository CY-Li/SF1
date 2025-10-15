using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Entity.TenderMgmt.Tender
{
    public class WinningTenderGetTMTD
    {
        /// <summary>
        /// RSOCA 主檔ID (Rotating Savings and Credit Association ID_民間互助會
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long tm_id { get; set; } = 0;

        /// <summary>
        /// RSOCA 明細檔ID (Rotating Savings and Credit Association ID_民間互助
        /// </summary>
        ///[Key]
        ///[Column(TypeName = "bigint(15)")]
        public long td_id { get; set; } = 0;

        /// <summary>
        /// 排程已經處理的期數
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int tm_settlement_period { get; set; } = 0;

        /// <summary>
        /// 目前期數(成組時，當下期數，剛寫入就是1)
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int tm_current_period { get; set; } = 0;

        /// <summary>
        /// 是否成組:0-未成組,1-成組,2-結束
        /// </summary>
        //[StringLength(1)]
        public string tm_status { get; set; } = string.Empty;

        /// <summary>
        /// 參加者mm_id
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        public long td_participants { get; set; } = 0;

        /// <summary>
        /// 該標中標期數
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int td_period { get; set; } = 0;

        /// <summary>
        /// 下標狀態
        /// </summary>
        //[StringLength(1)]
        public string td_status { get; set; } = string.Empty;

        /// <summary>
        /// 平安點數，system_parameters:13
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_peace { get; set; } = 0;
    }
}
