using DomainEntity.Common;
using DomainEntityDTO.Entity.MemberMgmt.Member;
using System.ComponentModel.DataAnnotations;

namespace DomainEntity.Entity.MemberMgmt.Member
{
    /// <summary>
    /// 後台-取得會員列表
    /// </summary>
    public class QueryMemberDTO : BaseGridReqDTO
    {
        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        /// [StringLength(10)]
        [RegularExpression("^09\\d{8}$", ErrorMessage = "帳號限定輸入10位數字，開頭為用09。")]
        //[SwaggerSchema(Description = "帳號")]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        [MaxLength(30)]
        //[SwaggerSchema(Description = "會員名稱")]
        public string mm_name { get; set; } = string.Empty;

        /// <summary>
        /// 介紹人
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "介紹人")]
        //public long mm_introduce_code { get; set; } = 0;
        public string mm_introduce_user { get; set; } = string.Empty;

        /// <summary>
        /// 邀請碼-Invitation code
        /// </summary>
        /// [Column(TypeName = "bigint(15)")]
        //[SwaggerSchema(Description = "邀請碼")]
        //public long mm_invite_code { get; set; } = 0;
        public string mm_invite_user { get; set; } = string.Empty;
    }

    /// <summary>
    /// 會員、後台-更新會員資料
    /// </summary>
    public class PutMemberDTO : PutMemberReqModel
    {
        /// <summary>
        /// 雜湊後密碼
        /// </summary>
        /// [StringLength(100)]
        //public string? mm_hash_pwd { get; set; } = string.Empty;


        /// <summary>
        /// 第二組密碼，下單用
        /// </summary>
        //[StringLength(100)]
        //public string? mm_2nd_hash_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 介紹人
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        public long mm_introduce_code { get; set; } = 0;

        /// [Column(TypeName = "datetime")]
        public DateTime mm_update_datetime { get; set; }
    }

    public class UpdatePwdDTO : UpdatePwdReqModel
    {
        /// <summary>
        /// 雜湊後密碼
        /// </summary>
        /// [StringLength(100)]
        public string? mm_hash_pwd { get; set; } = string.Empty;


        /// <summary>
        /// 第二組密碼，下單用
        /// </summary>
        //[StringLength(100)]
        public string? mm_2nd_hash_pwd { get; set; } = string.Empty;

        public DateTime mm_update_datetime { get; set; }
    }
}
