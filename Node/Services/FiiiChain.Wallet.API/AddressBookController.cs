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
using FiiiChain.Entities;

namespace FiiiChain.Wallet.API
{
    public class AddressBookController : BaseRpcController
    {
        public IRpcMethodResult AddNewAddressBookItem(string address, string tag)
        {
            try
            {
                var addressBookComponent = new AddressBookComponent();

                if(new AccountComponent().GetAccountById(address) != null)
                {
                    throw new CommonException(ErrorCode.Service.AddressBook.CAN_NOT_ADDED_SELF_ACCOUNT_INTO_ADDRESS_BOOK);
                }
                else
                {
                    addressBookComponent.SetTag(address, tag);
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

        public IRpcMethodResult GetAddressBook()
        {
            try
            {
                var result = new AddressBookComponent().GetWholeAddressBook();
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

        public IRpcMethodResult GetAddressBookByTag(string tag)
        {
            try
            {
                var result = new AddressBookComponent().GetByTag(tag);
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

        public IRpcMethodResult GetAddressBookItemByAddress(string address)
        {
            try
            {
                var result = new AddressBookComponent().GetByAddress(address);
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

        public IRpcMethodResult DeleteAddressBookByIds(long[] ids)
        {
            try
            {
                new AddressBookComponent().DeleteByIds(ids);
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
    }
}
