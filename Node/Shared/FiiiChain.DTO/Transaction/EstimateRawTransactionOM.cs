﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.DTO.Transaction
{
    public class EstimateRawTransactionOM
    {
        /// <summary>
        /// 总数据量
        /// </summary>
        public int totalSize { get; set; }

        /// <summary>
        /// 总手续费
        /// </summary>
        public long totalFee { get; set; }

        /// <summary>
        /// 找零
        /// </summary>
        public long Change { get; set; }

    }
}
