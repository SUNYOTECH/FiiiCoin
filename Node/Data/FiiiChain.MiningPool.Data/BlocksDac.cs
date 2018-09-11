using FiiiChain.MiningPool.Entities;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FiiiChain.MiningPool.Data
{
    public class BlocksDac : DataAccessComponent
    {
        public void Insert(Blocks block)
        {
            const string SQL_STATEMENT =
                "INSERT INTO Blocks " +
                "(Hash, Height, Timstamp, Generator, Nonce, TotalReward, TotalHash, Confirmed, IsDiscarded) " +
                "VALUES (@Id, @Hash, @Height, @Timstamp, @Generator, @Nonce, @TotalReward, @TotalHash, @Confirmed, @IsDiscarded);";
            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Hash", block.Hash);
                cmd.Parameters.AddWithValue("@Height", block.Height);
                cmd.Parameters.AddWithValue("@Timstamp", Framework.Time.EpochTime);
                cmd.Parameters.AddWithValue("@Generator", block.Generator);
                cmd.Parameters.AddWithValue("@Nonce", block.Nonce);
                cmd.Parameters.AddWithValue("@TotalReward", block.TotalReward);
                cmd.Parameters.AddWithValue("@TotalHash", block.TotalHash);
                cmd.Parameters.AddWithValue("@Confirmed", block.Confirmed);
                cmd.Parameters.AddWithValue("@IsDiscarded", block.IsDiscarded);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(long id)
        {
            const string SQL_STATEMENT =
                "DELETE FROM Blocks " +
                "WHERE Id = @Id;";

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(string hash)
        {
            const string SQL_STATEMENT =
                "DELETE FROM Blocks " +
                "WHERE Hash = @Hash;";

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Blocks> SelectAll()
        {
            const string SQL_STATEMENT =
                "SELECT Id, Hash, Height, Timstamp, Generator, Nonce, TotalReward, TotalHash, Confirmed, IsDiscarded " +
                "FROM Blocks;";

            List<Blocks> result = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Blocks>();

                    while (dr.Read())
                    {
                        Blocks block = new Blocks();
                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.Timstamp = GetDataValue<long>(dr, "Timstamp");
                        block.Generator = GetDataValue<string>(dr, "Generator");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.TotalReward = GetDataValue<long>(dr, "TotalReward");
                        block.TotalHash = GetDataValue<long>(dr, "TotalHash");
                        block.Confirmed = GetDataValue<int>(dr, "Confirmed");
                        block.IsDiscarded = GetDataValue<int>(dr, "IsDiscarded");

                        result.Add(block);
                    }
                }
            }

            return result;
        }

        public Blocks SelectById(long id)
        {
            const string SQL_STATEMENT =
                "SELECT Id, Hash, Height, Timstamp, Generator, Nonce, TotalReward, TotalHash, Confirmed, IsDiscarded " +
                "FROM Blocks WHERE Id=@Id";

            Blocks block = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        block = new Blocks();
                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.Timstamp = GetDataValue<long>(dr, "Timstamp");
                        block.Generator = GetDataValue<string>(dr, "Generator");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.TotalReward = GetDataValue<long>(dr, "TotalReward");
                        block.TotalHash = GetDataValue<long>(dr, "TotalHash");
                        block.Confirmed = GetDataValue<int>(dr, "Confirmed");
                        block.IsDiscarded = GetDataValue<int>(dr, "IsDiscarded");
                    }
                }
            }

            return block;
        }

        public Blocks SelectByHash(string hash)
        {
            const string SQL_STATEMENT =
                "SELECT TOP 1 Id, Hash, Height, Timstamp, Generator, Nonce, TotalReward, TotalHash, Confirmed, IsDiscarded " +
                "FROM Blocks WHERE Hash=@Hash";

            Blocks block = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        block = new Blocks();
                        block.Id = GetDataValue<long>(dr, "Id");
                        block.Hash = GetDataValue<string>(dr, "Hash");
                        block.Height = GetDataValue<long>(dr, "Height");
                        block.Timstamp = GetDataValue<long>(dr, "Timstamp");
                        block.Generator = GetDataValue<string>(dr, "Generator");
                        block.Nonce = GetDataValue<long>(dr, "Nonce");
                        block.TotalReward = GetDataValue<long>(dr, "TotalReward");
                        block.TotalHash = GetDataValue<long>(dr, "TotalHash");
                        block.Confirmed = GetDataValue<int>(dr, "Confirmed");
                        block.IsDiscarded = GetDataValue<int>(dr, "IsDiscarded");
                    }
                }
            }

            return block;
        }

        public void UpdateConfirmed(long id, int confirmed, int isDiscarded)
        {
            const string SQL_STATEMENT =
                "UPDATE Blocks " +
                "SET Confirmed = @Confirmed, IsDiscarded = @IsDiscarded " +
                "WHERE Id = @Id AND Confirmed = 0;";

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Confirmed", confirmed);
                cmd.Parameters.AddWithValue("@IsDiscarded", isDiscarded);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateConfirmed(string hash, int confirmed, int isDiscarded)
        {
            const string SQL_STATEMENT =
                "UPDATE Blocks " +
                "SET Confirmed = @Confirmed, IsDiscarded = @IsDiscarded " +
                "WHERE Hash = @Hash AND Confirmed = 0;";

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Parameters.AddWithValue("@Confirmed", confirmed);
                cmd.Parameters.AddWithValue("@IsDiscarded", isDiscarded);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsExisted(long id)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM Blocks " +
                "WHERE Id = @Id;";

            bool hasBlock = false;

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    hasBlock = dr.HasRows;
                }
            }

            return hasBlock;
        }

        public bool IsExisted(string hash)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM Blocks " +
                "WHERE Hash = @Hash;";

            bool hasBlock = false;

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    hasBlock = dr.HasRows;
                }
            }

            return hasBlock;
        }

        public List<string> GetAppointedHash(long height)
        {
            const string SQL_STATEMENT =
                "SELECT Hash " +
                "FROM Blocks " +
                "WHERE Height <= @Height;";

            List<string> result = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Height", height);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Blocks block = new Blocks();
                        block.Hash = GetDataValue<string>(dr, "Hash");

                        result.Add(block.Hash);
                    }
                }
            }

            return result;
        }
    }
}
