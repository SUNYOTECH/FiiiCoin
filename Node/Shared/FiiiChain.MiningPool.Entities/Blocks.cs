using System;

namespace FiiiChain.MiningPool.Entities
{
    public class Blocks
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 区块Hash
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// 区块高度
        /// </summary>
        public long Height { get; set; }

        /// <summary>
        /// 区块生成时间戳
        /// </summary>
        public long Timstamp { get; set; }

        /// <summary>
        /// 生成区块矿工钱包地址
        /// </summary>
        public string Generator { get; set; }

        /// <summary>
        /// 生成区块随机数
        /// </summary>
        public long Nonce { get; set; }

        /// <summary>
        /// 总的奖励
        /// </summary>
        public long TotalReward { get; set; }

        /// <summary>
        /// 总的Hash，总工作量
        /// </summary>
        public long TotalHash { get; set; }

        /// <summary>
        /// 是否确认，检查当前区块是否有效 0:无效，1：有效
        /// </summary>
        public int Confirmed { get; set; }

        /// <summary>
        /// 区块是否作废，0：正常，1：已作废
        /// </summary>
        public int IsDiscarded { get; set; }
    }
}
