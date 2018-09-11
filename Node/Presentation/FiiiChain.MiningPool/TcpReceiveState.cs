using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FiiiChain.MiningPool
{
    class TcpReceiveState : TcpState
    {
        internal byte[] Buffer { get; set; }
    }
}
