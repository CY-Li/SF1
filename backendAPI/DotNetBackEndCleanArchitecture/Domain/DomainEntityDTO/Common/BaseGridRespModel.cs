using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntityDTO.Common
{
    /// <summary>
    /// 目前等同於 BaseGridRespDTO
    /// </summary>
    /// <typeparam name="GM"></typeparam>
    public class BaseGridRespModel<GM>
    {
        public IEnumerable<GM> GridRespResult { get; set; } = new List<GM>();

        public int GridTotalCount { get; set; } = 0;
    }
}
