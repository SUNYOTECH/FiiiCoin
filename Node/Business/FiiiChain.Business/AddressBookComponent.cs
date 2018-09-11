// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Messages;
using FiiiChain.Entities;
using FiiiChain.DataAgent;
using System;
using System.Collections.Generic;
using System.Text;
using FiiiChain.Data;
using FiiiChain.Framework;
using FiiiChain.Consensus;
using System.Linq;


namespace FiiiChain.Business
{
    public class AddressBookComponent
    {
        public void SetTag(string address, string tag)
        {
            if(new AccountDac().SelectById(address) == null)
            {
                new AddressBookDac().InsertOrUpdate(address, tag);
            }
        }

        public void UpdateTimestamp(string address)
        {
            new AddressBookDac().UpdateTimestamp(address);
        }

        public void Delete(string address)
        {
            new AddressBookDac().Delete(address);
        }

        public void DeleteByTag(string tag)
        {
            new AddressBookDac().DeleteByTag(tag);
        }

        public void DeleteByIds(long[] ids)
        {
            new AddressBookDac().DeleteByIds(ids);
        }

        public List<AddressBookItem> GetWholeAddressBook()
        {
            return new AddressBookDac().SelectWholeAddressBook();
        }

        public List<AddressBookItem> GetByTag(string tag)
        {
            return new AddressBookDac().SelectAddessListByTag(tag);
        }

        public AddressBookItem GetByAddress(string address)
        {
            return new AddressBookDac().SelectByAddress(address);
        }

        public string GetTagByAddress(string address)
        {
            var item = new AddressBookDac().SelectByAddress(address);

            if(item != null)
            {
                return item.Tag;
            }
            else
            {
                return null;
            }
        }
    }
}
