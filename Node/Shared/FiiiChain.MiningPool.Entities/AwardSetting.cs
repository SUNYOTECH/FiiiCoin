using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.MiningPool.Entities
{
    public class AwardSetting
    {
        /// <summary>
        /// 手续费比例
        /// </summary>
        public double ServiceFeeProportion { get; set; }

        /// <summary>
        /// 提成比例
        /// </summary>
        public double ExtractProportion { get; set; }

        /// <summary>
        /// 提成收款地址
        /// </summary>
        public string ExtractReceiveAddress { get; set; }
    }
}
