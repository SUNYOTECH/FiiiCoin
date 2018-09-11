// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FiiiChain.DataAgent;
using FiiiChain.Messages;
using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.Entities;

namespace FiiiChain.Business
{
    public class UtxoComponent
    {
        public void Initialize()
        {
            var accountDac = new AccountDac();
            var accountIds = accountDac.SelectAll().Select(a=>a.Id).ToArray();

            UtxoSet.Initialize(accountIds);

            foreach(var id in accountIds)
            {
                this.RefreshUtxoSet(id);
            }
        }

        public void AddMonitoredAccountId(string accountId)
        {
            if(UtxoSet.Instance != null)
            {
                UtxoSet.Instance.AddAccountId(accountId);
            }
        }

        public void RemoveAccountId(string accountId)
        {
            if (UtxoSet.Instance != null)
            {
                UtxoSet.Instance.RemoveAccountId(accountId);
            }
        }

        public void AddNewUtxoToSet(UtxoMsg utxo)
        {
            if (UtxoSet.Instance != null)
            {
                UtxoSet.Instance.AddUtxoRecord(utxo);
            }
        }

        public void RemoveUtxoFromSet(string transactionHash, int outputIndex)
        {
            if (UtxoSet.Instance != null)
            {
                UtxoSet.Instance.RemoveUtxoRecord(transactionHash, outputIndex);
            }
        }

        public void RefreshUtxoSet(string accountId)
        {
            var outputDac = new OutputDac();
            var accountDac = new AccountDac();
            var txDac = new TransactionDac();

            var account = accountDac.SelectById(accountId);

            if (account != null && UtxoSet.Instance != null)
            {
                var set = UtxoSet.Instance.GetUtxoSetByAccountId(accountId);

                if(set != null)
                {
                    set.Clear();

                    //load from database
                    var outputsInDB = outputDac.SelectUnspentByReceiverId(accountId);

                    foreach(var output in outputsInDB)
                    {
                        var msg = new UtxoMsg();
                        msg.AccountId = output.ReceiverId;
                        msg.TransactionHash = output.TransactionHash;
                        msg.OutputIndex = output.Index;
                        msg.Amount = output.Amount;
                        msg.IsConfirmed = true;
                        msg.IsWatchedOnly = account.WatchedOnly;

                        var txEntity = txDac.SelectByHash(output.TransactionHash);
                        msg.BlockHash = txEntity != null ? txEntity.BlockHash : null;

                        set.Add(msg);
                    }

                    //load from transaction pool
                    var outputsInTxPool = TransactionPool.Instance.GetTransactionOutputsByAccountId(accountId);

                    foreach (var txHash in outputsInTxPool.Keys)
                    {
                        foreach (var output in outputsInTxPool[txHash])
                        {
                            var msg = new UtxoMsg();
                            msg.AccountId = accountId;
                            msg.TransactionHash = txHash;
                            msg.OutputIndex = output.Index;
                            msg.Amount = output.Amount;
                            msg.IsConfirmed = false;
                            msg.IsWatchedOnly = account.WatchedOnly;

                            set.Add(msg);
                        }
                    }

                }
            }
        }

        private void RefreshWholeUtxoSet()
        {
            var accountIds = UtxoSet.Instance.GetAllAccountIds();

            foreach(var accountId in accountIds)
            {
                this.RefreshUtxoSet(accountId);
            }
        }

        public long GetConfirmedBlanace(string accountId)
        {
            this.RefreshUtxoSet(accountId);
            return UtxoSet.Instance.GetAccountBlanace(accountId, true);
        }

        public long GetUnconfirmedBalance(string accountId)
        {
            this.RefreshUtxoSet(accountId);
            return UtxoSet.Instance.GetAccountBlanace(accountId, false);
        }

        public long GetOutputCounts()
        {
            return new OutputDac().CountSelfUnspentOutputs();
        }

        public long GetTransactionCounts()
        {
            return new TransactionDac().CountSelfUnspentTransactions();
        }

        public void GetBalanceInDB(out long confirmedBalance, out long unconfirmedBalance)
        {
            //return new OutputDac().SumSelfUnspentOutputs();
            confirmedBalance = 0;
            unconfirmedBalance = 0;
            var outputDac = new OutputDac();
            var txDac = new TransactionDac();
            var blockDac = new BlockDac();

            var lastHeight = -1L;
            var lastBlock = blockDac.SelectLast();

            if(lastBlock != null)
            {
                lastHeight = lastBlock.Height;
            }

            var outputs = outputDac.SelectAllUnspentOutputs();

            foreach(var output in outputs)
            {
                var tx = txDac.SelectByHash(output.TransactionHash);

                if(tx != null)
                {
                    var block = blockDac.SelectByHash(tx.BlockHash);

                    if(block != null)
                    {
                        if(tx.TotalInput == 0)
                        {
                            //coinbase
                            if(lastHeight - block.Height >= 100)
                            {
                                confirmedBalance += output.Amount;
                            }
                            else
                            {
                                unconfirmedBalance += output.Amount;
                            }
                        }
                        else
                        {
                            if(block.IsVerified)
                            {

                                if (Time.EpochTime >= tx.LockTime)
                                {
                                    confirmedBalance += output.Amount;
                                }
                                else
                                {
                                    unconfirmedBalance += output.Amount;
                                }

                            }
                            else
                            {
                                unconfirmedBalance += output.Amount;
                            }
                        }
                    }
                }
            }
        }

        //public List<UtxoMsg> GetAllConfirmedOutputs()
        //{
        //    return UtxoSet.Instance.GetAllUnspentOutputs();
        //}


        public List<UtxoMsg> GetAllConfirmedOutputs()
        {
            List<UtxoMsg> utxoMsgs = new List<UtxoMsg>();

            var outputDac = new OutputDac();
            var txDac = new TransactionDac();
            var blockDac = new BlockDac();
            var accountDac = new AccountDac();

            var lastHeight = -1L;
            var lastBlock = blockDac.SelectLast();

            if (lastBlock != null)
            {
                lastHeight = lastBlock.Height;
            }
            var outputs = outputDac.SelectAllUnspentOutputs();
            
            foreach (var output in outputs)
            {
                var msg = new UtxoMsg();
                msg.AccountId = output.ReceiverId;
                msg.TransactionHash = output.TransactionHash;
                msg.OutputIndex = output.Index;
                msg.Amount = output.Amount;
                msg.IsConfirmed = true;
                var account = accountDac.SelectById(msg.AccountId);
                msg.IsWatchedOnly = account.WatchedOnly;

                var txEntity = txDac.SelectByHash(output.TransactionHash);
                msg.BlockHash = txEntity != null ? txEntity.BlockHash : null;
                utxoMsgs.Add(msg);
            }

            return utxoMsgs;
        }

        //result is List<txid + "," + vout>
        public long GetAllUnconfirmedBalance()
        {
            var accountDac = new AccountDac();
            long totalAmount = 0;
            foreach(var item in TransactionPool.Instance.MainPool)
            {
                foreach(var output in item.Transaction.Outputs)
                {
                    var address = AccountIdHelper.CreateAccountAddressByPublicKeyHash(
                        Base16.Decode(
                            Script.GetPublicKeyHashFromLockScript(output.LockScript)));

                    if(accountDac.SelectById(address) != null)
                    {
                        totalAmount += output.Amount;
                    }
                }
            }

            return totalAmount;
        }
    }
}
