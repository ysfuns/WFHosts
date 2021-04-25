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
        public List<IPData> GetAllIPDatas()
        {
            List<IPData> pingInfoList = new List<IPData>();
            string jsonFileName = System.IO.Path.Combine(Environment.CurrentDirectory, @"Mode\WarframeIP.json");
            using (System.IO.StreamReader file = System.IO.File.OpenText(jsonFileName))
            {
                using JsonTextReader reader = new JsonTextReader(file);
                JObject o = (JObject)JToken.ReadFrom(reader);
                var w = o["warframe"];
                foreach (JObject e in w)
                {
                    IPData pingInfo = new IPData();
                    pingInfo.DomainName = e["domainname"].ToString();

                    var l = e["ipaddrs"];
                    List<string> list = new List<string>();
                    for (int i = 0; i < l.Count(); i++)
                    {
                        list.Add(l[i].ToString());
                    }
                    pingInfo.IPAddrs = list;
                    pingInfoList.Add(pingInfo);
                }
            }
            return pingInfoList;
        }
    }
}
