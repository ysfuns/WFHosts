using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFHosts.Models
{
    class PingInfo
    {
        private string domainname;
        private string ip;

        public string Domainname { get => domainname; set => domainname = value; }
        public string Ip { get => ip; set => ip = value; }
    }
}
