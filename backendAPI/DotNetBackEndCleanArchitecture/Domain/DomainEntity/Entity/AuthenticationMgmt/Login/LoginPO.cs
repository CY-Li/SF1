using DomainEntityDTO.Entity.AuthenticationMgmt.Login;

namespace DomainEntity.Entity.AuthenticationMgmt.Login
{
    public class LoginPO : LoginRespModel
    {
        /// <summary>
        /// 雜湊後密碼
        /// </summary>
        /// [StringLength(100)]
        public string mm_hash_pwd { get; set; } = null!;

        /// <summary>
        /// 貨幣種類
        /// </summary>
        /// [StringLength(10)]
        public string? mw_currency { get; set; }
    }
}
