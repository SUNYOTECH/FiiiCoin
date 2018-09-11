using System;
using System.Collections.Generic;
using System.Text;
using FiiiChain.MiningPool.Entities;
using FiiiChain.MiningPool.Data;
using FiiiChain.MiningPool.Entities.Tools;
using System.Net.Http.Headers;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.Consensus.Api;
using System.Threading.Tasks;

namespace FiiiChain.MiningPool.Business
{
    public class RewardListComponent
    {
        public List<RewardList> GetAllReward()
        {
            RewardListDac dac = new RewardListDac();
            return dac.SelectAll();
        }

        public RewardList GetRewardById(long id)
        {
            RewardListDac dac = new RewardListDac();
            return dac.SelectById(id);
        }

        public RewardList GetRewardByHash(string hash)
        {
            RewardListDac dac = new RewardListDac();
            return dac.SelectByHash(hash);
        }

        public async Task<RewardList> InsertRewardList(RewardList entity)
        {
            RewardListDac dac = new RewardListDac();
            if (dac.IsExisted(entity.BlockHash))
            {
                throw new Exception("block hash has existed");
            }
            //调接口获取奖励
            AuthenticationHeaderValue authHeaderValue = null;
            RpcClient client = new RpcClient(new Uri("http://localhost:5009"), authHeaderValue, null, null, "application/json");
            RpcRequest request = RpcRequest.WithParameterList("GetBlockReward", new List<object> { entity.BlockHash }, 1);
            RpcResponse response = await client.SendRequestAsync(request);
            if (response.HasError)
            {
                throw new ApiCustomException(response.Error.Code, response.Error.Message);
            }
            long totalReward = response.GetResult<long>();
            RewardList reward = new RewardList();
            ConfigurationTool tool = new ConfigurationTool();
            AwardSetting setting = tool.GetAppSettings<AwardSetting>("AwardSetting");
            double extractProportion = setting.ExtractProportion;
            double serviceFeeProportion = setting.ServiceFeeProportion;
            reward.ActualReward = Convert.ToInt64(entity.OriginalReward * (1 - (extractProportion + serviceFeeProportion)));
            reward.BlockHash = entity.BlockHash;
            reward.GenerateTime = entity.GenerateTime;
            reward.Hashes = entity.Hashes;
            reward.MinerAddress = entity.MinerAddress;
            reward.OriginalReward = totalReward;
            reward.Paid = 0;
            reward.PaidTime = Framework.Time.EpochStartTime.Millisecond;
            reward.TransactionHash = entity.TransactionHash;

            dac.Insert(reward);

            return reward;
        }

        public void UpdatePaidStatus(long id)
        {
            RewardListDac dac = new RewardListDac();
            dac.UpdatePaid(id, 1);
        }

        public void UpdatePaidStatus(string hash)
        {
            RewardListDac dac = new RewardListDac();
            dac.UpdatePaid(hash, 1);
        }

        public void DeleteReward(long id)
        {
            RewardListDac dac = new RewardListDac();
            dac.Delete(id);
        }

        public void DeleteReward(string hash)
        {
            RewardListDac dac = new RewardListDac();
            dac.Delete(hash);
        }

        /// <summary>
        /// 获取单个矿工的未发放的奖励
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public List<RewardList> GetUnPaidReward(string address)
        {
            RewardListDac dac = new RewardListDac();
            return dac.GetUnPaidReward(address, 0);
        }

        /// <summary>
        /// 获取单个矿工的已发放的奖励
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public List<RewardList> GetPaidReward(string address)
        {
            RewardListDac dac = new RewardListDac();
            return dac.GetPaidReward(address, 1);
        }

        public List<RewardList> GetAllUnPaidReward()
        {
            RewardListDac dac = new RewardListDac();
            return dac.GetAllUnPaidReward();
        }

        public long GetActualReward(string address, string blockHash)
        {
            RewardListDac dac = new RewardListDac();
            return dac.GetActualReward(address, blockHash);
        }
    }
}
