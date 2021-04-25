using System.Collections.Generic;

namespace WFHosts.Models
{
    public class IPData
    {
        /// <summary>
        /// 域名
        /// </summary>
        public string DomainName { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public List<string> IPAddrs { get; set; }
    }
}
