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
    public class InputDac : DataAccessComponent
    {
        public long Insert(Input input)
        {
            const string SQL_STATEMENT =
                "INSERT INTO InputList " +
                "(TransactionHash, OutputTransactionHash, OutputIndex, Size, Amount, UnlockScript, AccountId, IsDiscarded) " +
                "VALUES (@TransactionHash, @OutputTransactionHash, @OutputIndex, @Size, @Amount, @UnlockScript, @AccountId, @IsDiscarded);" +
                "SELECT LAST_INSERT_ROWID() newid;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@TransactionHash", input.TransactionHash);
                cmd.Parameters.AddWithValue("@OutputTransactionHash", input.OutputTransactionHash);
                cmd.Parameters.AddWithValue("@OutputIndex", input.OutputIndex);
                cmd.Parameters.AddWithValue("@Size", input.Size);
                cmd.Parameters.AddWithValue("@Amount", input.Amount);
                cmd.Parameters.AddWithValue("@UnlockScript", input.UnlockScript);

                if (string.IsNullOrWhiteSpace(input.AccountId))
                {
                    cmd.Parameters.AddWithValue("@AccountId", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@AccountId", input.AccountId);
                }

                cmd.Parameters.AddWithValue("@IsDiscarded", input.IsDiscarded);
                cmd.Connection.Open();
                return Convert.ToInt64(cmd.ExecuteScalar());
            }

        }

        public List<Input> SelectByTransactionHash(string txHash)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM InputList " +
                "WHERE TransactionHash = @TransactionHash " +
                "AND IsDiscarded = 0 ";

            List<Input> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@TransactionHash", txHash);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Input>();

                    while (dr.Read())
                    {
                        Input input = new Input();

                        input.Id = GetDataValue<long>(dr, "Id");
                        input.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                        input.OutputTransactionHash = GetDataValue<string>(dr, "OutputTransactionHash");
                        input.OutputIndex = GetDataValue<int>(dr, "OutputIndex");
                        input.Size = GetDataValue<int>(dr, "Size");
                        input.Amount = GetDataValue<long>(dr, "Amount");
                        input.UnlockScript = GetDataValue<string>(dr, "UnlockScript");
                        input.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");

                        result.Add(input);
                    }
                }
            }

            return result;
        }

        public Input SelectById(long id)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM InputList " +
                "WHERE Id = @Id " +
                "AND IsDiscarded = 0 ";

            Input input = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        input = new Input();

                        input.Id = GetDataValue<long>(dr, "Id");
                        input.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                        input.OutputTransactionHash = GetDataValue<string>(dr, "OutputTransactionHash");
                        input.OutputIndex = GetDataValue<int>(dr, "OutputIndex");
                        input.Size = GetDataValue<int>(dr, "Size");
                        input.Amount = GetDataValue<long>(dr, "Amount");
                        input.UnlockScript = GetDataValue<string>(dr, "UnlockScript");
                        input.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                    }
                }

                return input;
            }
        }
        public Input SelectByOutputHash(string outputHash, int outputIndex)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM InputList " +
                "WHERE OutputTransactionHash = @OutputTransactionHash AND OutputIndex = @OutputIndex " +
                "AND IsDiscarded = 0 ";

            Input input = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@OutputTransactionHash", outputHash);
                cmd.Parameters.AddWithValue("@OutputIndex", outputIndex);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        input = new Input();

                        input.Id = GetDataValue<long>(dr, "Id");
                        input.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                        input.OutputTransactionHash = GetDataValue<string>(dr, "OutputTransactionHash");
                        input.OutputIndex = GetDataValue<int>(dr, "OutputIndex");
                        input.Size = GetDataValue<int>(dr, "Size");
                        input.Amount = GetDataValue<long>(dr, "Amount");
                        input.UnlockScript = GetDataValue<string>(dr, "UnlockScript");
                        input.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                    }
                }

                return input;
            }
        }

    }
}
