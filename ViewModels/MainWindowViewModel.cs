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
        public DelegateCommand SelectMenuItemCommand;
        //这里是处理界面的一些逻辑
        private List<PingInfoItemViewModel> pingInfoMenu;

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


        public MainWindowViewModel()
        {
            this.LoadPingInfoItem();
            this.SelectMenuItemCommand = new DelegateCommand(new Action(this.SelectMenuItemExecute));//绑定选中datagrid事件
        }

        [DllImport("Pinginfo.dll", EntryPoint = "GetPingInfos")]
        static extern void GetPingInfos(IPInfo[] info,int num);


        [DllImport("Pinginfo.dll", EntryPoint = "StopGetPingInfos")]
        static extern void StopGetPingInfos();

        [DllImport("Pinginfo.dll", EntryPoint = "Register_callback")]
        static extern void Register_callback(Delegate callback);


        public delegate void RealCallback(PingInfoFromCallBack info,int h);

        private RealCallback realCallBack = null;


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
                pingInfoItemView.PingInfo.IPData = info;
                pingInfoItemView.ID = pingInfoList.IndexOf(info);
                pingInfoMenu.Add(pingInfoItemView);
            }
            IPInfo[] iPInfos = new IPInfo[pingInfoMenu.Count];
            int num = 0;
            foreach (var ipinfo in pingInfoMenu)
            {
                iPInfos[num].ID = ipinfo.ID;
                iPInfos[num].IPAddr = ipinfo.PingInfo.IPData.IPAddr;
                num++;
            }
            //给委托赋值
            realCallBack = RealCallbackFun;
            //注册回调函数
            Register_callback(realCallBack);
            GetPingInfos(iPInfos, iPInfos.Length);
        }

        private void RealCallbackFun(PingInfoFromCallBack info, int id)
        {
            Console.WriteLine("回调返回的 要修改的ID为:" + id+"---发包:"+info.PacketsSent+"--接收:"+info.PacketsRecv+"--丢包率:"+info.PacketLoss+"---最小ping:"+info.MinRtt);
        }

        private void SelectMenuItemExecute()
        {
            //选中之后是直接写入hosts文件 还是怎么处理？
            int count = this.pingInfoMenu.Count(i => i.IsSelected == true);
        }
    }
}
