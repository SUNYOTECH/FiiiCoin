using FiiiChain.PoolMessages;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FiiiChain.MiningPool
{
    class TcpSendState : TcpState
    {
        internal PoolCommand Command { get; set; }
    }
}
