// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FiiiChain.Data
{
    public class MiningPoolDac : DataAccessComponent
    {
        public int SaveToDB(MiningPool miningPool)
        {
            int rows = 0;
            StringBuilder sql = new StringBuilder("BEGIN TRANSACTION;");

            sql.Append("INSERT INTO MiningPool " +
                "(Name, PublicKey,Signature) " +
                $"VALUES ('{miningPool.Name}', '{miningPool.PublicKey}' , '{miningPool.Signature}');");

            sql.Append("END TRANSACTION;");
            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(sql.ToString(), con))
            {
                cmd.Connection.Open();
                rows = cmd.ExecuteNonQuery();
            }
            return rows;
        }

        public List<int> SaveToDB(IEnumerable<MiningPool> miningPools)
        {
            List<int> result = new List<int>();
            foreach (var item in miningPools)
            {
                result.Add( SaveToDB(item));
            }
            return result;
        }

        public List<MiningPool> GetAllMiningPools()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM MiningPool ";
                
            var result = new List<MiningPool>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var miningPool = new MiningPool();

                        miningPool.Id = GetDataValue<long>(dr, "Id");
                        miningPool.Name = GetDataValue<string>(dr, "NAME");
                        miningPool.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        miningPool.Signature = GetDataValue<string>(dr, "Signature");

                        result.Add(miningPool);
                    }
                }
            }

            return result;
        }

        public MiningPool SelectMiningPoolByPublicKey(string publicKey)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM MiningPool " +
                "WHERE PublicKey = @PublicKey LIMIT 1";

            MiningPool miningPool = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@PublicKey", publicKey);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        miningPool = new MiningPool();

                        miningPool.Id = GetDataValue<long>(dr, "Id");
                        miningPool.Name = GetDataValue<string>(dr, "NAME");
                        miningPool.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        miningPool.Signature = GetDataValue<string>(dr, "Signature");
                    }
                }
            }

            return miningPool;
        }
        public MiningPool SelectMiningPoolByName(string name)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM MiningPool " +
                "WHERE NAME = @NAME LIMIT 1";

            MiningPool miningPool = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@NAME", name);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        miningPool = new MiningPool();

                        miningPool.Id = GetDataValue<long>(dr, "Id");
                        miningPool.Name = GetDataValue<string>(dr, "NAME");
                        miningPool.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        miningPool.Signature = GetDataValue<string>(dr, "Signature");
                    }
                }
            }

            return miningPool;
        }
        public MiningPool SelectMiningPoolById(long id)
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM MiningPool " +
                "WHERE Id = @Id ";

            
            var miningPool = new MiningPool();
            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        miningPool.Id = GetDataValue<long>(dr, "Id");
                        miningPool.Name = GetDataValue<string>(dr, "NAME");
                        miningPool.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        miningPool.Signature = GetDataValue<string>(dr, "Signature");
                    }
                }
            }

            return miningPool;
        }

        public void UpdateMiningPools(IEnumerable<MiningPool> miningPools)
        {
            foreach (var item in miningPools)
            {
                UpdateMiningPool(item);
            }
        }

        public void UpdateMiningPool(MiningPool miningPool)
        {
            const string SQL_STATEMENT =
                "UPDATE MiningPool " +
                "SET NAME=@NAME " +
                "Where PublicKey = @PublicKey and Signature = @Signature";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@NAME", miningPool.Name);
                cmd.Parameters.AddWithValue("@PublicKey", miningPool.PublicKey);
                cmd.Parameters.AddWithValue("@Signature", miningPool.Signature);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}