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
    public class Accounts
    {
        public async Task<AccountInfoOM[]> GetAddressesByTag(string tag = "")
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("GetAddressesByTag", new[] { tag }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            AccountInfoOM[] responseValue = response.GetResult<AccountInfoOM[]>();
            return responseValue;
        }

        public async Task<AccountInfoOM> GetAccountByAddress(string address)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("GetAccountByAddress", new[] { address }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            AccountInfoOM responseValue = response.GetResult<AccountInfoOM>();
            return responseValue;
        }

        public async Task<AccountInfoOM> GetNewAddress(string tag)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("GetNewAddress", new[] { tag }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            AccountInfoOM responseValue = response.GetResult<AccountInfoOM>();
            return responseValue;
        }

        public async Task<AccountInfoOM> GetDefaultAccount()
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithNoParameters("GetDefaultAccount", 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            AccountInfoOM responseValue = response.GetResult<AccountInfoOM>();
            return responseValue;
        }

        public async Task SetDefaultAccount(string address)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("SetDefaultAccount", new[] { address }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
        }

        public async Task<AddressInfoOM> ValidateAddress(string address)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("ValidateAddress", new[] { address }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            AddressInfoOM responseValue = response.GetResult<AddressInfoOM>();
            return responseValue;
        }

        public async Task SetAccountTag(string address, string tag)
        {
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("SetAccountTag", new List<object> { address, tag }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
        }
    }
}
