using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFHosts.Models;

namespace WFHosts.ViewModels
{
    class MainWindowViewModel : NotificationObject
    {
        public DelegateCommand SelectMenuItemCommand;
        //这里是处理界面的一些逻辑
        private List<PingInfoItemViewModel> pingInfoMenu;

        internal List<PingInfoItemViewModel> PingInfoMenu 
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
            
        }

        private void SelectMenuItemExecute()
        {
            //选中之后是直接写入hosts文件 还是怎么处理？
            int count = this.pingInfoMenu.Count(i => i.IsSelected == true);
        }
    }
}
