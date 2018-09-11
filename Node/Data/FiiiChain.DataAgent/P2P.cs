// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Timers;
using FiiiChain.Framework;
using FiiiChain.Messages;
using System.Threading.Tasks;
using System.Threading;

namespace FiiiChain.DataAgent
{
    public class P2P
    {
        public static P2P Instance;
        public List<P2PNode> Peers;
        public List<string> BlackList;
        public long TotalBytesReceived = 0;
        public long TotalBytesSent = 0;
        public bool IsRunning = false;
        public string Identity = "";

        public long LastBlockHeight { get; set; }
        public long LastBlockTime { get; set; }

        private UdpClient server;
        private int bufferSize = 1024; //1KB
        private int maxConnections = 100;
        private Queue<P2PSendMessage> sendCommandQueue;
        private Dictionary<string, List<byte>> receivedMessageBuffer;
        private System.Timers.Timer peerCheckTimer;
        private byte[] defaultPrefixBytes, defaultSuffixByste;
        private bool isTrackerNode = false;

        static P2P()
        {
            Instance = new P2P();
        }
        public P2P()
        {
            LastBlockHeight = -1;
            LastBlockTime = 0;

            this.Peers = new List<P2PNode>();
            peerCheckTimer = new System.Timers.Timer(30 * 1000); //30 seconds
            peerCheckTimer.AutoReset = true;
            peerCheckTimer.Elapsed += peerCheckTimer_Elapsed;

            defaultPrefixBytes = P2PCommand.DefaultPrefixBytes;
            defaultSuffixByste = P2PCommand.DefaultSuffixBytes;
        }

        private void peerCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            for (int i = Peers.Count - 1; i >= 0; i--)
            {
                var peer = Peers[i];
                var nowTime = Time.EpochTime;

                //check unconnected peers
                if (!peer.IsConnected)
                {
                    if ((peer.ConnectedTime > nowTime || (nowTime - peer.ConnectedTime) > 1 * 60 * 1000))
                    {
                        Peers.RemoveAt(i);
                        this.receivedMessageBuffer.Remove(peer.IP + ":" + peer.Port);
                    }
                }
                else
                {
                    //check long time not received heartbeat peers
                    if (peer.LastHeartbeat < nowTime && (nowTime - peer.LastHeartbeat) > 10 * 60 * 1000)
                    {
                        peer.IsConnected = false;
                        this.raiseNodeConnectionStateChanged(peer);

                        Peers.RemoveAt(i);
                        this.receivedMessageBuffer.Remove(peer.IP + ":" + peer.Port);
                    }
                    else if(this.isTrackerNode)
                    {
                        var msg = new HeightMsg();
                        msg.Height = this.LastBlockHeight;
                        msg.BlockTime = this.LastBlockTime;
                        var command = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.Heartbeat, msg);
                        this.Send(command, peer.IP, peer.Port);
                    }
                }
            }

            if (!this.isTrackerNode)
            {
                //reconnect with tracker nodes
                if (this.Peers.Count == 0 || this.Peers.Where(p => p.IsTrackerServer && p.IsConnected).Count() == 0)
                {
                    var trackerNode = this.Peers.Where(t => t.IsTrackerServer).FirstOrDefault();

                    if(trackerNode != null)
                    {
                        this.Peers.Remove(trackerNode);
                        this.receivedMessageBuffer.Remove(trackerNode.IP + ":" + trackerNode.Port);
                    }

                    string trackerIp = GlobalParameters.IsTestnet ? Resource.TestnetTrackerServer : Resource.MainnetTrackerServer;
                    int trackerPort = int.Parse(GlobalParameters.IsTestnet ? Resource.TestnetTrackerPort : Resource.MainnetTrackerPort);
                    this.ConnectToNewPeer(trackerIp, trackerPort, true);
                }
                //get new node address from tracker
                else if (this.Peers.Count() < this.maxConnections)
                {
                    var tracker = this.Peers.Where(p => p.IsTrackerServer && p.IsConnected).FirstOrDefault();

                    if (tracker != null)
                    {
                        var payload = new GetAddrMsg();
                        payload.Count = this.maxConnections;

                        var command = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.GetAddr, payload);
                        this.Send(command, tracker.IP, tracker.Port);
                    }
                }
            }
        }

        public Action<P2PState> DataReceived;
        public Action<P2PState> OtherException;
        public Action<P2PState> PrepareSend;
        public Action<P2PState> CompletedSend;
        public Action<P2PNode> NodeConnectionStateChanged;

        public void Start(Guid guid, bool isTracker)
        {
            this.isTrackerNode = isTracker;
            this.Identity = guid.ToString();

            TotalBytesReceived = 0;
            TotalBytesSent = 0;

            //IPEndPoint ip = new IPEndPoint(IPAddress.Any, 54321);
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, int.Parse(GlobalParameters.IsTestnet ? Resource.TestnetPort : Resource.MainnetPort));

            if(isTracker)
            {
                ip = new IPEndPoint(IPAddress.Any, int.Parse(GlobalParameters.IsTestnet ? Resource.TestnetTrackerPort : Resource.MainnetTrackerPort));
            }

            this.server = new UdpClient(ip);

            this.Peers = new List<P2PNode>();
            this.sendCommandQueue = new Queue<P2PSendMessage>();
            this.receivedMessageBuffer = new Dictionary<string, List<byte>>();
            this.BlackList = new List<string>();

            this.IsRunning = true;
            this.server.Client.SendBufferSize = bufferSize;
            this.server.Client.ReceiveBufferSize = bufferSize;
            this.server.BeginReceive(receiveDataAsync, null);
            this.peerCheckTimer.Start();

            if (!this.isTrackerNode)
            {
                string trackerIp = GlobalParameters.IsTestnet ? Resource.TestnetTrackerServer : Resource.MainnetTrackerServer;
                int trackerPort = int.Parse(GlobalParameters.IsTestnet ? Resource.TestnetTrackerPort : Resource.MainnetTrackerPort);
                this.ConnectToNewPeer(trackerIp, trackerPort, true);
            }

            this.startSendCommand();
        }
        public void Stop()
        {
            this.peerCheckTimer.Stop();

            if (this.IsRunning)
            {
                this.IsRunning = false;
                this.server.Close();
            }
        }
        public void Send(P2PCommand command, string address, int port)
        {
            this.sendCommandQueue.Enqueue(new P2PSendMessage { Address = address, Port = port, Command = command });
        }
        public bool ConnectToNewPeer(string address, int port, bool isTracker = false)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(address, out ip))
            {
                try
                {
                    var ips = Dns.GetHostAddresses(address);

                    if (ips.Length > 0)
                    {
                        ip = ips[0];
                        address = ip.ToString();
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

            if (BlackList.Contains(address + ":" + port))
            {
                throw new CommonException(ErrorCode.Engine.P2P.Connection.PEER_IN_BLACK_LIST);
            }

            var peer = this.Peers.Where(p => p.IP == address && p.Port == port).FirstOrDefault();
            if (peer != null)
            {
                //if (!this.receivedMessageBuffer.ContainsKey(address + ":" + port))
                //{
                //    this.receivedMessageBuffer.Add(address + ":" + port, new List<byte>());
                //}
                ////else
                ////{
                ////    this.receivedMessageBuffer[address + ":" + port] = new List<byte>();
                ////}

                ////var command = P2PCommand.CreateCommand(CommandNames.P2P.Ping, null);
                ////this.Send(command, peer.IP, peer.Port);

                //LogHelper.Debug(DateTime.Now + " Connect to " + address);
                ////return true;
                //throw new CommonException(ErrorCode.Engine.P2P.Connection.THE_PEER_IS_EXISTED);

                return false;
            }
            else
            {
                if (this.Peers.Count >= this.maxConnections)
                {
                    throw new CommonException(ErrorCode.Engine.P2P.Connection.THE_NUMBER_OF_CONNECTIONS_IS_FULL);
                }

                peer = new P2PNode();
                peer.IP = address;
                peer.Port = port;
                peer.Identity = Guid.Empty.ToString();
                peer.IsConnected = false;
                peer.ConnectedTime = Time.EpochTime;
                peer.IsTrackerServer = isTracker;
                peer.IsInbound = false;

                this.Peers.Add(peer);

                if (this.receivedMessageBuffer.ContainsKey(address + ":" + port))
                {
                    this.receivedMessageBuffer[address + ":" + port] = new List<byte>();
                }
                else
                {
                    this.receivedMessageBuffer.Add(address + ":" + port, new List<byte>());
                }

                var command = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.Ping, null);
                this.Send(command, peer.IP, peer.Port);

                LogHelper.Debug(DateTime.Now + " Connect to " + address);
                return true;
            }
        }
        public bool RemovePeer(string address, int port)
        {
            var peer = this.Peers.Where(p => p.IP == address && p.Port == port).FirstOrDefault();

            if (peer != null)
            {
                this.Peers.Remove(peer);
                this.receivedMessageBuffer.Remove(peer.IP + ":" + peer.Port);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void startSendCommand()
        {
            while (this.server != null)
            {
                if (this.sendCommandQueue.Count > 0)
                {
                    var item = this.sendCommandQueue.Dequeue();

                    if (item != null)
                    {
                        try
                        {
                            raisePrepareSend(null);

                            var index = 0;
                            var data = item.Command.GetBytes();

                            while (index < data.Length)
                            {
                                byte[] buffer;

                                if (data.Length > index + this.bufferSize)
                                {
                                    buffer = new byte[this.bufferSize];
                                }
                                else
                                {
                                    buffer = new byte[data.Length - index];
                                }

                                Array.Copy(data, index, buffer, 0, buffer.Length);
                                this.server.BeginSend(buffer, buffer.Length, item.Address, item.Port, this.sendCallback, null);

                                this.TotalBytesSent += buffer.Length;
                                var peer = this.Peers.Where(p => p.IP == item.Address && p.Port == item.Port).FirstOrDefault();

                                if (peer != null)
                                {
                                    peer.TotalBytesSent += buffer.Length;
                                    peer.LastSentTime = Time.EpochTime;
                                }

                                index += buffer.Length;
                                Thread.Sleep(100);
                            }

                            LogHelper.Debug(DateTime.Now + " Send " + item.Command.CommandName + " to " + item.Address + ":" + item.Port);
                        }
                        catch (Exception)
                        {
                            raiseOtherException(null);
                        }
                    }

                }
                else
                {
                    Thread.Sleep(300);
                }

                if (!this.IsRunning)
                {
                    break;
                }
            }
        }
        private void sendCallback(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                try
                {
                    this.server.EndSend(ar);
                    raiseCompletedSend(null);
                }
                catch (Exception)
                {
                    raiseOtherException(null);
                }
            }

        }
        private void raisePrepareSend(P2PState state)
        {
            if (PrepareSend != null)
            {
                PrepareSend(state);
            }
        }
        private void raiseCompletedSend(P2PState state)
        {
            if (CompletedSend != null)
            {
                CompletedSend(state);
            }
        }
        private void receiveDataAsync(IAsyncResult ar)
        {
            IPEndPoint remote = null;
            byte[] buffer = null;

            try
            {
                buffer = server.EndReceive(ar, ref remote);
                this.TotalBytesReceived += buffer.Length;
                var peer = this.Peers.Where(p => p.IP == remote.Address.ToString() && p.Port == remote.Port).FirstOrDefault();

                if (peer != null)
                {
                    peer.TotalBytesReceived += buffer.Length;
                    peer.LastReceivedTime = Time.EpochTime;
                }

                LogHelper.Debug(DateTime.Now + " Received cmd from " + remote.Address + ":" + remote.Port + ", Data:" + Base16.Encode(buffer));

                var prefix = new byte[4];
                var suffix = new byte[4];
                bool isBufferEnd = false;
                var key = remote.Address + ":" + remote.Port;

                if (buffer.Length > 4)
                {
                    Array.Copy(buffer, 0, prefix, 0, 4);
                    Array.Copy(buffer, buffer.Length - 4, suffix, 0, 4);

                    if (!this.receivedMessageBuffer.ContainsKey(key))
                    {
                        this.receivedMessageBuffer.Add(key, new List<byte>());
                    }

                    //first data package
                    if (P2PCommand.BytesEquals(P2PCommand.DefaultPrefixBytes, prefix))
                    {
                        this.receivedMessageBuffer[key] = new List<byte>();
                        this.receivedMessageBuffer[key].AddRange(buffer);

                        //last data package
                        if (P2PCommand.BytesEquals(P2PCommand.DefaultSuffixBytes, suffix))
                        {
                            isBufferEnd = true;
                        }
                        else
                        {

                        }
                    }
                    else if (P2PCommand.BytesEquals(P2PCommand.DefaultSuffixBytes, suffix))
                    {
                        this.receivedMessageBuffer[key].AddRange(buffer);
                        isBufferEnd = true;
                    }
                    //other data package
                    else
                    {
                        this.receivedMessageBuffer[key].AddRange(buffer);
                    }
                }
                else
                {
                    this.receivedMessageBuffer[key].AddRange(buffer);
                    isBufferEnd = true;
                }

                if (isBufferEnd)
                {
                    var command = P2PCommand.ConvertBytesToMessage(this.receivedMessageBuffer[key].ToArray());

                    if(command.Identity == this.Identity)
                    {
                        return;
                    }

                    P2PState state = new P2PState();
                    state.IP = remote.Address.ToString();
                    state.Port = remote.Port;
                    state.Command = command;

                    if (command != null)
                    {
                        LogHelper.Debug(DateTime.Now + " Received cmd from " + remote.Address + ":" + remote.Port + ", Command:" + command.CommandName);

                        if (peer == null && command.CommandName != CommandNames.P2P.Ping)
                        {
                            this.ConnectToNewPeer(remote.Address.ToString(), remote.Port);
                            return;
                        }
                        else if(peer != null)
                        {
                            peer.Identity = command.Identity;
                        }

                        switch (command.CommandName)
                        {
                            case CommandNames.P2P.Ping:
                                this.pingMsgHandle(state);
                                break;
                            case CommandNames.P2P.Pong:
                                this.pongMsgHandle(state);
                                break;
                            case CommandNames.P2P.Version:
                                this.versionMsgHandle(state);
                                break;
                            case CommandNames.P2P.VerAck:
                                this.verAckMsgHandle(state);
                                break;
                            case CommandNames.P2P.GetAddr:
                                this.getAddrMsgHandle(state);
                                break;
                            case CommandNames.P2P.Addr:
                                this.addrMsgHandle(state);
                                break;
                            case CommandNames.P2P.Heartbeat:
                                this.heartbeatMsgHandle(state);
                                break;
                            case CommandNames.Other.Reject:
                                this.rejectMsgHandle(state);
                                break;
                            default:
                                raiseDataReceived(state);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                raiseOtherException(null);
            }
            finally
            {
                if (this.IsRunning && this.server != null)
                {
                    server.BeginReceive(receiveDataAsync, null);
                }
            }
        }
        private void raiseDataReceived(P2PState state)
        {
            if (DataReceived != null)
            {
                DataReceived(state);
            }
        }
        private void raiseOtherException(P2PState state, string descrip)
        {
            if (OtherException != null)
            {
                OtherException(state);
            }
        }
        private void raiseOtherException(P2PState state)
        {
            raiseOtherException(state, "");
        }
        private void raiseNodeConnectionStateChanged(P2PNode node)
        {
            if (this.NodeConnectionStateChanged != null)
            {
                NodeConnectionStateChanged(node);
            }
        }

        private void pingMsgHandle(P2PState state)
        {
            var peer = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer == null)
            {
                if (this.Peers.Count < this.maxConnections)
                {
                    var newPeer = new P2PNode();
                    newPeer.IP = state.IP;
                    newPeer.Port = state.Port;
                    newPeer.Identity = state.Command.Identity;
                    newPeer.IsConnected = false;
                    newPeer.ConnectedTime = Time.EpochTime;
                    newPeer.IsTrackerServer = false;
                    newPeer.IsInbound = true;

                    this.Peers.Add(newPeer);

                    if (this.receivedMessageBuffer.ContainsKey(state.IP + ":" + state.Port))
                    {
                        this.receivedMessageBuffer[state.IP + ":" + state.Port] = new List<byte>();
                    }
                    else
                    {
                        this.receivedMessageBuffer.Add(state.IP + ":" + state.Port, new List<byte>());
                    }

                    var pongCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.Pong, null);
                    this.Send(pongCommand, newPeer.IP, newPeer.Port);

                    var verPayload = new VersionMsg();
                    verPayload.Version = Versions.EngineVersion;
                    verPayload.Timestamp = Time.EpochTime;

                    var versionCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.Version, verPayload);
                    this.Send(versionCommand, state.IP, state.Port);
                }
                else
                {
                    var payload = new RejectMsg();
                    payload.ReasonCode = ErrorCode.Engine.P2P.Connection.THE_NUMBER_OF_CONNECTIONS_IS_FULL;

                    var rejectCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.Other.Reject, payload);
                    this.Send(rejectCommand, state.IP, state.Port);
                }
            }
            else
            {
                var pongCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.Pong, null);
                this.Send(pongCommand, state.IP, state.Port);

                var verPayload = new VersionMsg();
                verPayload.Version = Versions.EngineVersion;
                verPayload.Timestamp = Time.EpochTime;

                var versionCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.Version, verPayload);
                this.Send(versionCommand, state.IP, state.Port);

                if (!this.receivedMessageBuffer.ContainsKey(state.IP + ":" + state.Port))
                {
                    this.receivedMessageBuffer.Add(state.IP + ":" + state.Port, new List<byte>());
                }

                //var payload = new RejectMsg();
                //payload.ReasonCode = ErrorCode.Engine.P2P.Connection.THE_PEER_IS_EXISTED;

                //var rejectCommand = P2PCommand.CreateCommand(CommandNames.Other.Reject, payload);
                //this.Send(rejectCommand, state.IP, state.Port);
            }
        }
        private void pongMsgHandle(P2PState state)
        {
            var peer = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null)
            {
                var verPayload = new VersionMsg();
                verPayload.Version = Versions.EngineVersion;
                verPayload.Timestamp = Time.EpochTime;

                var versionCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.Version, verPayload);
                this.Send(versionCommand, state.IP, state.Port);
                //peer.IsConnected = true;
                //peer.ConnectedTime = Time.EpochTime;
                //peer.LatestHeartbeat = Time.EpochTime;
            }
        }
        private void versionMsgHandle(P2PState state)
        {
            var peer = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null)
            {
                var versionMsg = new VersionMsg();
                int index = 0;
                versionMsg.Deserialize(state.Command.Payload, ref index);
                bool checkResult;

                if (versionMsg.Version < Versions.MinimumSupportVersion)
                {
                    checkResult = false;
                    var data = new RejectMsg();
                    data.ReasonCode = ErrorCode.Engine.P2P.Connection.P2P_VERSION_NOT_BE_SUPPORT_BY_REMOTE_PEER;

                    var rejectCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.Other.Reject, data);
                    this.Send(rejectCommand, state.IP, state.Port);

                    this.RemovePeer(state.IP, state.Port);
                }
                else if (Math.Abs(Time.EpochTime - versionMsg.Timestamp) > 2 * 60 * 60 * 1000)
                {
                    checkResult = false;
                    var data = new RejectMsg();
                    data.ReasonCode = ErrorCode.Engine.P2P.Connection.TIME_NOT_MATCH_WITH_RMOTE_PEER;

                    var rejectCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.Other.Reject, data);
                    this.Send(rejectCommand, state.IP, state.Port);
                }
                else
                {
                    peer.Version = versionMsg.Version;
                    checkResult = true;
                }

                if (checkResult)
                {
                    var verAckCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.VerAck, null);
                    this.Send(verAckCommand, state.IP, state.Port);
                }
            }
        }
        private void verAckMsgHandle(P2PState state)
        {
            var peer = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null)
            {
                peer.IsConnected = true;
                peer.ConnectedTime = Time.EpochTime;
                peer.LastHeartbeat = Time.EpochTime;

                if (peer.IsTrackerServer)
                {
                    var payload = new GetAddrMsg();
                    payload.Count = this.maxConnections;

                    var command = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.GetAddr, payload);
                    this.Send(command, peer.IP, peer.Port);
                }
                else
                {
                    this.raiseNodeConnectionStateChanged(peer);
                }
            }
        }
        private void getAddrMsgHandle(P2PState state)
        {
            var peer = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null && peer.IsConnected)
            {
                var data = new GetAddrMsg();
                int index = 0;
                data.Deserialize(state.Command.Payload, ref index);

                if (data.Count <= 0 || data.Count > 100)
                {
                    data.Count = 100;
                }

                var list = this.Peers.Where(p => p.IP != state.IP || p.Port != state.Port).OrderByDescending(p => p.LastHeartbeat).Take(data.Count).ToList();

                var payload = new AddrMsg();

                foreach (var item in list)
                {
                    payload.AddressList.Add(new AddrMsg.AddressInfo() {
                        Ip = item.IP,
                        Port = item.Port,
                        Identity = item.Identity
                    });
                }

                var addrCommand = P2PCommand.CreateCommand(this.Identity, CommandNames.P2P.Addr, payload);
                this.Send(addrCommand, state.IP, state.Port);
            }
        }
        private void addrMsgHandle(P2PState state)
        {
            var peer = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null && peer.IsConnected)
            {
                var payload = new AddrMsg();
                int index = 0;
                payload.Deserialize(state.Command.Payload, ref index);

                foreach (var item in payload.AddressList)
                {
                    if(item.Identity != this.Identity)
                    {
                        if (this.Peers.Where(p => !p.IsTrackerServer && p.IP == item.Ip && p.Port == item.Port && p.IsConnected).Count() == 0)
                        {
                            this.ConnectToNewPeer(item.Ip, item.Port);
                        }
                    }
                }
            }

        }
        private void heartbeatMsgHandle(P2PState state)
        {
            var peer = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null && peer.IsConnected)
            {
                var payload = new HeightMsg();
                int index = 0;

                try
                {
                    payload.Deserialize(state.Command.Payload, ref index);
                    peer.LatestHeight = payload.Height;
                    peer.LatestBlockTime = payload.BlockTime;
                }
                catch
                {

                }

                peer.LastHeartbeat = Time.EpochTime;
            }
        }
        private void rejectMsgHandle(P2PState state)
        {
            var peer = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null && peer.IsConnected)
            {
                if (!peer.IsConnected)
                {
                    this.RemovePeer(peer.IP, peer.Port);
                }
                else
                {
                    raiseDataReceived(state);
                }
            }
        }
    }
}
