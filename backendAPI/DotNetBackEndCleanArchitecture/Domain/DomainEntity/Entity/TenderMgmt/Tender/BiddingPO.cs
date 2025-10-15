using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Entity.TenderMgmt.Tender
{
    public class BiddingGetInvitationOrg
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
        public string invite_level { get; set; } = string.Empty;
    }
    public class BiddingGetTenderDataPO
    {
        /// <summary>
        /// 標會種類:1-一般玩法(散會:可以選下標數)、2-1人玩法、3-2人玩法、3-4人玩法
        /// </summary>
        //[StringLength(1)]
        public string tm_type { get; set; } = string.Empty;

        public int has_participants { get; set; } = 0;

        /// <summary>
        /// 用來簡單判斷是否成組
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int tm_count { get; set; } = 0;

        /// <summary>
        /// 介紹人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        public long tar_mm_introduce_code { get; set; } = 0;

        /// <summary>
        /// 邀請人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        public long tar_mm_invite_code { get; set; } = 0;
    }

    public class BiddingGetTenderConditionData
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
        /// 參加者mm_id
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        public long td_participants { get; set; } = 0;

        /// <summary>
        /// 標案明細序列(用來方便做查找)
        /// </summary>
        ///[Column(TypeName = "int(15)")]
        public int td_sequence { get; set; } = 0;
    }

    public class BiddingGetMMConditionData
    {
        /// <summary>
        /// 儲值點數(可用點數)，system_parameters:11
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_stored { get; set; } = decimal.Zero;

        /// <summary>
        /// 紅利點數，system_parameters:12
        /// </summary>
        ////// [Precision(15, 5)]
        public decimal mw_reward { get; set; } = decimal.Zero;

        /// <summary>
        /// 第二組密碼，下單用
        /// </summary>
        //[StringLength(100)]
        public string mm_2nd_hash_pwd { get; set; } = string.Empty;
    }
}
