using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.SystemMgmt.SystemParameterSetting
{
    public class QuerySpsRespModel
    {
        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long sps_id { get; set; } = 0;

        /// <summary>
        /// 設定編號
        /// </summary>
        //[StringLength(25)]
        public string sps_code { get; set; } = string.Empty;

        /// <summary>
        /// 設定名稱
        /// </summary>
        //[StringLength(25)]
        public string sps_name { get; set; } = string.Empty;

        /// <summary>
        /// 參數描述
        /// </summary>
        //[StringLength(100)]
        public string sps_description { get; set; } = string.Empty;

        /// <summary>
        /// 圖片(會員儲值會需要QR CODE故這邊新增該欄位)
        /// </summary>
        /// [Column(TypeName = "blob")]
        public byte[]? sps_picture { get; set; }

        //[StringLength(30)]
        public string sps_parameter01 { get; set; } = string.Empty;

        //[StringLength(30)]
        public string sps_parameter02 { get; set; } = string.Empty;

        //[StringLength(30)]
        public string sps_parameter03 { get; set; } = string.Empty;

        //[StringLength(30)]
        public string sps_parameter04 { get; set; } = string.Empty;

        //[StringLength(30)]
        public string sps_parameter05 { get; set; } = string.Empty;

        //[StringLength(30)]
        public string sps_parameter06 { get; set; } = string.Empty;
        public string sps_parameter07 { get; set; } = string.Empty;
        public string sps_parameter08 { get; set; } = string.Empty;
        public string sps_parameter09 { get; set; } = string.Empty;
        public string sps_parameter10 { get; set; } = string.Empty;
    }
}
