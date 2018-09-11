// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.PoolMessages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FiiiChain.MinerTest
{
    class Miner
    {
        public string PoolServerAddress { get; set; }
        public int PoolServerPort { get; set; }
        public string SerialNo { get; set; }
        public EnumMinerType MinerType { get; set; }
        public string WalletAddress { get; set; }
        public string PlotFilePath { get; set; }
        public long Capacity { get; set; }
        public long TotalCountofNonce { get; set; }

        private SocketClient2 socketClient;
        private int nonceSize = 64 * 4096;
        private int noncePerFile = 100;
        private int scoopDataLen = 64;
        private bool isInMinging = false;
        private bool isRegist = false;

        public void Init(bool _isRegist = false)
        {
            IPAddress ip;            
            if (!IPAddress.TryParse(this.PoolServerAddress, out ip))
            {
                try
                {
                    var ips = Dns.GetHostAddresses(this.PoolServerAddress);

                    if (ips.Length > 0)
                    {
                        ip = ips[0];
                    }
                    else
                    {
                        throw new CommonException(ErrorCode.Engine.P2P.Connection.HOST_NAME_CAN_NOT_RESOLVED_TO_IP_ADDRESS);
                    }
                }
                catch
                {
                    throw new CommonException(ErrorCode.Engine.P2P.Connection.HOST_NAME_CAN_NOT_RESOLVED_TO_IP_ADDRESS);
                }
            }

            socketClient = new SocketClient2(new IPEndPoint(ip, this.PoolServerPort), Int16.MaxValue);
            socketClient.ConnectStatusChangedAction = ConnectStatusChanged;
            isRegist = _isRegist;
            socketClient.ReceivedCommandAction = ReceivedCommand;
            socketClient.ProcessErrorAction = SocketErroreceived;
        }

        public void Start()
        {
            socketClient.Connect();
            Console.ReadLine();
        }

        public void Stop()
        {
            socketClient.Disconnect();
        }

        public void InitPlotFiles()
        {
            TotalCountofNonce = Capacity / nonceSize;

            //var nonceData = new List<NonceData>();
            var origRow = Console.CursorTop;
            var origCol = Console.CursorLeft;

            var groups = (int)Math.Ceiling((double)TotalCountofNonce / (double)noncePerFile);
            long finishedNonce = 0;
            long finishedFiles = 0;
            long maxFiles = groups * 4096L;
            //int g = 0;

            for(int g = 0; g < groups; g ++)
            //Parallel.For(0, groups, new ParallelOptions() { MaxDegreeOfParallelism  = 2}, (g) => 
            {
                var min = g * (long)noncePerFile;
                var max = (g + 1) * (long)noncePerFile - 1L;

                if(max >= TotalCountofNonce)
                {
                    max = TotalCountofNonce - 1;
                }

                var nonceData = new List<NonceData>();

                for (long i = min; i <= max; i ++)
                {
                    var data = POC.GenerateNonceData(WalletAddress, i);
                    nonceData.Add(data);
                    finishedNonce++;

                    Console.SetCursorPosition(origCol, origRow);
                    Console.Write("Nonce Progress: " + finishedNonce.ToString().PadLeft((TotalCountofNonce - 1).ToString().Length, '0') + " / " + TotalCountofNonce.ToString());
                }

                var copyData = nonceData.ToArray();
                Task.Factory.StartNew(() => this.SaveNonce(PlotFilePath, WalletAddress, copyData, max, (TotalCountofNonce - 1).ToString().Length));
                finishedFiles += 4096;

                //Console.SetCursorPosition(origCol, origRow + 1);
                //Console.Write("Plot File Progress: " + finishedFiles.ToString().PadLeft(maxFiles.ToString().Length, '0') + " / " + maxFiles.ToString());
            }
            //);

            //for (int i = 0; i < TotalCountofNonce; i++)
            //{
            //    var data = POC.GenerateNonceData(WalletAddress, i);
            //    nonceData.Add(data);

            //    Console.SetCursorPosition(origCol, origRow);
            //    Console.Write("Progress: " + i.ToString().PadLeft((TotalCountofNonce - 1).ToString().Length, '0') + " / " + TotalCountofNonce.ToString());

            //    if (nonceData.Count == noncePerFile || i == (TotalCountofNonce - 1))
            //    {
            //        var copyData = nonceData.ToArray();
            //        var index = i;
            //        Task.Factory.StartNew(() => SaveNonce(PlotFilePath, WalletAddress, copyData, index, (TotalCountofNonce - 1).ToString().Length));
            //        nonceData.Clear();
            //    }
            //}

            Console.WriteLine("Init finished " + DateTime.Now.ToString());
            System.Threading.Thread.Sleep(5000);
        }

        private void SaveNonce(string location, string address, NonceData[] nonceData, long maxNonce, int endNonceLen)
        {
            System.Threading.Thread.CurrentThread.IsBackground = false;
            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }

            //var minNonceText = (maxNonce  - 99).ToString().PadLeft(endNonceLen, '0');
            var minNonceText = ((maxNonce / noncePerFile) * noncePerFile).ToString().PadLeft(endNonceLen, '0');
            var maxNonceText = maxNonce.ToString().PadLeft(endNonceLen, '0');
            for (int i = 0; i < 4096; i++)
            {
                var path = Path.Combine(location, address + "_" + i.ToString().PadLeft(4, '0') + "_" + minNonceText + "_" + maxNonceText);
                using (var file = new FileStream(path, FileMode.Create))
                {
                    foreach (var nonce in nonceData)
                    {
                        var scoop = nonce.DataList.Where(d => d.Index == i).First();
                        file.Write(scoop.FullData, 0, scoop.FullData.Length);
                    }

                    file.Flush();
                }
            }
        }

        public void SaveSettings()
        {
            var json = JsonConvert.SerializeObject(this);
            var path = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Setting.json");

            using (var fileStream = File.Open(path, FileMode.Create))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                fileStream.Write(bytes, 0, bytes.Length);

                fileStream.Flush();
            }
        }

        public static Miner LoadFromSetting()
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(Miner).Assembly.Location), "Setting.json");
            var json = File.ReadAllText(path);

            var miner = JsonConvert.DeserializeObject<Miner>(json);
            return miner;
        }

        private async void StartSendScoopData(StartMsg startMsg)
        {
            var scoop = startMsg.ScoopNumber.ToString();

            byte[] currentBytes = new byte[scoopDataLen];
            var files = Directory.GetFiles(PlotFilePath, WalletAddress + "_" + scoop.PadLeft(4, '0') + "_*");
            Array.Sort(files);
            int currentNonce = -1;

            Console.WriteLine();
            var origRow = Console.CursorTop;
            var origCol = Console.CursorLeft;

            foreach (var file in files)
            {
                var fileData = Path.GetFileNameWithoutExtension(file).Split('_');
                var minNonce = int.Parse(fileData[2]);
                var maxNonce = int.Parse(fileData[3]);

                var bytes = File.ReadAllBytes(file);
                var index = 0;
                var bytesLen = bytes.Length;
                currentNonce = minNonce;

                while (index < bytesLen)
                {
                    if(!isInMinging)
                    {
                        LogHelper.Info("Mining is stopped");
                        return;
                    }

                    await Task.Run(() => {
                        currentBytes = new byte[scoopDataLen];
                        Array.Copy(bytes, index, currentBytes, 0, currentBytes.Length);

                        List<byte> targetByteLists = new List<byte>();
                        targetByteLists.AddRange(currentBytes);
                        targetByteLists.AddRange(startMsg.GenHash);
                        var baseTarget = Sha3Helper.Hash(targetByteLists.ToArray());

                        this.SendScoopDataCommand(startMsg.BlockHeight, currentNonce, startMsg.ScoopNumber, baseTarget);

                        Console.SetCursorPosition(origCol, origRow);
                        Console.Write("Progress: " + currentNonce.ToString().PadLeft((TotalCountofNonce - 1).ToString().Length, '0') + " / " + (TotalCountofNonce - 1).ToString());
                    });

                    Task.Delay(10).Wait();
                    index += scoopDataLen;
                    currentNonce += 1;
                }
            }

            Console.WriteLine("All Nonces have been sent");
        }

        private void ConnectStatusChanged(bool connected)
        {
            if(connected && !isRegist)
            {
                LogHelper.Info("Connect successed, start to login");
                this.SendLoginCommand();
            }
            else
            {
                LogHelper.Info("Connect failed");
                this.isInMinging = false;
            }
        }

        private void SocketErroreceived(int errorCode)
        {
            this.isInMinging = false;
            LogHelper.Error("Socket error received " + ((SocketError)errorCode).ToString());
        }

        private void ReceivedCommand(PoolCommand cmd)
        {
            //LogHelper.Info("Received " + cmd.CommandName + " Command");
            switch(cmd.CommandName)
            {
                case CommandNames.MaxNonce:
                    this.ReceivedMaxNonceCommand(cmd.Payload);
                    break;
                case CommandNames.LoginResult:
                    this.ReceivedLoginResultCommand(cmd.Payload);
                    break;
                case CommandNames.Start:
                    this.ReceivedStartCommand(cmd.Payload);
                    break;
                case CommandNames.Stop:
                    this.ReceivedStopCommand(cmd.Payload);
                    break;
                case CommandNames.Reward:
                    this.ReceivedRewaradCommand(cmd.Payload);
                    break;
                case CommandNames.Reject:
                    this.ReceivedRejectCommand(cmd.Payload);
                    break;
                default:
                    break;
            }
        }

        private void ReceivedMaxNonceCommand(byte[] payload)
        {
            var msg = new MaxNonceMsg();
            int index = 0;
            msg.Deserialize(payload, ref index);

            this.SendNonceDataCommand(msg.RandomScoopNumber);
        }

        private void ReceivedLoginResultCommand(byte[] payload)
        {
            LogHelper.Info("Login successed, waiting for mining task.");
        }

        private void ReceivedRegistResultCommand(byte[] payload)
        {
            var msg = new RegistResultMsg();
            int index = 0;
            msg.Deserialize(payload, ref index);
            LogHelper.Info(string.Format("Regist Result : {0}", msg.Result ? "Success" : "Failure"));
        }

        private void ReceivedStartCommand(byte[] payload)
        {
            var msg = new StartMsg();
            int index = 0;
            msg.Deserialize(payload, ref index);

            LogHelper.Info("Received new block " + msg.BlockHeight + " mining task");
            this.isInMinging = true;
            this.StartSendScoopData(msg);
        }

        private void ReceivedStopCommand(byte[] payload)
        {
            var msg = new StopMsg();
            int index = 0;
            msg.Deserialize(payload, ref index);

            this.isInMinging = false;
            LogHelper.Info(string.Format("Block {0} mining task start at {1} and stop at {2}. Mining result is {3}", 
                msg.BlockHeight, msg.StartTime, msg.StopTime, msg.Result));
        }

        private void ReceivedRewaradCommand(byte[] payload)
        {
            var msg = new RewardMsg();
            int index = 0;
            msg.Deserialize(payload, ref index);

            LogHelper.Info(string.Format("New block mining reward received:\nBlock Height:{0}\n\tMiner Hashes:{1}\n\tTotal Hashes:{2}\n\tAssigned Reward:{3}",
                msg.BlockHeight, msg.MinerHashes, msg.TotalHashes, msg.MinerReward));
        }

        private void ReceivedRejectCommand(byte[] payload)
        {
            LogHelper.Info("Previous command has been rejected by pool server");
        }

        private void SendHeartbeatCommand()
        {
            var cmd = PoolCommand.CreateCommand(CommandNames.Heartbeat, null);
            this.socketClient.SendCommand(cmd);
        }

        private void SendLoginCommand()
        {
            var msg = new LoginMsg();
            msg.WalletAddress = this.WalletAddress;
            msg.MinerType = this.MinerType;
            msg.SerialNo = this.SerialNo;

            var cmd = PoolCommand.CreateCommand(CommandNames.Login, msg);
            this.socketClient.SendCommand(cmd);
        }

        public void SendRegistCommand(string walletAddress, string serialNo,string name)
        {
            var msg = new RegistMsg( );
            msg.WalletAddress = walletAddress;
            msg.SerialNo = serialNo;
            msg.Name = name;

            var cmd = PoolCommand.CreateCommand(CommandNames.Regist, msg);
            this.socketClient.SendCommand(cmd);
        }

        private void SendNonceDataCommand(int scoopNumber)
        {
            var maxNonce = this.TotalCountofNonce - 1;
            var minNonce = (maxNonce / noncePerFile) * noncePerFile;

            string maxNonceText = maxNonce.ToString();
            string minNonceText = minNonce.ToString().PadLeft(maxNonceText.Length, '0');

            var fileName = this.WalletAddress + "_" + scoopNumber.ToString().PadLeft(4, '0') + "_" + minNonceText + "_" + maxNonceText;
            var filePath = Path.Combine(PlotFilePath, fileName);

            var bytes = File.ReadAllBytes(filePath);
            var scoopData = new byte[64];
            Array.Copy(bytes, bytes.Length - scoopData.Length, scoopData, 0, scoopData.Length);

            var msg = new NonceDataMsg();
            msg.MaxNonce = maxNonce;
            msg.ScoopData = scoopData;

            var cmd = PoolCommand.CreateCommand(CommandNames.NonceData, msg);
            this.socketClient.SendCommand(cmd);
        }

        private void SendScoopDataCommand(long height, long nonce, int scoopNumber, byte[] target)
        {
            var msg = new ScoopDataMsg();
            msg.BlockHeight = height;
            msg.WalletAddress = WalletAddress;
            msg.Nonce = nonce;
            msg.ScoopNumber = scoopNumber;
            msg.Target = target;

            var cmd = PoolCommand.CreateCommand(CommandNames.ScoopData, msg);
            this.socketClient.SendCommand(cmd);
        }
    }
}
