using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        

        private void LoadPingInfoItem()
        {
            //这里通过xml或者json service读取文件 通过getAll方法
            pingInfoMenu = new List<PingInfoItemViewModel>();
            JsonDataService jsonDataService = new JsonDataService();
            List<PingInfo> pingInfoList = jsonDataService.GetAllPingInfos();
            if (pingInfoList == null)
            {
                Trace.WriteLine("pingInfoList为空");
                return;
            }
            foreach(var info in pingInfoList)
            {
                PingInfoItemViewModel pingInfoItemView = new PingInfoItemViewModel();
                pingInfoItemView.PingInfo = info;
                pingInfoMenu.Add(pingInfoItemView);
            }
        }

        private void SelectMenuItemExecute()
        {
            //选中之后是直接写入hosts文件 还是怎么处理？
            int count = this.pingInfoMenu.Count(i => i.IsSelected == true);
        }
    }
}
