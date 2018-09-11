// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using FiiiChain.Data;
using FiiiChain.DataAgent;
using FiiiChain.Entities;
using FiiiChain.Framework;
using FiiiChain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FiiiChain.Business
{
    public class BlockComponent
    {
        public void BlockPoolInitialize()
        {
            //var blockDac = new BlockDac();

            //long lastBlockHeight = -1;
            //string lastBlockHash = Base16.Encode(HashHelper.EmptyHash());
            //var blockEntity = blockDac.SelectLast();

            //if (blockEntity != null)
            //{
            //    lastBlockHeight = blockEntity.Height;
            //    lastBlockHash = blockEntity.Hash;
            //}
        }

        /// <summary>
        /// 创建新的区块
        /// </summary>
        /// <param name="minerName"></param>
        /// <param name="generatorId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public BlockMsg CreateNewBlock(string minerName, string generatorId, string remark = null, string accountId = null)
        {
            var accountDac = new AccountDac();
            var blockDac = new BlockDac();
            var outputDac = new OutputDac();
            var txDac = new TransactionDac();
            var txPool = TransactionPool.Instance;
            var transactionMsgs = new List<TransactionMsg>();

            long lastBlockHeight = -1;
            string lastBlockHash = Base16.Encode(HashHelper.EmptyHash());
            long lastBlockBits = -1;
            string lastBlockGenerator = null;

            //获取最后一个区块
            var blockEntity = blockDac.SelectLast();

            if (blockEntity != null)
            {
                lastBlockHeight = blockEntity.Height;
                lastBlockHash = blockEntity.Hash;
                lastBlockBits = blockEntity.Bits;
                lastBlockGenerator = blockEntity.GeneratorId;
            }

            long totalSize = 0;
            long maxSize = BlockSetting.MAX_BLOCK_SIZE - (1 * 1024);
            var poolItemList = txPool.MainPool.OrderByDescending(t => t.FeeRate).ToList();
            var index = 0;

            while (totalSize < maxSize && index < poolItemList.Count)
            {
                var tx = poolItemList[index].Transaction;

                if (tx != null && transactionMsgs.Where(t=>t.Hash == tx.Hash).Count() == 0)
                {
                    if(txDac.SelectByHash(tx.Hash) == null)
                    {
                        transactionMsgs.Add(tx);
                        totalSize += tx.Size;
                    }
                    else
                    {
                        txPool.RemoveTransaction(tx.Hash);
                    }
                }
                else
                {
                    break;
                }

                index++;
            }


            var minerAccount = accountDac.SelectDefaultAccount();

            if(accountId != null)
            {
                var account = accountDac.SelectById(accountId);

                if(account != null && !string.IsNullOrWhiteSpace(account.PrivateKey))
                {
                    minerAccount = account;
                }
            }

            var minerAccountId = minerAccount.Id;

            BlockMsg newBlockMsg = new BlockMsg();
            BlockHeaderMsg headerMsg = new BlockHeaderMsg();
            headerMsg.Hash = Base16.Encode(HashHelper.EmptyHash());
            headerMsg.GeneratorId = generatorId;
            newBlockMsg.Header = headerMsg;
            headerMsg.Height = lastBlockHeight + 1;
            headerMsg.PreviousBlockHash = lastBlockHash;

            if (headerMsg.Height == 0)
            {

                minerAccountId = BlockSetting.GenesisBlockReceiver;
                remark = BlockSetting.GenesisBlockRemark;
            }

            long totalAmount = 0;
            long totalFee = 0;

            foreach (var tx in transactionMsgs)
            {
                long totalInputsAmount = 0L;
                long totalOutputAmount = 0L;

                foreach (var input in tx.Inputs)
                {
                    var utxo = outputDac.SelectByHashAndIndex(input.OutputTransactionHash, input.OutputIndex);

                    if (utxo != null)
                    {
                        totalInputsAmount += utxo.Amount;
                    }
                }

                foreach (var output in tx.Outputs)
                {
                    totalOutputAmount += output.Amount;
                }

                totalAmount += totalOutputAmount;
                totalFee += (totalInputsAmount - totalOutputAmount);
            }

            //var work = new POW(headerMsg.Height);
            BlockMsg prevBlockMsg = null;
            BlockMsg prevStepBlockMsg = null;

            if(blockEntity != null)
            {
                prevBlockMsg = this.convertEntityToBlockMsg(blockEntity);
            }

            if (headerMsg.Height >= POC.DIFFIUCLTY_ADJUST_STEP)
            {
                prevStepBlockMsg = this.GetBlockMsgByHeight(headerMsg.Height - POC.DIFFIUCLTY_ADJUST_STEP - 1);
            }
            
            var newBlockReward = POC.GetNewBlockReward(headerMsg.Height);

            headerMsg.Bits = POC.CalculateBaseTarget(headerMsg.Height, prevBlockMsg, prevStepBlockMsg);
            headerMsg.TotalTransaction = transactionMsgs.Count + 1;

            var coinbaseTxMsg = new TransactionMsg();
            coinbaseTxMsg.Timestamp = Time.EpochTime;
            coinbaseTxMsg.Locktime = 0;

            var coinbaseInputMsg = new InputMsg();
            coinbaseTxMsg.Inputs.Add(coinbaseInputMsg);
            coinbaseInputMsg.OutputIndex = 0;
            coinbaseInputMsg.OutputTransactionHash = Base16.Encode(HashHelper.EmptyHash());
            coinbaseInputMsg.UnlockScript = Script.BuildMinerScript(minerName, remark);
            coinbaseInputMsg.Size = coinbaseInputMsg.UnlockScript.Length;

            var coinbaseOutputMsg = new OutputMsg();
            coinbaseTxMsg.Outputs.Add(coinbaseOutputMsg);
            coinbaseOutputMsg.Amount = newBlockReward + totalFee;
            coinbaseOutputMsg.LockScript = Script.BuildLockScipt(minerAccountId);
            coinbaseOutputMsg.Size = coinbaseOutputMsg.LockScript.Length;
            coinbaseOutputMsg.Index = 0;

            coinbaseTxMsg.Hash = coinbaseTxMsg.GetHash();

            newBlockMsg.Transactions.Insert(0, coinbaseTxMsg);

            foreach (var tx in transactionMsgs)
            {
                newBlockMsg.Transactions.Add(tx);
            }

            headerMsg.PayloadHash = newBlockMsg.GetPayloadHash();
            var dsa = ECDsa.ImportPrivateKey(Base16.Decode(minerAccount.PrivateKey));
            var signResult = dsa.SingnData(Base16.Decode(headerMsg.PayloadHash));
            headerMsg.BlockSignature = Base16.Encode(signResult);
            headerMsg.BlockSigSize = headerMsg.BlockSignature.Length;
            headerMsg.TotalTransaction = newBlockMsg.Transactions.Count;
            return newBlockMsg;
        }

        /// <summary>
        /// 估算交易费率
        /// </summary>
        /// <returns></returns>
        public long EstimateSmartFee()
        {
            //对象初始化
            var txDac = new TransactionDac();
            var transactionMsgs = new List<TransactionMsg>();
            var txPool = TransactionPool.Instance;
            long totalSize = 0;
            long totalFee = 0;
            //设置最大上限
            long maxSize = BlockSetting.MAX_BLOCK_SIZE - (1 * 1024);
            //交易池中的项目按照费率从高到低排列
            List<TransactionPoolItem> poolItemList = txPool.MainPool.OrderByDescending(t => t.FeeRate).ToList();
            var index = 0;

            while (totalSize < maxSize && index < poolItemList.Count)
            {
                //获取totalFee和totalSize
                TransactionMsg tx = poolItemList[index].Transaction;
                //判断交易Hash是否在交易Msg中
                if (tx != null && transactionMsgs.Where(t => t.Hash == tx.Hash).Count() == 0)
                {
                    totalFee += Convert.ToInt64(poolItemList[index].FeeRate * tx.Serialize().LongLength / 1024.0);
                    if (txDac.SelectByHash(tx.Hash) == null)
                    {
                        transactionMsgs.Add(tx);
                        totalSize += tx.Size;
                    }
                    else
                    {
                        txPool.RemoveTransaction(tx.Hash);
                    }
                }
                /*
                else
                {
                    break;
                }
                */
                index++;
            }
            //获取费率
            if (poolItemList.Count == 0)
            {
                return 1024;
            }
            long feeRate = Convert.ToInt64(Math.Ceiling((totalFee / (totalSize / 1024.0)) / poolItemList.Count));
            if (feeRate < 1024)
            {
                feeRate = 1024;
            }
            return feeRate;
        }

        public void SaveBlockIntoDB(BlockMsg msg)
        {
            try
            {
                VerifyBlock(msg);

                Block block = this.convertBlockMsgToEntity(msg);
                block.IsDiscarded = false;
                block.IsVerified = false;

                var blockDac = new BlockDac();
                var transactionDac = new TransactionDac();
                var inputDac = new InputDac();
                var outputDac = new OutputDac();

                //foreach (var tx in block.Transactions)
                //{
                //    foreach (var input in tx.Inputs)
                //    {
                //        if (input.OutputTransactionHash != Base16.Encode(HashHelper.EmptyHash()))
                //        {
                //            var output = outputDac.SelectByHashAndIndex(input.OutputTransactionHash, input.OutputIndex);

                //            if (output != null)
                //            {
                //                input.AccountId = output.ReceiverId;
                //                outputDac.UpdateSpentStatus(input.OutputTransactionHash, input.OutputIndex);
                //            }
                //        }
                //    }
                //}

                blockDac.Save(block);

                //update nextblock hash
                //blockDac.UpdateNextBlockHash(block.PreviousBlockHash, block.Hash);

                //remove transactions in tx pool
                foreach (var tx in block.Transactions)
                {
                    TransactionPool.Instance.RemoveTransaction(tx.Hash);

                    //save into utxo set
                    foreach (var output in tx.Outputs)
                    {
                        var accountId = AccountIdHelper.CreateAccountAddressByPublicKeyHash(
                                Base16.Decode(
                                    Script.GetPublicKeyHashFromLockScript(output.LockScript)
                                )
                            );

                        UtxoSet.Instance.AddUtxoRecord(new UtxoMsg
                        {
                            AccountId = accountId,
                            BlockHash = block.Hash,
                            TransactionHash = tx.Hash,
                            OutputIndex = output.Index,
                            Amount = output.Amount,
                            IsConfirmed = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                throw ex;
            }

        }

        public bool VerifyBlock(BlockMsg newBlock)
        {
            if (this.exists(newBlock.Header.Hash))
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.BLOCK_HAS_BEEN_EXISTED);
            }

            if (newBlock.Header.Hash != newBlock.Header.GetHash())
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.BLOCK_HASH_ERROR);
            }

            var blockComponent = new BlockComponent();
            var previousBlock = this.GetBlockMsgByHash(newBlock.Header.PreviousBlockHash);

            if(newBlock.Header.Height > 0 && previousBlock == null)
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.PREV_BLOCK_NOT_EXISTED);
            }

            if((newBlock.Header.Timestamp > Time.EpochTime && (newBlock.Header.Timestamp - Time.EpochTime) > 2 * 60 * 60 * 1000) ||
                (previousBlock != null && newBlock.Header.Timestamp <= previousBlock.Header.Timestamp))
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.BLOCK_TIME_IS_ERROR);
            }

            if (newBlock.Serialize().Length > BlockSetting.MAX_BLOCK_SIZE)
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.BLOCK_SIZE_LARGE_THAN_LIMIT);
            }

            var txComponent = new TransactionComponent();

            var newBlockReward = POC.GetNewBlockReward(newBlock.Header.Height);
            var prevStepBlock = GetBlockMsgByHeight(newBlock.Header.Height - POC.DIFFIUCLTY_ADJUST_STEP - 1);
            var minerInfo = string.Empty;

            if (newBlock.Transactions.Count > 0)
            {
                var coinbase = newBlock.Transactions[0];
                var totalFee = 0L;

                minerInfo = Encoding.UTF8.GetString(Base16.Decode(coinbase.Inputs[0].UnlockScript)).Split("`")[0];

                if(newBlock.Transactions.Count > 1)
                {
                    for (int i = 1; i < newBlock.Transactions.Count; i++)
                    {
                        long fee;
                        if (txComponent.VerifyTransaction(newBlock.Transactions[i], out fee))
                        {
                            totalFee += fee;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (coinbase.Inputs.Count != 1 || coinbase.Outputs.Count != 1 || coinbase.Inputs[0].OutputTransactionHash != Base16.Encode(HashHelper.EmptyHash()))
                {
                    throw new CommonException(ErrorCode.Engine.Transaction.Verify.COINBASE_FORMAT_ERROR);
                }
                else
                {
                    var output = coinbase.Outputs[0];

                    if (output.Amount > BlockSetting.OUTPUT_AMOUNT_MAX)
                    {
                        throw new CommonException(ErrorCode.Engine.Transaction.Verify.OUTPUT_EXCEEDED_THE_LIMIT);
                    }

                    if (!Script.VerifyLockScriptFormat(output.LockScript))
                    {
                        throw new CommonException(ErrorCode.Engine.Transaction.Verify.SCRIPT_FORMAT_ERROR);
                    }

                    if(output.Amount != (totalFee + newBlockReward))
                    {
                        throw new CommonException(ErrorCode.Engine.Transaction.Verify.COINBASE_OUTPUT_AMOUNT_ERROR);
                    }
                }
            }

            var pool = new MiningPoolComponent().GetMiningPoolByName(minerInfo);
            
            if(pool == null)
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.MINING_POOL_NOT_EXISTED);
            }

            if (!POC.VerifyBlockSignature(newBlock.Header.PayloadHash, newBlock.Header.BlockSignature, pool.PublicKey))
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.BLOCK_SIGNATURE_IS_ERROR);
            }

            long previousBlockHeight = -1;
            long previousBlockBits = -1;

            if (previousBlock != null)
            {
                previousBlockHeight = previousBlock.Header.Height;
                previousBlockBits = previousBlock.Header.Bits;
            }

            if (POC.CalculateBaseTarget(newBlock.Header.Height, previousBlock, prevStepBlock) != newBlock.Header.Bits)
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.BITS_IS_WRONG);
            }

            var targetResult = POC.CalculateTargetResult(newBlock);
            if (POC.Verify(newBlock.Header.Bits, targetResult))
            {
                return true;
            }
            else
            {
                throw new CommonException(ErrorCode.Engine.Block.Verify.POC_VERIFY_FAIL);
            }
        }

        public long GetLatestHeight()
        {
            var dac = new BlockDac();
            var block = dac.SelectLast();

            if (block != null)
            {
                return block.Height;
            }
            else
            {
                return -1;
            }
        }

        public long GetLatestConfirmedHeight()
        {
            var dac = new BlockDac();
            var block = dac.SelectLastConfirmed();

            if (block != null)
            {
                return block.Height;
            }
            else
            {
                return -1;
            }
        }

        public List<BlockHeaderMsg> GetBlockHeaderMsgByHeights(List<long> heights)
        {
            var blockDac = new BlockDac();
            var txDac = new TransactionDac();
            var headers = new List<BlockHeaderMsg>();

            foreach (var height in heights)
            {
                var items = blockDac.SelectByHeight(height);

                foreach(var entity in items)
                {
                    var header = new BlockHeaderMsg();
                    header.Version = entity.Version;
                    header.Hash = entity.Hash;
                    header.Height = entity.Height;
                    header.PreviousBlockHash = entity.PreviousBlockHash;
                    header.Bits = entity.Bits;
                    header.Nonce = entity.Nonce;
                    header.Timestamp = entity.Timestamp;
                    header.GeneratorId = entity.GeneratorId;
                    //header.GenerationSignature = entity.GenerationSignature;
                    //header.BlockSignature = entity.BlockSignature;
                    //header.CumulativeDifficulty = entity.CumulativeDifficulty;
                    header.PayloadHash = entity.PayloadHash;
                    header.BlockSignature = entity.BlockSignature;
                    header.BlockSigSize = entity.BlockSignature.Length;

                    var transactions = txDac.SelectByBlockHash(entity.Hash);
                    header.TotalTransaction = entity.Transactions == null ? 0 : entity.Transactions.Count;

                    headers.Add(header);
                }
            }

            return headers;
        }

        public List<BlockMsg> GetBlockMsgByHeights(List<long> heights)
        {
            var blockDac = new BlockDac();
            var txDac = new TransactionDac();
            var inputDac = new InputDac();
            var outputDac = new OutputDac();
            var blocks = new List<BlockMsg>();

            foreach (var height in heights)
            {
                var items = blockDac.SelectByHeight(height);

                foreach(var entity in items)
                {
                    blocks.Add(this.convertEntityToBlockMsg(entity));
                }
            }

            return blocks;
        }

        public BlockMsg GetBlockMsgByHeight(long height)
        {
            var blockDac = new BlockDac();
            var txDac = new TransactionDac();
            BlockMsg block = null;

            var items = blockDac.SelectByHeight(height);

            if (items.Count > 0)
            {
                block = this.convertEntityToBlockMsg(items[0]);
            }

            return block;
        }

        public BlockMsg GetBlockMsgByHash(string hash)
        {
            var blockDac = new BlockDac();
            var txDac = new TransactionDac();
            BlockMsg block = null;

            var entity = blockDac.SelectByHash(hash);

            if (entity != null)
            {
                block = this.convertEntityToBlockMsg(entity);
            }

            return block;
        }

        public List<Block> GetPreviousBlocks(long lastHeight, int blockCount)
        {
            return new BlockDac().SelectPreviousBlocks(lastHeight, blockCount);
        }

        public Block GetBlockEntiytByHash(string hash)
        {
            return new BlockDac().SelectByHash(hash);
        }

        public bool CheckBlockExists(string hash)
        {
            return (new BlockDac()).SelectByHash(hash) != null;
        }

        public bool CheckConfirmedBlockExists(long height)
        {
            var result = (new BlockDac()).SelectByHeight(height);

            if(result.Count > 0 && result[0].IsVerified)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Dictionary<Block, long> GetChainTips()
        {
            var dac = new BlockDac();
            var blocks = dac.SelectTipBlocks();

            var dict = new Dictionary<Block, long>();

            if(blocks.Count == 1)
            {
                dict.Add(blocks[0], 0);
            }
            else
            {
                var maxLength = 0;
                Block maxBlock = null;
                foreach (var block in blocks)
                {
                    var len = dac.CountBranchLength(block.Hash);

                    if(len > maxLength)
                    {
                        len = maxLength;
                        maxBlock = block;
                    }

                    dict.Add(block, len);
                }

                if(maxBlock != null && dict.ContainsKey(maxBlock))
                {
                    dict[maxBlock] = 0;
                }
            }

            return dict;
        }

        public string GetMiningWorkResult(BlockMsg block)
        {
            var listBytes = new List<Byte>();
            listBytes.AddRange(Base16.Decode(block.Header.PayloadHash));
            listBytes.AddRange(BitConverter.GetBytes(block.Header.Height));
            var genHash = Sha3Helper.Hash(listBytes.ToArray());
            //POC.CalculateScoopData(block.Header., block.Header.Nonce);
            



            var blockData = new List<byte>();

            foreach (var tx in block.Transactions)
            {
                blockData.AddRange(tx.Serialize());
            }

            var nonceBytes = BitConverter.GetBytes(block.Header.Nonce);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nonceBytes);
            }

            blockData.AddRange(nonceBytes);
            var result = Base16.Encode(
                HashHelper.Hash(
                    blockData.ToArray()
                ));

            return result;
        }

        private Block convertBlockMsgToEntity(BlockMsg blockMsg)
        {
            OutputDac outputDac = new OutputDac();

            var block = new Block();
            block.Hash = blockMsg.Header.Hash;
            block.Version = blockMsg.Header.Version;
            block.Height = blockMsg.Header.Height;
            block.PreviousBlockHash = blockMsg.Header.PreviousBlockHash;
            block.Bits = blockMsg.Header.Bits;
            block.Nonce = blockMsg.Header.Nonce;
            block.GeneratorId = blockMsg.Header.GeneratorId;
            block.Timestamp = blockMsg.Header.Timestamp;
            block.TotalAmount = 0;
            block.TotalFee = 0;

            block.Transactions = new List<Transaction>();

            foreach (var txMsg in blockMsg.Transactions)
            {
                var transaction = new Transaction();

                transaction.Hash = txMsg.Hash;
                transaction.BlockHash = block.Hash;
                transaction.Version = txMsg.Version;
                transaction.Timestamp = txMsg.Timestamp;
                transaction.LockTime = txMsg.Locktime;
                transaction.Inputs = new List<Input>();
                transaction.Outputs = new List<Output>();

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

                    transaction.Inputs.Add(input);
                    totalInput += input.Amount;
                }

                foreach (var outputMsg in txMsg.Outputs)
                {
                    var output = new Output();
                    output.Index = outputMsg.Index;
                    output.TransactionHash = transaction.Hash;
                    var address = AccountIdHelper.CreateAccountAddressByPublicKeyHash(
                        Base16.Decode(
                            Script.GetPublicKeyHashFromLockScript(outputMsg.LockScript)
                        ));
                    output.ReceiverId = address;
                    output.Amount = outputMsg.Amount;
                    output.Size = outputMsg.Size;
                    output.LockScript = outputMsg.LockScript;
                    transaction.Outputs.Add(output);
                    totalOutput += output.Amount;
                }

                transaction.TotalInput = totalInput;
                transaction.TotalOutput = totalOutput;
                transaction.Fee = totalInput - totalOutput;
                transaction.Size = txMsg.Size;

                if (txMsg.Inputs.Count == 1 &&
                    txMsg.Outputs.Count == 1 &&
                    txMsg.Inputs[0].OutputTransactionHash == Base16.Encode(HashHelper.EmptyHash()))
                {
                    transaction.Fee = 0;
                }

                block.Transactions.Add(transaction);
                block.TotalAmount += transaction.TotalOutput;
                block.TotalFee += transaction.Fee;

            }

            //block.GenerationSignature = blockMsg.Header.GenerationSignature;
            block.BlockSignature = blockMsg.Header.BlockSignature;
            //block.CumulativeDifficulty = blockMsg.Header.CumulativeDifficulty;
            block.PayloadHash = blockMsg.Header.PayloadHash;
            block.IsVerified = false;
            return block;
        }

        private BlockMsg convertEntityToBlockMsg(Block entity)
        {
            var txDac = new TransactionDac();
            var inputDac = new InputDac();
            var outputDac = new OutputDac();

            var blockMsg = new BlockMsg();
            var headerMsg = new BlockHeaderMsg();
            headerMsg.Version = entity.Version;
            headerMsg.Hash = entity.Hash;
            headerMsg.Height = entity.Height;
            headerMsg.PreviousBlockHash = entity.PreviousBlockHash;
            headerMsg.Bits = entity.Bits;
            headerMsg.Nonce = entity.Nonce;
            headerMsg.Timestamp = entity.Timestamp;

            var transactions = txDac.SelectByBlockHash(entity.Hash);
            headerMsg.TotalTransaction = transactions == null ? 0: transactions.Count;

            foreach (var tx in transactions)
            {
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

                blockMsg.Transactions.Add(txMsg);
            }

            headerMsg.GeneratorId = entity.GeneratorId;
            //headerMsg.GenerationSignature = entity.GenerationSignature;
            //headerMsg.BlockSignature = entity.BlockSignature;
            //headerMsg.CumulativeDifficulty = entity.CumulativeDifficulty;
            headerMsg.PayloadHash = entity.PayloadHash;
            headerMsg.BlockSignature = entity.BlockSignature;
            headerMsg.BlockSigSize = entity.BlockSignature.Length;
            blockMsg.Header = headerMsg;
            return blockMsg;
        }

        private bool exists(string blockHash)
        {
            var blockDac = new BlockDac();

            if (blockDac.SelectByHash(blockHash) != null)
            {
                return true;
            }

            return false;
        }

        public void ProcessUncsonfirmedBlocks()
        {
            var blockDac = new BlockDac();
            var txDac = new TransactionDac();
            var txComponent = new TransactionComponent();
            var utxoComponent = new UtxoComponent();

            long lastHeight = this.GetLatestHeight();
            long confirmedHeight = -1;
            var confirmedBlock = blockDac.SelectLastConfirmed();
            var confirmedHash = Base16.Encode(HashHelper.EmptyHash());

            if(confirmedBlock != null)
            {
                confirmedHeight = confirmedBlock.Height;
                confirmedHash = confirmedBlock.Hash;
            }

            if(lastHeight - confirmedHeight >= 6)
            {
                var blocks = blockDac.SelectByPreviousHash(confirmedHash);

                if(blocks.Count == 1)
                {
                    blockDac.UpdateBlockStatusToConfirmed(blocks[0].Hash);
                    this.ProcessUncsonfirmedBlocks();
                }
                else if(blocks.Count > 1)
                {
                    var countOfDescendants = new Dictionary<string, long>();
                    foreach (var block in blocks)
                    {
                        var count = blockDac.CountOfDescendants(block.Hash);

                        if(!countOfDescendants.ContainsKey(block.Hash))
                        {
                            countOfDescendants.Add(block.Hash, count);
                        }
                    }

                    var dicSort = countOfDescendants.OrderBy(d => d.Value).ToList();

                    var lastItem = blocks.Where(b => b.Hash == dicSort[dicSort.Count - 1].Key).First();
                    var index = 0;

                    while(index < dicSort.Count - 1)
                    {
                        var currentItem = blocks.Where(b => b.Hash == dicSort[index].Key).First();

                        if(lastHeight - currentItem.Height >= 6)
                        {
                            var txList = txDac.SelectByBlockHash(currentItem.Hash);

                            //Skip coinbase tx
                            for(int i = 1; i < txList.Count; i ++)
                            {
                                var tx = txList[i];
                                var txMsg = txComponent.ConvertTxEntityToMsg(tx);
                                txComponent.AddTransactionToPool(txMsg);
                            }

                            //remove coinbase from utxo
                            UtxoSet.Instance.RemoveUtxoRecord(txList[0].Hash, 0);

                            blockDac.UpdateBlockStatusToDiscarded(currentItem.Hash);
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if(dicSort.Count - index <= 1)
                    {
                        blockDac.UpdateBlockStatusToConfirmed(lastItem.Hash);
                        this.ProcessUncsonfirmedBlocks();
                    }
                }
            }
        }

        public List<Block> GetVerifiedHashes(List<string> hashes)
        {
            BlockDac dac = new BlockDac();
            return dac.DistinguishBlockVerifiedByHashes(hashes);
        }

        public ListSinceBlock ListSinceBlock(string blockHash, long heightLength, long confirmations)
        {
            BlockDac dac = new BlockDac();
            return dac.GetSinceBlock(blockHash, heightLength, confirmations);
        }

        /// <summary>
        /// 获取区块总的奖励
        /// </summary>
        /// <param name="blockHash"></param>
        /// <returns></returns>
        public long GetBlockReward(string blockHash)
        {
            //总的奖励分为两部分，一部分挖矿所得，一部分区块中交易手续费
            BlockDac dac = new BlockDac();
            Block block = dac.SelectByHash(blockHash);
            long miningAward = block.TotalAmount;
            long totalfee = block.TotalFee;
            return miningAward + totalfee;
        }
    }
}
