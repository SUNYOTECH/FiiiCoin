using FiiiChain.Consensus.Api;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FiiiChain.MiningPool.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MinersController : BaseController
    {
        /// <summary>
        /// Pos信息校验
        /// </summary>
        /// <param name="miners"></param>
        /// <returns></returns>
        [HttpPost]
        public CommonResponse POSValidate([FromBody]Miners miners)
        {
            try
            {
                MinersComponent component = new MinersComponent();
                bool result = component.MinerLogin(miners.Address, miners.SN);
                return OK(result);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }

        /// <summary>
        /// 保存矿工信息到数据库
        /// </summary>
        /// <param name="miners"></param>
        /// <returns></returns>
        [HttpPost]
        public CommonResponse SaveMiners([FromBody]Miners miners)
        {
            try
            {
                MinersComponent component = new MinersComponent();
                Miners entity = component.RegisterMiner(miners.Address, miners.Account, miners.SN);
                return OK(entity);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }
    }
}