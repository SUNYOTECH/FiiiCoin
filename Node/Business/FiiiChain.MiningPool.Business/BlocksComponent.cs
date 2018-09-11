using System;
using System.Collections.Generic;
using FiiiChain.MiningPool.Entities;
using FiiiChain.MiningPool.Data;
using System.Net.Http.Headers;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using System.Threading.Tasks;
using FiiiChain.Consensus.Api;
using FiiiChain.Entities;

namespace FiiiChain.MiningPool.Business
{
    public class BlocksComponent
    {
        public List<Blocks> GetAllBlocks()
        {
            BlocksDac dac = new BlocksDac();
            return dac.SelectAll();
        }

        public Blocks GetBlockById(long id)
        {
            BlocksDac dac = new BlocksDac();
            return dac.SelectById(id);
        }

        public Blocks GetBlockByHash(string hash)
        {
            BlocksDac dac = new BlocksDac();
            return dac.SelectByHash(hash);
        }

        public async Task<Blocks> SaveBlock(Blocks entity)
        {
            BlocksDac dac = new BlocksDac();
            if (dac.IsExisted(entity.Hash))
            {
                throw new Exception("block has existed");
            }
            //调接口获取奖励
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5009"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("GetBlockReward", new List<object> { entity.Hash }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            long totalReward = response.GetResult<long>();
            Blocks block = new Blocks();
            block.Confirmed = 0;
            block.Generator = entity.Generator;
            block.Hash = entity.Hash;
            block.Height = entity.Height;
            block.Nonce = entity.Nonce;
            block.Timstamp = Framework.Time.EpochTime;
            block.TotalHash = entity.TotalHash;
            block.TotalReward = totalReward;

            dac.Insert(block);
            return block;
        }

        public void UpdateBlockConfirmed(long id, int confirmed, int isDiacarded)
        {
            BlocksDac dac = new BlocksDac();
            dac.UpdateConfirmed(id, confirmed, isDiacarded);
        }

        public void UpdateBlockConfirmed(string hash, int confirmed, int isDiacarded)
        {
            BlocksDac dac = new BlocksDac();
            dac.UpdateConfirmed(hash, confirmed, isDiacarded);
        }

        public void DeleteBlock(long id)
        {
            BlocksDac dac = new BlocksDac();
            dac.Delete(id);
        }

        public void DeleteBlock(string hash)
        {
            BlocksDac dac = new BlocksDac();
            dac.Delete(hash);
        }


        /* 实现思路
         * 1、调取JsonRpc接口获取当前区块高度
         * 2、根据当前区块高度排除数据库中6个以内的区块（因为一定是未确认的）
         * 3、拿出剩余未确认的区块，调取Rpc接口判断区块是否被确认
         * 4、批量更新数据库的确认状态和是否作废状态
         * 
         * 备注：Rpc接口判断区块是否被确认这个接口需要自己用Rpc写
         * 接口：根据传入的区块Hash判断是否区块是否被确认
         * 接口返回值：返回被确认的区块Hash
         */
        
        /// <summary>
        /// 更新区块的确认状态和抛弃状态
        /// </summary>
        /// <returns></returns>
        public async Task GetVerifiedHashes()
        {
            //不能直接调用FiiiChain.Bussiness，需要使用JsonRpc调用接口
            //先通过JsonRpc获取当前区块高度
            BlocksDac dac = new BlocksDac();
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5009"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithNoParameters("GetBlockCount", 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            long responseValue = response.GetResult<long>();
            if (responseValue - 6 > 0)
            {
                //根据responseValue获取数据库中高度小于responseValue - 6的所有Hash值
                List<string> hashes = dac.GetAppointedHash(responseValue - 6);
                RpcRequest requestHash = RpcRequest.WithParameterList("GetVerifiedHashes", new List<object>{ hashes }, 1);
                RpcResponse responseHash = await client.SendRequestAsync(requestHash);
                if (responseHash.HasError)
                {
                    throw new ApiCustomException(response.Error.Code, response.Error.Message);
                }
                List<Block> list = responseHash.GetResult<List<Block>>();
                //根据list的值批量更新数据库
                foreach (var item in list)
                {
                    UpdateBlockConfirmed(item.Hash, item.IsVerified ? 1 : 0, item.IsDiscarded ? 1 : 0);
                }
            }
        }
    }
}
