using DomainEntityDTO.Common;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DomainEntityDTO.Entity.SystemMgmt.SystemParameterSetting
{
    public class QuerySpsReqModel : BaseGridReqModel
    {
        /// <summary>
        /// 設定編號
        /// </summary>
        [StringLength(25)]
        public string sps_code { get; set; } = string.Empty;

        /// <summary>
        /// 設定名稱
        /// </summary>
        [StringLength(25)]
        public string sps_name { get; set; } = string.Empty;
    }

    public class PutSpsReqModel
    {
        /// <summary>
        /// bigint(15) 流水號 會員ID
        /// </summary>
        [Required(ErrorMessage = "ID is required")]
        //[SwaggerSchema(Description = "新增人員ID")]
        public long mm_id { get; set; }

        /// [Key]
        /// [Column(TypeName = "bigint(15)")]
        public long sps_id { get; set; } = 0;

        /// <summary>
        /// 設定編號
        /// </summary>
        /// [StringLength(25)]
        [Required(ErrorMessage = "sps_code is required"), MaxLength(25)]
        //[SwaggerSchema(Description = "設定代號")]
        public string sps_code { get; set; } = string.Empty;

        /// <summary>
        /// 設定名稱
        /// </summary>
        /// [StringLength(25)]
        [Required(ErrorMessage = "sps_name is required"), MaxLength(25)]
        //[SwaggerSchema(Description = "設定名稱")]
        public string sps_name { get; set; } = string.Empty;

        /// <summary>
        /// 參數描述
        /// </summary>
        /// [StringLength(100)]
        [MaxLength(100)]
        //[SwaggerSchema(Description = "設定名稱")]
        public string? sps_description { get; set; } = string.Empty;

        /// <summary>
        /// 圖片(會員儲值會需要QR CODE故這邊新增該欄位)
        /// </summary>
        /// [Column(TypeName = "blob")]
        public byte[]? sps_picture { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter01 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter02 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter03 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter04 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter05 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter06 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter07 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter08 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter09 { get; set; } = string.Empty;

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter10 { get; set; } = string.Empty;
    }


    /// <summary>
    /// 後台-新增參數
    /// </summary>
    public class PostSpsReqModel
    {
        /// <summary>
        /// bigint(15) 流水號 會員ID
        /// </summary>
        [Required(ErrorMessage = "ID is required")]
        //[SwaggerSchema(Description = "新增人員ID")]
        public long mm_id { get; set; }

        /// <summary>
        /// 設定編號
        /// </summary>
        /// [StringLength(25)]
        [Required(ErrorMessage = "sps_code is required"), MaxLength(25)]
        //[SwaggerSchema(Description = "設定代號")]
        public string sps_code { get; set; } = string.Empty;

        /// <summary>
        /// 設定名稱
        /// </summary>
        /// [StringLength(25)]
        [Required(ErrorMessage = "sps_name is required"), MaxLength(25)]
        //[SwaggerSchema(Description = "設定名稱")]
        public string sps_name { get; set; } = string.Empty;

        /// <summary>
        /// 參數描述
        /// </summary>
        /// [StringLength(100)]
        [MaxLength(100)]
        //[SwaggerSchema(Description = "設定名稱")]
        public string? sps_description { get; set; } = string.Empty;

        /// <summary>
        /// 圖片(會員儲值會需要QR CODE故這邊新增該欄位)
        /// </summary>
        /// [Column(TypeName = "blob")]
        public byte[]? sps_picture { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter01 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter02 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter03 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter04 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter05 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter06 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter07 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter08 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter09 { get; set; }

        /// [StringLength(30)]
        [MaxLength(100)]
        public string? sps_parameter10 { get; set; }
    }
}
