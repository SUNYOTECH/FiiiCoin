﻿using FiiiChain.Entities;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FiiiChain.Messages
{
    public class BlockMsg : BasePayload
    {
        public BlockMsg()
        {
            this.Transactions = new List<TransactionMsg>();
        }


        public string GetPayloadHash()
        {
            var hashsBytes = this.Transactions.Select(tx => Base16.Decode(tx.Hash)).ToList();

            var bytes = MerkleTree.Hash(hashsBytes);

            return Base16.Encode(
                HashHelper.Hash(
                    bytes.ToArray()
            ));
        }

        public BlockHeaderMsg Header { get; set; }
        public List<TransactionMsg> Transactions { get; set; }

        public override void Deserialize(byte[] bytes, ref int index)
        {
            this.Header = new BlockHeaderMsg();
            this.Header.Deserialize(bytes, ref index);

            var txIndex = 0;
            while (txIndex < this.Header.TotalTransaction)
            {
                var transactionMsg = new TransactionMsg();
                transactionMsg.Deserialize(bytes, ref index);
                this.Transactions.Add(transactionMsg);

                txIndex++;
            }
        }

        public override byte[] Serialize()
        {
            var bytes = new List<byte>();
            bytes.AddRange(Header.Serialize());

            foreach (var tx in Transactions)
            {
                bytes.AddRange(tx.Serialize());
            }

            return bytes.ToArray();
        }
    }
}
