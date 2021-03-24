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
        public List<IPInfo> GetAllPingInfos()
        {
            List<IPInfo> pingInfoList = new List<IPInfo>();
            string jsonFileName = System.IO.Path.Combine(Environment.CurrentDirectory, @"Mode\WarframeIP.json");
            using (System.IO.StreamReader file = System.IO.File.OpenText(jsonFileName))
            {
                using JsonTextReader reader = new JsonTextReader(file);
                JObject o = (JObject)JToken.ReadFrom(reader);
                var w = o["warframe"];
                int id = 0;
                foreach (JObject e in w)
                {
                    IPInfo pingInfo = new IPInfo();
                    pingInfo.DomainName = e["domainname"].ToString();
                    pingInfo.IPAddr = e["ipaddr"].ToString();
                    pingInfo.ID = id;
                    id++;
                    pingInfoList.Add(pingInfo);
                }
            }
            return pingInfoList;
        }
    }
}
