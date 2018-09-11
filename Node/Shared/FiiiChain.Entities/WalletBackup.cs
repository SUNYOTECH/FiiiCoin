// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Entities
{
    public class WalletBackup
    {
        public List<Account> AccountList { get; set; }

        public List<AddressBookItem> AddressBookItemList { get; set; }

        public List<Setting> SettingList { get; set; }

        public List<TransactionComment> TransactionCommentList { get; set; }
    }
}
