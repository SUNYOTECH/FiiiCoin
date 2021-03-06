﻿// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using FiiiCoin.Business;
using FiiiCoin.Models;
using FiiiCoin.Utility.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiiiCoin.Wallet.Test.Bussiness
{
    [TestClass]
    public class AccountsApiTest
    {
        [TestMethod]
        public async Task GetAddressesByTag()
        {
            ApiResponse response = await AccountsApi.GetAddressesByTag();
            Assert.IsFalse(response.HasError);
            List<AccountInfo> result = response.GetResult<List<AccountInfo>>();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetAccountByAddress()
        {
            ApiResponse response = await AccountsApi.GetAccountByAddress("fiiitPBzhdXC28KrsbFiF6S6YnDdWX5WEGf5Z5");
            Assert.IsFalse(response.HasError);
            AccountInfo result = response.GetResult<AccountInfo>();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetNewAddress()
        {
            ApiResponse response = await AccountsApi.GetNewAddress("新地址");
            Assert.IsFalse(response.HasError);
            AccountInfo result = response.GetResult<AccountInfo>();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetDefaultAccount()
        {
            ApiResponse response = await AccountsApi.GetDefaultAccount();
            Assert.IsFalse(response.HasError);
            AccountInfo result = response.GetResult<AccountInfo>();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task SetDefaultAccount()
        {
            ApiResponse response = await AccountsApi.SetDefaultAccount("fiiitPBzhdXC28KrsbFiF6S6YnDdWX5WEGf5Z5");
            Assert.IsFalse(response.HasError);
        }

        [TestMethod]
        public async Task ValidateAddress()
        {
            ApiResponse response = await AccountsApi.ValidateAddress("fiiitPBzhdXC28KrsbFiF6S6YnDdWX5WEGf5Z5");
            Assert.IsFalse(response.HasError);
            AddressInfo result = response.GetResult<AddressInfo>();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task SetAccountTag()
        {
            string address = "fiiitPBzhdXC28KrsbFiF6S6YnDdWX5WEGf5Z5";
            ApiResponse response = await AccountsApi.ValidateAddress(address);
            if (!response.HasError)
            {
                AddressInfo addressInfo = response.GetResult<AddressInfo>();
                if (addressInfo.IsValid)
                {
                    await AccountsApi.SetAccountTag(address, "new tag");
                }
            }
        }
    }
}
