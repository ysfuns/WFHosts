using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WFHosts.Models;
using WFHosts.Services;

namespace WFHosts.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        public DelegateCommand SelectMenuItemCommand { get; }
        public DelegateCommand WriteHostsCommand { get; }
        public DelegateCommand StartOrStopCommand { get; set; }

        public DelegateCommand GoToGithubCommand { get; }

        private bool isStartPing = false;
        private bool isCanWrite = false;
        private bool isCanChanged = true;
        private string btn_StartOrStopContent="开始ping";
        private IPInfo[] iPInfos = null;
        private List<IPData> domainList = null;

        //这里是处理界面的一些逻辑
        private List<PingInfoItemViewModel> _pingInfoList;


        private ObservableCollection<CbxDomainDatas> _CbxSource = null;
        private CbxDomainDatas _CbxSelectItem;


        public PingInfoItemViewModel SelectItem { get; set; }

        //这里internal为什么不行？xaml访问不到
        public List<PingInfoItemViewModel> PingInfoList 
        { 
            get => _pingInfoList;
            set
            {
                _pingInfoList = value;
                this.RaisePropertyChanged(nameof(PingInfoList));
            }
        }
        /// <summary>
        /// button的按钮
        /// </summary>
        public string Btn_StartOrStopContent 
        { 
            get => btn_StartOrStopContent; 
            set
            {
                btn_StartOrStopContent = value;
                this.RaisePropertyChanged(nameof(Btn_StartOrStopContent));
            }
        }
        /// <summary>
        /// 是否可以写入hosts
        /// </summary>
        public bool IsCanWrite 
        { 
            get => isCanWrite;
            set 
            {
                isCanWrite = value;
                this.RaisePropertyChanged(nameof(IsCanWrite));
            }
        }
        /// <summary>
        /// 下拉框绑定的源数据
        /// </summary>
        public ObservableCollection<CbxDomainDatas> CbxSource 
        { 
            get => _CbxSource;
            set
            {
                _CbxSource = value;
                //this.RaisePropertyChanged(nameof(CbxSource));//ObservableCollection的特性 不需要自己通知更新
            }
        }
        /// <summary>
        /// 下拉框选中的item
        /// </summary>
        public CbxDomainDatas CbxSelectItem 
        { 
            get => _CbxSelectItem;
            set
            {
                _CbxSelectItem = value;
                setDataGridList(domainList[_CbxSelectItem.ID].IPAddrs);
                this.RaisePropertyChanged(nameof(CbxSelectItem));
            }
        }

        public bool IsCanChanged 
        { 
            get => isCanChanged;
            set
            {
                isCanChanged = value;
                this.RaisePropertyChanged(nameof(IsCanChanged));
            } 
        }

        public MainWindowViewModel()
        {
            this.LoadPingInfoItem();
            this.SelectMenuItemCommand = new DelegateCommand(new Action(this.SelectMenuItemExecute));//绑定选中datagrid事件
            this.WriteHostsCommand = new DelegateCommand(new Action(this.WriteHostsExecute));
            this.StartOrStopCommand = new DelegateCommand(new Action(this.OnStartOrStopExecute));
            this.GoToGithubCommand = new DelegateCommand(new Action(this.GoToGithub));
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
            //PingInfoList = new List<PingInfoItemViewModel>();
            JsonDataService jsonDataService = new JsonDataService();
            domainList = jsonDataService.GetAllIPDatas();
            CbxSource = new ObservableCollection<CbxDomainDatas>();
            if (domainList == null)
            {
                return;
            }
            foreach(var info in domainList)//设置combox的值
            {
                CbxDomainDatas cbxDomain = new CbxDomainDatas();
                cbxDomain.Domainname = info.DomainName;
                cbxDomain.ID= domainList.IndexOf(info);
                CbxSource.Add(cbxDomain);
            }
            CbxSelectItem = CbxSource[0];//默认Combox显示第一个
            //给委托赋值
            PingInfoCallBack = PingInfoCallbackFun;
            //注册回调函数
            Register_callback(PingInfoCallBack);
            //GetPingInfos(iPInfos, iPInfos.Length);
        }

        private void setDataGridList(List<string> ipList)
        {
            if (iPInfos != null)
            {
                iPInfos = null;
            }
            
            if (_pingInfoList != null)
            {
                _pingInfoList.Clear();
                _pingInfoList = null;
            }
            
            _pingInfoList = new List<PingInfoItemViewModel>();
            iPInfos = new IPInfo[ipList.Count];
            int num = 0;
            foreach (var ipinfo in ipList)
            {
                PingInfoItemViewModel pingInfoItemViewModel = new PingInfoItemViewModel();
                pingInfoItemViewModel.ID = iPInfos[num].ID = num+1;
                pingInfoItemViewModel.IP = iPInfos[num].IPAddr = ipinfo.ToString();
                pingInfoItemViewModel.DomainName = CbxSelectItem.Domainname;
                _pingInfoList.Add(pingInfoItemViewModel);
                num++;
            }
            PingInfoList = _pingInfoList;
        }

        private void PingInfoCallbackFun(PingInfoFromCallBack info, int id)
        {
            Console.WriteLine("回调返回的 要修改的ID为:" + id+"---发包:"+info.PacketsSent+"--接收:"+info.PacketsRecv+"--丢包率:"+info.PacketLoss+"---最小ping:"+info.MinRtt);
            PingInfo ping = new PingInfo();
            ping.PacketsSent = info.PacketsSent.ToString();
            ping.PacketLoss = Math.Round(info.PacketLoss, 1).ToString()+"%";//保留一位小数
            ping.PacketsRecv = info.PacketsRecv.ToString();
            ping.MaxRtt = (Math.Floor(info.MaxRtt/1000000)).ToString()+"ms";//下取整,只要有小数都丢弃
            ping.MinRtt = (Math.Floor(info.MinRtt/1000000)).ToString()+"ms";
            ping.AvgRtt = (Math.Floor(info.AvgRtt/1000000)).ToString()+"ms";
            this.PingInfoList.Find(i => i.ID == id).PingInfo = ping;
        }

        private void SelectMenuItemExecute()
        {
            //选中之后是直接写入hosts文件 还是怎么处理？
            //Console.WriteLine(SelectItem.IPData.DomainName);
            int count = this.PingInfoList.Count(i => (i.IsSelected == true && i.DomainName == SelectItem.DomainName));
            if (count > 1)
            {
                MessageBox.Show("一个域名只能对应一个IP地址哟！");
                SelectItem.IsSelected = false;
                count = 1;
            }
            if (count == 1)
            {
                IsCanWrite = true;
            }
            else
            {
                IsCanWrite = false;
            }

        }
        private void WriteHostsExecute()
        {
            //var selectedRows = this.PingInfoList.Where(i => i.IsSelected);
            PingInfoItemViewModel item = this.PingInfoList.Find(i => i.IsSelected);
            string host = item.IP + " " + item.DomainName;
            string[] entry = { host };
            if (ModifyHostsFile(entry))
            {
                FlushDNSResolverCache();//刷新缓存
            }
            Console.WriteLine("写入hosts文件,并刷新缓存");
        }

        private void OnStartOrStopExecute()
        {
            isStartPing = isStartPing ? false : true;
            if (isStartPing)
            {
                Btn_StartOrStopContent = "结束ping";
                IsCanChanged = false;
                GetPingInfos(iPInfos, iPInfos.Length);
            }
            else
            {
                Btn_StartOrStopContent = "开始ping";
                IsCanChanged = true;
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
        

        private void GoToGithub()
        {
            System.Diagnostics.Process.Start("https://www.github.com/ysfuns/WFHosts");
        }

    }
}
