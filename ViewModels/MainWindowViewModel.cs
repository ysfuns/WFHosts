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
                this.RaisePropertyChanged("PingInfoMenu");
            }
        }


        public MainWindowViewModel()
        {
            this.LoadPingInfoItem();
            this.SelectMenuItemCommand = new DelegateCommand(new Action(this.SelectMenuItemExecute));//绑定选中datagrid事件
        }

        [DllImport("Pinginfo.dll", EntryPoint = "GetPingInfos")]
        static extern void GetPingInfos(BarcodeInfo[] info,int num);
        [DllImport("Pinginfo.dll", EntryPoint = "Register_callback")]
        static extern void Register_callback(Delegate callback);


        public delegate void RealCallback(IPInfo info,int h);

        private RealCallback realCallBack = null;

        

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct BarcodeInfo
        {
            public int Num;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Domain;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string IPAddr;
        };
        
        private void LoadPingInfoItem()
        {
            //这里通过xml或者json service读取文件 通过getAll方法
            pingInfoMenu = new List<PingInfoItemViewModel>();
            JsonDataService jsonDataService = new JsonDataService();
            List<IPInfo> pingInfoList = jsonDataService.GetAllPingInfos();
            if (pingInfoList == null)
            {
                return;
            }
            foreach(var info in pingInfoList)
            {
                PingInfoItemViewModel pingInfoItemView = new PingInfoItemViewModel();
                pingInfoItemView.PingInfo = info;
                pingInfoItemView.ID = pingInfoList.IndexOf(info);
                pingInfoMenu.Add(pingInfoItemView);
            }
            Console.WriteLine("测试一下效果");


            //给委托赋值
            realCallBack = RealCallbackFun;
            //注册回调函数
            //Register_callback(realCallBack);
            BarcodeInfo info1 = new BarcodeInfo();
            info1.Num = 4;
            info1.Domain = "www.baidu.com";
            info1.IPAddr = "192.168.1.1";


            BarcodeInfo[] test = new BarcodeInfo[2];
            test[0] = info1;

            BarcodeInfo info2 = new BarcodeInfo();
            info2.Num = 7;
            info2.Domain = "www.google.com";
            info2.IPAddr = "192.168.1.2";
            test[1] = info2;


            GetPingInfos(test, test.Length);
        }




        private void RealCallbackFun(IPInfo info, int h)
        {
            Console.WriteLine("这是回调返回的:"+info.DomainName+ info .IPAddr+ h);
        }

        private void SelectMenuItemExecute()
        {
            //选中之后是直接写入hosts文件 还是怎么处理？
            int count = this.pingInfoMenu.Count(i => i.IsSelected == true);
        }
    }
}
