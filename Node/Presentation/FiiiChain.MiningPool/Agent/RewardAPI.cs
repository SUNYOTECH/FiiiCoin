// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.MiningPool.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.MiningPool.Agent
{
    public class RewardAPI : RpcBase
    {
        public long GetUnPaidReward(string address)
        {
            return 1000;
            //var result = this.SendRpcRequest<long>("GetUnPaidReward", new object[] { address });
            //return result;
        }

        public long GetPaidReward(string address)
        {
            return 1000;
            //var result = this.SendRpcRequest<long>("GetPaidReward", new object[] { address });
            //return result;
        }

        public void SaveReward(RewardList entity)
        {
            //this.SendRpcRequest<long>("GetPaidReward", new object[] { entity });
        }

        public long GetActualReward(string address, string blockHash)
        {
            return 1000;
            //return this.SendRpcRequest<long>("GetActualReward", new object[] { address, blockHash });
        }
    }
}
