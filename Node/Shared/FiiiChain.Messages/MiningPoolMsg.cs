﻿// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace FiiiChain.Messages
{
    public class MiningPoolMsg : BasePayload
    {
        public MiningPoolMsg()
        {
            MinerInfos = new List<MiningMsg>();
        }

        public List<MiningMsg> MinerInfos { get; set; }

        public int Count
        {
            get { return this.MinerInfos.Count; }
        }

        public override void Deserialize(byte[] bytes, ref int index)
        {
            var countBytes = new byte[4];
            this.MinerInfos.Clear();

            index += 4;

            while (index < bytes.Length)
            {
                MiningMsg minerInfo = new MiningMsg();
                minerInfo.Deserialize(bytes, ref index);
                this.MinerInfos.Add(minerInfo);
            }
        }

        public override byte[] Serialize()
        {
            var countBytes = BitConverter.GetBytes(this.Count);
            var data = new List<byte>();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(countBytes);
            }

            data.AddRange(countBytes);

            foreach (var minerInfo in MinerInfos)
            {
                var bytes = minerInfo.Serialize();
                data.AddRange(bytes);
            }
            return data.ToArray();
        }
    }
}
