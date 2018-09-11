// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using FiiiChain.Data;
using FiiiChain.DataAgent;
using FiiiChain.Entities;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiiiChain.Business
{
    public class TransactionCommentComponent
    {
        public long Add(string txHash, int outputIndex, string comment)
        {
            var item = new TransactionComment();
            item.TransactionHash = txHash;
            item.OutputIndex = outputIndex;
            item.Comment = comment;
            item.Timestamp = Time.EpochTime;

            return new TransactionCommentDac().Insert(item);
        }

        public TransactionComment GetByTransactionHashAndIndex(string txHash, int outputIndex)
        {
            return new TransactionCommentDac().SelectByTransactionHashAndIndex(txHash, outputIndex);
        }

        public List<TransactionComment> GetByTransactionHash(string txHash)
        {
            return new TransactionCommentDac().SelectByTransactionHash(txHash);
        }
    }
}
