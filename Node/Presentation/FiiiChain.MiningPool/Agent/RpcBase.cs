// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace FiiiChain.MiningPool.Agent
{
    public abstract class RpcBase
    {
        RpcClient client;

        public RpcBase()
        {
            try
            {
                AuthenticationHeaderValue authHeaderValue = null;// AuthenticationHeaderValue.Parse("Basic R2VrY3RlazpXZWxjMG1lIQ==");
                this.client = new RpcClient(new Uri(MiningPoolSetting.POOL_API), authHeaderValue);
            }
            catch
            {

            }
        }

        protected T SendRpcRequest<T>(string methodName, object[] parameters = null)
        {
            try
            {
                RpcRequest request = RpcRequest.WithParameterList(methodName, parameters, "Id1");
                var response = client.SendRequestAsync(request);

                var result = response.Result;
                
                if (result.HasError)
                {
                    throw new Exception("Failed in send " + methodName + " RPC request, error code is " + result.Error.Code);
                }
                else
                {
                    return response.Result.GetResult<T>();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed in send " + methodName + "rpc request, error code is UNKNOWN, please check and restart Node service");
            }
        }

        protected async void SendRpcRequest(string methodName, object[] parameters = null)
        {
            RpcRequest request = RpcRequest.WithParameterList(methodName, parameters, "Id1");
            await client.SendRequestAsync(request);
        }
    }
}
