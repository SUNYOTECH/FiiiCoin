// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Diagnostics;

namespace FiiiCoin.Wallet.Win.Biz.Monitor
{
    public class NodeMonitor : ServiceMonitorBase<bool?>
    {
        private static NodeMonitor _default;

        public static NodeMonitor Default
        {
            get
            {
                if (_default == null)
                    _default = new NodeMonitor();
                return _default;
            }
        }



        public NetworkType CurrentNetworkType;
        public bool Set_NetIsActive = true;

        protected override bool? ExecTaskAndGetResult()
        {
            if (!Set_NetIsActive)
                return null;
            return PortInUse(CurrentNetworkType);
        }


        public bool PortInUse(NetworkType networkType)
        {
            int port = 44111;
            switch (networkType)
            {
                case NetworkType.MainnetPort:
                    port = 44222;
                    break;
                case NetworkType.TestNetPort:
                    port = 44111;
                    break;
            }

            bool inUse = false;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c C:\\Windows\\System32\\cmd.exe";
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = false;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            //设置启动动作,确保以管理员身份运行
            startInfo.Verb = "RunAs";
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            var cmdStr = string.Format("netstat -ano|findstr \"{0}\"", port);
            process.StandardInput.WriteLine(cmdStr);
            process.StandardInput.WriteLine("exit");
            string strRst = process.StandardOutput.ReadToEnd();
            Console.WriteLine("\n" + strRst);
            process.WaitForExit();

            var matchText = ":" + port;
            if (strRst.Contains(matchText))
                inUse = true;

            return inUse;
        }

    }
}
