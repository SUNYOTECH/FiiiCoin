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
    public class PaymentRequest
    {
        public async Task<PayRequestOM> CreateNewPaymentRequest(string accountId, string tag, long amount, string comment)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("CreateNewPaymentRequest", new List<object> { accountId, tag, amount, comment }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            PayRequestOM responseValue = response.GetResult<PayRequestOM>();
            return responseValue;
        }

        public async Task<PayRequestOM[]> GetAllPaymentRequests()
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithNoParameters("GetAllPaymentRequests", 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            PayRequestOM[] responseValue = response.GetResult<PayRequestOM[]>();
            return responseValue;
        }

        public async Task DeletePaymentRequestsByIds(long[] ids)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("DeletePaymentRequestsByIds", new[] { ids }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
        }
    }
}
