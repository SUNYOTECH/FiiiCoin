// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using FiiiCoin.Utility.Helper;
using System;
using System.Linq;

namespace FiiiCoin.Utility
{
    public static class AddressTools
    {
        public static bool AddressVerfy(string networkType, string address)
        {
            /*
                fiiim AwTkyjENBMK3Zb1C6SqLKzaQotEjiMY9e
                fiiit 2sTHKSFrYJHo1ddUNNHCKqJnAhjATgr6c

                Base58解码->buffer->拿出后面四个字节->对剩余部分做Hash计算并取出开头四个字节->比对
            */
            if (!string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(networkType))
            {
                if (address.Length == 38)
                {
                    if (networkType.ToLower() == "testnet")
                    {
                        if (address.StartsWith("fiiit"))
                        {
                            byte[] bytes = Base58.Decode(address);
                            byte[] temp = bytes.Skip(bytes.Length - 4).Take(4).ToArray();
                            byte[] calc = bytes.Take(bytes.Length - 4).ToArray();
                            byte[] diff = HashHelper.DoubleHash(calc).Take(4).ToArray();
                            return diff.SequenceEqual(temp);
                        }
                    }
                    else
                    {
                        if (address.StartsWith("fiiim"))
                        {
                            byte[] bytes = Base58.Decode(address);
                            byte[] temp = bytes.Skip(bytes.Length - 4).Take(4).ToArray();
                            byte[] calc = bytes.Take(bytes.Length - 4).ToArray();
                            byte[] diff = HashHelper.DoubleHash(calc).Take(4).ToArray();
                            return diff.SequenceEqual(temp);
                        }
                    }
                }
            }
            return false;
        }
    }
}
