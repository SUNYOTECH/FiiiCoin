// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.Data
{
    public class WalletBackupDac : DataAccessComponent
    {
        public int Restore(WalletBackup entity)
        {
            int rows = 0;
            StringBuilder SQL_STATEMENT = new StringBuilder("BEGIN TRANSACTION;");
            SQL_STATEMENT.Append("DELETE FROM AddressBook;DELETE FROM Settings;DELETE FROM TransactionComments;");
            foreach (var item in entity.AccountList)
            {
                SQL_STATEMENT.Append($"REPLACE INTO Accounts VALUES('{item.Id}','{item.PrivateKey}','{item.PublicKey}',{item.Balance},{Convert.ToInt32(item.IsDefault)},{Convert.ToInt32(item.WatchedOnly)},{item.Timestamp},'{item.Tag}');");
            }
            foreach (var item in entity.AddressBookItemList)
            {
                SQL_STATEMENT.Append($"INSERT INTO AddressBook (Address, Tag, Timestamp) VALUES('{item.Address}','{item.Tag}',{item.Timestamp});");
            }
            foreach (var item in entity.SettingList)
            {
                SQL_STATEMENT.Append($"INSERT INTO Settings (Confirmations, FeePerKB, Encrypt, PassCiphertext) VALUES({item.Confirmations},{item.FeePerKB}, {Convert.ToInt32(item.Encrypt)}, '{item.PassCiphertext}');");
            }
            foreach (var item in entity.TransactionCommentList)
            {
                SQL_STATEMENT.Append($"INSERT INTO TransactionComments (TransactionHash, OutputIndex, Comment, Timestamp) VALUES('{item.TransactionHash}',{item.OutputIndex},'{item.Comment}',{item.Timestamp});");
            }
            SQL_STATEMENT.Append("END TRANSACTION;");
            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT.ToString(), con))
            {

                cmd.Connection.Open();
                rows = cmd.ExecuteNonQuery();
            }
            return rows;
        }
    }
}
