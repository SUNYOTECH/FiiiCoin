// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiiiCoin.Wallet.Win.Models
{
    public class BlockSyncInfo : NotifyBase
    {
        private long _blockLeft;
        private double _progress;
        private long _timeLeft;
        private long _startTimeOffset = 0;

        public long ConnectCount { get; set; }

        
        public long BlockLeft
        {
            get
            {
                return _blockLeft;
            }
            set
            {
                _blockLeft = value;
                RaisePropertyChanged("BlockLeft");
            }
        }

        
        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                RaisePropertyChanged("Progress");
            }
        }

        
        public long TimeLeft
        {
            get
            {
                return _timeLeft;
            }
            set
            {
                _timeLeft = value;
                RaisePropertyChanged("TimeLeft");
            }
        }

        
        public long AllBlockHeight = 0;

        
        public long StartTimeOffset
        {
            get { return _startTimeOffset; }
            set { _startTimeOffset = value; RaisePropertyChanged("StartTimeOffset"); }
        }

        
        public long beforeLocalLastBlockHeight = 0;

        public long needUpdateBlocksHeight = 0;


        public bool IsSyncComplete()
        {
            var x = this;
            var result = x.BlockLeft == 0 && x.Progress == 100 && x.StartTimeOffset >= 0 && x.AllBlockHeight > 0;
            return result;
        }

        public bool IsSyncStart()
        {
            var blockSyncInfo = this;
            var result = blockSyncInfo.BlockLeft > 0 || (blockSyncInfo.BlockLeft == 0 && blockSyncInfo.AllBlockHeight > 0 && blockSyncInfo.Progress == 100 && blockSyncInfo.StartTimeOffset >= 0);
            return result;
        }
    }
}
