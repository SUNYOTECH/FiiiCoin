// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Business;
using FiiiChain.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using FiiiChain.Framework;
using System.Threading;
using static FiiiChain.Messages.CommandNames;

namespace FiiiChain.Node
{
    public class P2PJob : BaseJob
    {
        P2PComponent p2pComponent;
        TransactionComponent txComponent;
        BlockComponent blockComponent;
        System.Timers.Timer blockSyncTimer;
        Thread thread;
        bool isRunning;
        bool needSendHeartbeat = false;
        Guid Identity = Guid.Empty;

        //type defined: 1st string: transaction hash, 2nd long: sync time
        Dictionary<string, long> txsInSynchronizing = new Dictionary<string, long>();
        //type defined: 1st long: block height, 2nd long: sync time
        Dictionary<long, long> blocksInSynchronizing = new Dictionary<long, long>();
        List<long> newBlocksInDownloading = new List<long>();
        List<string> newTxInDownloading = new List<string>();
        List<BlockMsg> tempBlockList = new List<BlockMsg>();
        public long LocalHeight = -1;
        public long LocalConfirmedHeight = -1;
        public long LocalLatestBlockTime = 0;
        public long RemoteLatestHeight = -1;
        public long RemoteLatestBlockTime = 0;

        public P2PJob()
        {
            this.p2pComponent = new P2PComponent();
            this.txComponent = new TransactionComponent();
            this.blockComponent = new BlockComponent();
            MiningPoolComponent.OnNewMiningPoolHandle = (msg) => receivedNewMiningPoolMessage(null, msg);
            blockSyncTimer = new System.Timers.Timer(10 * 1000);
            blockSyncTimer.AutoReset = true;
            blockSyncTimer.Elapsed += blockSyncTimer_Elapsed;
        }

        public override JobStatus Status
        {
            get
            {
                if (thread == null || (thread.ThreadState != ThreadState.Running && thread.ThreadState != ThreadState.WaitSleepJoin))
                {
                    return JobStatus.Stopped;
                }
                else if ((thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.WaitSleepJoin) && !isRunning)
                {
                    return JobStatus.Stopping;
                }
                else
                {
                    return JobStatus.Running;
                }
            }
        }

        public override void Start()
        {
            this.Identity = Guid.NewGuid();
            isRunning = true;
            thread = new Thread(new ThreadStart(this.start));
            //thread.IsBackground = true;
            thread.Start();
            blockSyncTimer.Start();
        }

        public override void Stop()
        {
            isRunning = false;
            blockSyncTimer.Stop();
            p2pComponent.P2PStop();
            p2pComponent.RegisterMessageReceivedCallback(null);
            p2pComponent.RegisterNodeConnectedStateChangedCallback(null);
        }

        public BlockChainInfo GetLatestBlockChainInfo()
        {
            var result = new BlockChainInfo();
            result.IsP2PRunning = p2pComponent.IsRunning();

            if (result.IsP2PRunning)
            {
                var nodes = p2pComponent.GetNodes();
                result.ConnectionCount = nodes.Where(n => n.IsConnected).Count();
                result.LastBlockHeightInCurrentNode = this.LocalHeight;
                result.LastBlockTimeInCurrentNode = this.LocalLatestBlockTime;
                result.LatestBlockHeightInNetwork = this.RemoteLatestHeight;
                result.LatestBlockTimeInNetwork = this.RemoteLatestBlockTime;
                result.TempBlockCount = this.tempBlockList.Count;
            }

            return result;
        }

        private void start()
        {
            p2pComponent.RegisterMessageReceivedCallback(this.dataReceived);
            p2pComponent.RegisterNodeConnectedStateChangedCallback(this.connectionStateChanged);
            p2pComponent.P2PStart(this.Identity, false);
        }

        private void dataReceived(P2PState state)
        {
            int index = 0;

            switch (state.Command.CommandName)
            {
                //case CommandNames.P2P.GetAddr:
                //    this.getAddrMsgHandle(state);
                //    break;
                //case CommandNames.P2P.Addr:
                //    this.addrMsgHandle(state);
                //    break;
                //case CommandNames.P2P.Heartbeat:
                //    this.heartbeatMsgHandle(state);
                //    break;
                case CommandNames.Transaction.GetTxPool:
                    this.receivedGetTransactionPool(state.IP, state.Port, state.Command.Nonce);
                    break;
                case CommandNames.Transaction.TxPool:
                    var txPoolMsg = new TxPoolMsg();
                    txPoolMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedTransacitonPoolMessage(state.IP, state.Port, txPoolMsg);
                    break;
                case CommandNames.Transaction.GetTx:
                    var getTxMsg = new GetTxsMsg();
                    getTxMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedGetTransaction(state.IP, state.Port, getTxMsg, state.Command.Nonce);
                    break;
                case CommandNames.Transaction.Tx:
                    var txsMsg = new TxsMsg();
                    txsMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedTransactionMessage(state.IP, state.Port, txsMsg);
                    break;
                case CommandNames.Transaction.NewTx:
                    var newTxMsg = new NewTxMsg();
                    newTxMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedNewTransactionMessage(state.IP, state.Port, newTxMsg);
                    break;
                case CommandNames.Block.GetHeight:
                    this.receivedGetHeight(state.IP, state.Port, state.Command.Nonce);
                    break;
                case CommandNames.Block.Height:
                    var heightMsg = new HeightMsg();
                    heightMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedHeightMessage(state.IP, state.Port, heightMsg);
                    break;
                case CommandNames.Block.GetHeaders:
                    var getHeadersMsg = new GetHeadersMsg();
                    getHeadersMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedGetHeaders(state.IP, state.Port, getHeadersMsg, state.Command.Nonce);
                    break;
                case CommandNames.Block.Headers:
                    var headersMsg = new HeadersMsg();
                    headersMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedHeadersMessage(state.IP, state.Port, headersMsg);
                    break;
                case CommandNames.Block.GetBlocks:
                    var getBlocksMsg = new GetBlocksMsg();
                    getBlocksMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedGetBlocks(state.IP, state.Port, getBlocksMsg, state.Command.Nonce);
                    break;
                case CommandNames.Block.Blocks:
                    var blocksMsg = new BlocksMsg();
                    blocksMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedBlocksMessage(state.IP, state.Port, blocksMsg);
                    break;
                case CommandNames.Block.NewBlock:
                    var newBlockMsg = new NewBlockMsg();
                    newBlockMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedNewBlockMessage(state.IP, state.Port, newBlockMsg, state.Command.Nonce);
                    break;
                case CommandNames.MiningPool.GetMiningPools:
                    this.receivedGetMiningPoolsMessage(state.IP, state.Port);
                    break;
                case CommandNames.MiningPool.MiningPools:
                    var miningPoolMsg = new MiningPoolMsg();
                    miningPoolMsg.Deserialize(state.Command.Payload, ref index);
                    receivedMiningPoolsMessage(miningPoolMsg);
                    break;
                case CommandNames.MiningPool.NewMiningPool:
                    var newMiningPoolMsg = new NewMiningPoolMsg();
                    newMiningPoolMsg.Deserialize(state.Command.Payload, ref index);
                    this.receivedNewMiningPoolMessage(state, newMiningPoolMsg);
                    break;
                case CommandNames.Other.Reject:

                    break;
                case CommandNames.Other.NotFound:
                    break;
                default:
                    break;
            }
        }
        private void getAddrMsgHandle(P2PState state)
        {
            var peers = this.p2pComponent.GetNodes();
            var peer = peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null && peer.IsConnected)
            {
                var data = new GetAddrMsg();
                int index = 0;
                data.Deserialize(state.Command.Payload, ref index);

                if (data.Count <= 0 || data.Count > 100)
                {
                    data.Count = 100;
                }

                var list = peers.Where(p => p.IP != state.IP || p.Port != state.Port).OrderByDescending(p => p.LastHeartbeat).Take(data.Count).ToList();

                var payload = new AddrMsg();

                foreach (var item in list)
                {
                    payload.AddressList.Add(new AddrMsg.AddressInfo()
                    {
                        Ip = item.IP,
                        Port = item.Port,
                        Identity = item.Identity
                    });
                }

                var addrCommand = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.P2P.Addr, payload);
                this.p2pComponent.SendCommand(state.IP, state.Port, addrCommand);
            }
        }
        private void addrMsgHandle(P2PState state)
        {
            var peers = this.p2pComponent.GetNodes();
            var peer = peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if (peer != null && peer.IsConnected)
            {
                var payload = new AddrMsg();
                int index = 0;
                payload.Deserialize(state.Command.Payload, ref index);

                foreach (var item in payload.AddressList)
                {
                    if (peers.Where(p => !p.IsTrackerServer && p.IP == item.Ip && p.Port == item.Port && p.IsConnected).Count() == 0)
                    {
                        this.p2pComponent.AddNode(item.Ip, item.Port);
                    }
                }
            }

        }
        private void heartbeatMsgHandle(P2PState state)
        {
            var peer = this.p2pComponent.GetNodes().Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

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

        private void connectionStateChanged(P2PNode node)
        {
            if (node.IsConnected)
            {
                this.sendHeartbeat(node.IP, node.Port);
                this.sendGetTransactionPool(node.IP, node.Port);
                this.sendGetMiningPool(node.IP, node.Port);
            }
        }

        private void blockSyncTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var blockList = this.tempBlockList.OrderBy(b => b.Header.Height).ToList();

            foreach (var item in blockList)
            {
                this.saveBlockToDB(item);
            }

            foreach (var item in this.blocksInSynchronizing.ToList())
            {
                if (blocksInSynchronizing.ContainsKey(item.Key) && Time.EpochTime - blocksInSynchronizing[item.Key] > 0.5 * 60 * 1000)
                {
                    this.blocksInSynchronizing.Remove(item.Key);
                }
            }

            var nodes = this.p2pComponent.GetNodes();

            LocalHeight = this.blockComponent.GetLatestHeight();
            LocalConfirmedHeight = this.blockComponent.GetLatestConfirmedHeight();
            var block = this.blockComponent.GetBlockMsgByHeight(LocalHeight);
            if (block != null)
            {
                this.LocalLatestBlockTime = block.Header.Timestamp;
            }

            p2pComponent.SetBlockHeightAndTime(LocalHeight, LocalLatestBlockTime);

            foreach (var node in nodes)
            {
                if (node.IsConnected)
                {
                    if (!node.IsTrackerServer && node.LatestHeight > RemoteLatestHeight)
                    {
                        RemoteLatestHeight = node.LatestHeight;
                        RemoteLatestBlockTime = node.LatestBlockTime;
                    }

                    //needSendHeartbeat = !needSendHeartbeat;
                    //if (needSendHeartbeat)
                    //{
                    this.sendHeartbeat(node.IP, node.Port);
                    //}
                }
            }

            if (RemoteLatestHeight > LocalHeight)
            {
                var random = new Random();
                var maxHeight = LocalConfirmedHeight + 20;

                if (maxHeight > RemoteLatestHeight)
                {
                    maxHeight = RemoteLatestHeight;
                }

                for (var i = LocalConfirmedHeight + 1; i <= maxHeight; i++)
                {
                    if (this.blocksInSynchronizing.ContainsKey(i))
                    {
                        continue;
                    }

                    if (this.tempBlockList.Where(t => t.Header.Height == i).Count() > 0)
                    {
                        continue;
                    }

                    if (!this.blockComponent.CheckConfirmedBlockExists(i))
                    {
                        var nodeList = nodes.Where(n => n.IsConnected && !n.IsTrackerServer && n.LatestHeight >= i).ToList();

                        if (nodeList.Count > 0)
                        {
                            var index = random.Next(0, nodeList.Count);
                            var node = nodeList[index];
                            var heightList = new List<long>();
                            heightList.Add(i);
                            this.sendGetBlocks(node.IP, node.Port, heightList);
                            blocksInSynchronizing[i] = Time.EpochTime;
                        }
                    }
                }
            }
        }
        private void sendHeartbeat(string address, int port)
        {
            var msg = new HeightMsg();
            msg.Height = this.LocalHeight;
            msg.BlockTime = this.LocalLatestBlockTime;
            var command = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.P2P.Heartbeat, msg);
            p2pComponent.SendCommand(address, port, command);
        }
        private void sendGetTransactionPool(string address, int port)
        {
            P2PCommand cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Transaction.GetTxPool, null);
            p2pComponent.SendCommand(address, port, cmd);
        }
        private void sendGetMiningPool(string address, int port)
        {
            P2PCommand cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.MiningPool.GetMiningPools, null);
            p2pComponent.SendCommand(address, port, cmd);
        }
        private void receivedGetTransactionPool(string address, int port, int nonce)
        {
            var hashes = this.txComponent.GetAllHashesFromPool();
            var payload = new TxPoolMsg();
            payload.Hashes.AddRange(hashes);

            var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Transaction.TxPool, nonce, payload);
            this.p2pComponent.SendCommand(address, port, cmd);
        }
        private void receivedTransacitonPoolMessage(string address, int port, TxPoolMsg msg)
        {
            var txHashes = new List<string>();

            foreach (var h in msg.Hashes)
            {
                if (!this.txComponent.CheckTxExisted(h))
                {
                    txHashes.Add(h);
                }
            }

            if (txHashes.Count > 0)
            {
                this.sendGetTransaction(address, port, txHashes);
            }
        }
        private void sendGetTransaction(string address, int port, List<string> txHashList)
        {
            for (int i = txHashList.Count - 1; i >= 0; i--)
            {
                var hash = txHashList[i];

                if (this.txsInSynchronizing.ContainsKey(hash))
                {
                    if (Time.EpochTime - this.txsInSynchronizing[hash] > 60 * 1000)
                    {
                        txsInSynchronizing[hash] = Time.EpochTime;
                    }
                    else
                    {
                        txHashList.RemoveAt(i);
                    }
                }
                else
                {
                    txsInSynchronizing.Add(hash, Time.EpochTime);
                }
            }

            var payload = new GetTxsMsg();
            payload.Hashes.AddRange(txHashList);

            var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Transaction.GetTx, payload);
            p2pComponent.SendCommand(address, port, cmd);
        }
        private void receivedGetTransaction(string address, int port, GetTxsMsg msg, int nonce)
        {
            var txList = new List<TransactionMsg>();

            foreach (var hash in msg.Hashes)
            {
                var tx = this.txComponent.GetTransactionMsgFromPool(hash);

                LogHelper.Info("Get Transaction:" + hash + ":" + tx == null ? "Not found" : "be found");

                if (tx != null)
                {
                    txList.Add(tx);
                }
            }

            if (txList.Count > 0)
            {
                var payload = new TxsMsg();
                payload.Transactions.AddRange(txList);

                var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Transaction.Tx, payload);
                this.p2pComponent.SendCommand(address, port, cmd);
            }
            else
            {
                this.sendDataNoFoundCommand(address, port, nonce);
            }
        }
        private void receivedTransactionMessage(string address, int port, TxsMsg msg)
        {
            foreach (var tx in msg.Transactions)
            {
                if (this.txsInSynchronizing.ContainsKey(tx.Hash))
                {
                    txsInSynchronizing.Remove(tx.Hash);
                }

                if (this.newTxInDownloading.Contains(tx.Hash))
                {
                    var nodes = this.p2pComponent.GetNodes();

                    //Broadcast to other node
                    foreach (var node in nodes)
                    {
                        if (node.IsConnected && !node.IsTrackerServer && node.IP != address)
                        {
                            var payload = new NewTxMsg();
                            payload.Hash = tx.Hash;

                            var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Transaction.NewTx, payload);
                            this.p2pComponent.SendCommand(node.IP, node.Port, cmd);
                        }
                    }

                    newTxInDownloading.Remove(tx.Hash);
                }

                this.txComponent.AddTransactionToPool(tx);
            }
        }
        private void receivedNewTransactionMessage(string address, int port, NewTxMsg msg)
        {
            if (!this.txComponent.CheckTxExisted(msg.Hash))
            {
                if (!this.newTxInDownloading.Contains(msg.Hash))
                {
                    newTxInDownloading.Add(msg.Hash);

                    var payload = new GetTxsMsg();
                    payload.Hashes.Add(msg.Hash);

                    var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Transaction.GetTx, payload);
                    this.p2pComponent.SendCommand(address, port, cmd);
                }
            }
        }
        private void receivedGetMiningPoolsMessage(string address, int port)
        {
            var list = (new MiningPoolComponent()).GetAllMiningPools();

            var payload = new MiningPoolMsg();

            foreach (var item in list)
            {
                MiningMsg itemMsg = new MiningMsg();
                itemMsg.Name = item.Name;
                itemMsg.PublicKey = item.PublicKey;
                itemMsg.Signature = item.Signature;
                payload.MinerInfos.Add(itemMsg);
            }

            var command = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.MiningPool.MiningPools, payload);
            this.p2pComponent.SendCommand(address, port, command);
        }
        private void receivedMiningPoolsMessage(MiningPoolMsg msg)
        {
            (new MiningPoolComponent()).UpdateMiningPools(msg.MinerInfos);
        }
        private void receivedNewMiningPoolMessage(P2PState state, NewMiningPoolMsg msg)
        {
            if (msg == null)
                return;
            if (state == null || (new MiningPoolComponent()).AddMiningToPool(msg.MinerInfo))
            {
                var nodes = this.p2pComponent.GetNodes();
                nodes.ForEach(peer =>
                {
                    if (!peer.IsTrackerServer)
                    {
                        var command = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.MiningPool.NewMiningPool, msg);
                        this.p2pComponent.SendCommand(peer.IP, peer.Port, command);
                    }
                });
            }
        }

        public void BroadcastNewTransactionMessage(string txHash)
        {
            var nodes = this.p2pComponent.GetNodes();

            //Broadcast to other node
            foreach (var node in nodes)
            {
                if (node.IsConnected && !node.IsTrackerServer)
                {
                    var payload = new NewTxMsg();
                    payload.Hash = txHash;

                    var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Transaction.NewTx, payload);
                    this.p2pComponent.SendCommand(node.IP, node.Port, cmd);
                }
            }
        }

        public void SendGetHeight(string address, int port)
        {
            var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.GetHeight, null);
            p2pComponent.SendCommand(address, port, cmd);
        }
        private void receivedGetHeight(string address, int port, int nonce)
        {
            var height = this.blockComponent.GetLatestHeight();
            var block = this.blockComponent.GetBlockMsgByHeight(height);

            if (block != null)
            {
                var payload = new HeightMsg();
                payload.Height = height;
                payload.BlockTime = block.Header.Timestamp;

                var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.Height, nonce, payload);
                this.p2pComponent.SendCommand(address, port, cmd);
            }
        }
        private void receivedHeightMessage(string address, int port, HeightMsg msg)
        {
            //var localHeight = this.blockComponent.GetLatestHeight();
            //if(localHeight < msg.Height)
            //{
            //    var 
            //}

            var nodes = this.p2pComponent.GetNodes();

            var node = nodes.Where(n => n.IP == address && n.Port == port).FirstOrDefault();

            if (node != null)
            {
                node.LatestHeight = msg.Height;
                node.LatestBlockTime = msg.BlockTime;
            }
        }
        private void sendGetHeaders(string address, int port, List<long> heightList)
        {
            var payload = new GetHeadersMsg();
            payload.Heights.AddRange(heightList);

            var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.GetHeaders, payload);
            p2pComponent.SendCommand(address, port, cmd);
        }
        private void receivedGetHeaders(string address, int port, GetHeadersMsg msg, int nonce)
        {
            var headers = this.blockComponent.GetBlockHeaderMsgByHeights(msg.Heights);
            var payload = new HeadersMsg();
            payload.Headers.AddRange(headers);

            var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.Headers, nonce, payload);
            this.p2pComponent.SendCommand(address, port, cmd);
        }
        private void receivedHeadersMessage(string address, int port, HeadersMsg msg)
        {
            //TODO: Received block headers
        }
        private void sendGetBlocks(string address, int port, List<long> heightList)
        {
            if (heightList.Count > 0)
            {
                var payload = new GetBlocksMsg();
                payload.Heights.AddRange(heightList);

                var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.GetBlocks, payload);
                p2pComponent.SendCommand(address, port, cmd);
            }
        }
        private void receivedGetBlocks(string address, int port, GetBlocksMsg msg, int nonce)
        {
            var blocks = this.blockComponent.GetBlockMsgByHeights(msg.Heights);

            if (blocks.Count > 0)
            {
                var payload = new BlocksMsg();
                payload.Blocks.AddRange(blocks);

                var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.Blocks, nonce, payload);
                this.p2pComponent.SendCommand(address, port, cmd);
            }
            else
            {
                var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Other.NotFound, nonce, null);
                this.p2pComponent.SendCommand(address, port, cmd);
            }
        }
        private void receivedBlocksMessage(string address, int port, BlocksMsg msg)
        {
            foreach (var block in msg.Blocks)
            {
                this.saveBlockToDB(block);

                if (this.blocksInSynchronizing.ContainsKey(block.Header.Height))
                {
                    blocksInSynchronizing.Remove(block.Header.Height);
                }

                if (this.newBlocksInDownloading.Contains(block.Header.Height))
                {
                    var nodes = this.p2pComponent.GetNodes();
                    var newMsg = new NewBlockMsg();
                    newMsg.Header = block.Header;

                    //Broadcast to other node
                    foreach (var node in nodes)
                    {
                        if (node.IsConnected && !node.IsTrackerServer && node.IP != address)
                        {
                            var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.NewBlock, newMsg);
                            this.p2pComponent.SendCommand(node.IP, node.Port, cmd);
                        }
                    }

                    newBlocksInDownloading.Remove(block.Header.Height);
                }

            }
        }
        public void BroadcastNewBlockMessage(BlockHeaderMsg blockHeader)
        {
            var nodes = this.p2pComponent.GetNodes();

            foreach (var node in nodes)
            {
                if (node.IsConnected && !node.IsTrackerServer)
                {
                    var payload = new NewBlockMsg();
                    payload.Header = blockHeader;

                    var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.NewBlock, payload);
                    this.p2pComponent.SendCommand(node.IP, node.Port, cmd);
                }
            }

        }
        private void receivedNewBlockMessage(string address, int port, NewBlockMsg msg, int nonce)
        {
            if (this.blockComponent.GetBlockMsgByHash(msg.Header.Hash) == null)
            {
                if (!this.newBlocksInDownloading.Contains(msg.Header.Height))
                {
                    this.newBlocksInDownloading.Add(msg.Header.Height);

                    var payload = new GetBlocksMsg();
                    payload.Heights.Add(msg.Header.Height);

                    var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Block.GetBlocks, nonce, payload);
                    this.p2pComponent.SendCommand(address, port, cmd);
                }
            }
        }
        private void sendDataNoFoundCommand(string address, int port, int nonce)
        {
            var cmd = P2PCommand.CreateCommand(this.Identity.ToString(), CommandNames.Other.NotFound, nonce, null);
        }


        private void saveBlockToDB(BlockMsg block)
        {
            if (this.blockComponent.CheckBlockExists(block.Header.Hash))
            {
                var item = this.tempBlockList.Where(t => t.Header.Hash == block.Header.Hash).FirstOrDefault();

                if (item != null)
                {
                    this.tempBlockList.Remove(item);
                }
            }
            else if (block.Header.Height > 0)
            {
                if (this.blockComponent.CheckBlockExists(block.Header.PreviousBlockHash))
                {
                    this.blockComponent.SaveBlockIntoDB(block);
                    this.LocalHeight = block.Header.Height;
                    this.LocalLatestBlockTime = block.Header.Timestamp;
                    var item = this.tempBlockList.Where(t => t.Header.Hash == block.Header.Hash).FirstOrDefault();

                    if (item != null)
                    {
                        this.tempBlockList.Remove(item);
                    }
                }
                else
                {
                    if (this.tempBlockList.Where(t => t.Header.Hash == block.Header.Hash).Count() == 0)
                    {
                        this.tempBlockList.Add(block);
                    }
                }
            }
            else
            {
                this.blockComponent.SaveBlockIntoDB(block);
            }
        }
    }
}
