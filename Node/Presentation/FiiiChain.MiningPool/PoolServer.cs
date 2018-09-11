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
    // Implements the connection logic for the socket server.  
    // After accepting a connection, all data read from the client 
    // is sent back to the client. The read and echo back to the client pattern 
    // is continued until the client disconnects.
    class PoolServer
    {
        private int m_numConnections;   // the maximum number of connections the sample is designed to handle simultaneously 
        private int m_receiveBufferSize;// buffer size to use for each socket I/O operation 
        BufferManager m_bufferManager;  // represents a large reusable set of buffers for all socket operations
        const int opsToPreAlloc = 2;    // read, write (don't alloc buffer space for accepts)
        Socket listenSocket;            // the socket used to listen for incoming connection requests
                                        // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations
        SocketAsyncEventArgsPool m_readPool;
        SocketAsyncEventArgsPool m_writePool;
        int m_totalBytesRead;           // counter of the total # bytes received by the server
        int m_numConnectedSockets;      // the total number of clients connected to the server 
        Semaphore m_maxNumberAcceptedClients;

        bool isInMining = false;

        public List<Miner> MinerList;
        public Action<Miner, ScoopDataMsg> ReceivedScoopDataMsgAction { get; set; }

        // Create an uninitialized server instance.  
        // To start the server listening for connection requests
        // call the Init method followed by Start method 
        //
        // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public PoolServer(int numConnections, int receiveBufferSize)
        {
            m_totalBytesRead = 0;
            m_numConnectedSockets = 0;
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and 
            //write posted to the socket simultaneously  
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            m_readPool = new SocketAsyncEventArgsPool(numConnections);
            m_writePool = new SocketAsyncEventArgsPool(numConnections);
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);

            this.MinerList = new List<Miner>();
        }

        // Initializes the server by preallocating reusable buffers and 
        // context objects.  These objects do not need to be preallocated 
        // or reused, but it is done this way to illustrate how the API can 
        // easily be used to create reusable objects to increase server performance.
        //
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            m_bufferManager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs readEventArg;
            SocketAsyncEventArgs writeEventArg;

            for (int i = 0; i < m_numConnections; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readEventArg = new SocketAsyncEventArgs();
                readEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readEventArg.UserToken = new AsyncUserToken();

                writeEventArg = new SocketAsyncEventArgs();
                writeEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                writeEventArg.UserToken = new AsyncUserToken();

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                m_bufferManager.SetBuffer(readEventArg);

                // add SocketAsyncEventArg to the pool
                m_readPool.Push(readEventArg);
                m_writePool.Push(writeEventArg);
            }

        }

        // Starts the server such that it is listening for 
        // incoming connection requests.    
        //
        // <param name="localEndPoint">The endpoint which the server will listening 
        // for connection requests on</param>
        public void Start(IPEndPoint localEndPoint)
        {
            // create the socket which listens for incoming connections
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            // start the server with a listen backlog of 100 connections
            listenSocket.Listen(100);

            // post accepts on the listening socket
            StartAccept(null);

            //Console.WriteLine("{0} connected sockets with one outstanding receive posted to each....press any key", m_outstandingReadCount);
            //Console.WriteLine("Press any key to terminate the server process....");
            //Console.ReadKey();
        }

        public void StartMining(long height, int scoopNumber, long startTime)
        {
            if(!isInMining)
            {
                isInMining = true;

                StartMsg msg = new StartMsg();
                msg.BlockHeight = height;
                msg.ScoopNumber = scoopNumber;
                msg.StartTime = startTime;

                var startCmd = PoolCommand.CreateCommand(CommandNames.Start, msg);

                foreach (var miner in this.MinerList)
                {
                    if (miner.IsConnected)
                    {
                        this.sendStartCommand(miner.State, startCmd);
                    }
                }
            }
        }

        public void StopMining(bool result, long blockHeight, long startTime, long stopTime)
        {
            if(isInMining)
            {
                isInMining = false;
                StopMsg msg = new StopMsg();
                msg.Result = result;
                msg.BlockHeight = blockHeight;
                msg.StartTime = startTime;
                msg.StopTime = stopTime;

                var cmd = PoolCommand.CreateCommand(CommandNames.Stop, msg);

                foreach (var miner in this.MinerList)
                {
                    if (miner.IsConnected)
                    {
                        this.sendStopCommand(miner.State, cmd);
                    }
                }
            }
        }

        public void SendRewardNotification(Dictionary<string, RewardMsg> rewardDict)
        {
            foreach(var key in rewardDict.Keys)
            {
                var miner = this.MinerList.Where(m => m.WalletAddress == key && m.IsConnected).FirstOrDefault();

                if(miner != null)
                {
                    var cmd = PoolCommand.CreateCommand(CommandNames.Reward, rewardDict[key]);
                    this.StartSend(miner.State, cmd);
                }
            }
        }

        // Begins an operation to accept a connection request from the client 
        //
        // <param name="acceptEventArg">The context object to use when issuing 
        // the accept operation on the server's listening socket</param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

            //m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync 
        // operations and is invoked when an accept operation is complete
        //
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            // As soon as the client is connected, post a receive to the connection
            byte[] receiveBuffer = new byte[m_receiveBufferSize];
            e.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
            //e.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
            bool willRaiseEvent = ((AsyncUserToken)e.UserToken).Socket.ReceiveAsync(e);
            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Interlocked.Increment(ref m_numConnectedSockets);
            LogHelper.Info(string.Format("Client {0} connection accepted. There are {1} clients connected to the server",
                e.AcceptSocket.RemoteEndPoint.ToString(), m_numConnectedSockets));

            // Get the socket for the accepted client connection and put it into the 
            //ReadEventArg object user token
            SocketAsyncEventArgs readEventArgs = m_readPool.Pop();
            ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;

            // As soon as the client is connected, post a receive to the connection
            StartReceive(readEventArgs);

            // Accept the next connection request
            StartAccept(e);
        }

        // This method is called whenever a receive or send operation is completed on a socket 
        //
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        // This method is invoked when an asynchronous receive operation completes. 
        // If the remote host closed the connection, then the socket is closed.  
        // If data was received then the data is echoed back to the client.
        //
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                //increment the count of the total bytes receive by the server
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);

                var buffer = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, buffer, 0, e.BytesTransferred);
                var commandDataList = new List<byte[]>();
                var index = 0;
                List<byte> bytes = null;

                while(index < buffer.Length)
                {
                    if(bytes == null)
                    {
                        if((index + 3) < buffer.Length &&
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
                        if((index + 3) < buffer.Length &&
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
                
                foreach(var data in commandDataList)
                {
                    try
                    {
                        var cmd = PoolCommand.ConvertBytesToMessage(data);
                        switch(cmd.CommandName)
                        {
                            case CommandNames.Login:
                                this.receivedLoginCommand(e, cmd);
                                break;
                            case CommandNames.NonceData:
                                this.receivedNonceDataCommand(e, cmd);
                                break;
                            case CommandNames.ScoopData:
                                this.receivedScoopDataCommand(e, cmd);
                                break;
                            case CommandNames.Heartbeat:

                                break;
                            default:
                                break;
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }

                //var receivedText = Encoding.UTF8.GetString(bytes);
                //echo the data received back to the client
                //Console.WriteLine("Received Data from {0}:{1}", ((AsyncUserToken)e.UserToken).Socket.RemoteEndPoint.ToString(), receivedText);
                //Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);

                StartReceive(e);

            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void StartSend(SocketAsyncEventArgs e, PoolCommand cmd)
        {
            Byte[] sendBuffer = cmd.GetBytes();
            e.SetBuffer(sendBuffer, 0, sendBuffer.Length);
            bool willRaiseEvent = ((AsyncUserToken)e.UserToken).Socket.SendAsync(e);

            if(cmd.CommandName == CommandNames.Reject)
            {
                this.CloseClientSocket(e);                
            }

            if (!willRaiseEvent)
            {
                ProcessSend(e);
            }
        }

        // This method is invoked when an asynchronous send operation completes.  
        // The method issues another receive on the socket to read any additional 
        // data sent from the client
        //
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                //// done echoing data back to the client
                //AsyncUserToken token = (AsyncUserToken)e.UserToken;
                //// read the next block of data send from the client
                //bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                //if (!willRaiseEvent)
                //{
                //    ProcessReceive(e);
                //}
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;
            var miner = this.MinerList.Where(m => ((AsyncUserToken)m.State.UserToken).Socket.RemoteEndPoint.ToString() == ((AsyncUserToken)e.UserToken).Socket.RemoteEndPoint.ToString()).FirstOrDefault();

            if (miner != null)
            {
                this.MinerList.Remove(miner);
            }

            // close the socket associated with the client
            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
            }
            // throws if client process has already closed
            catch (Exception)
            {
            }

            token.Socket.Close();

            // decrement the counter keeping track of the total number of clients connected to the server
            Interlocked.Decrement(ref m_numConnectedSockets);
            //m_maxNumberAcceptedClients.Release();
            LogHelper.Info(string.Format("A client has been disconnected from the server. There are {0} clients connected to the server", m_numConnectedSockets));

            // Free the SocketAsyncEventArg so they can be reused by another client
            m_readPool.Push(e);
        }

        private void receivedLoginCommand(SocketAsyncEventArgs e, PoolCommand cmd)
        {
            var loginMsg = new LoginMsg();
            int index = 0;
            loginMsg.Deserialize(cmd.Payload, ref index);

            //TODO: check serialNo and walletAddress

            var miner = this.MinerList.Where(m => m.WalletAddress == loginMsg.WalletAddress).FirstOrDefault();

            if(miner != null)
            {
                this.sendRejectCommand(e);
            }
            else
            {
                SocketAsyncEventArgs writeEventArgs = m_writePool.Pop();
                ((AsyncUserToken)writeEventArgs.UserToken).Socket = ((AsyncUserToken)e.UserToken).Socket;

                miner = new Miner();
                miner.SerialNo = loginMsg.SerialNo;
                miner.WalletAddress = loginMsg.WalletAddress;
                miner.State = writeEventArgs;

                Random random = new Random();
                miner.CheckScoopNumber = random.Next(0, POC.MAX_SCOOP_NUMBER + 1);
                this.MinerList.Add(miner);

                this.sendMaxNonceCommand(e, miner.CheckScoopNumber);
            }
        }

        private void receivedNonceDataCommand(SocketAsyncEventArgs e, PoolCommand cmd)
        {
            var msg = new NonceDataMsg();
            int index = 0;
            msg.Deserialize(cmd.Payload, ref index);

            var miner = this.MinerList.Where(m => ((AsyncUserToken)m.State.UserToken).Socket.RemoteEndPoint.ToString() == ((AsyncUserToken)e.UserToken).Socket.RemoteEndPoint.ToString()).FirstOrDefault();

            if(miner != null)
            {
                var data = POC.CalculateScoopData(miner.WalletAddress, msg.MaxNonce, miner.CheckScoopNumber);

                if(Base16.Encode(data) == Base16.Encode(msg.ScoopData))
                {
                    miner.IsConnected = true;
                    miner.ConnectedTime = Time.EpochTime;
                    miner.LatestHeartbeatTime = Time.EpochTime;
                    this.sendLoginResultCommand(e, true);
                }
                else
                {
                    this.sendLoginResultCommand(e, false);
                    this.sendRejectCommand(e);
                }
            }
            else
            {
                this.sendRejectCommand(e);
            }
        }

        private void receivedScoopDataCommand(SocketAsyncEventArgs e, PoolCommand cmd)
        {
            var msg = new ScoopDataMsg();
            int index = 0;
            msg.Deserialize(cmd.Payload, ref index);

            var miner = this.MinerList.Where(m => ((AsyncUserToken)m.State.UserToken).Socket.RemoteEndPoint.ToString() == ((AsyncUserToken)e.UserToken).Socket.RemoteEndPoint.ToString() && m.IsConnected).FirstOrDefault();
            if (miner != null)
            {
                if(this.ReceivedScoopDataMsgAction != null)
                {
                    ReceivedScoopDataMsgAction(miner, msg);
                }
            }
        }

        private void receivedHeartbeatCommand(SocketAsyncEventArgs e, PoolCommand cmd)
        {
            var miner = this.MinerList.Where(m => ((AsyncUserToken)m.State.UserToken).Socket.RemoteEndPoint.ToString() == ((AsyncUserToken)e.UserToken).Socket.RemoteEndPoint.ToString() && m.IsConnected).FirstOrDefault();

            if(miner != null)
            {
                miner.LatestHeartbeatTime = Time.EpochTime;
            }
        }

        private void sendLoginResultCommand(SocketAsyncEventArgs e, bool result)
        {
            LogHelper.Info(((AsyncUserToken)e.UserToken).Socket.RemoteEndPoint.ToString() + " login " + (result ? " success" : " failed"));
            var msg = new LoginResultMsg();
            msg.Result = result;
            var cmd = PoolCommand.CreateCommand(CommandNames.LoginResult, msg);
            this.StartSend(e, cmd);
        }

        private void sendMaxNonceCommand(SocketAsyncEventArgs e, int scoopNumber)
        {
            var msg = new MaxNonceMsg();
            msg.RandomScoopNumber = scoopNumber;
            var cmd = PoolCommand.CreateCommand(CommandNames.MaxNonce, msg);
            this.StartSend(e, cmd);
        }

        private void sendStartCommand(SocketAsyncEventArgs e, PoolCommand cmd)
        {
            this.StartSend(e, cmd);
        }

        private void sendStopCommand(SocketAsyncEventArgs e, PoolCommand cmd)
        {
            this.StartSend(e, cmd);
        }

        private void sendRewardCommand(SocketAsyncEventArgs e, PoolCommand cmd)
        {
            this.StartSend(e, cmd);
        }

        private void sendRejectCommand(SocketAsyncEventArgs e)
        {
            var rejectCmd = PoolCommand.CreateCommand(CommandNames.Reject, null);
            this.StartSend(e, rejectCmd);
        }
    }
}
