using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using WFHosts.Models;
using WFHosts.Services;

namespace WFHosts.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        public DelegateCommand SelectMenuItemCommand { get; }
        public DelegateCommand WriteHostsCommand { get; }
        public DelegateCommand StartOrStopCommand { get; set; }

        public bool isStartPing = false;
        private string btn_StartOrStopContent="开始ping";
        private IPInfo[] iPInfos = null;

        //这里是处理界面的一些逻辑
        private List<PingInfoItemViewModel> pingInfoMenu;

        public PingInfoItemViewModel SelectItem { get; set; }

        //这里internal为什么不行？xaml访问不到
        public List<PingInfoItemViewModel> PingInfoMenu 
        { 
            get => pingInfoMenu;
            set
            {
                pingInfoMenu = value;
                this.RaisePropertyChanged(nameof(PingInfoMenu));
            }
        }

        public string Btn_StartOrStopContent 
        { 
            get => btn_StartOrStopContent; set
            {
                btn_StartOrStopContent = value;
                this.RaisePropertyChanged(nameof(Btn_StartOrStopContent));
            }
        }

        public MainWindowViewModel()
        {
            this.LoadPingInfoItem();
            this.SelectMenuItemCommand = new DelegateCommand(new Action(this.SelectMenuItemExecute));//绑定选中datagrid事件
            this.WriteHostsCommand = new DelegateCommand(new Action(this.WriteHostsExecute));
            this.StartOrStopCommand = new DelegateCommand(new Action(this.OnStartOrStopExecute));
    }
        /// <summary>
        /// 获取ping的信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="num"></param>
        [DllImport("Pinginfo.dll", EntryPoint = "GetPingInfos")]
        static extern void GetPingInfos(IPInfo[] info,int num);

        /// <summary>
        /// 关闭DLL中ping功能
        /// </summary>
        [DllImport("Pinginfo.dll", EntryPoint = "StopGetPingInfos")]
        static extern void StopGetPingInfos();

        [DllImport("Pinginfo.dll", EntryPoint = "Register_callback")]
        static extern void Register_callback(Delegate callback);

        //声明委托
        public delegate void PingInfoCallback(PingInfoFromCallBack info,int h);
        private PingInfoCallback PingInfoCallBack = null;


        /// <summary>
        /// 传输给DLL的结构体,IP地址可能重复，所以用ID的唯一性来指向这个对象，IP占四个字节
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IPInfo
        {
            public int ID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string IPAddr;
        };
        /// <summary>
        /// 从DLL返回的结构体，ping ip时获取的所有信息
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PingInfoFromCallBack
        {
            public int PacketsSent;
            public int PacketsRecv;
            public double PacketLoss;
            public double MinRtt;
            public double AvgRtt;
            public double MaxRtt;
        }
        
        private void LoadPingInfoItem()
        {
            //这里通过xml或者json service读取文件 通过getAll方法
            pingInfoMenu = new List<PingInfoItemViewModel>();
            JsonDataService jsonDataService = new JsonDataService();
            List<IPData> pingInfoList = jsonDataService.GetAllIPDatas();
            if (pingInfoList == null)
            {
                return;
            }
            foreach(var info in pingInfoList)
            {
                PingInfoItemViewModel pingInfoItemView = new PingInfoItemViewModel();
                pingInfoItemView.IPData = info;
                pingInfoItemView.ID = pingInfoList.IndexOf(info);
                pingInfoMenu.Add(pingInfoItemView);
            }
            iPInfos = new IPInfo[pingInfoMenu.Count];
            int num = 0;
            foreach (var ipinfo in pingInfoMenu)
            {
                iPInfos[num].ID = ipinfo.ID;
                iPInfos[num].IPAddr = ipinfo.IPData.IPAddr;
                num++;
            }
            //给委托赋值
            PingInfoCallBack = PingInfoCallbackFun;
            //注册回调函数
            Register_callback(PingInfoCallBack);
            //GetPingInfos(iPInfos, iPInfos.Length);
        }

        private void PingInfoCallbackFun(PingInfoFromCallBack info, int id)
        {
            Console.WriteLine("回调返回的 要修改的ID为:" + id+"---发包:"+info.PacketsSent+"--接收:"+info.PacketsRecv+"--丢包率:"+info.PacketLoss+"---最小ping:"+info.MinRtt);
            PingInfo ping = new PingInfo();
            ping.PacketsSent = info.PacketsSent.ToString();
            ping.PacketLoss = info.PacketLoss.ToString();
            ping.PacketsRecv = info.PacketsRecv.ToString();
            ping.MaxRtt = info.MaxRtt.ToString();
            ping.MinRtt = info.MinRtt.ToString();
            ping.AvgRtt = info.AvgRtt.ToString();
            this.PingInfoMenu.Find(i => i.ID == id).PingInfo = ping;
        }

        private void SelectMenuItemExecute()
        {
            //选中之后是直接写入hosts文件 还是怎么处理？

            Console.WriteLine(SelectItem.ID);



            int count = this.pingInfoMenu.Count(i => i.IsSelected == true);
            Console.WriteLine(count);
        }
        private void WriteHostsExecute()
        {

            var selectedRows = this.pingInfoMenu.Where(i => i.IsSelected);



            Console.WriteLine("写入hosts文件,并刷新缓存");
            //string[] entry = { "127.0.0.2 api.warframe.com", "127.0.0.1 gaga.warframe.com" };
            //if (ModifyHostsFile(entry))
            //{
            //    FlushDNSResolverCache();   
            //}
        }

        private void OnStartOrStopExecute()
        {
            isStartPing = isStartPing ? false : true;
            if (isStartPing)
            {
                Console.WriteLine("开始ping");
                Btn_StartOrStopContent = "结束ping";
                GetPingInfos(iPInfos, iPInfos.Length);
            }
            else
            {
                Console.WriteLine("结束ping");
                Btn_StartOrStopContent = "开始ping";
                StopGetPingInfos();
            }
        }

        //修改或写入hosts文件
        private bool ModifyHostsFile(string[] entry)
        {
            //读取hosts文件
            string hostfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
            string[] lines = File.ReadAllLines(hostfile);

            for (int i = 0; i < entry.Length; i++)
            {
                string[] strArray = entry[i].Split(' ');
                if (lines.Any(s => s.Contains(strArray[1])))
                {
                    for (int j = 0; j < lines.Length; j++)
                    {
                        if (lines[j].Contains(strArray[1]))
                            lines[j] = entry[i];
                    }
                    File.WriteAllLines(hostfile, lines);
                }
                else
                {
                    File.AppendAllLines(hostfile, new String[] { entry[i] });
                }
            }
            return true;
        }

        //效果等同于ipconfig /flushdns 命令。
        [DllImport("dnsapi", EntryPoint = "DnsFlushResolverCache")]
        public static extern uint FlushDNSResolverCache();
        

    }
}
