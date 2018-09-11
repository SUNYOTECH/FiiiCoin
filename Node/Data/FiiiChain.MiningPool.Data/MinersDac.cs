using System;
using System.Collections.Generic;
using System.Text;
using FiiiChain.MiningPool.Entities;
using System.Data.SqlClient;

namespace FiiiChain.MiningPool.Data
{
    public class MinersDac : DataAccessComponent
    {
        public void Insert(Miners miner)
        {
            const string SQL_STATEMENT =
                "INSERT INTO Miners " +
                "(Address, Account, Type, SN, Status, Timstamp, LastLoginTime) " +
                "VALUES (@Address, @Account, @Type, @SN, @Status, @Timstamp, @LastLoginTime);";
            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Address", miner.Address);
                cmd.Parameters.AddWithValue("@Account", miner.Account);
                cmd.Parameters.AddWithValue("@Type", miner.Type);
                cmd.Parameters.AddWithValue("@SN", miner.SN);
                cmd.Parameters.AddWithValue("@Status", miner.Status);
                cmd.Parameters.AddWithValue("@Timstamp", miner.Timstamp);
                cmd.Parameters.AddWithValue("@LastLoginTime", miner.LastLoginTime);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(long id)
        {
            const string SQL_STATEMENT =
                "DELETE FROM Miners " +
                "WHERE Id = @Id;";

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(string address)
        {
            const string SQL_STATEMENT =
                "DELETE FROM Miners " +
                "WHERE Address = @Address;";

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Miners> SelectAll()
        {
            const string SQL_STATEMENT =
                "SELECT Id, Address, Account, Type, SN, Status, Timstamp, LastLoginTime " +
                "FROM Miners;";

            List<Miners> result = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Miners>();

                    while (dr.Read())
                    {
                        Miners miner = new Miners();
                        miner.Id = GetDataValue<long>(dr, "Id");
                        miner.Address = GetDataValue<string>(dr, "Address");
                        miner.Account = GetDataValue<string>(dr, "Account");
                        miner.Type = GetDataValue<int>(dr, "Type");
                        miner.SN = GetDataValue<string>(dr, "SN");
                        miner.Status = GetDataValue<int>(dr, "Status");
                        miner.Timstamp = GetDataValue<long>(dr, "Timstamp");
                        miner.LastLoginTime = GetDataValue<long>(dr, "LastLoginTime");

                        result.Add(miner);
                    }
                }
            }

            return result;
        }

        public Miners SelectById(long id)
        {
            const string SQL_STATEMENT =
                "SELECT Id, Address, Account, Type, SN, Status, Timstamp, LastLoginTime " +
                "FROM Miners WHERE Id=@Id";

            Miners miner = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        miner = new Miners();
                        miner.Id = GetDataValue<long>(dr, "Id");
                        miner.Address = GetDataValue<string>(dr, "Address");
                        miner.Account = GetDataValue<string>(dr, "Account");
                        miner.Type = GetDataValue<int>(dr, "Type");
                        miner.SN = GetDataValue<string>(dr, "SN");
                        miner.Status = GetDataValue<int>(dr, "Status");
                        miner.Timstamp = GetDataValue<long>(dr, "Timstamp");
                        miner.LastLoginTime = GetDataValue<long>(dr, "LastLoginTime");
                    }
                }
            }

            return miner;
        }
        
        public Miners GetMinerByAddress(string address)
        {
            const string SQL_STATEMENT =
                "SELECT TOP 1 Id, Address, Account, Type, SN, Status, Timstamp, LastLoginTime " +
                "FROM Miners WHERE Address=@Address";

            Miners miner = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
                {
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Connection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            miner = new Miners();
                            miner.Id = GetDataValue<long>(dr, "Id");
                            miner.Address = GetDataValue<string>(dr, "Address");
                            miner.Account = GetDataValue<string>(dr, "Account");
                            miner.Type = GetDataValue<int>(dr, "Type");
                            miner.SN = GetDataValue<string>(dr, "SN");
                            miner.Status = GetDataValue<int>(dr, "Status");
                            miner.Timstamp = GetDataValue<long>(dr, "Timstamp");
                            miner.LastLoginTime = GetDataValue<long>(dr, "LastLoginTime");
                        }
                    }
                }
            }

            return miner;
        }

        public List<Miners> GetMinersBySN(string sn)
        {
            const string SQL_STATEMENT =
                "SELECT Id, Address, Account, Type, SN, Status, Timstamp, LastLoginTime " +
                "FROM Miners WHERE SN=@SN";

            List<Miners> miners = null;

            using (SqlConnection conn = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, conn))
            {
                cmd.Parameters.AddWithValue("@SN", sn);
                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Miners miner = new Miners();
                        miner.Id = GetDataValue<long>(dr, "Id");
                        miner.Address = GetDataValue<string>(dr, "Address");
                        miner.Account = GetDataValue<string>(dr, "Account");
                        miner.Type = GetDataValue<int>(dr, "Type");
                        miner.SN = GetDataValue<string>(dr, "SN");
                        miner.Status = GetDataValue<int>(dr, "Status");
                        miner.Timstamp = GetDataValue<long>(dr, "Timstamp");
                        miner.LastLoginTime = GetDataValue<long>(dr, "LastLoginTime");

                        miners.Add(miner);
                    }
                }
            }

            return miners;
        }

        public void UpdateStatus(long id, int status)
        {
            const string SQL_STATEMENT =
                "UPDATE Miners " +
                "SET Status = @Status " +
                "WHERE Id = @Id;";

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Status", status);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateStatus(string address, int status, string sn)
        {
            const string SQL_STATEMENT =
                "UPDATE Miners " +
                "SET Status = @Status, Timstamp = @Timstamp " +
                "WHERE Address = @Address AND SN = @SN;";

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@Timstamp", Framework.Time.EpochTime);
                cmd.Parameters.AddWithValue("@SN", sn);
                cmd.Parameters.AddWithValue("@Status", status);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsExisted(long id)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM Miners " +
                "WHERE Id = @Id;";

            bool hasMiner = false;

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    hasMiner = dr.HasRows;
                }
            }

            return hasMiner;
        }

        public bool IsSNExisted(string sn)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM Miners " +
                "WHERE SN = @SN;";

            bool hasSN = false;

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@SN", sn);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    hasSN = dr.HasRows;
                }
            }

            return hasSN;
        }

        public bool IsAddressExisted(string address)
        {
            const string SQL_STATEMENT =
                "SELECT 1 " +
                "FROM Miners " +
                "WHERE Address = @Address;";

            bool hasAddress = false;

            using (SqlConnection con = new SqlConnection(CacheConnectionString))
            using (SqlCommand cmd = new SqlCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Address", address);

                cmd.Connection.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    hasAddress = dr.HasRows;
                }
            }

            return hasAddress;
        }
    }
}
