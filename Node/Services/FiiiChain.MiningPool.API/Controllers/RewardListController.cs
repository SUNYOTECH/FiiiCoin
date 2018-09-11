using FiiiChain.Consensus.Api;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiiiChain.MiningPool.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RewardListController : BaseController
    {
        /// <summary>
        /// 根据地址获取未付款的奖励信息
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost]
        public CommonResponse GetUnPaidReward([FromBody]string address)
        {
            try
            {
                /* 设计思路
                 * 1、根据地址获取所有没有支付的奖励，如果区块作废，需要排除
                 */
                RewardListComponent component = new RewardListComponent();
                List<RewardList> list = component.GetUnPaidReward(address);
                long result = 0;
                foreach (var item in list)
                {
                    result += item.ActualReward;
                }
                return OK(result);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }

        /// <summary>
        /// 根据地址获取已付款的奖励信息
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost]
        public CommonResponse GetPaidReward([FromBody]string address)
        {
            try
            {
                /* 设计思路
                 * 1、根据地址获取所有已经支付的奖励
                 */
                RewardListComponent component = new RewardListComponent();
                List<RewardList> list = component.GetUnPaidReward(address);
                long result = 0;
                foreach (var item in list)
                {
                    result += item.ActualReward;
                }
                return OK(result);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }

        /// <summary>
        /// 保存奖励信息到数据库
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<CommonResponse> SaveReward([FromBody]RewardList entity)
        {
            try
            {
                RewardListComponent component = new RewardListComponent();
                await component.InsertRewardList(entity);
                return OK();
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }

        /// <summary>
        /// 根据地址和区块哈希获取实际的奖励信息
        /// </summary>
        /// <param name="address"></param>
        /// <param name="blockHash"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        public CommonResponse GetActualReward(string address, string blockHash)
        {
            try
            {
                RewardListComponent component = new RewardListComponent();
                long result = component.GetActualReward(address, blockHash);
                return OK(result);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }
    }
}
