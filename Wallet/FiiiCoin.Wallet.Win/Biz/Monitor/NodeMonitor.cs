// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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
        
        public bool Set_NetIsActive = true;

        protected override bool? ExecTaskAndGetResult()
        {
            if (!Set_NetIsActive)
                return null;
            return PortInUse();
        }

        public bool PortInUse()
        {
            bool inUse = false;

            var allUsedUdpPort = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            inUse = allUsedUdpPort.Any(x => x.Port == NodeSetting.NodePort);

            return inUse;
        }

        public bool PortInUse_TCP()
        {
            var tcpPorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Select(x => x.LocalEndPoint);
            var inUse = tcpPorts.Any(x => x.Port == NodeSetting.NodeAPIPort);
            return inUse;
        }

        public void ActiveNode()
        {
            var tcpPorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Select(x => x.LocalEndPoint);
            var isActiveNode = tcpPorts.Any(x => x.Port == NodeSetting.NodeAPIPort);
            if (!isActiveNode)
                NetWorkService.Default.SetNetworkActive(true);
        }
    }
}
