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
    public static class AccountsApi
    {
        public static async Task<ApiResponse> GetAddressesByTag(string tag = "")
        {
            ApiResponse response = new ApiResponse();
            try
            {
                Accounts account = new Accounts();
                List<AccountInfo> list = new List<AccountInfo>();
                AccountInfoOM[] result = await account.GetAddressesByTag(tag);
                if (result != null)
                {
                    for (int i = 0; i < result.Length; i++)
                    {
                        list.Add(new AccountInfo()
                        {
                            Address = result[i].Address,
                            Balance = result[i].Balance,
                            IsDefault = result[i].IsDefault,
                            PublicKey = result[i].PublicKey,
                            Tag = result[i].Tag,
                            WatchOnly = result[i].WatchOnly
                        });
                    }
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(list);
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

        public static async Task<ApiResponse> GetAccountByAddress(string address)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                Accounts account = new Accounts();
                AccountInfo info = new AccountInfo();
                AccountInfoOM result = await account.GetAccountByAddress(address);
                if (result != null)
                {
                    info.Address = result.Address;
                    info.Balance = result.Balance;
                    info.IsDefault = result.IsDefault;
                    info.PublicKey = result.PublicKey;
                    info.Tag = result.Tag;
                    info.WatchOnly = result.WatchOnly;
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(info);
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

        public static async Task<ApiResponse> GetNewAddress(string tag)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                Accounts account = new Accounts();
                AccountInfo info = new AccountInfo();
                AccountInfoOM result = await account.GetNewAddress(tag);
                if (result != null)
                {
                    info.Address = result.Address;
                    info.Balance = result.Balance;
                    info.IsDefault = result.IsDefault;
                    info.PublicKey = result.PublicKey;
                    info.Tag = result.Tag;
                    info.WatchOnly = result.WatchOnly;
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(info);
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

        public static async Task<ApiResponse> GetDefaultAccount()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                Accounts account = new Accounts();
                AccountInfo info = new AccountInfo();
                AccountInfoOM result = await account.GetDefaultAccount();
                if (result != null)
                {
                    info.Address = result.Address;
                    info.Balance = result.Balance;
                    info.IsDefault = result.IsDefault;
                    info.PublicKey = result.PublicKey;
                    info.Tag = result.Tag;
                    info.WatchOnly = result.WatchOnly;
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(info);
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

        public static async Task<ApiResponse> SetDefaultAccount(string address)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                Accounts account = new Accounts();
                await account.SetDefaultAccount(address);
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

        public static async Task<ApiResponse> ValidateAddress(string address)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                Accounts account = new Accounts();
                AddressInfo info = new AddressInfo();
                AddressInfoOM result = await account.ValidateAddress(address);
                if (result != null)
                {
                    info.Address = result.Address;
                    info.Account = result.Account;
                    info.Addresses = result.Addresses;
                    info.Hdkeypath = result.Hdkeypath;
                    info.Hdmasterkeyid = result.Hdmasterkeyid;
                    info.Hex = result.Hex;
                    info.IsCompressed = result.IsCompressed;
                    info.IsMine = result.IsMine;
                    info.IsScript = result.IsScript;
                    info.IsValid = result.IsValid;
                    info.IsWatchOnly = result.IsWatchOnly;
                    info.PubKey = result.PubKey;
                    info.Script = result.Script;
                    info.ScriptPubKey = result.ScriptPubKey;
                    info.Sigrequired = result.Sigrequired;
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(info);
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

        public static async Task<ApiResponse> SetAccountTag(string address, string tag)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                Accounts account = new Accounts();
                await account.SetAccountTag(address, tag);
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
