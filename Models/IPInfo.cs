using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WFHosts.Models
{
    public class IPInfo
    {
        public int ID { get; set; }
        /// <summary>
        /// 域名
        /// </summary>
        public string DomainName { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddr { get; set; }
    }
}
