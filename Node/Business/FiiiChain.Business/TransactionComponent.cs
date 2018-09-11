// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Messages;
using FiiiChain.Entities;
using FiiiChain.DataAgent;
using System;
using System.Collections.Generic;
using System.Text;
using FiiiChain.Data;
using FiiiChain.Framework;
using FiiiChain.Consensus;
using System.Linq;

namespace FiiiChain.Business
{
    public class TransactionComponent
    {
        public TransactionMsg CreateNewTransactionMsg(string senderAccountId, Dictionary<string, long> receiverIdAndValues, long fee)
        {
            var outputDac = new OutputDac();
            var accountDac = new AccountDac();

            var account = accountDac.SelectById(senderAccountId);

            if(account == null || account.WatchedOnly)
            {
                //TODO: throw exception;
                return null;
            }

            var balance = UtxoSet.Instance.GetAccountBlanace(senderAccountId, true);
            long totalOutput = fee;
            long totalInput = 0;

            foreach(var key in receiverIdAndValues.Keys)
            {
                totalOutput += receiverIdAndValues[key];
            }

            if(totalOutput > balance)
            {
                //TODO: throw exception
                return null;
            }

            var transaction = new TransactionMsg();
            transaction.Timestamp = Time.EpochTime;
            transaction.Locktime = 0;

            var outputs = outputDac.SelectUnspentByReceiverId(senderAccountId);

            foreach(var output in outputs)
            {
                var inputMsg = new InputMsg();
                inputMsg.OutputTransactionHash = output.TransactionHash;
                inputMsg.OutputIndex = output.Index;
                inputMsg.UnlockScript = Script.BuildUnlockScript(output.TransactionHash, output.Index, Base16.Decode(account.PrivateKey), Base16.Decode(account.PublicKey));
                inputMsg.Size = inputMsg.UnlockScript.Length;

                transaction.Inputs.Add(inputMsg);
                outputDac.UpdateSpentStatus(output.TransactionHash, output.Index);

                totalInput += output.Amount;
                if(totalInput >= totalOutput)
                {
                    break;
                }

            }

            int index = 0;

            foreach(var key in receiverIdAndValues.Keys)
            {
                var value = receiverIdAndValues[key];

                var outputMsg = new OutputMsg();
                outputMsg.Index = index;
                outputMsg.Amount = value;
                outputMsg.LockScript = Script.BuildLockScipt(key);
                outputMsg.Size = outputMsg.LockScript.Length;

                transaction.Outputs.Add(outputMsg);
                index++;
            }

            if(totalInput > totalOutput)
            {
                var value = totalInput - totalOutput;

                var outputMsg = new OutputMsg();
                outputMsg.Index = index;
                outputMsg.Amount = value;
                outputMsg.LockScript = Script.BuildLockScipt(senderAccountId);
                outputMsg.Size = outputMsg.LockScript.Length;

                transaction.Outputs.Add(outputMsg);
                index++;
            }

            transaction.Hash = transaction.GetHash();

            return transaction;
        }

        public TransactionMsg CreateNewTransactionMsg(List<UtxoMsg> utxos, Dictionary<string, long> receiverIdAndValues)
        {
            var outputDac = new OutputDac();
            var accountDac = new AccountDac();

            var transaction = new TransactionMsg();
            transaction.Timestamp = Time.EpochTime;
            transaction.Locktime = 0;

            foreach(var utxo in utxos)
            {
                var account = accountDac.SelectById(utxo.AccountId);

                if (account == null || account.WatchedOnly)
                {
                    //TODO: throw exception;
                    return null;
                }

                var privateKey = account.PrivateKey;
                var inputMsg = new InputMsg();
                inputMsg.OutputTransactionHash = utxo.TransactionHash;
                inputMsg.OutputIndex = utxo.OutputIndex;
                inputMsg.UnlockScript = Script.BuildUnlockScript(utxo.TransactionHash, utxo.OutputIndex, Base16.Decode(privateKey), Base16.Decode(account.PublicKey));
                inputMsg.Size = inputMsg.UnlockScript.Length;

                transaction.Inputs.Add(inputMsg);
                outputDac.UpdateSpentStatus(utxo.TransactionHash, utxo.OutputIndex);

            }

            int index = 0;

            foreach (var key in receiverIdAndValues.Keys)
            {
                var value = receiverIdAndValues[key];

                var outputMsg = new OutputMsg();
                outputMsg.Index = index;
                outputMsg.Amount = value;
                outputMsg.LockScript = Script.BuildLockScipt(key);
                outputMsg.Size = outputMsg.LockScript.Length;

                transaction.Outputs.Add(outputMsg);
                index++;
            }

            transaction.Hash = transaction.GetHash();
            return transaction;
        }

        public void AddTransactionToPool(TransactionMsg transaction)
        {
            var accountDac = new AccountDac();
            var outputDac = new OutputDac();
            long feeRate = 0;
            long totalInput = 0;
            long totalOutput = 0;

            foreach(var input in transaction.Inputs)
            {
                long amount;
                string lockCript;
                long blockHeight;

                if(this.getOutput(input.OutputTransactionHash, input.OutputIndex,out amount, out lockCript, out blockHeight))
                {
                    totalInput += amount;
                }
            }

            foreach(var output in transaction.Outputs)
            {
                totalOutput += output.Amount;
            }

            feeRate = (totalInput - totalOutput) / transaction.Size;

            try
            {
                long fee = 0;
                var result = this.VerifyTransaction(transaction, out fee);

                if(result)
                {
                    TransactionPool.Instance.AddNewTransaction(feeRate, transaction);

                    //recheck isolate transactions
                    var txList = TransactionPool.Instance.GetIsolateTransactions();

                    foreach(var tx in txList)
                    {
                        try
                        {
                            if(this.VerifyTransaction(tx, out fee))
                            {
                                TransactionPool.Instance.MoveIsolateTransactionToMainPool(tx.Hash);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    TransactionPool.Instance.AddIsolateTransaction(feeRate, transaction);
                }

                foreach (var input in transaction.Inputs)
                {
                    outputDac.UpdateSpentStatus(input.OutputTransactionHash, input.OutputIndex);
                    UtxoSet.Instance.RemoveUtxoRecord(input.OutputTransactionHash, input.OutputIndex);
                }

                foreach(var output in transaction.Outputs)
                {
                    var accountId = AccountIdHelper.CreateAccountAddressByPublicKeyHash(
                            Base16.Decode(
                                Script.GetPublicKeyHashFromLockScript(output.LockScript)
                            )
                        );

                    UtxoSet.Instance.AddUtxoRecord(new UtxoMsg
                    {
                        AccountId = accountId,
                        BlockHash = null,
                        TransactionHash = transaction.Hash,
                        OutputIndex = output.Index,
                        Amount = output.Amount,
                        IsConfirmed = false
                    });
                }
            }
            catch (CommonException ex)
            {
                throw ex;
            }
        }
        public bool VerifyTransaction(TransactionMsg transaction, out long txFee)
        {
            var blockComponent = new BlockComponent();

            //step 0
            if(transaction.Hash != transaction.GetHash())
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.TRANSACTION_HASH_ERROR);
            }
            //step 1
            if (transaction.InputCount == 0 || transaction.OutputCount == 0)
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.INPUT_AND_OUTPUT_CANNOT_BE_EMPTY);
            }

            //step 2
            if (transaction.Hash == Base16.Encode(HashHelper.EmptyHash()))
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.HASH_CANNOT_BE_EMPTY);
            }

            //step 3
            if (transaction.Locktime < 0 || transaction.Locktime > (Time.EpochTime + BlockSetting.LOCK_TIME_MAX))
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.LOCK_TIME_EXCEEDED_THE_LIMIT);
            }

            //step 4
            if (transaction.Serialize().Length < BlockSetting.TRANSACTION_MIN_SIZE)
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.TRANSACTION_SIZE_BELOW_THE_LIMIT);
            }

            //step 5
            if (this.existsInDB(transaction.Hash))
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.TRANSACTION_HAS_BEEN_EXISTED);
            }

            long totalOutput = 0;
            long totalInput = 0;

            foreach (var output in transaction.Outputs)
            {
                if (output.Amount <= 0 || output.Amount > BlockSetting.OUTPUT_AMOUNT_MAX)
                {
                    throw new CommonException(ErrorCode.Engine.Transaction.Verify.OUTPUT_EXCEEDED_THE_LIMIT);
                }

                if (!Script.VerifyLockScriptFormat(output.LockScript))
                {
                    throw new CommonException(ErrorCode.Engine.Transaction.Verify.SCRIPT_FORMAT_ERROR);
                }

                totalOutput += output.Amount;
            }

            foreach (var input in transaction.Inputs)
            {
                if (!Script.VerifyUnlockScriptFormat(input.UnlockScript))
                {
                    throw new CommonException(ErrorCode.Engine.Transaction.Verify.SCRIPT_FORMAT_ERROR);
                }

                long amount;
                string lockScript;
                long blockHeight;
                var result = this.getOutput(input.OutputTransactionHash, input.OutputIndex, out amount, out lockScript, out blockHeight);

                if (result)
                {
                    var utxoTx = this.GetTransactionMsgByHash(input.OutputTransactionHash);

                    if(utxoTx.InputCount == 1 && utxoTx.Inputs[0].OutputTransactionHash == Base16.Encode(HashHelper.EmptyHash()) && 
                        (blockHeight < 0 || (blockComponent.GetLatestHeight() - blockHeight < 100)))
                    {
                        throw new CommonException(ErrorCode.Engine.Transaction.Verify.COINBASE_NEED_100_CONFIRMS);
                    }

                    if(Time.EpochTime < utxoTx.Locktime)
                    {
                        throw new CommonException(ErrorCode.Engine.Transaction.Verify.TRANSACTION_IS_LOCKED);
                    }

                    //check whether contain two or more same utxo in one transaction
                    var count = transaction.Inputs.Where(i => i.OutputTransactionHash == input.OutputTransactionHash && i.OutputIndex == input.OutputIndex).Count();

                    if (count > 1 || this.checkOutputSpent(transaction.Hash, input.OutputTransactionHash, input.OutputIndex))
                    {
                        throw new CommonException(ErrorCode.Engine.Transaction.Verify.UTXO_HAS_BEEN_SPENT);
                    }

                    if (!Script.VerifyLockScriptByUnlockScript(input.OutputTransactionHash, input.OutputIndex, lockScript, input.UnlockScript))
                    {
                        throw new CommonException(ErrorCode.Engine.Transaction.Verify.UTXO_UNLOCK_FAIL);
                    }

                    totalInput += amount;
                }
                else
                {
                    //not found output, wait for other transactions or blocks;
                    txFee = 0;
                    return false;
                }
            }

            if (totalOutput >= totalInput)
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.OUTPUT_LARGE_THAN_INPUT);
            }

            if ((totalInput - totalOutput) < BlockSetting.TRANSACTION_MIN_FEE)
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.TRANSACTION_FEE_IS_TOO_FEW);
            }

            if (totalInput > BlockSetting.INPUT_AMOUNT_MAX)
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.INPUT_EXCEEDED_THE_LIMIT);
            }

            if (totalOutput > BlockSetting.OUTPUT_AMOUNT_MAX)
            {
                throw new CommonException(ErrorCode.Engine.Transaction.Verify.OUTPUT_EXCEEDED_THE_LIMIT);
            }

            txFee = totalInput - totalOutput;
            return true;
        }

        public List<string> GetAllHashesFromPool()
        {
            return TransactionPool.Instance.GetAllTransactionHashes();
        }

        public List<string> GetAllHashesRelevantWithCurrentWalletFromPool()
        {
            var accountIds = new AccountDac().SelectAll().Select(a => a.Id).ToList();
            var txs = TransactionPool.Instance.GetAllTransactions();
            var result = new List<string>();

            foreach(var tx in txs)
            {
                bool isOK = false;
                var entity = this.ConvertTxMsgToEntity(tx);

                foreach(var input in entity.Inputs)
                {
                    if(accountIds.Contains(input.AccountId))
                    {
                        result.Add(tx.Hash);
                        isOK = true;
                    }
                }

                if(!isOK)
                {
                    foreach(var output in entity.Outputs)
                    {
                        result.Add(tx.Hash);
                        isOK = true;
                    }
                }
            }

            return result;
        }

        public bool CheckTxExisted(string txHash)
        {
            var dac = new TransactionDac();

            if(dac.SelectByHash(txHash) != null)
            {
                return true;
            }
            else
            {
                var hashes = this.GetAllHashesFromPool();

                if(hashes.Contains(txHash))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public TransactionMsg GetTransactionMsgByHash(string txHash)
        {
            var txDac = new TransactionDac();
            var entity = txDac.SelectByHash(txHash);

            if (entity != null)
            {
                return this.ConvertTxEntityToMsg(entity);
            }
            else
            {
                return TransactionPool.Instance.GetTransactionByHash(txHash);
            }
        }

        public TransactionMsg GetTransactionMsgFromDB(string txHash, out string blockHash)
        {
            var txDac = new TransactionDac();
            var entity = txDac.SelectByHash(txHash);
            blockHash = null;

            if(entity != null)
            {
                blockHash = entity.BlockHash;
                return this.ConvertTxEntityToMsg(entity);
            }
            else
            {
                return null;
            }
        }

        public TransactionMsg GetTransactionMsgFromPool(string txHash)
        {
            return TransactionPool.Instance.GetTransactionByHash(txHash);
        }

        public List<Transaction> GetTransactionEntitiesContainUnspentUTXO()
        {
            var items = new TransactionDac().SelectTransactionsContainUnspentUTXO();
            return items;
            //var result = new List<TransactionMsg>();

            //foreach(var item in items)
            //{
            //    result.Add(this.convertTxEntityToMsg(item));
            //}

            //return result;
        }

        public List<Output> GetOutputEntitesByTxHash(string txHash)
        {
            return new OutputDac().SelectByTransactionHash(txHash);
        }

        public Output GetOutputEntiyByIndexAndTxHash(string txHash, int index)
        {
            return new OutputDac().SelectByHashAndIndex(txHash, index);
        }

        public List<Transaction> SearchTransactionEntities(string account, int count, int skip = 0, bool includeWatchOnly = true)
        {
            var inputDac = new InputDac();
            var outputDac = new OutputDac();
            var items = new TransactionDac().SelectTransactions(account, count, skip, includeWatchOnly);
            var accounts = new AccountDac().SelectAll();

            foreach(var item in items)
            {
                item.Inputs = inputDac.SelectByTransactionHash(item.Hash);
                item.Outputs = outputDac.SelectByTransactionHash(item.Hash);
            }

            if(skip == 0)
            {
                var txList = TransactionPool.Instance.GetAllTransactions();
                bool containsUnconfirmedTx = false;

                foreach(var tx in txList)
                {
                    var entity = this.ConvertTxMsgToEntity(tx);
                    bool isSendTransaction = false;
                    if(entity != null)
                    {
                        foreach(var input in entity.Inputs)
                        {
                            if(accounts.Where(a=>a.Id == input.AccountId).Count() > 0)
                            {
                                items.Add(entity);
                                isSendTransaction = true;
                                containsUnconfirmedTx = true;
                                break;
                            }
                        }

                        if(!isSendTransaction)
                        {
                            foreach (var output in entity.Outputs)
                            {
                                if (accounts.Where(a => a.Id == output.ReceiverId).Count() > 0)
                                {
                                    items.Add(entity);
                                    containsUnconfirmedTx = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if(containsUnconfirmedTx)
                {
                    items = items.OrderByDescending(t => t.Timestamp).ToList();
                }
            }
            //var result = new List<TransactionMsg>();

            //foreach(var item in items)
            //{
            //    result.Add(convertTxEntityToMsg(item));
            //}

            //return result;
            return items;
        }

        public Transaction GetTransactionEntityByHash(string hash)
        {
            var inputDac = new InputDac();
            var outputDac = new OutputDac();
            var item = new TransactionDac().SelectByHash(hash);

            if(item != null)
            {
                item.Inputs = inputDac.SelectByTransactionHash(item.Hash);
                item.Outputs = outputDac.SelectByTransactionHash(item.Hash); 
            }
            else
            {
                var msg = TransactionPool.Instance.GetTransactionByHash(hash);

                if(msg != null)
                {
                    item = this.ConvertTxMsgToEntity(msg);
                }
            }

            return item;
        }

        public TransactionMsg ConvertTxEntityToMsg(Transaction tx)
        {
            var inputDac = new InputDac();
            var outputDac = new OutputDac();

            var txMsg = new TransactionMsg();
            txMsg.Version = tx.Version;
            txMsg.Hash = tx.Hash;
            txMsg.Timestamp = tx.Timestamp;
            txMsg.Locktime = tx.LockTime;

            var inputs = inputDac.SelectByTransactionHash(tx.Hash);
            var outputs = outputDac.SelectByTransactionHash(tx.Hash);

            foreach (var input in inputs)
            {
                txMsg.Inputs.Add(new InputMsg
                {
                    OutputTransactionHash = input.OutputTransactionHash,
                    OutputIndex = input.OutputIndex,
                    Size = input.Size,
                    UnlockScript = input.UnlockScript
                });
            }

            foreach (var output in outputs)
            {
                txMsg.Outputs.Add(new OutputMsg
                {
                    Index = output.Index,
                    Amount = output.Amount,
                    Size = output.Size,
                    LockScript = output.LockScript
                });
            }

            return txMsg;
        }

        public Transaction ConvertTxMsgToEntity(TransactionMsg txMsg)
        {
            var outputDac = new OutputDac();
            var entity = new Transaction();

            entity.Hash = txMsg.Hash;
            entity.BlockHash = null;
            entity.Version = txMsg.Version;
            entity.Timestamp = txMsg.Timestamp;
            entity.LockTime = txMsg.Locktime;
            entity.Inputs = new List<Input>();
            entity.Outputs = new List<Output>();

            long totalInput = 0L;
            long totalOutput = 0L;

            foreach (var inputMsg in txMsg.Inputs)
            {
                var input = new Input();
                input.TransactionHash = txMsg.Hash;
                input.OutputTransactionHash = inputMsg.OutputTransactionHash;
                input.OutputIndex = inputMsg.OutputIndex;
                input.Size = inputMsg.Size;
                input.UnlockScript = inputMsg.UnlockScript;

                var output = outputDac.SelectByHashAndIndex(inputMsg.OutputTransactionHash, inputMsg.OutputIndex);

                if (output != null)
                {
                    input.Amount = output.Amount;
                    input.AccountId = output.ReceiverId;
                }

                entity.Inputs.Add(input);
                totalInput += input.Amount;
            }

            foreach (var outputMsg in txMsg.Outputs)
            {
                var output = new Output();
                output.Index = outputMsg.Index;
                output.TransactionHash = entity.Hash;
                var address = AccountIdHelper.CreateAccountAddressByPublicKeyHash(
                    Base16.Decode(
                        Script.GetPublicKeyHashFromLockScript(outputMsg.LockScript)
                    ));
                output.ReceiverId = address;
                output.Amount = outputMsg.Amount;
                output.Size = outputMsg.Size;
                output.LockScript = outputMsg.LockScript;
                entity.Outputs.Add(output);
                totalOutput += output.Amount;
            }

            entity.TotalInput = totalInput;
            entity.TotalOutput = totalOutput;
            entity.Fee = totalInput - totalOutput;
            entity.Size = txMsg.Size;

            if (txMsg.Inputs.Count == 1 &&
                txMsg.Outputs.Count == 1 &&
                txMsg.Inputs[0].OutputTransactionHash == Base16.Encode(HashHelper.EmptyHash()))
            {
                entity.Fee = 0;
            }

            return entity;
        }

        //check whether output is existed. get amount and lockscript from output.
        private bool getOutput(string transactionHash, int outputIndex, out long outputAmount, out string lockScript, out long blockHeight)
        {
            var outputDac = new OutputDac();
            var outputEntity = outputDac.SelectByHashAndIndex(transactionHash, outputIndex);

            if (outputEntity == null)
            {
                long height = -1;
                var outputMsg = TransactionPool.Instance.GetOutputMsg(transactionHash, outputIndex);

                if (outputMsg != null)
                {
                    outputAmount = outputMsg.Amount;
                    lockScript = outputMsg.LockScript;
                    blockHeight = height;
                    return true;
                }
            }
            else
            {
                outputAmount = outputEntity.Amount;
                lockScript = outputEntity.LockScript;
                var tx = new TransactionDac().SelectByHash(outputEntity.TransactionHash);

                if(tx!= null)
                {
                    var blockEntity = new BlockDac().SelectByHash(tx.BlockHash);
                    blockHeight = blockEntity.Height;
                    return true;
                }

            }

            outputAmount = 0;
            lockScript = null;
            blockHeight = -1;
            return false;
        }

        //Check whether output has been spent or contained in another transaction, true = spent, false = unspent
        private bool checkOutputSpent(string currentTxHash, string outputTxHash, int outputIndex)
        {
            var outputDac = new OutputDac();
            var inputDac = new InputDac();
            var txDac = new TransactionDac();
            var blockDac = new BlockDac();

            var outputEntity = outputDac.SelectByHashAndIndex(outputTxHash, outputIndex);
            var inputEntity = inputDac.SelectByOutputHash(outputTxHash, outputIndex);

            if(inputEntity != null && inputEntity.TransactionHash != currentTxHash)
            {
                var tx = txDac.SelectByHash(inputEntity.TransactionHash);

                if (tx != null)
                {
                    var blockEntity = blockDac.SelectByHash(tx.BlockHash);

                    if (blockEntity != null && blockEntity.IsVerified)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool existsInDB(string transactionHash)
        {
            var transactionDac = new TransactionDac();

            if(transactionDac.HasTransactionByHash(transactionHash))
            {
                return true;
            }

            return false;
        }

        private bool existsInTransactionPool(string transactionHash)
        {
            if (TransactionPool.Instance.SearchByTransactionHash(transactionHash) != null)
            {
                return true;
            }

            return false;
        }

    }
}
