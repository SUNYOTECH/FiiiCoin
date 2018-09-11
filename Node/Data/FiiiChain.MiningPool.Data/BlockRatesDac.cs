using FiiiChain.MiningPool.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace FiiiChain.MiningPool.Data
{
    public class BlockRatesDac : DataAccessComponent
    {
        public void Insert(BlockRates blockRates)
        {
            const string SQL_STATEMENT =
                "INSERT INTO BlockRates " +
                "(Time, Blocks, Difficulty) " +
                "VALUES (@Time, @Blocks, @Difficulty);";
            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Time", blockRates.Time);
                cmd.Parameters.AddWithValue("@Blocks", blockRates.Blocks);
                cmd.Parameters.AddWithValue("@Difficulty", blockRates.Difficulty);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(long id)
        {
            const string SQL_STATEMENT =
                "DELETE FROM BlockRates " +
                "WHERE Id = @Id;";
            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<BlockRates> SelectAll()
        {
            const string SQL_STATEMENT =
                "SELECT Id, Time, Blocks, Difficulty " +
                "FROM BlockRates;";

            List<BlockRates> result = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<BlockRates>();

                    while (dr.Read())
                    {
                        BlockRates blockRates = new BlockRates();
                        blockRates.Id = GetDataValue<long>(dr, "Id");
                        blockRates.Time = GetDataValue<long>(dr, "Time");
                        blockRates.Blocks = GetDataValue<long>(dr, "Blocks");
                        blockRates.Difficulty = GetDataValue<long>(dr, "Difficulty");

                        result.Add(blockRates);
                    }
                }
            }

            return result;
        }

        public BlockRates SelectById(long id)
        {
            const string SQL_STATEMENT =
                "SELECT Id, Time, Blocks, Difficulty " +
                "FROM BlockRates WHERE Id = @Id";

            BlockRates blockRates = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        blockRates = new BlockRates();
                        blockRates.Id = GetDataValue<long>(dr, "Id");
                        blockRates.Time = GetDataValue<long>(dr, "Time");
                        blockRates.Blocks = GetDataValue<long>(dr, "Blocks");
                        blockRates.Difficulty = GetDataValue<long>(dr, "Difficulty");
                    }
                }
            }

            return blockRates;
        }

        public void Update(BlockRates entity)
        {
            const string SQL_STATEMENT =
                "UPDATE BlockRates " +
                "SET Time = @Time, Blocks = @Blocks, Difficulty = @Difficulty " +
                "WHERE Id = @Id;";

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Time", entity.Time);
                cmd.Parameters.AddWithValue("@Blocks", entity.Blocks);
                cmd.Parameters.AddWithValue("@Difficulty", entity.Difficulty);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsExisted(long id)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM BlockRates " +
                "WHERE Id = @Id;";

            bool hasEntity = false;

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    hasEntity = dr.HasRows;
                }
            }

            return hasEntity;
        }
    }
}
