// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using FiiiChain.Data;
using FiiiChain.DataAgent;
using FiiiChain.Entities;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiiiChain.Business
{
    public class AccountComponent
    {
        public Account GenerateNewAccount()
        {
            var dac = new AccountDac();

            byte[] privateKey;
            byte[] publicKey;
            using (var dsa = ECDsa.GenerateNewKeyPair())
            {
                privateKey = dsa.PrivateKey;
                publicKey = dsa.PublicKey;
            }

            var id = AccountIdHelper.CreateAccountAddress(publicKey);
            if(dac.IsExisted(id))
            {
                throw new Exception("Account id is existed");
            }

            Account account = new Account();
            account.Id = id;
            account.PrivateKey = Base16.Encode(privateKey);
            account.PublicKey = Base16.Encode(publicKey);
            account.Balance = 0;
            account.IsDefault = false;
            account.WatchedOnly = false;

            dac.Insert(account);

            if(UtxoSet.Instance != null)
            {
                UtxoSet.Instance.AddAccountId(account.Id);
            }

            return account;
        }

        public Account ImportAccount(string privateKeyText)
        {
            var dac = new AccountDac();

            byte[] privateKey = Base16.Decode(privateKeyText);
            byte[] publicKey;
            using (var dsa = ECDsa.ImportPrivateKey(privateKey))
            {
                publicKey = dsa.PublicKey;
            }

            var id = AccountIdHelper.CreateAccountAddress(publicKey);
            Account account = dac.SelectById(id);

            if (account == null)
            {
                account = new Account();
                account.Id = AccountIdHelper.CreateAccountAddress(publicKey);
                account.PrivateKey = Base16.Encode(privateKey);
                account.PublicKey = Base16.Encode(publicKey);
                account.Balance = 0;
                account.IsDefault = false;
                account.WatchedOnly = false;

                dac.Insert(account);
                UtxoSet.Instance.AddAccountId(account.Id);
            }

            return account;
        }

        public Account ImportObservedAccount(string publicKeyText)
        {
            var dac = new AccountDac();

            var publicKey = Base16.Decode(publicKeyText);
            var id = AccountIdHelper.CreateAccountAddress(publicKey);

            Account account = dac.SelectById(id);

            if (account == null)
            {
                account = new Account();
                account.Id = AccountIdHelper.CreateAccountAddress(publicKey);
                account.PrivateKey = null;
                account.PublicKey = Base16.Encode(publicKey);
                account.Balance = 0;
                account.IsDefault = false;
                account.WatchedOnly = true;

                dac.Insert(account);
                UtxoSet.Instance.AddAccountId(account.Id);
            }

            return account;
        }

        public List<Account> GetAllAccounts()
        {
            var dac = new AccountDac();
            return dac.SelectAll();
        }

        public Account GetAccountById(string id)
        {
            var dac = new AccountDac();
            return dac.SelectById(id);
        }

        public List<Account> GetAccountsByTag(string tag)
        {
            return new AccountDac().SelectByTag(tag);
        }

        public Account GetDefaultAccount()
        {
            var dac = new AccountDac();
            return dac.SelectDefaultAccount();
        }

        public void SetDefaultAccount(string id)
        {
            var dac = new AccountDac();
            dac.SetDefaultAccount(id);
        }

        public void UpdateBalance(string id, long amount)
        {
            var dac = new AccountDac();
            dac.UpdateBalance(id, amount);
        }

        public void UpdatePrivateKeyAr(Account account)
        {
            new AccountDac().UpdatePrivateKeyAr(new List<Account>(new Account[]{ account }));
        }

        public void DeleteAccount(string id)
        {
            var dac = new AccountDac();
            dac.Delete(id);
            UtxoSet.Instance.RemoveAccountId(id);
        }

        public void UpdateTag(string id, string tag)
        {
            new AccountDac().UpdateTag(id, tag);
        }
    }
}
