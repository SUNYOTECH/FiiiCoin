using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.MiningPool
{
    class DeadlineInfo
    {
        internal long BlockHeight { get; set; }
        internal int ScoopNumber { get; set; }
        internal string WalletAddress { get; set; }
        internal long Nonce { get; set; }
        internal long Deadline { get; set; }
    }
}
