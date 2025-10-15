using DomainEntityDTO.Common;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Entity.AuthenticationMgmt.Kyc
{
    public class QueryKycAdminReqModel : BaseGridReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long akc_id { get; set; } = 0;

        /// <summary>
        /// 帳號
        /// </summary>
        [MaxLength(10)]
        public string mm_account { get; set; } = string.Empty;

        /// <summary>
        /// 申請狀態:10-待認證、20-認證通過、30-認證駁回、40-認證失敗
        /// </summary>
        [RegularExpression("^(?:10|20|30)?$", ErrorMessage = "申請狀態限定為10、20、30數字字串，可不輸入撈取全部")]
        public string akc_status { get; set; } = string.Empty;
    }
    public class PostKycReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long akc_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        public long akc_mm_id { get; set; }

        [Required, MaxLength(30)]
        [SwaggerSchema(Description = "名稱")]
        public string akc_name { get; set; } = null!;

        /// <summary>
        /// 身分證正面
        /// </summary>
        //[StringLength(100)]
        public string? akc_front { get; set; }

        /// <summary>
        /// 身分證反面
        /// </summary>
        //[StringLength(100)]
        public string? akc_back { get; set; }

        /// <summary>
        /// 性別(1:男生、2:女生)
        /// </summary>
        [StringLength(1)]
        [SwaggerSchema(Description = "性別")]
        public string akc_gender { get; set; } = null!;

        /// <summary>
        /// 身分證字號
        /// </summary>
        [StringLength(10)]
        [SwaggerSchema(Description = "身分證字號")]
        public string akc_personal_id { get; set; } = null!;

        /// <summary>
        /// 錢包地址
        /// </summary>
        [StringLength(100)]
        [SwaggerSchema(Description = "錢包地址")]
        public string akc_mw_address { get; set; } = null!;

        /// <summary>
        /// 信箱
        /// </summary>
        [StringLength(50)]
        [SwaggerSchema(Description = "信箱")]
        public string akc_email { get; set; } = null!;

        /// <summary>
        /// 銀行代號
        /// </summary>
        [StringLength(10)]
        [SwaggerSchema(Description = "銀行代號")]
        public string akc_bank_code{ get; set; } = null!;

        /// <summary>
        /// 銀行帳號
        /// </summary>
        [StringLength(25)]
        [SwaggerSchema(Description = "銀行帳號")]
        public string akc_bank_account { get; set; } = null!;

        /// <summary>
        /// 戶名
        /// </summary>
        [StringLength(20)]
        [SwaggerSchema(Description = "戶名")]
        public string akc_bank_account_name { get; set; } = null!;

        /// <summary>
        /// 分行
        /// </summary>
        [StringLength(10)]
        [SwaggerSchema(Description = "分行")]
        public string akc_branch { get; set; } = null!;

        /// <summary>
        /// 受益人名稱
        /// </summary>
        [StringLength(30)]
        [SwaggerSchema(Description = "受益人名稱")]
        public string akc_beneficiary_name { get; set; } = null!;

        /// <summary>
        /// 受益人電話
        /// </summary>
        [StringLength(15)]
        [SwaggerSchema(Description = "受益人電話")]
        public string akc_beneficiary_phone { get; set; } = null!;

        /// <summary>
        /// 受益人關係
        /// </summary>
        [StringLength(30)]
        [SwaggerSchema(Description = "受益人關係")]
        public string akc_beneficiary_relationship { get; set; } = null!;

        [SwaggerSchema(Description = "身分證正面")]
        public IFormFile? akc_front_image { get; set; }

        [SwaggerSchema(Description = "身分證反面")]
        public IFormFile? akc_back_image { get; set; }

        public Guid ImageGuid = Guid.NewGuid();
    }

    public class PutKycReqModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long akc_id { get; set; } = 0!;

        /// <summary>
        /// 申請狀態:10-待認證、20-認證通過、30-認證駁回、40-認證失敗
        /// </summary>
        [StringLength(2)]
        [Required(ErrorMessage = "akc_status is required")]
        [RegularExpression("^(20|30)$", ErrorMessage = "申請狀態限定為20、30數字")]
        public string akc_status { get; set; } = null!;

        /// <summary>
        /// 覆核人
        /// </summary>
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long mm_id { get; set; }
    }
}
