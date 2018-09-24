// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Biz.Services;
using FiiiCoin.Wallet.Win.Common;
using System;
using System.Windows;

namespace FiiiCoin.Wallet.Win.ViewModels
{
    public class ClosingViewModel : VmBase
    {
        protected override void OnLoaded()
        {
            base.OnLoaded();
            RegeistMessenger<Window>(OnClosing);
        }

        protected override string GetPageName()
        {
            return Pages.ClosingPage;
        }

        void OnClosing(Window window)
        {
            ServiceMonitor.StopAll();
            if (NodeMonitor.Default.PortInUse(NodeMonitor.Default.CurrentNetworkType))
            {
                var result = EngineService.Default.AppClosed();
            }
            Environment.Exit(1);
        }
    }
}