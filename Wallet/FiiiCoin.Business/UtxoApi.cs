// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using FiiiCoin.DTO;
using FiiiCoin.Models;
using FiiiCoin.ServiceAgent;
using FiiiCoin.Utility;
using FiiiCoin.Utility.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiiiCoin.Business
{
    public static class UtxoApi
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetTxOutSetInfo()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                UTXO utxo = new UTXO();
                TxOutSet set = new TxOutSet();

                TxOutSetOM result = await utxo.GetTxOutSetInfo();
                if (result != null)
                {
                    set.BestBlock = result.BestBlock;
                    set.Height = result.Height;
                    set.Total_amount = result.Total_amount;
                    set.Transactions = result.Transactions;
                    set.Txouts = result.Txouts;
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(set);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minConfirmations"></param>
        /// <param name="maxConfirmations"></param>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public static async Task<ApiResponse> ListUnspent(int minConfirmations, int maxConfirmations = 9999, string[] addresses = null)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                UTXO utxo = new UTXO();
                List<UnspentUtxo> unspentList = new List<UnspentUtxo>();

                List<UnspentOM> result = await utxo.ListUnspent(minConfirmations, maxConfirmations, addresses);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        UnspentUtxo unspent = new UnspentUtxo();
                        unspent.Account = item.Account;
                        unspent.Address = item.Address;
                        unspent.Amount = item.Amount;
                        unspent.Confirmations = item.Confirmations;
                        unspent.RedeemScript = item.RedeemScript;
                        unspent.ScriptPubKey = item.ScriptPubKey;
                        unspent.Solvable = item.Solvable;
                        unspent.Spendable = item.Spendable;
                        unspent.TXID = item.TXID;
                        unspent.Vout = item.Vout;
                        unspentList.Add(unspent);
                    }

                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(unspentList);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetUnconfirmedBalance()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                UTXO utxo = new UTXO();

                long result = await utxo.GetUnconfirmedBalance();
                response.Result = Newtonsoft.Json.Linq.JToken.FromObject(result);
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="vout"></param>
        /// <param name="unconfirmed"></param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetTxOut(string txid, int vout, bool unconfirmed)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                UTXO utxo = new UTXO();
                TxOutModel model = new TxOutModel();

                TxOutOM result = await utxo.GetTxOut(txid, vout, unconfirmed);
                if (result != null)
                {
                    model.BestBlock = result.BestBlock;
                    model.CoinBase = result.CoinBase;
                    model.Confirmations = result.Confirmations;
                    model.ScriptPubKey = result.ScriptPubKey;
                    model.TxValue = result.Value;
                    model.Version = result.Version;

                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(model);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }
    }
}
