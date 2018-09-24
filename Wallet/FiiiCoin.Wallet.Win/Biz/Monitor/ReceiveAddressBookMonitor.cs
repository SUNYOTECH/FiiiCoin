// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Models;
using FiiiCoin.Wallet.Win.Biz.Services;
using System.Collections.Generic;

namespace FiiiCoin.Wallet.Win.Biz.Monitor
{
    public class ReceiveAddressBookMonitor : ServiceMonitorBase<List<AccountInfo>>
    {
        private static ReceiveAddressBookMonitor _default;

        public static ReceiveAddressBookMonitor Default
        {
            get
            {
                if (_default == null)
                    _default = new ReceiveAddressBookMonitor();
                return _default;
            }
        }

        protected override List<AccountInfo> ExecTaskAndGetResult()
        {
            var result = AccountsService.Default.GetAddressesByTag();
            if (result.IsFail)
                return null;
            else
                return result.Value;
        }
    }
}
