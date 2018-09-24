// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using EdjCase.JsonRpc.Router;
using EdjCase.JsonRpc.Router.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FiiiChain.DTO;
using FiiiChain.Framework;
using FiiiChain.Business;
using FiiiChain.Messages;
using FiiiChain.Consensus;
using FiiiChain.Entities;
using Newtonsoft.Json;
using System.Numerics;
using System.Text;

namespace FiiiChain.Wallet.API
{
    public class BlockChainController : BaseRpcController
    {
        public IRpcMethodResult StopEngine()
        {
            try
            {
                Startup.EngineStopAction();
                return Ok();
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }
        public IRpcMethodResult GetBlockChainStatus()
        {
            try
            {
                var result = Startup.GetEngineJobStatusFunc();
                return Ok(result);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetBlock(string blockHash, int format = 0)
        {
            try
            {
                var blockComponent = new BlockComponent();

                var block = blockComponent.GetBlockMsgByHash(blockHash);

                if(block != null)
                {
                    if(format == 0)
                    {
                        var bytes = block.Serialize();
                        var result = Base16.Encode(bytes);
                        return Ok(result);
                    }
                    else
                    {
                        return Ok(block);
                    }
                }
                else
                {
                    return Ok();
                }
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetBlockCount()
        {
            try
            {
                var blockComponent = new BlockComponent();
                var height = blockComponent.GetLatestHeight();
                return Ok(height);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetBlockHash(long blockHeight)
        {
            try
            {
                var blockComponent = new BlockComponent();
                var block = blockComponent.GetBlockMsgByHeight(blockHeight);

                if(block != null)
                {
                    return Ok(block.Header.Hash);
                }
                else
                {
                    return Ok();
                }
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetBlockHeader(string blockHash, int format = 0)
        {
            try
            {
                var blockComponent = new BlockComponent();
                var block = blockComponent.GetBlockMsgByHash(blockHash);

                if (block != null)
                {
                    if (format == 0)
                    {
                        var bytes = block.Header.Serialize();
                        var result = Base16.Encode(bytes);
                        return Ok(result);
                    }
                    else
                    {
                        return Ok(block.Header);
                    }
                }
                else
                {
                    return Ok();
                }
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetChainTips()
        {
            try
            {
                var dict = new BlockComponent().GetChainTips();
                var result = new List<GetChainTipsOM>();

                foreach(var block in dict.Keys)
                {
                    var item = new GetChainTipsOM();
                    item.height = block.Height;
                    item.hash = block.Hash;
                    item.branchLen = dict[block];

                    if(item.branchLen == 0)
                    {
                        item.status = "active";
                    }
                    else
                    {
                        if(block.IsDiscarded)
                        {
                            item.status = "invalid";
                        }
                        else
                        {
                            item.status = "unknown";
                        }
                    }

                    result.Add(item);
                }

                return Ok(result.ToArray());
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetDifficulty()
        {
            try
            {
                var blockComponent = new BlockComponent();
                var height = blockComponent.GetLatestHeight();
                var newHeight = height + 1;
                
                var previousBlockMsg = blockComponent.GetBlockMsgByHeight(height);
                var prevStepBlockMsg = blockComponent.GetBlockMsgByHeight(newHeight - POC.DIFFIUCLTY_ADJUST_STEP - 1);
                var bits = POC.CalculateBaseTarget(height, previousBlockMsg, prevStepBlockMsg).ToString();

                var result = new GetDifficultyOM()
                {
                    height = newHeight,
                    hashTarget = bits
                };
                return Ok(result);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GenerateNewBlock(string minerName, string address = null, string remark = null, int format = 0)
        {
            try
            {
                var block = new BlockComponent().CreateNewBlock(minerName, address, remark);

                if (block != null)
                {
                    if (format == 0)
                    {
                        var bytes = block.Serialize();

                        var block1 = new BlockMsg();
                        int index = 0;
                        block1.Deserialize(bytes, ref index);
                        var result = Base16.Encode(bytes);
                        return Ok(result);
                    }
                    else
                    {
                        return Ok(block);
                    }
                }
                else
                {
                    return Ok();
                }
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult SubmitBlock(string blockData)
        {
            try
            {
                var bytes = Base16.Decode(blockData);
                var block = new BlockMsg();
                int index = 0;
                try
                {
                    block.Deserialize(bytes, ref index);
                }
                catch
                {
                    throw new CommonException(ErrorCode.Service.BlockChain.BLOCK_DESERIALIZE_FAILED);
                }

                var blockComponent = new BlockComponent();

                var blockInDB = blockComponent.GetBlockMsgByHeight(block.Header.Height);

                if(blockInDB == null)
                {
                    blockComponent.SaveBlockIntoDB(block);
                    Startup.P2PBroadcastBlockHeaderAction(block.Header);
                }
                else
                {
                    throw new CommonException(ErrorCode.Service.BlockChain.SAME_HEIGHT_BLOCK_HAS_BEEN_GENERATED);
                }

                return Ok();
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetBaseTarget(long blockHeight)
        {
            try
            {
                var blockComponent = new BlockComponent();
                BlockMsg lastBlock = null;
                BlockMsg prevStepBlock = null;

                if (blockHeight > 0)
                {
                    lastBlock = blockComponent.GetBlockMsgByHeight(blockHeight - 1);
                    prevStepBlock = blockComponent.GetBlockMsgByHeight(blockHeight - POC.DIFFIUCLTY_ADJUST_STEP);
                }
                long baseTarget;
                if (lastBlock != null)
                    baseTarget = POC.CalculateBaseTarget(blockHeight, lastBlock, prevStepBlock);
                else
                    baseTarget = POC.CalculateBaseTarget(0, null, null);
                return Ok(baseTarget);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetVerifiedHashes(List<string> hashes)
        {
            try
            {
                BlockComponent component = new BlockComponent();
                List<Block> result = component.GetVerifiedHashes(hashes);
                return Ok(result);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        /// <summary>
        /// 估算交易费用
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult EstimateSmartFee()
        {
            try
            {
                BlockComponent block = new BlockComponent();
                long fee = block.EstimateSmartFee();
                return Ok(fee);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取区块的所有奖励，包含挖矿和手续费
        /// </summary>
        /// <param name="blockHash"></param>
        /// <returns></returns>
        public IRpcMethodResult GetBlockReward(string blockHash)
        {
            try
            {
                BlockComponent block = new BlockComponent();
                long reward = block.GetBlockReward(blockHash);
                return Ok(reward);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult SignMessage(string address, string message)
        {
            try
            {
                var account = new AccountComponent().GetAccountById(address);

                if (account == null || string.IsNullOrWhiteSpace(account.PrivateKey))
                {
                    throw new CommonException(ErrorCode.Service.Account.ACCOUNT_NOT_FOUND);
                }

                ECDsa dsa = ECDsa.ImportPrivateKey(Base16.Decode(account.PrivateKey));
                var signResult = Base16.Encode(dsa.SingnData(Encoding.UTF8.GetBytes(message)));

                var result = new
                {
                    signature = signResult,
                    publicKey = account.PublicKey
                };

                return Ok(result);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult VerifyMessage(string publicKey, string signature, string message)
        {
            try
            {
                ECDsa dsa = ECDsa.ImportPublicKey(Base16.Decode(publicKey));
                var result = dsa.VerifyData(Encoding.UTF8.GetBytes(message), Base16.Decode(signature));
                return Ok(result);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch (Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }
    }
}
