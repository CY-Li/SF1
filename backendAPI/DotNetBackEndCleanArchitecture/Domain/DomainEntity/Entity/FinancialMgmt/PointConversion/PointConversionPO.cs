using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Entity.FinancialMgmt.PointConversion
{
    public class PostPointConversionPO
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
        /// 註冊點數
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        public int mw_registration { get; set; } = 0;

        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long tr_id { get; set; } = 0;
    }

    public class PostGiftPointPO
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long mm_id { get; set; } = 0; 

        public long mm_invite_code { get; set; } = 0;

        /// <summary>
        /// 第二組密碼，下單用
        /// </summary>
        //[StringLength(100)]
        public string mm_2nd_hash_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 註冊點數
        /// </summary>
        /// [Column(TypeName = "int(15)")]
        public int mw_registration { get; set; } = 0;

        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long tr_id { get; set; } = 0;
    }
}
