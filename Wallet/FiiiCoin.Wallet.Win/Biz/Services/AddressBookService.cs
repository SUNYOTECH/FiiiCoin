// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Business;
using FiiiCoin.Models;
using FiiiCoin.Utility;
using FiiiCoin.Utility.Api;
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Models;
using System;
using System.Collections.Generic;

namespace FiiiCoin.Wallet.Win.Biz.Services
{
    public class AddressBookService : ServiceBase<AddressBookService>
    {
        public Result<bool> AddNewAddressBookItem(string account, string tag)
        {
            var status = GetBlockChainStatus();
            if (status.IsFail)
                return new Result<bool> { IsFail = true };

            var check = VerfyAddress(status.Value.ChainNetwork, account);
            if (!check)
            {
                return new Result<bool>() { IsFail = true ,ErrorCode= 70000001 };
            }
            ApiResponse response = AddressBookApi.AddNewAddressBookItem(account, tag).Result;
            return GetResult<bool>(response);
        }

        public  Result<List<AddressBookInfo>> GetAddressBook()
        {
            ApiResponse response = AddressBookApi.GetAddressBook().Result;
            return GetResult<List<AddressBookInfo>>(response);
        }


        public Result<AddressBookInfo> GetAddressBookItemByAddress(string address)
        {
            ApiResponse response = AddressBookApi.GetAddressBookItemByAddress(address).Result;
            return GetResult<AddressBookInfo>(response);
        }


        public Result<List<AddressBookInfo>> GetAddressBookByTag(string tag)
        {
            ApiResponse response = AddressBookApi.GetAddressBookByTag(tag).Result;
            return GetResult<List<AddressBookInfo>>(response);
        }

        public Result<bool> DeleteAddressBookByIds(params long[] ids)
        {
            ApiResponse response = AddressBookApi.DeleteAddressBookByIds(ids).Result;
            return GetResult<bool>(response);
        }


        public bool VerfyAddress(string netType, string account)
        {
            bool result = false;
            try
            {
                result = AddressTools.AddressVerfy(netType, account);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
            }
            return result;
        }



        private BlockChainStatus chainStatus = null;
        public Result<BlockChainStatus> GetBlockChainStatus()
        {
            if (chainStatus == null)
            {
                ApiResponse apiResponse = BlockChainEngineApi.GetBlockChainStatus().Result;
                return GetResult<BlockChainStatus>(apiResponse);
            }
            else
                return new Result<BlockChainStatus>() { Value = chainStatus, IsFail = false };
        }
    }
}