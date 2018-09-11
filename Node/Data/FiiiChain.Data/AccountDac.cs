// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Entities;
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text;

namespace FiiiChain.Data
{
    public class AccountDac : DataAccessComponent
    {

        public void Insert(Account account)
        {
            const string SQL_STATEMENT =
                "INSERT INTO Accounts " +
                "(Id, PrivateKey, PublicKey, Balance, IsDefault, WatchedOnly, Timestamp, Tag) " +
                "VALUES (@Id, @PrivateKey, @PublicKey, @Balance, @IsDefault, @WatchedOnly, @Timestamp, @Tag);";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", account.Id);
                cmd.Parameters.AddWithValue("@PrivateKey", account.PrivateKey);
                cmd.Parameters.AddWithValue("@PublicKey", account.PublicKey);
                cmd.Parameters.AddWithValue("@Balance", account.Balance);
                cmd.Parameters.AddWithValue("@IsDefault", account.IsDefault);
                cmd.Parameters.AddWithValue("@WatchedOnly", account.WatchedOnly);
                cmd.Parameters.AddWithValue("@Timestamp", Time.EpochTime);

                if(account.Tag == null)
                {
                    cmd.Parameters.AddWithValue("@Tag", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Tag", account.Tag);
                }

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }

        }

        public void UpdateBalance(string id, long amount)
        {
            const string SQL_STATEMENT =
                "UPDATE Accounts " +
                "SET Balance = (Balance +  @Amount) " +
                "WHERE Id = @Id;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateTag(string id, string tag)
        {
            const string SQL_STATEMENT =
                "UPDATE Accounts " +
                "SET Tag = @Tag " +
                "WHERE Id = @Id;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                if(string.IsNullOrWhiteSpace(tag))
                {
                    cmd.Parameters.AddWithValue("@Tag", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Tag", tag);
                }

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public int UpdatePrivateKeyAr(List<Account> aclist)
        {
            StringBuilder SQL_STATEMENT = new StringBuilder("BEGIN TRANSACTION;");
            foreach(var item in aclist)
            {
                SQL_STATEMENT.Append($"UPDATE Accounts SET PrivateKey = '{item.PrivateKey}' WHERE Id = '{item.Id}';");
            }
            SQL_STATEMENT.Append("END TRANSACTION;");
            int rows = -1;
            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT.ToString(), con))
            {

                cmd.Connection.Open();
                rows = cmd.ExecuteNonQuery();
            }
            return rows;
        }

        public void SetDefaultAccount(string id)
        {
            const string SQL_STATEMENT =
                "UPDATE Accounts " +
                "SET IsDefault = 0 " +
                "WHERE Id = @Id;" +
                "UPDATE Accounts " +
                "SET IsDefault = 1 " +
                "WHERE Id = @Id;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(string id)
        {
            const string SQL_STATEMENT =
                "DELETE FROM Accounts " +
                "WHERE Id = @Id;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Account> SelectAll()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Accounts;";

           List<Account> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Account>();

                    while (dr.Read())
                    {
                        Account account = new Account();
                        account.Id = GetDataValue<string>(dr, "Id");
                        account.PrivateKey = GetDataValue<string>(dr, "PrivateKey");
                        account.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        account.Balance = GetDataValue<long>(dr, "Balance");
                        account.IsDefault = GetDataValue<bool>(dr, "IsDefault");
                        account.WatchedOnly = GetDataValue<bool>(dr, "WatchedOnly");
                        account.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        account.Tag = GetDataValue<string>(dr, "Tag");

                        result.Add(account);
                    }
                }
            }

            return result;
        }
        public List<Account> SelectAllOwnedAccounts()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Accounts " + 
                "WHERE PrivateKey IS NOT NULL;";

            List<Account> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Account>();

                    while (dr.Read())
                    {
                        Account account = new Account();
                        account.Id = GetDataValue<string>(dr, "Id");
                        account.PrivateKey = GetDataValue<string>(dr, "PrivateKey");
                        account.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        account.Balance = GetDataValue<long>(dr, "Balance");
                        account.IsDefault = GetDataValue<bool>(dr, "IsDefault");
                        account.WatchedOnly = GetDataValue<bool>(dr, "WatchedOnly");
                        account.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        account.Tag = GetDataValue<string>(dr, "Tag");

                        result.Add(account);
                    }
                }
            }

            return result;
        }
        public List<Account> SelectAllWatchedAccounts()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Accounts " +
                "WHERE PrivateKey IS NULL;";

            List<Account> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Account>();

                    while (dr.Read())
                    {
                        Account account = new Account();
                        account.Id = GetDataValue<string>(dr, "Id");
                        account.PrivateKey = GetDataValue<string>(dr, "PrivateKey");
                        account.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        account.Balance = GetDataValue<long>(dr, "Balance");
                        account.IsDefault = GetDataValue<bool>(dr, "IsDefault");
                        account.WatchedOnly = GetDataValue<bool>(dr, "WatchedOnly");
                        account.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        account.Tag = GetDataValue<string>(dr, "Tag");

                        result.Add(account);
                    }
                }
            }

            return result;
        }

        public Account SelectById(string id)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Accounts " + 
                "WHERE Id = @Id;";

            Account account = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        account = new Account();
                        account.Id = GetDataValue<string>(dr, "Id");
                        account.PrivateKey = GetDataValue<string>(dr, "PrivateKey");
                        account.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        account.Balance = GetDataValue<long>(dr, "Balance");
                        account.IsDefault = GetDataValue<bool>(dr, "IsDefault");
                        account.WatchedOnly = GetDataValue<bool>(dr, "WatchedOnly");
                        account.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        account.Tag = GetDataValue<string>(dr, "Tag");
                    }
                }
            }

            return account;
        }

        public List<Account> SelectByTag(string tag)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Accounts " +
                "WHERE IFNULL(Tag,'') LIKE @Tag;";

            List<Account> result = new List<Account>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                if(tag == null || tag == "*")
                {
                    tag = "";
                }

                tag = "%" + tag + "%";

                cmd.Parameters.AddWithValue("@Tag", tag);
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var account = new Account();
                        account.Id = GetDataValue<string>(dr, "Id");
                        account.PrivateKey = GetDataValue<string>(dr, "PrivateKey");
                        account.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        account.Balance = GetDataValue<long>(dr, "Balance");
                        account.IsDefault = GetDataValue<bool>(dr, "IsDefault");
                        account.WatchedOnly = GetDataValue<bool>(dr, "WatchedOnly");
                        account.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        account.Tag = GetDataValue<string>(dr, "Tag");

                        result.Add(account);
                    }
                }
            }

            return result;
        }

        public Account SelectDefaultAccount()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Accounts " +
                "WHERE IsDefault = 1 LIMIT 1;";

            Account account = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        account = new Account();
                        account.Id = GetDataValue<string>(dr, "Id");
                        account.PrivateKey = GetDataValue<string>(dr, "PrivateKey");
                        account.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        account.Balance = GetDataValue<long>(dr, "Balance");
                        account.IsDefault = GetDataValue<bool>(dr, "IsDefault");
                        account.WatchedOnly = GetDataValue<bool>(dr, "WatchedOnly");
                        account.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        account.Tag = GetDataValue<string>(dr, "Tag");
                    }
                }
            }

            return account;
        }

        public bool IsExisted(string id)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Accounts " +
                "WHERE Id = @Id LIMIT 1;";

            bool hasAccount = false;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    hasAccount = dr.HasRows;
                }
            }

            return hasAccount;
        }

    }
}
