// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FiiiChain.Consensus;
using FiiiChain.Entities;
using System.IO;
using System.Linq;
using FiiiChain.Framework;
using FiiiChain.Messages;
using System.Numerics;
using System.Threading;
using FiiiChain.PoolMessages;

namespace FiiiChain.MinerTest
{
    class Program
    {
        static Dictionary<int, byte[]> loadData = new Dictionary<int, byte[]>();
        static void Main(string[] args)
        {
            GlobalParameters.IsTestnet = true;
            Initialize();

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "register":
                        Register(args);
                        break;
                    case "init":
                        Init(args);
                        break;
                    case "start":
                        Start();
                        break;
                    //case "stop":
                    //    Stop(args);
                    //    break;
                    default:
                        UnknownCommand();
                        break;
                }

            }
            else
            {
                UnknownCommand();
            }
        }

        static void UnknownCommand()
        {
            Console.WriteLine("Useage: dotnet MinerTest.dll [command] [options]");
            Console.WriteLine("");
            Console.WriteLine("Supported Commands:");
            Console.WriteLine("     Register: register current device to mining pool");
            Console.WriteLine("     Init: initialize local devie, prepare to start mining");
            //Console.WriteLine("     Read: read local plot data");
            Console.WriteLine("     Start: start mining");
            //Console.WriteLine("     Stop: stop mining");
            Console.WriteLine("");
            Console.WriteLine("Run ' [command] --help' for more information about a command.");
        }
        public static void Initialize()
        {
            //BlockchainComponent blockChainComponent = new BlockchainComponent();
            //AccountComponent accountComponent = new AccountComponent();
            //UtxoComponent utxoComponent = new UtxoComponent();

            //blockChainComponent.Initialize();
            //var accounts = accountComponent.GetAllAccounts();

            //if (accounts.Count == 0)
            //{
            //    var account = accountComponent.GenerateNewAccount();
            //    accountComponent.SetDefaultAccount(account.Id);
            //}

            //new UtxoComponent().Initialize();
        }
        static void Register(string[] args)
        {
            string name = "";
            var miner = new Miner();
            try
            {
                miner.PoolServerAddress = args[1];
                miner.PoolServerPort = int.Parse(args[2]);
                miner.SerialNo = args[3];
                miner.WalletAddress = args[4];
                if (args.Length >= 6)
                    name = args[5];
            }
            catch
            {
                Console.WriteLine("Useage: dotnet MinerTest.dll Register <PoolServerAddress> <PoolServerPort> <SerialNo> <WalletAddress> <Name = null> ");
                Console.WriteLine("");
                Console.WriteLine("Parameters:");
                Console.WriteLine("     PoolServerAddress: IP address of pool server");
                Console.WriteLine("     PoolServerPort: Tcp port of pool server");
                Console.WriteLine("     SerialNo: The Serial No of POS");
                Console.WriteLine("     WalletAddress: Miner's wallet address");
                Console.WriteLine("     Name: The name in MiningPool,default is null");
                Console.WriteLine("");
                return;
            }
            miner.Init(true);
            miner.Start();
            miner.SendRegistCommand(miner.WalletAddress, miner.SerialNo, name);
        }

        static void Init(string[] args)
        {
            var miner = new Miner();

            try
            {
                miner.PoolServerAddress = args[1];
                miner.PoolServerPort = int.Parse(args[2]);
                miner.SerialNo = args[3];
                miner.MinerType = EnumMinerType.POS;
                miner.WalletAddress = args[4];
                miner.PlotFilePath = args[5];
                miner.Capacity = long.Parse(args[6]);

            }
            catch
            {
                Console.WriteLine("Useage: dotnet MinerTest.dll Init <PoolServerAddress> <PoolServerPort> <SerialNo> <WalletAddress> <PlotFilePath> <Capacity>");
                Console.WriteLine("");
                Console.WriteLine("Parameters:");
                Console.WriteLine("     PoolServerAddress: IP address of pool server");
                Console.WriteLine("     PoolServerPort: Tcp port of pool server");
                Console.WriteLine("     SerialNo: The Serial No of POS");
                Console.WriteLine("     WalletAddress: Miner's wallet address");
                Console.WriteLine("     PlotFilePath: The directory path used to storage plot files");
                Console.WriteLine("     Capacity: Max storage capacity used to storage plot files");
                Console.WriteLine("");
                return;
            }

            miner.InitPlotFiles();
            miner.SaveSettings();
            //var location = @"D:\Plot";
            //var capacity = 500 * 1024 * 1024; //500MB
            //var nonceSize = 64 * 4096;
            //var nonceNum = capacity / nonceSize;
            //var walletAddress = "fiiitLkG3NM4FrXWFFLCB3FCD6c6SXiNqU3VcC";

            //var nonceData = new List<NonceData>();
            //Console.WriteLine("Start initialize storage " + DateTime.Now.ToString());
            //var origRow = Console.CursorTop;
            //var origCol = Console.CursorLeft;

            //for (int i = 0; i < nonceNum; i ++)
            //{
            //    var data = POC.GenerateNonceData(walletAddress, i);
            //    nonceData.Add(data);

            //    Console.SetCursorPosition(origCol, origRow);
            //    Console.Write("Progress: " + i.ToString().PadLeft(nonceNum.ToString().Length, '0') + " / " + nonceNum.ToString());

            //    if(nonceData.Count == 100 || i == (nonceNum - 1))
            //    {
            //        var copyData = nonceData.ToArray();
            //        var index = i;
            //        Task.Factory.StartNew(() => saveNonce(location, walletAddress, copyData, index, nonceNum.ToString().Length));
            //        nonceData.Clear();
            //    }
            //}

            //Console.WriteLine("Init finished " + DateTime.Now.ToString());
            //System.Threading.Thread.Sleep(5000);
        }

        static void Start()
        {
            var miner = Miner.LoadFromSetting();
            miner.Init();
            miner.Start();
        }

        static void Stop(string[] args)
        {

        }
    }
}
