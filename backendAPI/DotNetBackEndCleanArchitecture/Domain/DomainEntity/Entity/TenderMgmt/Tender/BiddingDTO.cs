using DomainEntityDTO.Entity.TenderMgmt.Tender;
using System.ComponentModel.DataAnnotations;

namespace DomainEntity.Entity.TenderMgmt.Tender
{
    /// <summary>
    /// 會員-下標
    /// </summary>
    public class BiddingDTO: BiddingReqModel
    {
        /// [Column(TypeName = "datetime")]
        public DateTime update_datetime { get; set; }

        /// <summary>
        /// 自製交易編號
        /// </summary>
        [StringLength(20)]
        public string tr_code { get; set; } = string.Empty;
    }
}
