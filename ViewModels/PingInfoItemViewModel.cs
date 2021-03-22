using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFHosts.Models;


namespace WFHosts.ViewModels
{
    //抽象出的DataGrid的每一行的对象，是否选中是这个UI界面对象的属性，不是pinginfo的属性，所以要抽离出来
    class PingInfoItemViewModel: BindableBase
    {
        public int ID { get; set; }
        private PingInfo pingInfo;
        private bool isSelected;
        public bool IsSelected 
        { 
            get => isSelected; 
            set 
            { 
                isSelected = value; 
                this.RaisePropertyChanged("IsSelected"); 
            } 
        }

        public PingInfo PingInfo 
        { 
            get => pingInfo;
            set
            { 
                pingInfo = value;
                this.RaisePropertyChanged("PingInfo");
            }
        }
    }
}
