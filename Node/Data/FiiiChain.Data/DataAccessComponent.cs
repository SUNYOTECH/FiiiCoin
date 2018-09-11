// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using System;
using System.Data;
using System.Text;

namespace FiiiChain.Data
{
    public abstract class DataAccessComponent
    {
        public string CacheConnectionString
        {
            get
            {
                return GlobalParameters.IsTestnet ? Resource.TestnetConnectionString : Resource.MainnetConnectionString;
            }
        }

        protected static T GetDataValue<T>(IDataReader dr, string columnName)
        {
            // NOTE: GetOrdinal() is used to automatically determine where the column
            //       is physically located in the database table. This allows the
            //       schema to be changed without affecting this piece of code.
            //       This of course sacrifices a little performance for maintainability.
            int i = dr.GetOrdinal(columnName);
            var mydr = (Microsoft.Data.Sqlite.SqliteDataReader)dr;
            if (!dr.IsDBNull(i))
                return mydr.GetFieldValue<T>(i);
            else
                return default(T);
        }
    }
}
