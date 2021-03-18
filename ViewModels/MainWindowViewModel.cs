using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFHosts.Models;

namespace WFHosts.ViewModels
{
    class MainWindowViewModel : NotificationObject
    {
        private List<PingInfo> pingInfos;
        public List<PingInfo> PingInfos { get => pingInfos; set { pingInfos = value; this.RaisePropertyChanged("PingInfos"); } }
        public MainWindowViewModel()
        {
            pingInfos = new List<PingInfo>();
            PingInfo pingInfo = new PingInfo();
            pingInfo.Domainname = "wwww.baidu.com";
            pingInfo.Ip = "192.168.1.1";
            PingInfos.Add(pingInfo);
        }


    }
}
