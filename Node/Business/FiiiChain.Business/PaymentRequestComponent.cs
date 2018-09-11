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
    public class PaymentRequestComponent
    {
        public PaymentRequest Add(string accountId, string tag, string comment, long amount)
        {
            var paymentRequestDac = new PaymentRequestDac();
            var item = new PaymentRequest();
            item.AccountId = accountId;
            item.Tag = tag;
            item.Comment = comment;
            item.Amount = amount;
            item.Timestamp = Time.EpochTime;

            var id = paymentRequestDac.Insert(item);

            return paymentRequestDac.SelectById(id);
        }

        public void DeleteByIds(long[] ids)
        {
            new PaymentRequestDac().DeleteByIds(ids);
        }

        public List<PaymentRequest> GetAll()
        {
            return new PaymentRequestDac().SelectAll();
        }
    }
}
