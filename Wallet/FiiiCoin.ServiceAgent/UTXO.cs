// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiCoin.DTO;
using FiiiCoin.Utility.Api;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FiiiCoin.ServiceAgent
{
    public class UTXO
    {
        public async Task<TxOutSetOM> GetTxOutSetInfo()
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithNoParameters("GetTxOutSetInfo", 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            TxOutSetOM responseValue = response.GetResult<TxOutSetOM>();
            return responseValue;
        }

        public async Task<List<UnspentOM>> ListUnspent(int minConfirmations, int maxConfirmations = 9999, string[] addresses = null)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("ListUnspent", new List<object> { minConfirmations, maxConfirmations, addresses }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            List<UnspentOM> responseValue = response.GetResult<List<UnspentOM>>();
            return responseValue;
        }

        public async Task<long> GetUnconfirmedBalance()
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithNoParameters("GetUnconfirmedBalance", 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            long responseValue = response.GetResult<long>();
            return responseValue;
        }

        public async Task<TxOutOM> GetTxOut(string txid, int vout, bool unconfirmed)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("GetTxOut", new List<object> { txid, vout, unconfirmed }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            TxOutOM responseValue = response.GetResult<TxOutOM>();
            return responseValue;
        }
    }
}
