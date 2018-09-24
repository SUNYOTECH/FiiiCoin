// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Utility;
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FiiiCoin.Wallet.Win.Biz
{
    public enum NetworkType
    {
        MainnetPort,
        TestNetPort
    }

    public class NodeInitializer : InstanceBase<NodeInitializer>
    {
        private string _targetDir;
        public bool Set_NetIsActive;

        public void StartNode(NetworkType networkType = NetworkType.TestNetPort)
        {
            _targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Node");
            if (!Directory.Exists(_targetDir))
                Application.Current.MainWindow.Close();
            NodeMonitor.Default.CurrentNetworkType = networkType;
            NodeMonitor.Default.MonitorCallBack += StartNode;
            NodeMonitor.Default.Start(1000);
        }

        void StartNode(bool? portIsUse)
        {
            if (portIsUse.HasValue && portIsUse.Value)
                return;

            if (Set_NetIsActive)
            {
                Thread.Sleep(3000);
                return;
            }
            try
            {
                Process p = new Process();
                p.StartInfo.WorkingDirectory = _targetDir;
                p.StartInfo.FileName = "dotnet.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = false;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.StartInfo.Arguments = "FiiiChain.Node.dll -testnet";
                p.StartInfo.Verb = "RunAs";
                p.Start();//启动程序
                p.OutputDataReceived += (s, e) =>
                {
                    Console.WriteLine(e.Data);
                };

                p.BeginOutputReadLine();

                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
            }
        }
    }
}