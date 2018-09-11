using FiiiChain.Consensus.Api;
using FiiiChain.MiningPool.Business;
using FiiiChain.MiningPool.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FiiiChain.MiningPool.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlocksController : BaseController
    {
        /// <summary>
        /// 保存区块信息到数据库
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<CommonResponse> SaveBlocks([FromBody]Blocks entity)
        {
            try
            {
                BlocksComponent component = new BlocksComponent();
                await component.SaveBlock(entity);
                return OK();
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }

        /// <summary>
        /// 自动同步区块验证状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<CommonResponse> GetVerifiedHashes()
        {
            try
            {
                BlocksComponent component = new BlocksComponent();
                await component.GetVerifiedHashes();
                return OK();
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }
    }
}
