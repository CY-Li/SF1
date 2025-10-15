using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainEntity.Common
{
    public class AppConfig
    {
        /// <summary>
        /// 連線字串
        /// </summary>
        public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    }

    public class ConnectionStrings
    {
        /// <summary>
        /// 預設連線字串
        /// </summary>
        public string BackEndDatabase { get; set; } = string.Empty;
        /// <summary>
        /// Mysql預設連線字串
        /// </summary>
        public string MysqlBackEndDatabase { get; set; } = string.Empty;


    }
}
