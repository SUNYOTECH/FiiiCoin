using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.MiningPool.Agent;
using FiiiChain.PoolMessages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Timers;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using FiiiChain.MiningPool.Entities;

namespace FiiiChain.MiningPool
{
    class MiningJob
    {
        SocketServer server;
        WalletAPI api;
        string minerName;
        string walletAddress;
        byte[] privateKey;

        const long MaxNonceCount = 262144; //64GB

        long currentHeight;
        long currentBaseTarget;
        int currentScoopNumber;
        long miningStartTime;
        long lastScoopDataTime;
        Messages.BlockMsg currentBlock;

        List<Miner> MinerList;
        Dictionary<string, long> minerEffort;
        long totalEffort;        
        bool isInMining = false;
        long baseTarget;

        Timer timer;
        Timer uploadtimer;

        public bool IsRunning { get; set; }
        public MiningJob()
        {
            MinerList = new List<Miner>();
            this.minerEffort = new Dictionary<string, long>();
            server = new SocketServer(10000, Int16.MaxValue);
            var uri = MiningPoolSetting.API_URI;
            api = new WalletAPI(uri);
            minersAPI = new MinersAPI();
            blocksAPI = new BlocksAPI();
            rewardAPI = new RewardAPI();
        }

        MinersAPI minersAPI;
        BlocksAPI blocksAPI;
        RewardAPI rewardAPI;

        public void Init(string minerName, string walletAddress, string password = null)
        {
            server.ReceivedCommandAction = receivedCommand;
            server.ReceivedMinerConnectionAction = receivedConnection;
            this.minerName = minerName;
            this.walletAddress = walletAddress;
            try
            {
                this.privateKey = api.DumpPrivateKey(walletAddress, password);
                if (privateKey == null)
                {
                    LogHelper.Error("Address does not exist");
                    return;
                }
            }
            catch
            {
                throw new Exception("Please wait Node service startup");
            }
            
            timer = new Timer(5000);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;

            uploadtimer = new Timer(1000);
            uploadtimer.AutoReset = true;
            uploadtimer.Elapsed += Uploadtimer_Elapsed;
        }

        private void receivedCommand(TcpReceiveState state, PoolCommand cmd)
        {
            switch (cmd.CommandName)
            {
                case CommandNames.Login:
                    this.receivedLoginCommand(state, cmd);
                    break;
                case CommandNames.NonceData:
                    this.receivedNonceDataCommand(state, cmd);
                    break;
                case CommandNames.ScoopData:
                    this.receivedScoopDataCommand(state, cmd);
                    break;
                case CommandNames.Heartbeat:

                    break;
                default:
                    break;
            }
        }

        private void Uploadtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            blocksAPI.GetVerifiedHashes();
        }

        private bool receivedConnection(TcpState e, bool connected)
        {
            var miner = this.MinerList.Where(m => m.ClientAddress == e.Client.Client.RemoteEndPoint.ToString()).FirstOrDefault();

            if(connected)
            {
                if (miner != null && miner.IsConnected)
                {
                    this.sendRejectCommand(e);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (miner != null)
                {
                    miner.IsConnected = false;
                    this.MinerList.Remove(miner);
                }

                return true;
            }

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(isInMining)
            {
                if (Time.EpochTime - lastScoopDataTime > 10 * 1000)
                {
                    LogHelper.Info("Not received scoop data for a long time, mining task is auto stopped.");
                    this.stopMining(false);
                }

                //if(this.bestDeadline != null && (Time.EpochTime - this.currentBlock.Transactions[0].Timestamp) >= this.bestDeadline.Deadline)
                //{
                //    this.forgeBlock(this.bestDeadline);
                //}
            }
            else
            {
                if(this.MinerList.Where(m=>m.IsConnected).Count() >= 1)//100)
                {
                    this.startMining();
                }
            }
        }

        public void Start(IPEndPoint ep)
        {
            this.MinerList.Clear();
            this.minerEffort.Clear();
            this.server.Start(ep);
            var result = api.GetBlockChainInfo();

            if (!result.isRunning)
            {
                throw new Exception("Please wait Node service startup");
            }
            else if (result.connections < 2)
            {
                LogHelper.Error("Node Server must more than 2");
            }

            if(result.localLastBlockHeight < result.remoteLatestBlockHeight)
            {
                throw new Exception("Please wait block data synchronize finished");
            }

            this.IsRunning = true;
            this.timer.Start();
            LogHelper.Info("Server started");
            Console.ReadLine();
        }

        private void startTask(IPEndPoint ep)
        {

        }

        public void Stop()
        {
            this.IsRunning = false;
            this.timer.Stop();
            this.stopMining(false);
        }

        private void startMining()
        {
            //this.timer.Stop();
            this.miningStartTime = Time.EpochTime;
            //此处添加remark
            this.currentBlock = api.GenerateNewBlock(minerName, walletAddress);
            this.currentHeight = this.currentBlock.Header.Height;
            this.currentBaseTarget = api.GetBaseTarget(this.currentHeight);
            this.lastScoopDataTime = 0;

            Messages.BlockMsg lastBlock = null;

            if (this.currentHeight > 0)
            {
                lastBlock = api.GetBlockByHeight(this.currentHeight - 1);
                var prevStepBlock = api.GetBlockByHeight(this.currentHeight - POC.DIFFIUCLTY_ADJUST_STEP - 1);
                baseTarget = POC.CalculateBaseTarget(currentHeight, lastBlock, prevStepBlock);
            }
            else
            {
                baseTarget = POC.CalculateBaseTarget(0, null, null);
            }

            this.currentScoopNumber = POC.GetScoopNumber(this.currentBlock.Header.PayloadHash, this.currentHeight);
            
            this.isInMining = true;
            LogHelper.Info("Start mining block " + this.currentBlock.Header.Height);
            //timer.Start();
            this.totalEffort = 0;
            this.minerEffort.Clear();

            StartMsg msg = new StartMsg();
            msg.BlockHeight = this.currentHeight;
            msg.ScoopNumber = this.currentScoopNumber;
            msg.StartTime = this.miningStartTime;
            msg.GenHash = GenHash(currentBlock.Header.PayloadHash, currentBlock.Header.Height);

            var startCmd = PoolCommand.CreateCommand(CommandNames.Start, msg);

            foreach (var miner in this.MinerList)
            {
                if (miner.IsConnected)
                {
                    this.sendStartCommand(new TcpState { Client = miner.Client, Stream = miner.Stream }, startCmd);
                }
            }
        }

        private void stopMining(bool result)
        {
            this.isInMining = false;
            StopMsg stopMsg = new StopMsg();
            stopMsg.Result = result;
            stopMsg.BlockHeight = this.currentHeight;
            stopMsg.StartTime = this.miningStartTime;
            stopMsg.StopTime = Time.EpochTime;

            var stopCmd = PoolCommand.CreateCommand(CommandNames.Stop, stopMsg);

            foreach (var miner in this.MinerList)
            {
                if (miner.IsConnected)
                {
                    this.sendStopCommand(new TcpState { Client = miner.Client, Stream = miner.Stream }, stopCmd);
                }
            }

            if (result)
            {
                LogHelper.Info("Block height " + this.currentHeight + " is generated success");

                foreach (var key in this.minerEffort.Keys)
                {
                    var miner = this.MinerList.Where(m => m.WalletAddress == key).FirstOrDefault();

                    if (miner != null && miner.IsConnected)
                    {
                        var rewardMsg = new RewardMsg();
                        rewardMsg.BlockHeight = this.currentHeight;
                        rewardMsg.WalletAddress = this.currentBlock.Header.GeneratorId;
                        rewardMsg.Nonce = this.currentBlock.Header.Nonce;
                        rewardMsg.Timestamp = this.currentBlock.Header.Timestamp;
                        rewardMsg.TotalHashes = this.totalEffort;
                        rewardMsg.MinerHashes = this.minerEffort[key];

                        RewardAPI rewardAPI = new RewardAPI();
                        var unPaidReward = rewardAPI.GetUnPaidReward(rewardMsg.WalletAddress);
                        var paidReward = rewardAPI.GetPaidReward(rewardMsg.WalletAddress);
                        rewardMsg.TotalReward = unPaidReward + paidReward;
                        rewardMsg.MinerReward = paidReward;

                        var rewardCmd = PoolCommand.CreateCommand(CommandNames.Reward, rewardMsg);
                        this.sendRewardCommand(new TcpState { Client = miner.Client, Stream = miner.Stream }, rewardCmd);
                    }
                }
            }
            else
            {
                LogHelper.Info("Block height " + this.currentHeight + " is generated failed");
            }
        }

        private bool forgeBlock(string walletaddress,long nonce,byte[] targetBytes)
        {
            try
            {
                this.currentBlock.Header.GeneratorId = walletaddress;
                this.currentBlock.Header.Nonce = nonce;
                this.currentBlock.Header.Timestamp = Time.EpochTime;

                var dsa = ECDsa.ImportPrivateKey(this.privateKey);
                    
                this.currentBlock.Header.BlockSignature = Base16.Encode(dsa.SingnData(Base16.Decode(this.currentBlock.Header.PayloadHash)));

                this.currentBlock.Header.BlockSigSize = this.currentBlock.Header.BlockSignature.Length;
                this.currentBlock.Header.Hash = this.currentBlock.Header.GetHash();
                this.currentBlock.Header.TotalTransaction = this.currentBlock.Transactions.Count;

                var hashResult = this.GetMiningWorkResult(currentBlock);

                if (POC.Verify(currentBlock.Header.Bits, hashResult))
                {
                    this.isInMining = false;
                    this.stopMining(true);

                    api.SubmitBlock(this.currentBlock);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Verify(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length)
                return false;
            for (int i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }
            return true;
        }


        private byte[] GetMiningWorkResult(Messages.BlockMsg block)
        {
            var listBytes = new List<Byte>();
            listBytes.AddRange(Base16.Decode(block.Header.PayloadHash));
            listBytes.AddRange(BitConverter.GetBytes(block.Header.Height));
            var genHash = Sha3Helper.Hash(listBytes.ToArray());
            var scoopNumber = POC.GetScoopNumber(block.Header.PayloadHash, block.Header.Height);
            var scoopData = POC.CalculateScoopData(block.Header.GeneratorId, block.Header.Nonce, scoopNumber);
            List<byte> targetByteLists = new List<byte>();
            targetByteLists.AddRange(scoopData);
            targetByteLists.AddRange(genHash);
            var baseTarget = Sha3Helper.Hash(targetByteLists.ToArray());
            return baseTarget;
        }

        private void receivedRegistCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var registMsg = new RegistMsg();
            int index = 0;
            registMsg.Deserialize(cmd.Payload, ref index);

            var miner = this.MinerList.FirstOrDefault(m => m.WalletAddress == registMsg.WalletAddress || m.SerialNo == registMsg.SerialNo);

            if (miner != null || minersAPI.POSValidate(registMsg.WalletAddress, registMsg.SerialNo))
            {
                this.sendRejectCommand(e);
            }
            else
            {
                var miners = minersAPI.SaveMiners(registMsg.WalletAddress, registMsg.Name, registMsg.SerialNo);

                var result = miners != null;
                this.sendRegistResultCommand(e, result);
            }
        }

        private void receivedLoginCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var loginMsg = new LoginMsg();
            int index = 0;
            loginMsg.Deserialize(cmd.Payload, ref index);

            //TODO: address and SerialNo and account only for one Minner

            var miner = this.MinerList.FirstOrDefault(m => m.WalletAddress == loginMsg.WalletAddress || m.ClientAddress == e.Client.Client.RemoteEndPoint.ToString() || m.SerialNo == loginMsg.SerialNo);

            if (miner != null || !minersAPI.POSValidate(loginMsg.WalletAddress, loginMsg.SerialNo))
            {
                this.sendRejectCommand(e);
                return;
            }


            miner = new Miner();
            miner.SerialNo = loginMsg.SerialNo;
            miner.WalletAddress = loginMsg.WalletAddress;
            miner.ClientAddress = e.Client.Client.RemoteEndPoint.ToString();
            miner.Client = e.Client;
            miner.Stream = e.Stream;

            Random random = new Random();
            miner.CheckScoopNumber = random.Next(0, POC.MAX_SCOOP_NUMBER + 1);
            this.MinerList.Add(miner);

            //skip max nonce command
            //this.sendMaxNonceCommand(e, miner.CheckScoopNumber);

            miner.IsConnected = true;
            miner.ConnectedTime = Time.EpochTime;
            miner.LatestHeartbeatTime = Time.EpochTime;
            this.sendLoginResultCommand(e, true);
            LogHelper.Info(miner.ClientAddress + " login success");

        }

        private void receivedNonceDataCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var msg = new NonceDataMsg();
            int index = 0;
            msg.Deserialize(cmd.Payload, ref index);

            var miner = this.MinerList.FirstOrDefault(m => m.ClientAddress == e.Client.Client.RemoteEndPoint.ToString());

            if (miner == null)
            {
                this.sendRejectCommand(e);
                return;
            }
            
            var data = POC.CalculateScoopData(miner.WalletAddress, msg.MaxNonce, miner.CheckScoopNumber);

            if (Base16.Encode(data) == Base16.Encode(msg.ScoopData))
            {
                miner.IsConnected = true;
                miner.ConnectedTime = Time.EpochTime;
                miner.LatestHeartbeatTime = Time.EpochTime;
                this.sendLoginResultCommand(e, true);
                LogHelper.Info(miner.ClientAddress + " login success");
            }
            else
            {
                this.sendLoginResultCommand(e, false);
                this.sendRejectCommand(e);
                LogHelper.Info(miner.ClientAddress + " login fail");
            }

        }

        private void receivedScoopDataCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var msg = new ScoopDataMsg();
            int index = 0;
            msg.Deserialize(cmd.Payload, ref index);

            if (!isInMining)
            {
                LogHelper.Info("Received invalid scoop data from " + e.Client.Client.RemoteEndPoint + ", nonce is " + msg.Nonce);
                return;
            }


            var miner = this.MinerList.FirstOrDefault(m => m.ClientAddress == e.Client.Client.RemoteEndPoint.ToString() && m.IsConnected);

            if (miner == null)
            {
                LogHelper.Info("Received invalid scoop data from " + e.Client.Client.RemoteEndPoint + ", nonce is " + msg.Nonce);
                return;
            }


            LogHelper.Info("Received scoop data from " + miner.ClientAddress + ", nonce is " + msg.Nonce + ", scoop number is " + msg.ScoopNumber + ", block height is " + msg.BlockHeight);

            if (msg.BlockHeight != this.currentHeight || msg.ScoopNumber != this.currentScoopNumber)
            {
                return;
            }

            if (!this.minerEffort.ContainsKey(miner.WalletAddress))
            {
                this.minerEffort[miner.WalletAddress] = 1;
            }
            else
            {
                if (minerEffort[miner.WalletAddress] == MaxNonceCount)
                {
                    this.sendRejectCommand(e);
                    return;
                }
                this.minerEffort[miner.WalletAddress]++;
            }

            this.totalEffort++;

            this.lastScoopDataTime = Time.EpochTime;

            if (POC.Verify(baseTarget, msg.Target))
            {
                var reusult = this.forgeBlock(msg.WalletAddress, msg.Nonce, msg.Target);
                if (reusult)
                {
                    Blocks blocks = new Blocks();
                    blocks.Generator = currentBlock.Header.GeneratorId;
                    blocks.Hash = currentBlock.Header.Hash;
                    blocks.Height = currentBlock.Header.Height;
                    blocks.Nonce = currentBlock.Header.Nonce;
                    blocks.TotalHash = totalEffort;
                    blocksAPI.SaveBlocks(blocks);
                    
                    RewardList rewardList = new RewardList();
                    var reward = Convert.ToInt64(rewardAPI.GetActualReward(blocks.Generator, blocks.Hash) * 0.9);
                    rewardList.ActualReward = reward;
                    rewardList.BlockHash = blocks.Hash;
                    rewardList.GenerateTime = blocks.Timstamp;
                    rewardList.MinerAddress = blocks.Generator;
                    var hashCount = minerEffort[miner.WalletAddress];
                    rewardList.Hashes = hashCount;

                    rewardAPI.SaveReward(rewardList);
                }
            }
        }

        private void receivedHeartbeatCommand(TcpReceiveState e, PoolCommand cmd)
        {
            var miner = this.MinerList.Where(m => m.ClientAddress == e.Client.Client.RemoteEndPoint.ToString() && m.IsConnected).FirstOrDefault();

            if (miner != null)
            {
                miner.LatestHeartbeatTime = Time.EpochTime;
            }
        }

        private void sendRegistResultCommand(TcpState e, bool result)
        {
            var msg = new RegistResultMsg();
            msg.Result = result;
            var cmd = PoolCommand.CreateCommand(CommandNames.RegistResult, msg);
            this.server.SendCommand(e, cmd);
        }

        private void sendLoginResultCommand(TcpState e, bool result)
        {
            var msg = new LoginResultMsg();
            msg.Result = result;
            var cmd = PoolCommand.CreateCommand(CommandNames.LoginResult, msg);
            this.server.SendCommand(e, cmd);
        }

        private void sendMaxNonceCommand(TcpState e, int scoopNumber)
        {
            var msg = new MaxNonceMsg();
            msg.RandomScoopNumber = scoopNumber;
            var cmd = PoolCommand.CreateCommand(CommandNames.MaxNonce, msg);
            this.server.SendCommand(e, cmd);
        }

        private void sendStartCommand(TcpState e, PoolCommand cmd)
        {
            this.server.SendCommand(e, cmd);
        }

        private void sendStopCommand(TcpState e, PoolCommand cmd)
        {
            this.server.SendCommand(e, cmd);
        }

        private void sendRewardCommand(TcpState e, PoolCommand cmd)
        {
            this.server.SendCommand(e, cmd);
        }

        private void sendRejectCommand(TcpState e)
        {
            var rejectCmd = PoolCommand.CreateCommand(CommandNames.Reject, null);
            this.server.SendCommand(e, rejectCmd);
        }

        private byte[] GenHash(string payloadHash, long blockHeight)
        {
            var payloadBytes = Base16.Decode(payloadHash);
            var heightBytes = BitConverter.GetBytes(blockHeight);
            var hashSeed = new List<byte>();
            hashSeed.AddRange(payloadBytes);
            hashSeed.AddRange(heightBytes);
            return Sha3Helper.Hash(hashSeed.ToArray());
        }
    }
}
