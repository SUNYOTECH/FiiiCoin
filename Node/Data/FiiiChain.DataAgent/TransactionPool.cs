// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FiiiChain.Framework;
using FiiiChain.Consensus;

namespace FiiiChain.DataAgent
{
    public class TransactionPool
    {
        private static TransactionPool instance;
        public List<TransactionPoolItem> MainPool;
        public List<TransactionPoolItem> IsolateTransactionPool;
        private readonly string containerName = "Transaction";

        static TransactionPool()
        {
            instance = new TransactionPool();
        }

        public TransactionPool()
        {
            this.MainPool = new List<TransactionPoolItem>();
            this.IsolateTransactionPool = new List<TransactionPoolItem>();

            var keys = Storage.Instance.GetAllKeys(this.containerName);
            foreach(var key in keys)
            {
                try
                {
                    //var item = JsonConvert.DeserializeObject<TransactionPoolItem>(dict[key], new JsonSerializerSettings()
                    //{
                    //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    //});

                    var item = Storage.Instance.GetData<TransactionPoolItem>(this.containerName, key);

                    if(item != null)
                    {
                        if(item.Isolate)
                        {
                            this.IsolateTransactionPool.Add(item);
                        }
                        else
                        {
                            this.MainPool.Add(item);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public static TransactionPool Instance
        {
            get { return instance; }
        }

        public int Count
        {
            get
            {
                return this.MainPool.Count;
            }
        }

        public void AddNewTransaction(long feeRate, TransactionMsg transaction)
        {
            if(this.MainPool.Where(item=>item.Transaction.Hash == transaction.Hash).Count() == 0)
            {
                var item = new TransactionPoolItem(feeRate, transaction);
                item.Isolate = false;
                this.MainPool.Add(item);
                Storage.Instance.PutData(this.containerName, transaction.Hash, item);
                //Storage.Instance.Put(this.containerName, transaction.Hash, JsonConvert.SerializeObject(item, Formatting.Indented, new JsonSerializerSettings() {
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //}));
            }
        }

        public void AddIsolateTransaction(long feeRate, TransactionMsg transaction)
        {
            if (this.MainPool.Where(item => item.Transaction.Hash == transaction.Hash).Count() == 0)
            {
                var item = new TransactionPoolItem(feeRate, transaction);
                item.Isolate = true;
                this.IsolateTransactionPool.Add(item);
                Storage.Instance.PutData(this.containerName, transaction.Hash, item);
                //Storage.Instance.Put(this.containerName, transaction.Hash, JsonConvert.SerializeObject(item, Formatting.Indented, new JsonSerializerSettings()
                //{
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //}));
            }
        }

        public void MoveIsolateTransactionToMainPool(string trasactionHash)
        {
            var txItem = this.IsolateTransactionPool.Where(i => i.Transaction.Hash == trasactionHash).FirstOrDefault();

            if(txItem != null)
            {
                txItem.Isolate = false;
                this.MainPool.Add(txItem);
                this.IsolateTransactionPool.Remove(txItem);
            }
        }

        public TransactionMsg TakeHighestFeeRateTransaction()
        {
            if (MainPool.Count > 0)
            {
                var item = MainPool.OrderByDescending(t => t.FeeRate).FirstOrDefault();

                if(item != null)
                {
                    MainPool.Remove(item);
                    Storage.Instance.Delete(this.containerName, item.Transaction.Hash);

                    return item.Transaction;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void RemoveTransaction(string txHash)
        {
            if (MainPool.Count > 0)
            {
                var item = MainPool.Where(t=>t.Transaction.Hash == txHash).FirstOrDefault();

                if (item != null)
                {
                    MainPool.Remove(item);
                    Storage.Instance.Delete(this.containerName, item.Transaction.Hash);
                }
            }
            else if(IsolateTransactionPool.Count > 0)
            {
                var item = IsolateTransactionPool.Where(t => t.Transaction.Hash == txHash).FirstOrDefault();

                if (item != null)
                {
                    IsolateTransactionPool.Remove(item);
                    Storage.Instance.Delete(this.containerName, item.Transaction.Hash);
                }
            }
        }

        public TransactionMsg GetMaxFeeRateTransaction()
        {
            if(MainPool.Count > 0)
            {
                var item = MainPool.OrderByDescending(t => t.FeeRate).FirstOrDefault();

                if (item != null)
                {
                    return item.Transaction;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public TransactionMsg SearchByTransactionHash(string hash)
        {
            var item = this.MainPool.Where(t => t.Transaction.Hash == hash).Select(t=>t.Transaction).FirstOrDefault();
            return item;
        }

        public List<TransactionMsg> SearchByAccountId(string accountId)
        {
            var publicKeyText = Base16.Encode(
                    AccountIdHelper.GetPublicKeyHash(accountId)
                );

            var result = this.MainPool.Where(t => t.Transaction.Outputs.Where(o => o.LockScript.Contains(publicKeyText)).Count() > 0).
                Select(t => t.Transaction).ToList();

            return result;
        }
        
        public Dictionary<string, List<OutputMsg>> GetTransactionOutputsByAccountId(string accountId)
        {
            var dict = new Dictionary<string, List<OutputMsg>>();

            foreach (var item in this.MainPool)
            {
                var tx = item.Transaction;
                if (!dict.ContainsKey(tx.Hash))
                {
                    dict.Add(tx.Hash, new List<OutputMsg>());
                }

                foreach (var output in tx.Outputs)
                {
                    var publicKeyHash = Script.GetPublicKeyHashFromLockScript(output.LockScript);
                    var id = AccountIdHelper.CreateAccountAddressByPublicKeyHash(Base16.Decode(publicKeyHash));

                    if (id == accountId)
                    {
                        dict[tx.Hash].Add(output);
                    }
                }
            }

            return dict;
        }

        public OutputMsg GetOutputMsg(string transactionHash, int index)
        {
            foreach (var item in this.MainPool)
            {
                var tx = item.Transaction;

                if(tx.Hash == transactionHash && tx.Outputs.Count > index)
                {
                    return tx.Outputs[index];
                }
            }

            return null;
        }

        public bool CheckUTXOSpent(string currentTxHash, string outputTxHash, int outputIndex)
        {
            foreach (var item in this.MainPool)
            {
                var tx = item.Transaction;

                if(tx.Hash == currentTxHash)
                {
                    continue;
                }

                foreach(var input in tx.Inputs)
                {
                    if(input.OutputTransactionHash == outputTxHash && input.OutputIndex == outputIndex)
                    {
                        return true;
                    }
                }
            }

            foreach (var item in this.IsolateTransactionPool)
            {
                var tx = item.Transaction;

                if (tx.Hash == currentTxHash)
                {
                    continue;
                }

                foreach (var input in tx.Inputs)
                {
                    if (input.OutputTransactionHash == outputTxHash && input.OutputIndex == outputIndex)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<TransactionMsg> GetIsolateTransactions()
        {
            return this.IsolateTransactionPool.Select(i=>i.Transaction).ToList();
        }

        public List<string> GetAllTransactionHashes()
        {
            var hashes = new List<string>();

            foreach(var item in this.MainPool)
            {
                hashes.Add(item.Transaction.Hash);
            }

            foreach(var item in this.IsolateTransactionPool)
            {
                hashes.Add(item.Transaction.Hash);
            }

            return hashes;
        }

        public List<TransactionMsg> GetAllTransactions()
        {
            var txList = new List<TransactionMsg>();

            foreach (var item in this.MainPool)
            {
                txList.Add(item.Transaction);
            }

            foreach (var item in this.IsolateTransactionPool)
            {
                txList.Add(item.Transaction);
            }

            return txList;
        }

        public TransactionMsg GetTransactionByHash(string hash)
        {
            foreach (var item in this.MainPool)
            {
                if(item.Transaction.Hash == hash)
                {
                    return item.Transaction;
                }
            }

            foreach (var item in this.IsolateTransactionPool)
            {
                if (item.Transaction.Hash == hash)
                {
                    return item.Transaction;
                }
            }

            return null;
        }

    }
}
