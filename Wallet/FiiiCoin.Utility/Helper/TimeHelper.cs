// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace FiiiCoin.Utility.Helper
{
    public static class TimeHelper
    {
        public static DateTime EpochStartTime => new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        public static long EpochTime => (long)(DateTime.UtcNow - EpochStartTime).TotalMilliseconds;
    }
}
