// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Business;
using FiiiCoin.Models;
using FiiiCoin.Utility.Api;
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Common.Utils;
using FiiiCoin.Wallet.Win.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiiiCoin.Wallet.Win.Biz.Services
{
    public class NetWorkService : ServiceBase<NetWorkService>
    {

        public Result<BlockSyncInfo> GetBlockChainInfoSync(BlockSyncInfo block)
        {
            Result<BlockSyncInfo> result = new Result<BlockSyncInfo>();
            
            ApiResponse response = NetworkApi.GetBlockChainInfo().Result;
            result.IsFail = false;
            result.Value = block;
            result.ApiResponse = response;
            if (!response.HasError)
            {
                BlockChainInfo info = response.GetResult<BlockChainInfo>();
                if (!info.IsRunning)
                {
                    return result; ;
                }

                block.ConnectCount = info.Connections;
                if (block.AllBlockHeight < 0)
                {
                    //远程没同步的时候
                    block.Progress = 100;
                    block.AllBlockHeight = 0;
                    block.beforeLocalLastBlockHeight = info.LocalLastBlockHeight;
                }
                if (block.AllBlockHeight == 0)
                {
                    //第一次获取数据，远程区块高度
                    block.AllBlockHeight = info.RemoteLatestBlockHeight;
                    //本地更新之前区块的高度
                    block.beforeLocalLastBlockHeight = info.LocalLastBlockHeight;
                    //刚开始时剩余区块高度
                    block.BlockLeft = block.AllBlockHeight - block.beforeLocalLastBlockHeight;
                    if (block.AllBlockHeight < 0 || block.BlockLeft < 0)
                    {
                        block.Progress = 100;
                    }
                    else
                    {
                        //本次更新开始时间
                        block.StartTimeOffset = DateTimeUtil.GetDateTimeStamp(DateTime.Now);
                        block.needUpdateBlocksHeight = block.BlockLeft;
                        if (block.AllBlockHeight == info.LocalLastBlockHeight)
                            block.Progress = 100;
                        else
                            block.Progress = 0;
                    }
                }
                else
                {
                    if (info.TimeOffset == -1)
                        return result;
                    //不是第一次，开始组织数据
                    block.BlockLeft = block.AllBlockHeight - info.LocalLastBlockHeight - info.TempBlockCount;
                    var doubleValue = Convert.ToDouble(info.LocalLastBlockHeight + info.TempBlockCount - block.beforeLocalLastBlockHeight);
                    if (block.AllBlockHeight <= info.LocalLastBlockHeight)
                    {
                        if (info.TempBlockCount > 5)
                            block.Progress = 99;
                        block.Progress = 100;
                        block.TimeLeft = 0;
                    }
                    else
                        block.Progress = (doubleValue / block.needUpdateBlocksHeight) * 100;
                    //剩余时间 = 当前更新时间 / 进度
                    if (block.Progress > 0 && block.Progress < 100)
                        block.TimeLeft = Convert.ToInt64((DateTimeUtil.GetDateTimeStamp(DateTime.Now) - block.StartTimeOffset) / block.Progress) * 100;
                    if (block.BlockLeft < 0)
                        block.BlockLeft = 0;
                }
            }
            return result;
        }

        public Result<List<PeerInfo>> GetPeerInfo()
        {
            ApiResponse response =  NetworkApi.GetPeerInfo().Result;
            return GetResult<List<PeerInfo>>(response);
        }

        public Result SetNetworkActive(bool isActive)
        {
            ApiResponse response =  NetworkApi.SetNetworkActive(isActive).Result;
            var result = GetResult(response);
            if (result.IsFail)
                return result;
            if (isActive)
            {
                UpdateBlocksMonitor.Default.Reset();
                UpdateBlocksMonitor.Default.Start(3000);
            }
            else
            {
                UpdateBlocksMonitor.Default.Stop();
            }
            NodeMonitor.Default.Set_NetIsActive = isActive;
            return result;
        }

        public Result<long> GetConnectionCount()
        {
            ApiResponse response =  NetworkApi.GetConnectionCount().Result;
            return GetResult<long>(response);
        }

        public Result<NetworkInfo> GetNetworkInfo()
        {
            ApiResponse response =  NetworkApi.GetNetworkInfo().Result;
            return GetResult<NetworkInfo>(response);
        }
    }
}
