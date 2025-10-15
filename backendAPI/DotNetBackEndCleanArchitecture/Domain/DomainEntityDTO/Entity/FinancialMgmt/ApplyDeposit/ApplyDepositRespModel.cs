using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Entity.FinancialMgmt.ApplyDeposit
{
    /// <summary>
    /// 會員-取得儲值資料
    /// </summary>
    public class GetDepositDataRespModel
    {
        /// <summary>
        /// blob 圖片(這裡為儲值QR CODE)
        /// </summary>
        [SwaggerSchema(Description = "儲值QR CODE")]
        public byte[] sps_picture { get; set; }

        /// <summary>
        /// varchar(30) 參數01(這裡為儲值流水號)
        /// </summary>
        [SwaggerSchema(Description = "儲值流水號")]
        public string sps_parameter01 { get; set; } = string.Empty;
    }

    public class GettransactionDataRespModel
    {
        [SwaggerSchema(Description = "KYC ID")]
        public long mm_kyc_id { get; set; }

        /// <summary>
        /// 會員錢包地址
        /// </summary>
        [SwaggerSchema(Description = "會員錢包地址")]
        public string akc_mw_address { get; set; }
        /// <summary>
        /// 銀行代號
        /// </summary>
        [SwaggerSchema(Description = "銀行代號")]
        public string akc_bank_code { get; set; }
        /// <summary>
        /// 銀行帳號
        /// </summary>
        [SwaggerSchema(Description = "銀行代號")]
        public string akc_bank_account { get; set; }
        /// <summary>
        /// 銀行戶名
        /// </summary>
        [SwaggerSchema(Description = "銀行戶名")]
        public string akc_bank_account_name { get; set; }
        /// <summary>
        /// 分行
        /// </summary>
        [SwaggerSchema(Description = "分行")]
        public string akc_branch { get; set; }
        /// <summary>
        /// 公司銀行代號
        /// </summary>
        [SwaggerSchema(Description = "公司銀行代號")]
        public string company_bank_code { get; set; }
        /// <summary>
        /// 公司銀行帳號
        /// </summary>
        [SwaggerSchema(Description = "公司銀行帳號")]
        public string company_bank_account { get; set; }
        /// <summary>
        /// 公司銀行戶名
        /// </summary>
        [SwaggerSchema(Description = "公司銀行戶名")]
        public string company_bank_account_name { get; set; }
        /// <summary>
        /// 公司銀行分行
        /// </summary>
        [SwaggerSchema(Description = "公司銀行分行")]
        public string company_branch { get; set; }

        /// <summary>
        /// 儲值匯率
        /// </summary>
        [SwaggerSchema(Description = "儲值匯率")]
        public string deposit_rate { get; set; }
        /// <summary>
        /// 提領匯率
        /// </summary>
        [SwaggerSchema(Description = "提領匯率")]
        public string withdraw_rate { get; set; }
    }
}
