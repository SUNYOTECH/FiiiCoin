using System.Data.SqlClient;
using System.Collections.Generic;
using FiiiChain.MiningPool.Entities;

namespace FiiiChain.MiningPool.Data
{
    public class RewardListDac : DataAccessComponent
    {
        public void Insert(RewardList reward)
        {
            const string SQL_STATEMENT =
                "INSERT INTO RewardList " +
                "(Id, BlockHash, MinerAddress, Hashes, OriginalReward, ActualReward, Paid, GenerateTime, PaidTime, TransactionHash) " +
                "VALUES (@Id, @BlockHash, @MinerAddress, @Hashes, @OriginalReward, @ActualReward, @Paid, @GenerateTime, @PaidTime, @TransactionHash);";
            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Id", reward.Id);
                cmd.Parameters.AddWithValue("@BlockHash", reward.BlockHash);
                cmd.Parameters.AddWithValue("@MinerAddress", reward.MinerAddress);
                cmd.Parameters.AddWithValue("@Hashes", reward.Hashes);
                cmd.Parameters.AddWithValue("@OriginalReward", reward.OriginalReward);
                cmd.Parameters.AddWithValue("@ActualReward", reward.ActualReward);
                cmd.Parameters.AddWithValue("@Paid", reward.Paid);
                cmd.Parameters.AddWithValue("@GenerateTime", reward.GenerateTime);
                cmd.Parameters.AddWithValue("@PaidTime", reward.PaidTime);
                cmd.Parameters.AddWithValue("@TransactionHash", reward.TransactionHash);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(long id)
        {
            const string SQL_STATEMENT =
                "DELETE FROM RewardList " +
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
                "DELETE FROM RewardList " +
                "WHERE Hash = @Hash;";

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<RewardList> SelectAll()
        {
            const string SQL_STATEMENT =
                "SELECT Id, BlockHash, MinerAddress, Hashes, OriginalReward, ActualReward, Paid, GenerateTime, PaidTime, TransactionHash " +
                "FROM RewardList;";

            List<RewardList> result = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<RewardList>();

                    while (dr.Read())
                    {
                        RewardList reward = new RewardList();
                        reward.Id = GetDataValue<long>(dr, "Id");
                        reward.BlockHash = GetDataValue<string>(dr, "BlockHash");
                        reward.MinerAddress = GetDataValue<string>(dr, "MinerAddress");
                        reward.Hashes = GetDataValue<long>(dr, "Hashes");
                        reward.OriginalReward = GetDataValue<long>(dr, "OriginalReward");
                        reward.ActualReward = GetDataValue<long>(dr, "ActualReward");
                        reward.Paid = GetDataValue<int>(dr, "Paid");
                        reward.GenerateTime = GetDataValue<long>(dr, "GenerateTime");
                        reward.PaidTime = GetDataValue<int>(dr, "PaidTime");
                        reward.TransactionHash = GetDataValue<string>(dr, "TransactionHash");

                        result.Add(reward);
                    }
                }
            }

            return result;
        }

        public RewardList SelectById(long id)
        {
            const string SQL_STATEMENT =
                "SELECT Id, BlockHash, MinerAddress, Hashes, OriginalReward, ActualReward, Paid, GenerateTime, PaidTime, TransactionHash " +
                "FROM RewardList WHERE Id=@Id";

            RewardList reward = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        reward = new RewardList();
                        reward.Id = GetDataValue<long>(dr, "Id");
                        reward.BlockHash = GetDataValue<string>(dr, "BlockHash");
                        reward.MinerAddress = GetDataValue<string>(dr, "MinerAddress");
                        reward.Hashes = GetDataValue<long>(dr, "Hashes");
                        reward.OriginalReward = GetDataValue<long>(dr, "OriginalReward");
                        reward.ActualReward = GetDataValue<long>(dr, "ActualReward");
                        reward.Paid = GetDataValue<int>(dr, "Paid");
                        reward.GenerateTime = GetDataValue<long>(dr, "GenerateTime");
                        reward.PaidTime = GetDataValue<int>(dr, "PaidTime");
                        reward.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                    }
                }
            }

            return reward;
        }

        public RewardList SelectByHash(string hash)
        {
            const string SQL_STATEMENT =
                "SELECT TOP 1 Id, BlockHash, MinerAddress, Hashes, OriginalReward, ActualReward, Paid, GenerateTime, PaidTime, TransactionHash " +
                "FROM RewardList WHERE Hash=@Hash";

            RewardList reward = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        reward = new RewardList();
                        reward.Id = GetDataValue<long>(dr, "Id");
                        reward.BlockHash = GetDataValue<string>(dr, "BlockHash");
                        reward.MinerAddress = GetDataValue<string>(dr, "MinerAddress");
                        reward.Hashes = GetDataValue<long>(dr, "Hashes");
                        reward.OriginalReward = GetDataValue<long>(dr, "OriginalReward");
                        reward.ActualReward = GetDataValue<long>(dr, "ActualReward");
                        reward.Paid = GetDataValue<int>(dr, "Paid");
                        reward.GenerateTime = GetDataValue<long>(dr, "GenerateTime");
                        reward.PaidTime = GetDataValue<int>(dr, "PaidTime");
                        reward.TransactionHash = GetDataValue<string>(dr, "TransactionHash");
                    }
                }
            }

            return reward;
        }

        public void UpdatePaid(long id, int paid)
        {
            const string SQL_STATEMENT =
                "UPDATE RewardList " +
                "SET Paid = @Paid, PaidTime = @PaidTime " +
                "WHERE Id = @Id;";

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Paid", paid);
                cmd.Parameters.AddWithValue("@PaidTime", Framework.Time.EpochTime);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdatePaid(string hash, int paid)
        {
            const string SQL_STATEMENT =
                "UPDATE RewardList " +
                "SET Paid = @Paid, PaidTime = @PaidTime " +
                "WHERE Hash = @Hash;";

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Parameters.AddWithValue("@Paid", paid);
                cmd.Parameters.AddWithValue("@PaidTime", Framework.Time.EpochTime);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsExisted(long id)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM RewardList " +
                "WHERE Id = @Id;";

            bool hasReward = false;

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    hasReward = dr.HasRows;
                }
            }

            return hasReward;
        }

        public bool IsExisted(string hash)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM RewardList " +
                "WHERE Hash = @Hash;";

            bool hasReward = false;

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    hasReward = dr.HasRows;
                }
            }

            return hasReward;
        }

        public List<RewardList> GetUnPaidReward(string address, int payStatus)
        {
            const string SQL_STATEMENT =
                "SELECT Id, BlockHash, MinerAddress, Hashes, OriginalReward, ActualReward, Paid, GenerateTime, PaidTime, TransactionHash " +
                "FROM RewardList " +
                "WHERE MinerAddress = @MinerAddress AND Paid = @Paid AND BlockHash IN (SELECT Hash FROM Blocks WHERE Confirmed = 1 AND IsDiscarded = 0);";

            List<RewardList> result = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@MinerAddress", address);
                cmd.Parameters.AddWithValue("@Paid", payStatus);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<RewardList>();

                    while (dr.Read())
                    {
                        RewardList reward = new RewardList();
                        reward.Id = GetDataValue<long>(dr, "Id");
                        reward.BlockHash = GetDataValue<string>(dr, "BlockHash");
                        reward.MinerAddress = GetDataValue<string>(dr, "MinerAddress");
                        reward.Hashes = GetDataValue<long>(dr, "Hashes");
                        reward.OriginalReward = GetDataValue<long>(dr, "OriginalReward");
                        reward.ActualReward = GetDataValue<long>(dr, "ActualReward");
                        reward.Paid = GetDataValue<int>(dr, "Paid");
                        reward.GenerateTime = GetDataValue<long>(dr, "GenerateTime");
                        reward.PaidTime = GetDataValue<int>(dr, "PaidTime");
                        reward.TransactionHash = GetDataValue<string>(dr, "TransactionHash");

                        result.Add(reward);
                    }
                }
            }

            return result;
        }

        public List<RewardList> GetPaidReward(string address, int payStatus)
        {
            const string SQL_STATEMENT =
                "SELECT Id, BlockHash, MinerAddress, Hashes, OriginalReward, ActualReward, Paid, GenerateTime, PaidTime, TransactionHash " +
                "FROM RewardList " +
                "WHERE MinerAddress = @MinerAddress AND Paid = @Paid;";

            List<RewardList> result = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@MinerAddress", address);
                cmd.Parameters.AddWithValue("@Paid", payStatus);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<RewardList>();

                    while (dr.Read())
                    {
                        RewardList reward = new RewardList();
                        reward.Id = GetDataValue<long>(dr, "Id");
                        reward.BlockHash = GetDataValue<string>(dr, "BlockHash");
                        reward.MinerAddress = GetDataValue<string>(dr, "MinerAddress");
                        reward.Hashes = GetDataValue<long>(dr, "Hashes");
                        reward.OriginalReward = GetDataValue<long>(dr, "OriginalReward");
                        reward.ActualReward = GetDataValue<long>(dr, "ActualReward");
                        reward.Paid = GetDataValue<int>(dr, "Paid");
                        reward.GenerateTime = GetDataValue<long>(dr, "GenerateTime");
                        reward.PaidTime = GetDataValue<int>(dr, "PaidTime");
                        reward.TransactionHash = GetDataValue<string>(dr, "TransactionHash");

                        result.Add(reward);
                    }
                }
            }

            return result;
        }

        public List<RewardList> GetAllUnPaidReward()
        {
            const string SQL_STATEMENT =
                "SELECT Id, BlockHash, MinerAddress, Hashes, OriginalReward, ActualReward, Paid, GenerateTime, PaidTime, TransactionHash " +
                "FROM RewardList " +
                "WHERE Paid = 0 AND BlockHash IN (SELECT Hash FROM Blocks WHERE Confirmed = 1 AND IsDiscarded = 0);";

            List<RewardList> result = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<RewardList>();

                    while (dr.Read())
                    {
                        RewardList reward = new RewardList();
                        reward.Id = GetDataValue<long>(dr, "Id");
                        reward.BlockHash = GetDataValue<string>(dr, "BlockHash");
                        reward.MinerAddress = GetDataValue<string>(dr, "MinerAddress");
                        reward.Hashes = GetDataValue<long>(dr, "Hashes");
                        reward.OriginalReward = GetDataValue<long>(dr, "OriginalReward");
                        reward.ActualReward = GetDataValue<long>(dr, "ActualReward");
                        reward.Paid = GetDataValue<int>(dr, "Paid");
                        reward.GenerateTime = GetDataValue<long>(dr, "GenerateTime");
                        reward.PaidTime = GetDataValue<int>(dr, "PaidTime");
                        reward.TransactionHash = GetDataValue<string>(dr, "TransactionHash");

                        result.Add(reward);
                    }
                }
            }

            return result;
        }

        public long GetActualReward(string address, string blockHash)
        {
            const string SQL_STATEMENT =
                "SELECT TOP 1 ActualReward " +
                "FROM RewardList " +
                "WHERE BlockHash = @BlockHash AND MinerAddress = @MinerAddress;";
            long result = 0L;
            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@BlockHash", blockHash);
                cmd.Parameters.AddWithValue("@MinerAddress", address);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result = GetDataValue<long>(dr, "ActualReward");
                    }
                }
            }

            return result;
        }
    }
}
