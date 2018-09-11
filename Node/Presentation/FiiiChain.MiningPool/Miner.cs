using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FiiiChain.MiningPool
{
    class Miner
    {
        public bool IsConnected { get; set; }
        public string SerialNo { get; set; }
        public string WalletAddress { get; set; }
        public long MaxNonce { get; set; }
        public int CheckScoopNumber { get; set; }
        public long ConnectedTime { get; set; }
        public long LatestHeartbeatTime { get; set; }
        //public bool IsInMining { get; set; }
        //public long BlockHeight { get; set; }
        //public long TotalDataCount { get; set; }

        //public SocketAsyncEventArgs State { get; set; }
        public string ClientAddress { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
    }
}
