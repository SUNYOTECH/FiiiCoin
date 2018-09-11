// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiiiChain.MiningPool
{
    internal class MiningPoolSetting
    {
        private const string API_URI_MAIN = "http://localhost:5007/";
        private const string API_URI_TEST = "http://localhost:5006/";

        public static string API_URI
        {
            get
            {
                if (GlobalParameters.IsTestnet)
                    return API_URI_TEST;
                else
                    return API_URI_MAIN;
            }
        }

        private const int POOL_MAIN_PORT = 5009;
        private const int POOL_TEST_PORT = 5008;

        public static int POOL_PORT
        {
            get
            {
                if (GlobalParameters.IsTestnet)
                    return POOL_TEST_PORT;
                else
                    return POOL_MAIN_PORT;
            }
        }

        private const string POOL_MAIN_API = "http://*:5010/";
        private const string POOL_TEST_API = "http://*:5011/";

        public static string POOL_API
        {
            get
            {
                if (GlobalParameters.IsTestnet)
                    return POOL_TEST_API;
                else
                    return POOL_MAIN_API;
            }
        }
    }
}
