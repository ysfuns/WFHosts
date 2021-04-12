using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFHosts.Models
{
    public class PingInfo
    {
        
        /// <summary>
        /// 主机名字
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// 发包的个数
        /// </summary>
        public string PacketsSent { get; set; }
        /// <summary>
        /// 收到包的个数
        /// </summary>
        public string PacketsRecv { get; set; }
        /// <summary>
        /// 丢包率
        /// </summary>
        public string PacketLoss { get; set; }
        /// <summary>
        /// 最小ping值
        /// </summary>
        public string MinRtt { get; set; }
        /// <summary>
        /// 平均ping值
        /// </summary>
        public string AvgRtt { get; set; }
        /// <summary>
        /// 最大ping值
        /// </summary>
        public string MaxRtt { get; set; }



        
        //ping ip时候获取的数据
    }
}
