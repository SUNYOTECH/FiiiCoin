// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Models;
using System.Windows;

namespace FiiiCoin.Wallet.Win.ViewModels
{
    public class SynchroDataViewModel : PopupShellBase
    {
        public SynchroDataViewModel()
        {
            RegeistMessenger<BlockSyncInfo>(OnGetRequest);
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            UpdateBlocksMonitor.Default.MonitorCallBack += x => {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BlockSyncInfo = x;
                });
            };
        }

        private BlockSyncInfo _blockSyncInfo;

        public BlockSyncInfo BlockSyncInfo
        {
            get { return _blockSyncInfo; }
            set { _blockSyncInfo = value; RaisePropertyChanged("BlockSyncInfo"); }
        }

        void OnGetRequest(BlockSyncInfo info)
        {
            BlockSyncInfo = info;
        }


        protected override string GetPageName()
        {
            return Pages.SynchroDataPage;
        }
    }
}