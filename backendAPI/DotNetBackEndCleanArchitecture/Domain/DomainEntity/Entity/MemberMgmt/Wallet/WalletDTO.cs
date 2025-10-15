using DomainEntity.Common;

namespace DomainEntity.Entity.MemberMgmt.Wallet
{
    /// <summary>
    /// 後台-取得會員錢包列表
    /// </summary>
    public class QueryWalletDTO: BaseGridReqDTO
    {
        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        //[SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        //[SwaggerSchema(Description = "會員顯示名稱")]
        public string mm_name { get; set; } = string.Empty;
    }
}
