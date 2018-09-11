using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using FiiiChain.PoolMessages;
using System.Linq;
using FiiiChain.Consensus;
using FiiiChain.Framework;
namespace FiiiChain.MiningPool
{
    class SocketServer
    {
        TcpListener listener = null;
        int m_numConnections;
        int m_receiveBufferSize;
        int m_totalBytesRead;
        int m_numConnectedSockets;
        bool isStarted = false;
        Queue<TcpSendState> sendCommandQueue;
        // Thread signal.
        static ManualResetEvent tcpClientSend =
            new ManualResetEvent(false);
        static ManualResetEvent tcpClientConnect =
            new ManualResetEvent(false);

        public Action<TcpReceiveState, PoolCommand> ReceivedCommandAction { get; set; }
        public Func<TcpState, bool, bool> ReceivedMinerConnectionAction { get; set; }

        public SocketServer(int numConnections, int receiveBufferSize)
        {
            m_totalBytesRead = 0;
            m_numConnectedSockets = 0;
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;
            sendCommandQueue = new Queue<TcpSendState>();
        }
        public void Start(IPEndPoint localEndPoint)
        {
            this.listener = new TcpListener(localEndPoint);
            this.listener.Start();
            this.isStarted = true;

            this.startAccept();
            tcpClientSend.Reset();
            tcpClientConnect.Reset();
            //this.startSend();
        }

        public void Stop()
        {
            this.isStarted = false;
            this.listener.Stop();
        }

        public void CloseSocket(TcpState e)
        {
            if (this.ReceivedMinerConnectionAction != null)
            {
                this.ReceivedMinerConnectionAction(e, false);
            }

            if (e.Stream != null)
            {
                try
                {
                    e.Stream.Close();
                }
                catch
                {

                }
            }

            try
            {
                e.Client.Close();
            }
            catch
            {

            }

        }

        private void startAccept()
        {
            this.listener.BeginAcceptTcpClient(new AsyncCallback(processConnect), this.listener);
        }

        public void SendCommand(TcpState state, PoolCommand command)
        {
            //this.sendCommandQueue.Enqueue(new TcpSendMessage {
            //    Stream = stream,
            //    Command = command
            //});
            var buffer = command.GetBytes();
            state.Stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(processSend), state);
            tcpClientSend.WaitOne();
        }

        private void processConnect(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);

            var stream = client.GetStream();
            TcpReceiveState state = new TcpReceiveState();
            state.Client = client;
            state.Stream = stream;
            state.Buffer = new byte[m_receiveBufferSize];

            if (this.ReceivedMinerConnectionAction != null)
            {
                if(ReceivedMinerConnectionAction(state, true))
                {
                    state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length,
                        new AsyncCallback(processReceive), state);
                }
            }

            this.startAccept();
        }

        private void processReceive(IAsyncResult ar)
        {
            TcpReceiveState state = (TcpReceiveState)ar.AsyncState;
            Console.WriteLine(state.Client.Client.RemoteEndPoint);
            try
            {
                int numberOfBytesRead = state.Stream.EndRead(ar);

                if (numberOfBytesRead > 0)
                {
                    var buffer = new byte[numberOfBytesRead];
                    Array.Copy(state.Buffer, 0, buffer, 0, buffer.Length);

                    var commandDataList = new List<byte[]>();
                    var index = 0;
                    List<byte> bytes = null;

                    while (index < buffer.Length)
                    {
                        if (bytes == null)
                        {
                            if ((index + 3) < buffer.Length &&
                            buffer[index] == PoolCommand.DefaultPrefixBytes[0] &&
                            buffer[index + 1] == PoolCommand.DefaultPrefixBytes[1] &&
                            buffer[index + 2] == PoolCommand.DefaultPrefixBytes[2] &&
                            buffer[index + 3] == PoolCommand.DefaultPrefixBytes[3])
                            {
                                bytes = new List<byte>();
                                bytes.AddRange(PoolCommand.DefaultPrefixBytes);
                                index += 4;
                            }
                            else
                            {
                                index++;
                            }
                        }
                        else
                        {
                            if ((index + 3) < buffer.Length &&
                            buffer[index] == PoolCommand.DefaultSuffixBytes[0] &&
                            buffer[index + 1] == PoolCommand.DefaultSuffixBytes[1] &&
                            buffer[index + 2] == PoolCommand.DefaultSuffixBytes[2] &&
                            buffer[index + 3] == PoolCommand.DefaultSuffixBytes[3])
                            {
                                bytes.AddRange(PoolCommand.DefaultSuffixBytes);
                                commandDataList.Add(bytes.ToArray());
                                bytes = null;

                                index += 4;
                            }
                            else
                            {
                                bytes.Add(buffer[index]);
                                index++;
                            }
                        }
                    }

                    if (this.ReceivedCommandAction != null)
                    {
                        foreach (var data in commandDataList)
                        {
                            try
                            {
                                var cmd = PoolCommand.ConvertBytesToMessage(data);
                                this.ReceivedCommandAction(state, cmd);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("\r\nError occured on deserialize messgae: " + ex.Message);
                            }
                        }
                    }

                    state.Buffer = new byte[m_receiveBufferSize];
                    state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length,
                        new AsyncCallback(processReceive), state);
                }
                else
                {
                    this.CloseSocket(state);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error occured on receive messgae: " + ex.Message);
                this.CloseSocket(state);
            }
        }

        private void startSend()
        {
            while (isStarted)
            {
                if (this.sendCommandQueue.Count > 0)
                {

                    var item = this.sendCommandQueue.Dequeue();
                    if (item != null)
                    {
                        var buffer = item.Command.GetBytes();
                        item.Stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(processSend), item.Stream);
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void processSend(IAsyncResult ar)
        {
            var state = (TcpState)ar.AsyncState;

            try
            {
                state.Stream.EndWrite(ar);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error occured on send messgae: " + ex.Message);
                this.CloseSocket(state);
            }

            tcpClientSend.Set();
        }
    }
}
