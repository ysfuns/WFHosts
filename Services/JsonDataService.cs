using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFHosts.Models;

namespace WFHosts.Services
{
    class JsonDataService : IDataService
    {
        public List<PingInfo> GetAllPingInfos()
        {
            List<PingInfo> pingInfoList = new List<PingInfo>();
            string jsonFileName = System.IO.Path.Combine(Environment.CurrentDirectory, @"Mode\WarframeIP.json");
            using (System.IO.StreamReader file = System.IO.File.OpenText(jsonFileName))
            {
                using JsonTextReader reader = new JsonTextReader(file);
                JObject o = (JObject)JToken.ReadFrom(reader);
                var w = o["warframe"];
                foreach (JObject e in w)
                {
                    PingInfo pingInfo = new PingInfo();
                    pingInfo.DomainName = e["domainname"].ToString();
                    pingInfo.IPAddr = e["ipaddr"].ToString();
                    pingInfoList.Add(pingInfo);
                }
            }
            return pingInfoList;
        }
    }
    public class Warframe
    {
        public string domainname { get; set; }
        public string ipaddr { get; set; }
    }

    public class RootObject
    {
        public List<Warframe> warframe { get; set; }
    }
}
