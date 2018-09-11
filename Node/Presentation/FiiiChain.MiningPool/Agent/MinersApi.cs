// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace FiiiChain.MiningPool.Agent
{
    public class MinersAPI :RpcBase
    {
        public bool POSValidate(string address, string sn)
        {
            return true;
            //var isValidate = this.SendRpcRequest<bool>("POSValidate", new object[] { address, sn });
            //return isValidate;
        }

        public Miners SaveMiners(string address, string account, string sn)
        {
            return new Miners { Address = address, Account = account, SN = sn };
            //var miners = this.SendRpcRequest<Miners>("SaveMiners", new object[] { address, account, sn });
            //return miners;
        }
    }
}
