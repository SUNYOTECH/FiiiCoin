using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using System.Threading.Tasks;
using FiiiChain.Consensus.Api;
using FiiiChain.DTO;
using FiiiChain.Framework;

namespace FiiiChain.MiningPool.Award
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /* 奖励发放设计思路
             * 1) 找出所有已经确认区块的待发放奖励（Block.Confirmed = true, Reward.Paid = false）
             * 2）发放金额为实际金额 *（1-10%）
             * 3) 循环发放，将n个矿工组合在一条交易中 （Send Many)
             * 4）n以及交易数据大小不能超出限制(需要考虑分批发放了)
             * 5）发起给Fiiipay WalletAddress的转账交易
             * 6) 更改数据库状态
             * 7) 放入操作系统的计划任务中定时执行(一周一次）
             */
            try
            {
                RewardListComponent component = new RewardListComponent();
                Tools.ConfigurationTool tool = new Tools.ConfigurationTool();
                AwardSetting setting = tool.GetAppSettings<AwardSetting>("AwardSetting");
                List<RewardList> rewardList = component.GetAllUnPaidReward();
                List<SendManyOutputIM> many = new List<SendManyOutputIM>();
                long totalExtractAward = 0;
                foreach (var item in rewardList)
                {
                    totalExtractAward += item.ActualReward;
                    many.Add(new SendManyOutputIM { address = item.MinerAddress, amount = item.ActualReward, comment = "", tag = "" });
                }
                //
                many.Add(new SendManyOutputIM { address = setting.ExtractReceiveAddress, amount = Convert.ToInt64(totalExtractAward * setting.ExtractProportion), comment = "", tag = "" });
                //调用SendMany接口发放奖励
                AuthenticationHeaderValue authHeaderValue = null;
                RpcClient client = new RpcClient(new Uri("http://localhost:5009"), authHeaderValue, null, null, "application/json");
                RpcRequest request = RpcRequest.WithParameterList("SendMany", new List<object> { "", many, null }, 1);
                RpcResponse response = await client.SendRequestAsync(request);
                if (response.HasError)
                {
                    LogHelper.Error(response.Error.Message);
                    throw new ApiCustomException(response.Error.Code, response.Error.Message);
                }
                //更新数据库状态
                foreach (var item in rewardList)
                {
                    component.UpdatePaidStatus(item.Id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }
    }
}
