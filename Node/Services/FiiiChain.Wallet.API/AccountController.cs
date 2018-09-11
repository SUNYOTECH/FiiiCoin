// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
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
using Microsoft.Extensions.Caching.Memory;

namespace FiiiChain.Wallet.API
{
    public class AccountController : BaseRpcController
    {
        private IMemoryCache _cache;

        public AccountController(IMemoryCache memoryCache) { _cache = memoryCache; }
        public IRpcMethodResult GetAccountByAddress(string address)
        {
            try
            {
                var utxoComponent = new UtxoComponent();
                var account = new AccountComponent().GetAccountById(address);

                if(account != null)
                {
                    var result = new AccountOM();
                    result.Address = account.Id;
                    result.PublicKey = account.PublicKey;
                    result.Balance = utxoComponent.GetConfirmedBlanace(account.Id);
                    result.IsDefault = account.IsDefault;
                    result.WatchOnly = account.WatchedOnly;
                    result.Tag = account.Tag;

                    return Ok(result);
                }
                else
                {
                    throw new CommonException(ErrorCode.Service.Account.ACCOUNT_NOT_FOUND);
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

        public IRpcMethodResult GetAddressesByTag(string tag)
        {
            try
            {
                var accountComponent = new AccountComponent();
                var accounts = accountComponent.GetAccountsByTag(tag);
                var result = new List<AccountOM>();

                foreach(var account in accounts)
                {
                    var item = new AccountOM();
                    item.Address = account.Id;
                    item.PublicKey = account.PublicKey;
                    item.Balance = account.Balance;
                    item.IsDefault = account.IsDefault;
                    item.WatchOnly = account.WatchedOnly;
                    item.Tag = account.Tag;

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

        public IRpcMethodResult GetNewAddress(string tag)
        {
            try
            {
                var accountComponent = new AccountComponent();
                var account = accountComponent.GenerateNewAccount();
                var setting = new SettingComponent().GetSetting();
                AccountOM result = null;

                if(account != null)
                {
                    if (setting.Encrypt)
                    {
                        if (!string.IsNullOrWhiteSpace(_cache.Get<string>("WalletPassphrase")))
                        {
                            account.PrivateKey = AES128.Encrypt(account.PrivateKey, _cache.Get<string>("WalletPassphrase"));
                            accountComponent.UpdatePrivateKeyAr(account);
                        }
                        else
                        {
                            throw new CommonException(ErrorCode.Service.Wallet.WALLET_HAS_BEEN_LOCKED);
                        }
                    }

                    account.Tag = tag;
                    accountComponent.UpdateTag(account.Id, tag);

                    result = new AccountOM();
                    result.Address = account.Id;
                    result.PublicKey = account.PublicKey;
                    result.Balance = account.Balance;
                    result.IsDefault = account.IsDefault;
                    result.WatchOnly = account.WatchedOnly;
                    result.Tag = account.Tag;
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

        public IRpcMethodResult SetAccountTag(string address, string tag)
        {
            try
            {
                new AccountComponent().UpdateTag(address, tag);

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

        public IRpcMethodResult GetDefaultAccount()
        {
            try
            {
                var accountComponent = new AccountComponent();
                var account = accountComponent.GetDefaultAccount();

                if (account != null)
                {
                    var result = new AccountOM();
                    result.Address = account.Id;
                    result.PublicKey = account.PublicKey;
                    result.Balance = account.Balance;
                    result.IsDefault = account.IsDefault;
                    result.WatchOnly = account.WatchedOnly;
                    result.Tag = account.Tag;

                    return Ok(result);
                }
                else
                {
                    throw new CommonException(ErrorCode.Service.Account.ACCOUNT_NOT_FOUND);
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

        public IRpcMethodResult SetDefaultAccount(string address)
        {
            try
            {
                var accountComponent = new AccountComponent();
                var account = accountComponent.GetAccountById(address);

                if(account != null)
                {
                    accountComponent.SetDefaultAccount(account.Id);
                }
                else
                {
                    throw new CommonException(ErrorCode.Service.Account.ACCOUNT_NOT_FOUND);
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

        public IRpcMethodResult ValidateAddress(string address)
        {
            try
            {
                var result = new ValidateAddressOM();
                var accountComponent = new AccountComponent();

                result.address = address;

                if(AccountIdHelper.AddressVerify(address))
                {
                    result.isValid = true;
                    var account = accountComponent.GetAccountById(address);

                    if (account != null)
                    {
                        result.isMine = true;
                        result.isWatchOnly = string.IsNullOrWhiteSpace(account.PrivateKey);

                    }
                    else
                    {
                        result.isMine = false;
                    }

                    result.scriptPubKey = Script.BuildLockScipt(address);
                    result.isScript = false;
                    result.script = "P2PKH";
                    result.hex = null;
                    result.addresses = null;
                    result.pubKey = account.PublicKey;
                    result.isCompressed = false;
                    result.account = account.Tag;
                    result.hdKeyPath = null;
                    result.hdMasterKeyId = null;
                }
                else
                {
                    result.isValid = false;
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
    }
}
