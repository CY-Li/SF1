using DomainEntityDTO.Entity.FinancialMgmt.PointConversion;

namespace DomainEntity.Entity.FinancialMgmt.PointConversion
{
    /// <summary>
    /// 會員-點數轉換
    /// </summary>
    public class PostPointConversionDTO : PostPointConversionReqModel
    {
        /// [Column(TypeName = "datetime")]
        public DateTime update_datetime { get; set; }
    }

    /// <summary>
    /// 會員-點數贈送
    /// </summary>
    public class PostGiftPointDTO : PostGiftPointReqModel
    {
        /// [Column(TypeName = "datetime")]
        public DateTime update_datetime { get; set; }
    }
}
