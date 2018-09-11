using System;
using System.Collections.Generic;
using FiiiChain.MiningPool.Data;
using FiiiChain.MiningPool.Entities;
using FiiiChain.Consensus;
using FiiiChain.Consensus.Api;

namespace FiiiChain.MiningPool.Business
{
    public class MinersComponent
    {
        /// <summary>
        /// 矿工注册
        /// </summary>
        /// <param name="address"></param>
        /// <param name="account"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        public Miners RegisterMiner(string address, string account, string sn)
        {
            MinersDac dac = new MinersDac();
            Miners miner = new Miners();
            //验证address是否合法
            if (!AccountIdHelper.AddressVerify(address))
            {
                throw new Exception("address is invalid");
            }

            //判断数据库中是否存在address
            if (dac.IsAddressExisted(address))
            {
                throw new Exception("address has existed");
            }

            /* 设计思路
             * 1、用户注册时先通过调用接口，判断用户和SN是否合法
             * 2、如果非法直接抛错误，如果合法判断数据中是否存在SN，如果存在就更新SN的状态为disabled
             * 3、向数据库中插入记录
             */
            string url = "http://192.168.1.13:9000/Api/Account/CheckAccount";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("MerchantId", account);
            dic.Add("SN", sn);

            string response = ApiHelper.PostApi(url, dic);
            Dictionary<string, string> returnDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            if (returnDic["Data"] == "true")
            {
                if (dac.IsSNExisted(sn))
                {
                    dac.UpdateStatus(address, 1, sn);
                }
            }
            else
            {
                throw new Exception("account and sn is invalid");
            }
            miner.Address = address;
            miner.LastLoginTime = Framework.Time.EpochTime;
            miner.Account = account;
            miner.SN = sn;
            miner.Status = 0;
            miner.Timstamp = Framework.Time.EpochTime;
            miner.Type = 0;

            dac.Insert(miner);
            return miner;
        }

        public bool MinerLogin(string address, string sn)
        {
            /* 设计思路
             * 1、根据address从数据库获取account，如果数据库没有记录直接抛错误
             * 2、根据account和sn调接口，根据接口返回值提供返回值
             * 
             */
            MinersDac dac = new MinersDac();
            Miners miner = dac.GetMinerByAddress(address);
            if (miner == null)
            {
                throw new Exception("address not exist");
            }
            string url = "http://192.168.1.200:9013/Api/Account/CheckAccount";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("MerchantId", miner.Account);
            dic.Add("SN", sn);

            string response = ApiHelper.PostApi(url, dic);
            Dictionary<string, string> returnDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            return returnDic["Data"] == "true" ? true : false;
        }

        public List<Miners> GetAllMiners()
        {
            MinersDac dac = new MinersDac();
            List<Miners> list = dac.SelectAll();
            return list;
        }

        public Miners GetMinerByAddress(string address)
        {
            MinersDac dac = new MinersDac();
            Miners miner = dac.GetMinerByAddress(address);
            return miner;
        }

        public List<Miners> GetMinersBySN(string sn)
        {
            MinersDac dac = new MinersDac();
            return dac.GetMinersBySN(sn);
        }

        public Miners GetMinerById(long id)
        {
            MinersDac dac = new MinersDac();
            return dac.SelectById(id);
        }

        public void DeleteMiner(long id)
        {
            MinersDac dac = new MinersDac();
            dac.Delete(id);
        }

        public void DeleteMiner(string address)
        {
            MinersDac dac = new MinersDac();
            dac.Delete(address);
        }

        public void UpdateStatus(string address, string sn)
        {
            MinersDac dac = new MinersDac();
            dac.UpdateStatus(address, 1, sn);
        }
    }
}
