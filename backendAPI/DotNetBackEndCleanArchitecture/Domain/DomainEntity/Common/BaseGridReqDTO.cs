using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Common
{
    public class BaseGridReqDTO
    {
        /// <summary>
        /// 每頁取得筆數
        /// </summary>
        public int pageSize { get; set; } = 10;

        /// <summary>
        /// 從第幾頁開始取
        /// </summary>
        public int pageIndex { get; set; } = 1;

        /// <summary>
        /// 預計取的頁數
        /// </summary>
        public int preGetIndex { get; set; } = 1;

        /// <summary>
        /// 頁面的排序欄位
        /// </summary>
        public string pageSort { get; set; } = string.Empty;

        /// <summary>
        /// 頁面的排序順序
        /// </summary>
        public string pageSortDirection { get; set; } = string.Empty;

        /// <summary>
        /// 因標案1筆串接明細24筆故需要有這個參數
        /// </summary>
        public int specPageSize { get; set; } = 1;

        /// <summary>
        /// 跳過筆數
        /// </summary>
        public int skipCNT { get; set; } = 0;

        /// <summary>
        /// 取得筆數
        /// </summary>
        public int takeCNT { get; set; } = 10;
    }
}
