using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Common
{
    /// <summary>
    /// 目前等同於ApiResultModel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceResultDTO<T>
    {
        /// <summary>
        /// 回傳值單純有資料或是條件取得的資料
        /// </summary>
        public T? Result { get; set; } = default!;
        /// <summary>
        /// 回傳代碼
        /// </summary>
        public int returnStatus { get; set; } = 0;
        /// <summary>
        /// 回傳訊息
        /// </summary>
        public string returnMsg { get; set; } = string.Empty;
    }
}
