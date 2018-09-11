// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Data;
using FiiiChain.Framework;

namespace FiiiChain.Data
{
    public class DBManager
    {
        public static void Initialization()
        {
            var sql = Resource.InitScript;

            using (SqliteConnection con = new SqliteConnection(GlobalParameters.IsTestnet ?
                Resource.TestnetConnectionString : Resource.MainnetConnectionString))
            {
                con.Open();
                using (SqliteCommand cmd = new SqliteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }

                //if(DateTime.Now <= DateTime.Parse("2018/08/30"))
                //{
                //    var sql2 = "DROP INDEX TxHash; " +
                //        "CREATE INDEX TxHash ON Transactions ( " +
                //        "    Hash " +
                //        "); " +
                //        "DROP INDEX InputListUniqueIndex; " +
                //        "CREATE INDEX InputListUniqueIndex ON InputList ( " +
                //        "    TransactionHash " +
                //        "); " +
                //        "DROP INDEX OutputListUniqueIndex; " +
                //        "CREATE INDEX OutputListUniqueIndex ON OutputList ( " +
                //        "    \"Index\", " +
                //        "    TransactionHash " +
                //        ");";

                //    using (SqliteCommand cmd = new SqliteCommand(sql2, con))
                //    {
                //        cmd.ExecuteNonQuery();
                //    }
                //}
            }
        }
    }
}
