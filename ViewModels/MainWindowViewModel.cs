using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        static extern void GetPingInfos(List<PingInfoItemViewModel> list);
        [DllImport("Pinginfo.dll", EntryPoint = "Register_callback")]
        static extern void Register_callback(Delegate callback);


        public delegate void RealCallback(string name,int h);

        private RealCallback realCallBack = null;


        private void LoadPingInfoItem()
        {
            //这里通过xml或者json service读取文件 通过getAll方法
            pingInfoMenu = new List<PingInfoItemViewModel>();
            JsonDataService jsonDataService = new JsonDataService();
            List<PingInfo> pingInfoList = jsonDataService.GetAllPingInfos();
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
            Register_callback(realCallBack);
            GetPingInfos(pingInfoMenu);
        }

        private void RealCallbackFun(string name, int h)
        {
            Console.WriteLine("这是回调返回的:"+name+h);
        }

        private void SelectMenuItemExecute()
        {
            //选中之后是直接写入hosts文件 还是怎么处理？
            int count = this.pingInfoMenu.Count(i => i.IsSelected == true);
        }
    }
}
