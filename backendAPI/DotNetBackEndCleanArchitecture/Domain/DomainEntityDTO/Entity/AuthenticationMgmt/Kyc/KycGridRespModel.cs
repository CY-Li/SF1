using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Entity.AuthenticationMgmt.Kyc
{
    public class QueryKycAdminRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long akc_id { get; set; } = 0;

        /// [Column(TypeName = "bigint(15)")]
        public long akc_mm_id { get; set; } = 0;

        /// <summary>
        /// 身分證正面
        /// </summary>
        //[StringLength(100)]
        public string akc_front { get; set; } = string.Empty;

        /// <summary>
        /// 身分證反面
        /// </summary>
        //[StringLength(100)]
        public string akc_back { get; set; } = string.Empty;

        /// <summary>
        /// 性別(1:男生、2:女生)
        /// </summary>
        //[StringLength(1)]
        public string akc_gender { get; set; } = string.Empty;

        /// <summary>
        /// 身分證字號
        /// </summary>
        //[StringLength(10)]
        public string akc_personal_id { get; set; } = string.Empty;

        /// <summary>
        /// 通訊地址
        /// </summary>
        //[StringLength(100)]
        public string akc_mw_address { get; set; } = string.Empty;

        /// <summary>
        /// 信箱
        /// </summary>
        //[StringLength(50)]
        public string akc_email { get; set; } = string.Empty;

        /// <summary>
        /// 銀行帳號
        /// </summary>
        //[StringLength(25)]
        public string akc_bank_account { get; set; } = string.Empty;

        /// <summary>
        /// 戶名
        /// </summary>
        //[StringLength(20)]
        public string akc_bank_account_name { get; set; } = string.Empty;

        /// <summary>
        /// 分行
        /// </summary>
        //[StringLength(10)]
        public string akc_branch { get; set; } = string.Empty;

        /// <summary>
        /// 申請狀態:10-待認證、20-認證通過、30-認證駁回、40-認證失敗
        /// </summary>
        //[StringLength(2)]
        public string akc_status { get; set; } = string.Empty;

        /// <summary>
        /// 申請狀態名稱
        /// </summary>
        public string akc_status_name { get; set; } = string.Empty;

        /// <summary>
        /// 帳號(現在為手機號碼)
        /// </summary>
        //[StringLength(10)]
        public string mm_account { get; set; } = string.Empty;
    }
}
