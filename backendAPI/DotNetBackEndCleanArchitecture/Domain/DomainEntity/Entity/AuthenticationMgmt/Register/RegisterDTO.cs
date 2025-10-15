using DomainEntity.DBModels;
using DomainEntityDTO.Entity.AuthenticationMgmt.Register;
using Microsoft.Extensions.Logging.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace DomainEntity.Entity.AuthenticationMgmt.Register
{
    public class RegisterDTO : RegisterReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long mm_id { get; set; } = 0;

        /// <summary>
        /// 雜湊後密碼
        /// </summary>
        /// [StringLength(100)]
        public string? mm_hash_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 第二組密碼，下單用
        /// </summary>
        //[StringLength(25)]
        public string mm_2nd_hash_pwd { get; set; } = string.Empty;

        /// <summary>
        /// 會員顯示名稱
        /// </summary>
        /// [StringLength(30)]
        //[Required(ErrorMessage = "name is required"), MinLength(2), MaxLength(30)]
        //[SwaggerSchema(Description = "會員名稱")]
        public string? mm_name { get; set; } = null;

        /// <summary>
        /// 介紹人
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        public long mm_introduce_code { get; set; } = 0;

        /// <summary>
        /// 邀請碼-Invitation code
        /// </summary>
        ///[Column(TypeName = "bigint(15)")]
        public long mm_invite_code { get; set; } = 0;

        /// <summary>
        /// 性別(1:男生、2:女生)
        /// </summary>
        /// [StringLength(1)]
        public string? mm_gender { get; set; } = null;

        /// <summary>
        /// 身分證字號
        /// </summary>
        /// [StringLength(10)]
        public string? mm_personal_id { get; set; } = null;

        /// <summary>
        /// 電話號碼(目前手機號碼)
        /// </summary>
        /// [StringLength(15)]
        public string? mm_phone_number { get; set; } = null;

        /// <summary>
        /// 通訊地址
        /// </summary>
        /// [StringLength(100)]
        public string? mm_mw_address { get; set; } = null;

        /// <summary>
        /// 信箱
        /// </summary>
        /// [StringLength(50)]
        public string? mm_email { get; set; } = null;

        /// <summary>
        /// 角色權限:1-使用者、2-管理者
        /// </summary>
        /// [StringLength(1)]
        public string? mm_role_type { get; set; } = null!;

        /// [Column(TypeName = "datetime")]
        public DateTime mm_create_datetime { get; set; }

        /// [Column(TypeName = "datetime")]
        public DateTime mm_update_datetime { get; set; }
    }
}
