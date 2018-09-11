using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FiiiChain.MiningPool
{
    class TcpState
    {
        internal TcpClient Client { get; set; }
        internal NetworkStream Stream { get; set; }
    }
}
