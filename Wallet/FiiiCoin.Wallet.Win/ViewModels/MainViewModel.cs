// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Biz.Services;
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Common.Utils;
using FiiiCoin.Wallet.Win.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FiiiCoin.Wallet.Win.ViewModels
{
    public class MainViewModel : VmBase
    {
        protected override string GetPageName()
        {
            return Pages.MainPage;
        }

        #region Private Proterties
        private string _netToolTip;
        private HeaderInfo _selectedTabItem;
        private Page _viewPage;
        private ObservableCollection<HeaderInfo> _tabHeaders;
        private bool _isSyncComplete;
        #endregion

        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                #region Tab页签
                TabHeaders.Add(new HeaderInfo("概况(O)"));
                TabHeaders.Add(new HeaderInfo("发送(S)"));
                TabHeaders.Add(new HeaderInfo("接收(R)"));
                TabHeaders.Add(new HeaderInfo("交易记录(T)"));
                #endregion
            }
            else
            {
                Init();
            }
        }

        #region Public Propities
        public ObservableCollection<HeaderInfo> TabHeaders
        {
            get
            {
                if (_tabHeaders == null)
                    _tabHeaders = new ObservableCollection<HeaderInfo>();
                return _tabHeaders;
            }
            set
            {
                _tabHeaders = value;
                RaisePropertyChanged("TabHeaders");
            }
        }

        public Page ViewPage
        {
            get { return _viewPage; }
            set { _viewPage = value; RaisePropertyChanged("ViewPage"); }
        }

        public string NetToolTip
        {
            get
            {
                return _netToolTip;
            }
            set
            {
                _netToolTip = value;
                RaisePropertyChanged("NetToolTip");
            }
        }

        public bool IsSyncComplete {
            get { return _isSyncComplete; }
            private set { _isSyncComplete = value; RaisePropertyChanged("IsSyncComplete"); }
        }
        #endregion

        #region Commands
        public ICommand TabItemCheckedCommand { get; private set; }
        public ICommand PageCommand { get; private set; }
        public ICommand ProgressCommand { get; private set; }
        public ICommand NetWorkCommand { get; private set; }
        #endregion

        void Init()
        {
            if (AppSettingConfig.Default.AppConfig.MainWindowTabs != null)
            {
                AppSettingConfig.Default.AppConfig.MainWindowTabs.ForEach(x => TabHeaders.Add(x));
                if (TabHeaders.Any(x => x.IsSelected))
                {
                    _selectedTabItem = TabHeaders.FirstOrDefault(x => x.IsSelected);
                }
            }

            TabItemCheckedCommand = new RelayCommand<HeaderInfo>(OnTabItemChecked);
            PageCommand = new RelayCommand<string>(OnPageCommand);
            ProgressCommand = new RelayCommand(OnProgressClick);
            NetWorkCommand = new RelayCommand<bool>(OnNetWorkClick);
            SetTaskInfos();
            RegeistMessenger<string>(OnGetRequestTab);
        }

        #region Command Event

        void OnGetRequestTab(string tabName)
        {
            if (!TabHeaders.Any(x => x.PageName == tabName))
                return;
            var headerInfo = TabHeaders.FirstOrDefault(x => x.PageName == tabName);
            OnTabItemChecked(headerInfo);
        }

        void OnTabItemChecked(HeaderInfo headerInfo)
        {
            if (_selectedTabItem == headerInfo)
            {
                _selectedTabItem.IsSelected = true;
                return;
            }
            if (_selectedTabItem != null)
                _selectedTabItem.IsSelected = false;
            _selectedTabItem = headerInfo;

            SendMessenger(headerInfo.PageName, SendMessageTopic.Refresh);
            ViewPage = BootStrapService.Default.GetPage(headerInfo.PageName);
        }

        void OnPageCommand(string msg)
        {
            switch (msg)
            {
                case "OnLoaded":
                    if (_selectedTabItem != null)
                    {
                        ViewPage = BootStrapService.Default.GetPage(_selectedTabItem.PageName);
                    };
                    break;
                default:
                    break;
            }
        }

        void OnProgressClick()
        {
            SendMessenger(Pages.SynchroDataPage, BlockSyncInfo);
            UpdatePage(Pages.SynchroDataPage);
        }

        void OnNetWorkClick(bool isactive)
        {
            NetWorkService.Default.SetNetworkActive(!isactive);
            NetToolTip = null;
            if (!isactive)
            {
                BlockSyncInfo blockSyncInfo = new BlockSyncInfo();
                blockSyncInfo.AllBlockHeight = BlockSyncInfo.AllBlockHeight;
                blockSyncInfo.StartTimeOffset = DateTimeUtil.GetDateTimeStamp(DateTime.Now);
                BlockSyncInfo = blockSyncInfo;
            }
        }
        #endregion

        private BlockSyncInfo _blockSyncInfo;

        public BlockSyncInfo BlockSyncInfo
        {
            get
            {
                if (_blockSyncInfo == null)
                    _blockSyncInfo = new BlockSyncInfo();
                return _blockSyncInfo;
            }
            set
            {
                _blockSyncInfo = value;
                RaisePropertyChanged("BlockSyncInfo");
            }
        }


        void SetTaskInfos()
        {
            UpdateBlocksMonitor.Default.MonitorCallBack += x => {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (x.IsSyncComplete())
                    {
                        Messenger.Default.Send(Pages.SynchroDataPage,MessageTopic.ClosePopUpWindow);
                        UpdateBlocksMonitor.Default.Stop();
                        IsSyncComplete = true;
                        AmountMonitor.Default.Start(3000);
                    }
                    else
                        IsSyncComplete = false;
                    BlockSyncInfo = x;

                    var formatter = LanguageService.Default.GetLanguageValue("netTooltipFormat");
                    var msg = string.Format(formatter, x.ConnectCount, Environment.NewLine);
                    NetToolTip = msg;
                });
            };
        }
    }
}