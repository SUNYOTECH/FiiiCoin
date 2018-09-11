using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.DTO;
using FiiiChain.Framework;
using FiiiChain.Messages;

namespace FiiiChain.MiningPool.Agent
{
    class WalletAPI
    {
        RpcClient client;

        public WalletAPI(string uri)
        {
            AuthenticationHeaderValue authHeaderValue = null;// AuthenticationHeaderValue.Parse("Basic R2VrY3RlazpXZWxjMG1lIQ==");
            this.client = new RpcClient(new Uri(uri), authHeaderValue);
        }

        public T SendRpcRequest<T>(string methodName, object[] parameters = null)
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
                throw new Exception("Failed in send " + methodName + "rpc request, error code is UNKNOWN, please check and restart Node service \n" + ex.ToString());
            }
        }

        public async void SendRpcRequest(string methodName, object[] parameters = null)
        {
            RpcRequest request = RpcRequest.WithParameterList(methodName, parameters, "Id1");
            await client.SendRequestAsync(request);
        }

        public long GetBlockCount()
        {
            return this.SendRpcRequest<long>("GetBlockCount");
        }

        public BlockMsg GenerateNewBlock(string minerName, string walletAddress)
        {
            var data = this.SendRpcRequest<string>("GenerateNewBlock", new object[] { minerName, walletAddress, 0 });
            var blockMsg = new BlockMsg();
            int index = 0;
            blockMsg.Deserialize(Base16.Decode(data), ref index);

            return blockMsg;
        }

        public long GetBaseTarget(long height)
        {
            return this.SendRpcRequest<long>("GetBaseTarget", new object[] { height });
        }

        public void SubmitBlock(BlockMsg block)
        {
            var blockData = Base16.Encode(block.Serialize());
            this.SendRpcRequest("SubmitBlock", new object[] { blockData });
        }

        public BlockMsg GetBlockByHeight(long height)
        {
            var hash = this.SendRpcRequest<string>("GetBlockHash", new object[] { height });

            if(string.IsNullOrWhiteSpace(hash))
            {
                return null;
            }

            var data = this.SendRpcRequest<string>("GetBlock", new object[] { hash, 0 });

            var blockMsg = new BlockMsg();
            int index = 0;
            blockMsg.Deserialize(Base16.Decode(data), ref index);

            return blockMsg;
        }

        public GetBlockChainInfoOM GetBlockChainInfo()
        {
            return this.SendRpcRequest<GetBlockChainInfoOM>("GetBlockChainInfo");
        }

        public byte[] DumpPrivateKey(string walletAddress, string password)
        {
            var setting = this.SendRpcRequest<GetTxSettingsOM>("GetTxSettings");

            if (setting.Encrypt)
            {
                if (string.IsNullOrWhiteSpace(password) ||
                    !this.SendRpcRequest<bool>("WalletPassphrase", new object[] { password }))
                {
                    throw new Exception("Wallet unlock failed");
                }
            }

            var privateKeyText = this.SendRpcRequest<string>("DumpPrivKey", new object[] { walletAddress });

            if (string.IsNullOrWhiteSpace(privateKeyText))
            {
                throw new Exception("Initial error, please check wallet address and password");
            }

            if(setting.Encrypt)
            {
                this.SendRpcRequest("WalletLock");
            }

            return Base16.Decode(privateKeyText);
        }
    }
}
