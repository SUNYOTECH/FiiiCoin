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
    public class BlackListDac : DataAccessComponent
    {
        public void Save(string address, long? expired)
        {
            const string SQL_STATEMENT =
                "DELETE FROM BlackList " +
                "WHERE Address = @Address; " +
                "INSERT INTO BlackList " +
                "(Address, Timestamp, Expired) " +
                "VALUES (@Address, @Timestamp, @Expired);";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@Timestamp", Time.EpochTime);

                if(expired.HasValue)
                {
                    cmd.Parameters.AddWithValue("@Expired", expired.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Expired", DBNull.Value);
                }

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(string address)
        {
            const string SQL_STATEMENT =
                "DELETE FROM BlackList " +
                "WHERE Address = @Address;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Address", address);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteAll()
        {
            const string SQL_STATEMENT =
                "DELETE FROM BlackList;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool CheckExists(string address)
        {
            const string SQL_STATEMENT =
                "SELECT * FROM BlackList " +
                "WHERE Address = @Address " +
                "AND (Expired IS NULL OR EXpired < @CurrentTime);";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@CurrentTime", Time.EpochTime);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public List<BlackListItem> GetAll()
        {
            const string SQL_STATEMENT =
                "SELECT * FROM BlackList;";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                var result = new List<BlackListItem>();

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var item = new BlackListItem();
                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Address = GetDataValue<string>(dr, "Address");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.Expired = GetDataValue<long?>(dr, "Expired");

                        result.Add(item);
                    }
                }

                return result;
            }
        }
    }
}
