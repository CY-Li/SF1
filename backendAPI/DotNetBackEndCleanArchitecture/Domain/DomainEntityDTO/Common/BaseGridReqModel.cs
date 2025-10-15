using Swashbuckle.AspNetCore.Annotations;

namespace DomainEntityDTO.Common
{
    public class BaseGridReqModel
    {
        /// <summary>
        /// 每頁取得筆數
        /// </summary>
        [SwaggerSchema(Description = "每頁取得筆數(預設10)")]
        public int pageSize { get; set; } = 10;

        /// <summary>
        /// 從第幾頁開始取
        /// </summary>
        [SwaggerSchema(Description = "從第幾頁開始取(預設1)")]
        public int pageIndex { get; set; } = 1;

        /// <summary>
        /// 預計取的頁數
        /// </summary>
        [SwaggerSchema(Description = "預計取的頁數(預設預設1，0取全部)")]
        public int preGetIndex { get; set; } = 1;

        /// <summary>
        /// 頁面的排序欄位
        /// </summary>
        [SwaggerSchema(Description = "頁面的排序欄位")]
        public string pageSort { get; set; } = string.Empty;

        /// <summary>
        /// 頁面的排序順序
        /// </summary>
        [SwaggerSchema(Description = "頁面的排序順序")]
        public string pageSortDirection { get; set; } = string.Empty;
    }
}
