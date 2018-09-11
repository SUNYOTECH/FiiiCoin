// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using EdjCase.JsonRpc.Router.Abstractions;
using FiiiChain.Business;
using FiiiChain.Consensus;
using FiiiChain.DTO;
using FiiiChain.Framework;
using FiiiChain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiiiChain.Wallet.API
{
    public class UtxoController : BaseRpcController
    {
        public IRpcMethodResult GetTxOut(string txid, int vount, bool unconfirmed = false)
        {
            try
            {
                GetTxOutOM result = null;

                var txComponent = new TransactionComponent();
                var blockComponent = new BlockComponent();
                string blockHash = null;
                TransactionMsg tx = txComponent.GetTransactionMsgFromDB(txid, out blockHash);

                if(tx == null && unconfirmed)
                {
                    tx = txComponent.GetTransactionMsgFromPool(txid);
                }


                if (tx !=  null && tx.OutputCount > vount)
                {
                    var output = tx.Outputs[vount];
                    long confirmations = 0;

                    if(!string.IsNullOrWhiteSpace(blockHash))
                    {
                        var block = blockComponent.GetBlockMsgByHash(blockHash);

                        if(block != null)
                        {
                            var latestHeight = blockComponent.GetLatestHeight();

                            if(latestHeight > block.Header.Height)
                            {
                                confirmations = latestHeight - block.Header.Height;
                            }
                        }
                    }

                    result = new GetTxOutOM();
                    result.bestblock = blockHash;
                    result.confirmations = confirmations;
                    result.value = output.Amount;
                    result.scriptPubKey = output.LockScript;
                    result.version = tx.Version;
                    result.coinbase = (tx.InputCount == 0 && tx.Inputs[0].OutputTransactionHash == Base16.Encode(HashHelper.EmptyHash()));
                }

                return Ok(result);
            }
            catch (CommonException ce)
            {
                return Error(ce.ErrorCode, ce.Message, ce);
            }
            catch(Exception ex)
            {
                return Error(ErrorCode.UNKNOWN_ERROR, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetTxOutSetInfo()
        {
            try
            {
                GetTxOutSetInfoOM result = new GetTxOutSetInfoOM();
                var blockComponent = new BlockComponent();
                var txComponent = new TransactionComponent();
                var utxoComponent = new UtxoComponent();

                result.height = blockComponent.GetLatestHeight();
                result.bestblock = null;

                if(result.height >= 0)
                {
                    var bestBlock = blockComponent.GetBlockMsgByHeight(result.height);
                    result.bestblock = bestBlock != null ? bestBlock.Header.Hash : null;
                }

                result.transactions = utxoComponent.GetTransactionCounts();
                result.txouts = utxoComponent.GetOutputCounts();

                long confirmedBalance, unconfirmedBalance;
                utxoComponent.GetBalanceInDB(out confirmedBalance, out unconfirmedBalance);
                result.total_amount = confirmedBalance;
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

        public IRpcMethodResult ListUnspent(int minConfirmations, int maxConfirmations = 9999999, string[] addresses = null)
        {
            try
            {
                var result = new List<ListUnspentOM>();
                var txComponent = new TransactionComponent();
                var blockComponent = new BlockComponent();
                var outputComponent = new UtxoComponent();
                var accountComponent = new AccountComponent();
                var addressBookComponent = new AddressBookComponent();

                var accounts = accountComponent.GetAllAccounts();
                var transactions = txComponent.GetTransactionEntitiesContainUnspentUTXO();
                var latestHeight = blockComponent.GetLatestHeight();

                foreach(var tx in transactions)
                {
                    var blockMsg = blockComponent.GetBlockMsgByHash(tx.BlockHash);

                    if(blockMsg != null)
                    {
                        var confirmations = latestHeight - blockMsg.Header.Height;

                        if(confirmations >= minConfirmations && confirmations <= maxConfirmations)
                        {
                            var outputs = txComponent.GetOutputEntitesByTxHash(tx.Hash);

                            foreach(var output in outputs)
                            {
                                var pubKeyHash = Script.GetPublicKeyHashFromLockScript(output.LockScript);
                                var address = AccountIdHelper.CreateAccountAddressByPublicKeyHash(Base16.Decode(pubKeyHash));

                                var account = accounts.Where(a => a.Id == address).FirstOrDefault();
                                if (account != null)
                                {
                                    if(addresses == null || addresses.Contains(address))
                                    {
                                        result.Add(new ListUnspentOM
                                        {
                                            txid = output.TransactionHash,
                                            vout = output.Index,
                                            address = address,
                                            account = addressBookComponent.GetTagByAddress(address),
                                            scriptPubKey = output.LockScript,
                                            redeemScript = null,
                                            amount = output.Amount,
                                            confirmations = confirmations,
                                            spendable = string.IsNullOrWhiteSpace(account.PrivateKey),
                                            solvable = false
                                        });
                                    }
                                }                                
                            }                            
                        }
                    }
                }

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

        private IRpcMethodResult GetBalance(string account, int minConfirmations, bool includeWachOnly)
        {
            try
            {
                var result = new List<ListLockUnspentOM>();

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

        public IRpcMethodResult GetUnconfirmedBalance()
        {
            try
            {
                var utxoComponent = new UtxoComponent();
                long result = utxoComponent.GetAllUnconfirmedBalance();

                long confirmedBalance, unconfirmedBalance;
                utxoComponent.GetBalanceInDB(out confirmedBalance, out unconfirmedBalance);
                result += unconfirmedBalance;

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
        /// 锁定未花费的输出
        /// </summary>
        /// <param name="isLocked">true为已锁定，false为未锁定</param>
        /// <param name="transaction">需要锁定的交易</param>
        /// <returns></returns>
        public IRpcMethodResult LockUnspent(bool isLocked, ListLockUnspentOM[] transaction)
        {
            try
            {
                /* 1、把输入的参数用个静态变量存储起来
                 * 2、当转账或其他交易的时候先判断是否在静态变量中存在，如果存在就跳过，
                 * 3、注意修改对应的转账接口
                 */
                if (isLocked)
                {
                    foreach (var item in transaction)
                    {
                        Startup.lockUnspentList.Add(item);
                    }
                }
                else
                {
                    foreach (var item in transaction)
                    {
                        Startup.lockUnspentList.Remove(item);
                    }
                }
                return Ok(isLocked);
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
        /// 列出锁定的未花费
        /// </summary>
        /// <returns>ListLockUnspentOM对象数组</returns>
        public IRpcMethodResult ListLockUnspent()
        {
            try
            {
                return Ok(Startup.lockUnspentList);
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
