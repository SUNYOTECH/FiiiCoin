// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text;

namespace FiiiChain.Data
{
    public class OutputDac : DataAccessComponent
    {
        public long Insert(Output output)
        {
            const string SQL_STATEMENT =
                "INSERT INTO OutputList " +
                "([Index], TransactionHash, ReceiverId, Amount, Size, LockScript, Spent, IsDiscarded) " +
                "VALUES (@Index, @TransactionHash, @ReceiverId, @Amount, @Size, @LockScript, @Spent, @IsDiscarded); " +
                "SELECT LAST_INSERT_ROWID() newid;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Index", output.Index);
                cmd.Parameters.AddWithValue("@TransactionHash", output.TransactionHash);
                cmd.Parameters.AddWithValue("@ReceiverId", output.ReceiverId);
                cmd.Parameters.AddWithValue("@Amount", output.Amount);
                cmd.Parameters.AddWithValue("@Size", output.Size);
                cmd.Parameters.AddWithValue("@LockScript", output.LockScript);
                cmd.Parameters.AddWithValue("@Spent", output.Spent);
                cmd.Parameters.AddWithValue("@IsDiscarded", output.IsDiscarded);

                cmd.Connection.Open();
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        public void UpdateSpentStatus(string transactionHash, int index)
        {
            const string SQL_STATEMENT =
                "UPDATE OutputList " +
                "SET Spent = @Spent " +
                "WHERE TransactionHash = @TransactionHash AND [Index] = @Index;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@TransactionHash", transactionHash);
                cmd.Parameters.AddWithValue("@Index", index);
                cmd.Parameters.AddWithValue("@Spent", true);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Output> SelectByTransactionHash(string transactionHash)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM OutputList " +
                "WHERE TransactionHash = @TransactionHash " +
                "AND IsDiscarded = 0 ";

            List<Output> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@TransactionHash", transactionHash);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Output>();

                    while (dr.Read())
                    {
                        Output output = new Output();

                        output.Id = GetDataValue<long>(dr, "Id");
                        output.Index = GetDataValue<int>(dr, "Index");
                        output.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                        output.ReceiverId = GetDataValue<string>(dr, "ReceiverId");
                        output.Size = GetDataValue<int>(dr, "Size");
                        output.Amount = GetDataValue<long>(dr, "Amount");
                        output.LockScript = GetDataValue<string>(dr, "LockScript");
                        output.Spent = GetDataValue<bool>(dr, "Spent");
                        output.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");

                        result.Add(output);
                    }
                }
            }

            return result;
        }

        public Output SelectById(long id)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM OutputList " +
                "WHERE Id = @Id " +
                "AND IsDiscarded = 0 ";

            Output output = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        output = new Output();

                        output.Id = GetDataValue<long>(dr, "Id");
                        output.Index = GetDataValue<int>(dr, "Index");
                        output.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                        output.ReceiverId = GetDataValue<string>(dr, "ReceiverId");
                        output.Size = GetDataValue<int>(dr, "Size");
                        output.Amount = GetDataValue<long>(dr, "Amount");
                        output.LockScript = GetDataValue<string>(dr, "LockScript");
                        output.Spent = GetDataValue<bool>(dr, "Spent");
                        output.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                    }
                }

                return output;
            }
        }
        public Output SelectByHashAndIndex(string transactionHash, int index)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM OutputList " +
                "WHERE TransactionHash = @TransactionHash AND [Index] = @Index " +
                "AND IsDiscarded = 0 ";

            Output output = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@TransactionHash", transactionHash);
                cmd.Parameters.AddWithValue("@Index", index);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        output = new Output();

                        output.Id = GetDataValue<long>(dr, "Id");
                        output.Index = GetDataValue<int>(dr, "Index");
                        output.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                        output.ReceiverId = GetDataValue<string>(dr, "ReceiverId");
                        output.Size = GetDataValue<int>(dr, "Size");
                        output.Amount = GetDataValue<long>(dr, "Amount");
                        output.LockScript = GetDataValue<string>(dr, "LockScript");
                        output.Spent = GetDataValue<bool>(dr, "Spent");
                        output.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                    }
                }

                return output;
            }
        }

        public List<Output> SelectUnspentByReceiverId(string receiverId)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM OutputList " +
                "WHERE ReceiverId = @ReceiverId AND Spent = @Spent " +
                "AND IsDiscarded = 0 ";

            List<Output> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                cmd.Parameters.AddWithValue("@Spent", false);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Output>();

                    while (dr.Read())
                    {
                        var output = new Output();

                        output.Id = GetDataValue<long>(dr, "Id");
                        output.Index = GetDataValue<int>(dr, "Index");
                        output.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                        output.ReceiverId = GetDataValue<string>(dr, "ReceiverId");
                        output.Size = GetDataValue<int>(dr, "Size");
                        output.Amount = GetDataValue<long>(dr, "Amount");
                        output.LockScript = GetDataValue<string>(dr, "LockScript");
                        output.Spent = GetDataValue<bool>(dr, "Spent");
                        output.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");

                        result.Add(output);
                    }
                }

                return result;
            }
        }

        public List<Output> SelectAllUnspentOutputs()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM OutputList " +
                "WHERE Spent = 0 AND ReceiverId IN " +
                "(SELECT Id FROM Accounts WHERE PrivateKey IS NOT NULL) " +
                "AND IsDiscarded = 0 ";

            List<Output> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Output>();

                    while (dr.Read())
                    {
                        var output = new Output();

                        output.Id = GetDataValue<long>(dr, "Id");
                        output.Index = GetDataValue<int>(dr, "Index");
                        output.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                        output.ReceiverId = GetDataValue<string>(dr, "ReceiverId");
                        output.Size = GetDataValue<int>(dr, "Size");
                        output.Amount = GetDataValue<long>(dr, "Amount");
                        output.LockScript = GetDataValue<string>(dr, "LockScript");
                        output.Spent = GetDataValue<bool>(dr, "Spent");
                        output.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");

                        result.Add(output);
                    }
                }

                return result;
            }
        }

        public long CountSelfUnspentOutputs()
        {
            const string SQL_STATEMENT =
                "SELECT COUNT(0) " +
                "FROM OutputList " +
                "WHERE Spent = 0 AND ReceiverId IN " +
                "(SELECT Id FROM Accounts WHERE PrivateKey IS NOT NULL) " +
                "AND IsDiscarded = 0 ";

            long result = 0;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                result = (long)cmd.ExecuteScalar();
            }

            return result;
        }
        public long SumSelfUnspentOutputs()
        {
            const string SQL_STATEMENT =
                "SELECT CASE WHEN SUM(Amount) IS NULL THEN 0 ELSE SUM(Amount) END AS Balance " +
                "FROM OutputList " +
                "WHERE Spent = 0 AND ReceiverId IN " +
                "(SELECT Id FROM Accounts WHERE PrivateKey IS NOT NULL) " +
                "AND IsDiscarded = 0 ";

            long result = 0;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                result = (long)cmd.ExecuteScalar();
            }

            return result;
        }
    }
}
