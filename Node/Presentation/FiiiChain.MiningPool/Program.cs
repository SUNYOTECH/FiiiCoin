using FiiiChain.Consensus;
using FiiiChain.Framework;
using FiiiChain.MiningPool.API;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net;

namespace FiiiChain.MiningPool
{
    class Program
    {
        static void Main(string[] args)
        {
            var poolArgs = AnalysisArgs(args);

            if (!AccountIdHelper.AddressVerify(poolArgs.Address))
            {
                LogHelper.Error("Wallet Address is invalid");
                return;
            }

            GlobalParameters.IsTestnet = poolArgs.IsTestNet;

            try
            {
                //StartMiningPoolAPI();

                IPEndPoint ep = new IPEndPoint(IPAddress.Any, MiningPoolSetting.POOL_PORT);
                MiningJob job = new MiningJob();
                job.Init(poolArgs.Name, poolArgs.Address, poolArgs.Password);
                job.Start(ep);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                Process.GetCurrentProcess().Kill();
            }
        }

        static void ShowCommandHelp()
        {
            LogHelper.Info("Usage: ");
            LogHelper.Info("\t dotnet FiiiChain.MiningPool.dll <MinerName> <WalletAddress> [Wallet Password]");
        }

        //启动Mining Api
        static void StartMiningPoolAPI()
        {
            string url = MiningPoolSetting.POOL_API;
            var host = WebHost.CreateDefaultBuilder()
                .UseKestrel().ConfigureServices(cssc => cssc.AddMemoryCache())
                .UseStartup<Startup>()
                .UseUrls(url)
                .Build();
            host.Start();
            LogHelper.Info("MiningPool Service API Start at " + url);
        }

        static PoolArgs AnalysisArgs(string[] args)
        {
            PoolArgs poolArgs = null;
            if (args.Length < 2 || args.Length > 4)
                return poolArgs;
            if (args.Length == 2)
            {
                poolArgs = new PoolArgs() { Name = args[0], Address = args[1] };
            }
            else if (args.Length == 3)
            {
                poolArgs = new PoolArgs()
                {
                    IsTestNet = args[0].ToLower() == "-testnet",
                    Name = args[1],
                    Address = args[2]
                };
            }
            else if(args.Length == 4)
            {
                poolArgs = new PoolArgs()
                {
                    IsTestNet = args[0].ToLower() == "-testnet",
                    Name = args[1],
                    Address = args[2],
                    Password = args[3]
                };
            }
            return poolArgs;
        }
    }

    class PoolArgs
    {
        public bool IsTestNet = false;
        public string Name;
        public string Address;
        public string Password;
    }
}
