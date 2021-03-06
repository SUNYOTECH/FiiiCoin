﻿// Copyright (c) 2018 FiiiLab Technology Ltd
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
    public class SettingDac : DataAccessComponent
    {
        public void SaveSetting(Setting setting)
        {
            const string SQL_STATEMENT =
                "DELETE FROM Settings; " +
                "INSERT INTO Settings " +
                "(Confirmations, FeePerKB, Encrypt, PassCiphertext) " +
                "VALUES (@Confirmations, @FeePerKB, @Encrypt, @PassCiphertext);";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Confirmations", setting.Confirmations);
                cmd.Parameters.AddWithValue("@FeePerKB", setting.FeePerKB);
                cmd.Parameters.AddWithValue("@Encrypt", setting.Encrypt);

                if(string.IsNullOrEmpty(setting.PassCiphertext))
                {
                    cmd.Parameters.AddWithValue("@PassCiphertext", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@PassCiphertext", setting.PassCiphertext);
                }

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public Setting GetSetting()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Settings " +
                "LIMIT 1;";

            Setting setting = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        setting = new Setting();
                        setting.Confirmations = GetDataValue<int>(dr, "Confirmations");
                        setting.FeePerKB = GetDataValue<long>(dr, "FeePerKB");
                        setting.Encrypt = GetDataValue<bool>(dr, "Encrypt");
                        setting.PassCiphertext = GetDataValue<string>(dr, "PassCiphertext");
                    }
                }
            }

            return setting;
        }

        public List<Setting> SelectAll()
        {
            const string SQL_STATEMENT =
                "SELECT * " +
                "FROM Settings;";

            List<Setting> result = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    result = new List<Setting>();

                    while (dr.Read())
                    {
                        Setting setting = new Setting();
                        setting.Confirmations = GetDataValue<int>(dr, "Confirmations");
                        setting.FeePerKB = GetDataValue<long>(dr, "FeePerKB");
                        setting.Encrypt = GetDataValue<bool>(dr, "Encrypt");
                        setting.PassCiphertext = GetDataValue<string>(dr, "PassCiphertext");

                        result.Add(setting);
                    }
                }
            }

            return result;
        }
    }
}
