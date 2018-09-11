// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.MiningPool.Entities;

namespace FiiiChain.MiningPool.Agent
{
    public class BlocksAPI : RpcBase
    {
        public void SaveBlocks(Blocks entity)
        {
            //this.SendRpcRequest<bool>("SaveBlocks", new object[] { entity });
        }

        public void GetVerifiedHashes()
        {
            //this.SendRpcRequest<BlocksComponent>("GetVerifiedHashes", new object[] {  });
        }
    }
}
