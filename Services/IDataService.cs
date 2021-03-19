using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFHosts.Models;

namespace WFHosts.Services
{
    public interface IDataService
    {
        List<PingInfo> GetAllPingInfos();
    }
}
