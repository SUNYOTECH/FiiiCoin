// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

namespace FiiiCoin.Models
{
    public class TxFeeForSend : BaseModel
    {
        private long totalSize;
        public long TotalSize
        {
            get { return totalSize; }
            set
            {
                totalSize = value;
                OnChanged("TotalSize");
            }
        }

        private long totalFee;
        public long TotalFee
        {
            get { return totalFee; }
            set
            {
                totalFee = value;
                OnChanged("TotalFee");
            }
        }
    }
}
