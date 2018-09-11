// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Messages
{
    public class P2PSendMessage
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public P2PCommand Command { get; set; }
    }
}
