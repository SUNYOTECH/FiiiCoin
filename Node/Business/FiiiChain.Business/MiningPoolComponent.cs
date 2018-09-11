// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using FiiiChain.Data;
using FiiiChain.DataAgent;
using FiiiChain.Entities;
using FiiiChain.Framework;
using FiiiChain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FiiiChain.Business
{
    public class MiningPoolComponent
    {
        public static Action<NewMiningPoolMsg> OnNewMiningPoolHandle; 

        List<MiningPool> currentMiningPools = new List<MiningPool>();

        public MiningPoolComponent()
        {
            MiningPoolDac miningPoolDac = new MiningPoolDac();
            currentMiningPools = miningPoolDac.GetAllMiningPools();
        }

        public MiningMsg CreateNewMiningPool(string minerName, string publicKey)
        {
            MiningPoolDac miningPoolDac = new MiningPoolDac();
            MiningMsg newMiningPoolMsg = new MiningMsg();
            newMiningPoolMsg.Name = minerName;
            newMiningPoolMsg.PublicKey = publicKey;
            return newMiningPoolMsg;
        }

        public List<MiningPool> GetAllMiningPools()
        {
            MiningPoolDac miningPoolDac = new MiningPoolDac();
            var result = miningPoolDac.GetAllMiningPools();
            return result;
        }

        public void UpdateMiningPools(List<MiningMsg> miningMsgs)
        {
            MiningPoolDac miningPoolDac = new MiningPoolDac();
            List<MiningMsg> needUpdateItems = null;
            List<MiningMsg> needAddItems = null;
            GetItemsUpdate(miningMsgs, out needUpdateItems, out needAddItems);

            var updateItems = needUpdateItems.Select(x => ConvertToMiningPool(x));
            miningPoolDac.UpdateMiningPools(updateItems);

            var addItems = needAddItems.Select(x => ConvertToMiningPool(x));
            miningPoolDac.SaveToDB(addItems);
        }

        public bool AddMiningToPool(MiningMsg msg)
        {
            if (!POC.VerifyMiningPoolSignature(msg.PublicKey, msg.Signature))
                return false;

            var result = false;
            var item = currentMiningPools.FirstOrDefault(x => x.PublicKey == msg.PublicKey && x.Signature == msg.Signature);
            if (item == null)
            {
                MiningPoolDac miningPoolDac = new MiningPoolDac();
                MiningPool miningPool = ConvertToMiningPool(msg);
                result = miningPoolDac.SaveToDB(miningPool) > 0;
            }
            else if (item.Name != msg.Name)
            {
                MiningPoolDac miningPoolDac = new MiningPoolDac();
                MiningPool miningPool = new MiningPool() { Name = msg.Name, PublicKey = msg.PublicKey ,Signature = msg.Signature };
                miningPoolDac.UpdateMiningPool(miningPool);
                result = true;
            }
            if (result && OnNewMiningPoolHandle != null)
            {
                NewMiningPoolMsg newMsg = new NewMiningPoolMsg();
                newMsg.MinerInfo = new MiningMsg() { Name = msg.Name, PublicKey = msg.PublicKey, Signature = msg.Signature };
                OnNewMiningPoolHandle(newMsg);
            }

            return false;
        }

        private MiningPool ConvertToMiningPool(MiningMsg msg)
        {
            MiningPool miningPool = new MiningPool() { Name = msg.Name, PublicKey = msg.PublicKey, Signature = msg.Signature };
            return miningPool;
        }

        void GetItemsUpdate(List<MiningMsg> miningMsgs ,out List<MiningMsg> updateItems,out List<MiningMsg> addItems)
        {
            updateItems = new List<MiningMsg>();
            addItems = new List<MiningMsg>();
            foreach (var item in miningMsgs)
            {
                if (!currentMiningPools.Any(x => x.PublicKey == item.PublicKey))
                {
                    addItems.Add(item);
                }
                else if (currentMiningPools.Any(x => x.PublicKey == item.PublicKey && x.Name != item.Name))
                {
                    updateItems.Add(item);
                }
            }
        }

        public long GetLocalMiningPoolCount()
        {
            return currentMiningPools.Count;
        }

        public MiningPool GetMiningPoolByName(string poolName)
        {
            return new MiningPoolDac().SelectMiningPoolByName(poolName);
        }
    }
}
