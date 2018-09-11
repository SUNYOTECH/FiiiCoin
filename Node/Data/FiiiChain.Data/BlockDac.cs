// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text;
using FiiiChain.Framework;

namespace FiiiChain.Data
{
    public class BlockDac : DataAccessComponent
    {
        public long Insert2(Block block)
        {
            long blockId;
            const string BLOCK_SQL_STATEMENT =
                "INSERT INTO Blocks " +
                "(Hash, Version, Height, PreviousBlockHash, Bits, Nonce, Timestamp, NextBlockHash, TotalAmount, TotalFee, GeneratorId, BlockSignature, CumulativeDifficulty, PayloadHash, IsDiscarded, IsVerified) " +
                "VALUES (@Hash, @Version, @Height, @PreviousBlockHash, @Bits, @Nonce, @Timestamp, @NextBlockHash, @TotalAmount, @TotalFee, @GeneratorId, @BlockSignature, @CumulativeDifficulty, @PayloadHash, @IsDiscarded, @IsVerified);" +
                "SELECT LAST_INSERT_ROWID() newid;";

            const string TRANSACTION_SQL_STATEMENT =
                "INSERT INTO Transactions " +
                "(Hash, BlockHash, Version, Timestamp, LockTime, TotalInput, TotalOutput, Fee, Size) " +
                "VALUES (@Hash, @BlockHash, @Version, @Timestamp, @LockTime, @TotalInput, @TotalOutput, @Fee, @Size);" ;

            const string INPUT_SQL_STATEMENT =
                "INSERT INTO InputList " +
                "(TransactionHash, OutputTransactionHash, OutputIndex, Size, Amount, UnlockScript, AccountId) " +
                "VALUES (@TransactionHash, @OutputTransactionHash, @OutputIndex, @Size, @Amount, @UnlockScript, @AccountId);";

            const string OUTPUT_SQL_STATEMENT =
                "INSERT INTO OutputList " +
                "([Index], TransactionHash, ReceiverId, Amount, Size, LockScript, Spent) " +
                "VALUES (@Index, @TransactionHash, @ReceiverId, @Amount, @Size, @LockScript, @Spent); ";

            const string NEXT_BLOCK_HASH_SQL_STATEMENT =
                "UPDATE Blocks " +
                "SET NextBlockHash = @NextBlockHash " +
                "WHERE Hash = @Hash ";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            {
                con.Open();

                using (var scope = new System.Transactions.TransactionScope())
                {
                    using (SqliteCommand cmd = new SqliteCommand(BLOCK_SQL_STATEMENT, con))
                    {
                        cmd.Parameters.AddWithValue("@Hash", block.Hash);
                        cmd.Parameters.AddWithValue("@Version", block.Version);
                        cmd.Parameters.AddWithValue("@Height", block.Height);
                        cmd.Parameters.AddWithValue("@PreviousBlockHash", block.PreviousBlockHash);
                        cmd.Parameters.AddWithValue("@Bits", block.Bits);
                        cmd.Parameters.AddWithValue("@Nonce", block.Nonce);
                        cmd.Parameters.AddWithValue("@Timestamp", block.Timestamp);
                        cmd.Parameters.AddWithValue("@NextBlockHash", (block.NextBlockHash == null? DBNull.Value: (object)block.NextBlockHash));
                        cmd.Parameters.AddWithValue("@TotalAmount", block.TotalAmount);
                        cmd.Parameters.AddWithValue("@TotalFee", block.TotalFee);
                        cmd.Parameters.AddWithValue("@GeneratorId", block.GeneratorId);
                        cmd.Parameters.AddWithValue("@BlockSignature", block.BlockSignature);
                        cmd.Parameters.AddWithValue("@PayloadHash", block.PayloadHash);

                        cmd.Parameters.AddWithValue("@IsDiscarded", block.IsDiscarded);
                        cmd.Parameters.AddWithValue("@IsVerified", block.IsVerified);

                        blockId = Convert.ToInt64(cmd.ExecuteScalar());
                    }


                    foreach (var transaction in block.Transactions)
                    {
                        using (SqliteCommand cmd = new SqliteCommand(TRANSACTION_SQL_STATEMENT, con))
                        {
                            cmd.Parameters.AddWithValue("@Hash", transaction.Hash);
                            cmd.Parameters.AddWithValue("@BlockHash", transaction.BlockHash);
                            cmd.Parameters.AddWithValue("@Version", transaction.Version);
                            cmd.Parameters.AddWithValue("@Timestamp", transaction.Timestamp);
                            cmd.Parameters.AddWithValue("@LockTime", transaction.LockTime);
                            cmd.Parameters.AddWithValue("@TotalInput", transaction.TotalInput);
                            cmd.Parameters.AddWithValue("@TotalOutput", transaction.TotalOutput);
                            cmd.Parameters.AddWithValue("@Fee", transaction.Fee);
                            cmd.Parameters.AddWithValue("@Size", transaction.Size);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var input in transaction.Inputs)
                        {
                            using (SqliteCommand cmd = new SqliteCommand(INPUT_SQL_STATEMENT, con))
                            {
                                cmd.Parameters.AddWithValue("@TransactionHash", input.TransactionHash);
                                cmd.Parameters.AddWithValue("@OutputTransactionHash", input.OutputTransactionHash);
                                cmd.Parameters.AddWithValue("@OutputIndex", input.OutputIndex);
                                cmd.Parameters.AddWithValue("@Size", input.Size);
                                cmd.Parameters.AddWithValue("@Amount", input.Amount);
                                cmd.Parameters.AddWithValue("@UnlockScript", input.UnlockScript);

                                if(string.IsNullOrWhiteSpace(input.AccountId))
                                {
                                    cmd.Parameters.AddWithValue("@AccountId", DBNull.Value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@AccountId", input.AccountId);
                                }

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var output in transaction.Outputs)
                        {
                            using (SqliteCommand cmd = new SqliteCommand(OUTPUT_SQL_STATEMENT, con))
                            {
                                cmd.Parameters.AddWithValue("@Index", output.Index);
                                cmd.Parameters.AddWithValue("@TransactionHash", output.TransactionHash);
                                cmd.Parameters.AddWithValue("@ReceiverId", output.ReceiverId);
                                cmd.Parameters.AddWithValue("@Amount", output.Amount);
                                cmd.Parameters.AddWithValue("@Size", output.Size);
                                cmd.Parameters.AddWithValue("@LockScript", output.LockScript);
                                cmd.Parameters.AddWithValue("@Spent", output.Spent);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    using (SqliteCommand cmd = new SqliteCommand(NEXT_BLOCK_HASH_SQL_STATEMENT, con))
                    {
                        cmd.Parameters.AddWithValue("@Hash", block.PreviousBlockHash);
                        cmd.Parameters.AddWithValue("@NextBlockHash", block.Hash);
                        cmd.ExecuteNonQuery();
                    }

                    scope.Complete();
                }
            }

            return blockId;
        }

        public int Save(Block block)
        {
            int rows = 0;
            StringBuilder sql = new StringBuilder("BEGIN TRANSACTION;");

            sql.Append("INSERT INTO Blocks " +
                "(Hash, Version, Height, PreviousBlockHash, Bits, Nonce, Timestamp, NextBlockHash, TotalAmount, TotalFee, GeneratorId, BlockSignature, PayloadHash, IsDiscarded, IsVerified) " +
                $"VALUES ('{block.Hash}', {block.Version}, {block.Height}, '{block.PreviousBlockHash}', {block.Bits}, {block.Nonce}, {block.Timestamp}, null, {block.TotalAmount}, {block.TotalFee}, '{block.GeneratorId}', '{block.BlockSignature}', '{block.PayloadHash}', {Convert.ToInt32(block.IsDiscarded)}, {Convert.ToInt32(block.IsVerified)});");

            foreach (var transaction in block.Transactions)
            {
                sql.Append("INSERT INTO Transactions " +
                "(Hash, BlockHash, Version, Timestamp, LockTime, TotalInput, TotalOutput, Fee, Size) " +
                $"VALUES ('{transaction.Hash}', '{transaction.BlockHash}', {transaction.Version}, {transaction.Timestamp}, {transaction.LockTime}, {transaction.TotalInput}, {transaction.TotalOutput}, {transaction.Fee}, {transaction.Size});");

                foreach (var input in transaction.Inputs)
                {
                    sql.Append("INSERT INTO InputList " +
                    "(TransactionHash, OutputTransactionHash, OutputIndex, Size, Amount, UnlockScript, AccountId) " +
                    $"VALUES ('{input.TransactionHash}', '{input.OutputTransactionHash}', {input.OutputIndex}, {input.Size}, {input.Amount}, '{input.UnlockScript}', '{Convert.ToString(input.AccountId)}');");

                    if (input.OutputTransactionHash != Base16.Encode(HashHelper.EmptyHash()))
                    {
                        sql.Append($"UPDATE OutputList SET Spent = 1 WHERE TransactionHash = '{input.OutputTransactionHash}' AND [Index] = {input.OutputIndex};");
                    }
                }

                foreach (var output in transaction.Outputs)
                {
                    sql.Append("INSERT INTO OutputList " +
                    "([Index], TransactionHash, ReceiverId, Amount, Size, LockScript, Spent) " +
                    $"VALUES ({output.Index}, '{output.TransactionHash}', '{output.ReceiverId}', {output.Amount}, {output.Size}, '{output.LockScript}', {Convert.ToInt32(output.Spent)}); ");
                }
            }

            sql.Append($"UPDATE Blocks SET NextBlockHash = '{block.Hash}' WHERE Hash = '{block.PreviousBlockHash}';");
            sql.Append("END TRANSACTION;");
            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(sql.ToString(), con))
            {

                cmd.Connection.Open();
                rows = cmd.ExecuteNonQuery();
            }
            return rows;
        }

        public void UpdateNextBlockHash(string currentHash, string nextHash)
        {
            const string SQL_STATEMENT =
                "UPDATE Blocks " +
                "SET NextBlockHash = @NextBlockHash " +
                "WHERE Hash = @Hash ";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", currentHash);
                cmd.Parameters.AddWithValue("@NextBlockHash", nextHash);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateBlockStatusToDiscarded(string hash)
        {
            const string TX_SELECT_SQL_STATEMENT =
                "SELECT Hash " +
                "FROM Transactions " +
                "WHERE BlockHash = @BlockHash " +
                "AND IsDiscarded = 0 ";

            const string BLOCK_UPDATE_SQL_STATEMENT =
                "UPDATE Blocks " +
                "SET IsDiscarded = 1 " +
                "WHERE Hash = @Hash ";

            const string TX_UPDATE_SQL_STATEMENT =
                "UPDATE Transactions " +
                "SET IsDiscarded = 1 " +
                "WHERE BlockHash = @BlockHash ";

            const string INPUT_UPDATE_SQL_STATEMENT =
                "UPDATE InputList " +
                "SET IsDiscarded = 1 " +
                "WHERE TransactionHash = @TransactionHash ";

            const string OUTPUT_UPDATE_SQL_STATEMENT =
                "UPDATE OutputList " +
                "SET IsDiscarded = 1 " +
                "WHERE TransactionHash = @TransactionHash ";



            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            {
                con.Open();
                var txHashList = new List<string>();

                using (SqliteCommand cmd = new SqliteCommand(TX_SELECT_SQL_STATEMENT, con))
                {
                    cmd.Parameters.AddWithValue("@BlockHash", hash);

                    cmd.Connection.Open();
                    using (SqliteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            txHashList.Add(GetDataValue<string>(dr, "Hash"));
                        }
                    }
                }

                using (var scope = new System.Transactions.TransactionScope())
                {
                    using (SqliteCommand cmd = new SqliteCommand(BLOCK_UPDATE_SQL_STATEMENT, con))
                    {
                        cmd.Parameters.AddWithValue("@Hash", hash);
                        cmd.ExecuteNonQuery();
                    }

                    using (SqliteCommand cmd = new SqliteCommand(TX_UPDATE_SQL_STATEMENT, con))
                    {
                        cmd.Parameters.AddWithValue("@BlockHash", hash);
                        cmd.ExecuteNonQuery();
                    }

                    foreach(var txHash in txHashList)
                    {
                        using (SqliteCommand cmd = new SqliteCommand(INPUT_UPDATE_SQL_STATEMENT, con))
                        {
                            cmd.Parameters.AddWithValue("@TransactionHash", txHash);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqliteCommand cmd = new SqliteCommand(OUTPUT_UPDATE_SQL_STATEMENT, con))
                        {
                            cmd.Parameters.AddWithValue("@TransactionHash", txHash);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    scope.Complete();
                }
            }
        }

        public void UpdateBlockStatusToConfirmed(string hash)
        {
            const string SQL_STATEMENT =
                "UPDATE Blocks " +
                "SET IsVerified = 1 " +
                "WHERE Hash = @Hash ";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public Dictionary<long, Block> Select()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE IsDiscarded = 0";

            Dictionary<long, Block> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new Dictionary<long, Block>();

                    while (dr.Read())
                    {
                        Block block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");


                        result.Add(block.Id, block);
                    }
                }
            }

            return result;
        }

        public Dictionary<long, Block> SelectByHeightRange(long from, long to)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE Id BETWEEN @FromHeight AND @ToHeight " +
                "AND IsDiscarded = 0 ";

            Dictionary<long, Block> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@FromHeight", from);
                cmd.Parameters.AddWithValue("@ToHeight", to);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new Dictionary<long, Block>();

                    while (dr.Read())
                    {
                        Block block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");

                        result.Add(block.Id, block);
                    }
                }
            }

            return result;
        }

        public bool HasBlock(long height)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM Blocks " +
                "WHERE IsDiscarded = 0 " +
                "AND Height = @Height LIMIT 1 ";

            bool hasBlock = false;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Height", height);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    hasBlock = dr.HasRows;
                }
            }

            return hasBlock;
        }

        public Block SelectLast()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE IsDiscarded = 0 " +
                "ORDER BY Height DESC,Timestamp DESC LIMIT 1 ";

            Block block = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");
                    }
                }
            }

            return block;
        }

        public Block SelectLastConfirmed()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE IsDiscarded = 0 AND IsVerified = 1 " +
                "ORDER BY Height DESC,Timestamp DESC LIMIT 1 ";

            Block block = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");
                    }
                }
            }

            return block;
        }

        public Block SelectById(long id)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE Id = @Id " +
                "AND IsDiscarded = 0 ";

            Block block = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");
                    }
                }
            }

            return block;
        }

        public List<Block> SelectByHeight(long height)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE Height = @Height " +
                "AND IsDiscarded = 0 ";

            var result = new List<Block>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Height", height);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");

                        result.Add(block);
                    }
                }
            }

            return result;
        }

        public Block SelectByHash(string hash)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE Hash = @Hash " +
                "AND IsDiscarded = 0 ";

            Block block = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");
                    }
                }
            }

            return block;
        }

        public List<Block> SelectByPreviousHash(string prevHash)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE PreviousBlockHash = @PreviousBlockHash " +
                "AND IsDiscarded = 0 ";

            var result = new List<Block>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@PreviousBlockHash", prevHash);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");

                        result.Add(block);
                    }
                }
            }

            return result;
        }

        public List<Block> SelectTipBlocks()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE NextBlockHash IS NULL ";

            var result = new List<Block>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");

                        result.Add(block);
                    }
                }
            }

            return result;
        }

        public long CountBranchLength(string blockHash)
        {
            const string SQL_STATEMENT =
                "WITH RECURSIVE down(Hash, PreviousBlockHash, NextBlockHash) AS " +
                "( " +
                "   SELECT Hash, PreviousBlockHash, NextBlockHash " +
                "   FROM Blocks " +
                "   WHERE Hash = @Hash " +
                "   UNION " +
                "   SELECT a.Hash, a.PreviousBlockHash, a.NextBlockHash " +
                "   FROM Blocks a, down b " +
                "   WHERE b.Hash = a.NextBlockHash AND " +
                "   (SELECT COUNT(0) FROM Blocks WHERE PreviousBlockHash = b.Hash) <= 1 " +
                ") SELECT count(0) FROM down WHERE Hash != @Hash ";

            long result = 0;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", blockHash);

                cmd.Connection.Open();
                result = Convert.ToInt64(cmd.ExecuteScalar());
            }

            return result;
        }

        public long CountOfDescendants(string hash)
        {
            const string SQL_STATEMENT =
                "WITH RECURSIVE down(Hash, PreviousBlockHash, NextBlockHash) AS " +
                "( " +
                "   SELECT Hash, PreviousBlockHash, NextBlockHash " +
                "   FROM Blocks " +
                "   WHERE Hash = @Hash " +
                "   UNION " +
                "   SELECT a.Hash, a.PreviousBlockHash, a.NextBlockHash " +
                "   FROM Blocks a, down b " +
                "   WHERE b.Hash = a.PreviousBlockHash " +
                ") SELECT COUNT(0) - 1 FROM down ;";

            long result = 0;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);

                cmd.Connection.Open();
                result = Convert.ToInt64(cmd.ExecuteScalar());
            }

            return result;
        }

        public List<string> SelectHashesOfDescendants(string hash)
        {
            const string SQL_STATEMENT =
            "WITH RECURSIVE down(Hash) AS " +
            "( " +
            "   SELECT Hash " +
            "   FROM Blocks " +
            "   WHERE Hash = @Hash " +
            "   UNION " +
            "   SELECT a.Hash " +
            "   FROM Blocks a, down b " +
            "   WHERE b.Hash = a.PreviousBlockHash " +
            ") SELECT Hash FROM down WHERE Hash != @Hash ";

            var result = new List<string>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add(GetDataValue<string>(dr, "Hash"));
                    }
                }
            }

            return result;
        }

        public List<Block> SelectPreviousBlocks(long lastHeight, int blockCount)
        {
            const string SQL_STATEMENT =
            "WITH RECURSIVE down(Id, Hash, Version, Height, PreviousBlockHash, Bits, Nonce, Timestamp, NextBlockHash, TotalAmount, TotalFee, GeneratorId, BlockSignature, PayloadHash, IsDiscarded, IsVerified, num) AS " +
            "( " +
            "   SELECT Id, Hash, Version, Height, PreviousBlockHash, Bits, Nonce, Timestamp, NextBlockHash, TotalAmount, TotalFee, GeneratorId, BlockSignature, PayloadHash, IsDiscarded, IsVerified, 1 as num " +
            "   FROM Blocks " +
            "   WHERE Height = @LastHeight " +
            "   UNION " +
            "   SELECT a.Id, a.Hash, a.Version, a.Height, a.PreviousBlockHash, a.Bits, a.Nonce, a.Timestamp, a.NextBlockHash, a.TotalAmount, a.TotalFee, a.GeneratorId, a.BlockSignature, a.PayloadHash, a.IsDiscarded, a.IsVerified, b.num + 1 as num " +
            "   FROM Blocks a, down b " +
            "   WHERE b.PreviousBlockHash = a.Hash and b.num < @BlockCount " +
            ") SELECT Id, Hash, Version, Height, PreviousBlockHash, Bits, Nonce, Timestamp, NextBlockHash, TotalAmount, TotalFee, GeneratorId, BlockSignature, PayloadHash, IsDiscarded, IsVerified FROM down; ";

            var result = new List<Block>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@LastHeight", lastHeight);
                cmd.Parameters.AddWithValue("@BlockCount", blockCount);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.BlockSignature = GetDataValue<string>(dr, "BlockSignature");
                        block.PayloadHash = GetDataValue<string>(dr, "PayloadHash");

                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");

                        result.Add(block);
                    }
                }
            }

            return result;
        }

        public long SelectIdByHeight(long height)
        {
            const string SQL_STATEMENT =
                "SELECT Id " +
                "FROM Blocks " +
                "WHERE Height = @Height " +
                "AND IsDiscarded = 0 ";

            long blockId = 0;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Height", height);

                cmd.Connection.Open();
                blockId = (long)cmd.ExecuteScalar();
            }

            return blockId;
        }

        public List<long> SelectIdByLimit(long blockId, int limit)
        {
            const string SQL_STATEMENT =
                "SELECT Id " +
                "FROM Blocks " +
                "WHERE Id > @Id " +
                "AND IsDiscarded = 0 " +
                "LIMIT @Id, @Limit";

            List<long> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", blockId);
                cmd.Parameters.AddWithValue("@Limit", limit);
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<long>();

                    while (dr.Read())
                    {
                        Block block = new Block();

                        long id = GetDataValue<long>(dr, "Id");

                        result.Add(id);
                    }
                }
            }

            return result;
        }

        public Dictionary<long, Block> SelectByLimit(long blockId, int limit)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Blocks " +
                "WHERE Id > @Id " +
                "AND IsDiscarded = 0 " +
                "LIMIT @Id, @Limit ";

            Dictionary<long, Block> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", blockId);
                cmd.Parameters.AddWithValue("@Limit", limit);
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new Dictionary<long, Block>();

                    while (dr.Read())
                    {
                        Block block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");

                        result.Add(block.Id, block);
                    }
                }
            }

            return result;
        }

        public List<Block> DistinguishBlockVerifiedByHashes(List<string> hashes)
        {
            string SQL_STATEMENT = $"SELECT * FROM Blocks WHERE Hash IN('{string.Join("','", hashes.ToArray())}');";
            List<Block> result = null;
            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Block>();

                    while (dr.Read())
                    {
                        Block block = new Block();

                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Version = GetDataValue<int>(dr, "Version");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.PreviousBlockHash = GetDataValue<string>(dr, "PreviousBlockHash");
                        block.Bits = GetDataValue<long>(dr, "Bits");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        block.NextBlockHash = GetDataValue<string>(dr, "NextBlockHash");
                        block.TotalAmount = GetDataValue<long>(dr, "TotalAmount");
                        block.TotalFee = GetDataValue<long>(dr, "TotalFee");
                        block.GeneratorId = GetDataValue<string>(dr, "GeneratorId");
                        block.IsDiscarded = GetDataValue<bool>(dr, "IsDiscarded");
                        block.IsVerified = GetDataValue<bool>(dr, "IsVerified");

                        result.Add(block);
                    }
                }
            }

            return result;
        }

        public ListSinceBlock GetSinceBlock(string blockHash, long height, long confirmations)
        {
            const string SQL_STATEMENT =
                "SELECT t1.TransactionHash, t1.'Index', t1.amount, t4.id AS Address, t4.Tag, t2.TotalInput, t2.Fee, t2.Timestamp, t3.Hash as BlockHash, t3.Timestamp as BlockTime, (@Height - t3.Height) as Confirmations "
                + "FROM OutputList t1 JOIN Transactions t2 ON t1.TransactionHash = t2.Hash "
                + "JOIN Blocks t3 ON t2.BlockHash = t3.Hash "
                + "JOIN Accounts t4 ON t1.ReceiverId = t4.Id "
                + "WHERE t1.IsDiscarded = 0 AND t2.IsDiscarded = 0 AND t3.IsDiscarded = 0 AND t3.height > (SELECT Height FROM Blocks WHERE Hash = @Hash) AND t3.height < @Height;";
            
            List<SinceBlock> sinceBlock = null;
            string lastestBlock = "";
            long blockTime = 0;
            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", blockHash);
                cmd.Parameters.AddWithValue("@Height", height);
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    sinceBlock = new List<SinceBlock>();
                    while (dr.Read())
                    {
                        SinceBlock block  = new SinceBlock();
                        block.Account = "";
                        block.Address = GetDataValue<string>(dr, "Address");
                        block.amount = GetDataValue<long>(dr, "Amount");
                        block.Category = (GetDataValue<long>(dr, "TotalInput") == 0 && GetDataValue<long>(dr, "Fee") == 0 && GetDataValue<long>(dr, "Confirmations") > 100) ? "generate" 
                            : (GetDataValue<long>(dr, "TotalInput") == 0 && GetDataValue<long>(dr, "Fee") == 0 && GetDataValue<long>(dr, "Confirmations") < 100) ? "immature" : "receive";
                        block.Vout = GetDataValue<long>(dr, "Index");
                        block.Fee = GetDataValue<long>(dr, "Fee");
                        block.Confirmations = GetDataValue<long>(dr, "Confirmations");
                        block.BlockHash = GetDataValue<string>(dr, "BlockHash");
                        block.BlockTime = GetDataValue<long>(dr, "BlockTime");
                        block.Label = GetDataValue<string>(dr, "Tag") ?? "";
                        block.TxId = GetDataValue<string>(dr, "TransactionHash");
                        if (block.BlockTime > blockTime)
                        {
                            lastestBlock = block.BlockHash;
                        }
                        sinceBlock.Add(block);
                    }
                }
            }
            if (sinceBlock == null && sinceBlock.Count == 0)
            {
                return null;
            }
            else
            {
                return new ListSinceBlock { LastBlock = lastestBlock, Transactions = sinceBlock.ToArray() };
            }
        }

        public long GetBlockReward(string blockHash)
        {
            
            /*
            const string SQL_STATEMENT =
                "SELECT t1.TransactionHash, t1.'Index', t1.amount, t4.id AS Address, t4.Tag, t2.TotalInput, t2.Fee, t2.Timestamp, t3.Hash as BlockHash, t3.Timestamp as BlockTime, (@Height - t3.Height) as Confirmations "
                + "FROM OutputList t1 JOIN Transactions t2 ON t1.TransactionHash = t2.Hash "
                + "JOIN Blocks t3 ON t2.BlockHash = t3.Hash "
                + "JOIN Accounts t4 ON t1.ReceiverId = t4.Id "
                + "WHERE t1.IsDiscarded = 0 AND t2.IsDiscarded = 0 AND t3.IsDiscarded = 0 AND t3.height > (SELECT Height FROM Blocks WHERE Hash = @Hash) AND t3.height < @Height;";

            List<SinceBlock> sinceBlock = null;
            string lastestBlock = "";
            long blockTime = 0;
            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", blockHash);
                cmd.Parameters.AddWithValue("@Height", height);
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    sinceBlock = new List<SinceBlock>();
                    while (dr.Read())
                    {
                        SinceBlock block = new SinceBlock();
                        block.Account = "";
                        block.Address = GetDataValue<string>(dr, "Address");
                        block.amount = GetDataValue<long>(dr, "Amount");
                        block.Category = (GetDataValue<long>(dr, "TotalInput") == 0 && GetDataValue<long>(dr, "Fee") == 0 && GetDataValue<long>(dr, "Confirmations") > 100) ? "generate"
                            : (GetDataValue<long>(dr, "TotalInput") == 0 && GetDataValue<long>(dr, "Fee") == 0 && GetDataValue<long>(dr, "Confirmations") < 100) ? "immature" : "receive";
                        block.Vout = GetDataValue<long>(dr, "Index");
                        block.Fee = GetDataValue<long>(dr, "Fee");
                        block.Confirmations = GetDataValue<long>(dr, "Confirmations");
                        block.BlockHash = GetDataValue<string>(dr, "BlockHash");
                        block.BlockTime = GetDataValue<long>(dr, "BlockTime");
                        block.Label = GetDataValue<string>(dr, "Tag") ?? "";
                        block.TxId = GetDataValue<string>(dr, "TransactionHash");
                        if (block.BlockTime > blockTime)
                        {
                            lastestBlock = block.BlockHash;
                        }
                        sinceBlock.Add(block);
                    }
                }
            }
            if (sinceBlock == null && sinceBlock.Count == 0)
            {
                return null;
            }
            else
            {
                return new ListSinceBlock { LastBlock = lastestBlock, Transactions = sinceBlock.ToArray() };
            }
            */
            return 0;
        }
    }
}
