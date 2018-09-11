using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using FiiiChain.DTO.Transaction;
using FiiiChain.Framework;
using System.Threading.Tasks;
using System.Threading;

namespace FiiiChain.InvestorAward
{
    class Program
    {
        public static readonly long LOCKTIME1 = Time.GetEpochTime(2018, 10, 1, 0, 0, 0);
        public static readonly long LOCKTIME2 = Time.GetEpochTime(2018, 11, 1, 0, 0, 0);
        public static readonly long LOCKTIME3 = Time.GetEpochTime(2018, 12, 1, 0, 0, 0);
        public static readonly long LOCKTIME4 = Time.GetEpochTime(2019, 1, 1, 0, 0, 0);
        public static readonly long LOCKTIME5 = Time.GetEpochTime(2019, 2, 1, 0, 0, 0);
        public static readonly long LOCKTIME6 = Time.GetEpochTime(2019, 3, 1, 0, 0, 0);
        public static readonly long LOCKTIME7 = Time.GetEpochTime(2019, 4, 1, 0, 0, 0);
        public static readonly long LOCKTIME8 = Time.GetEpochTime(2019, 5, 1, 0, 0, 0);
        public static readonly long LOCKTIME9 = Time.GetEpochTime(2019, 6, 1, 0, 0, 0);
        public static readonly long LOCKTIME10 = Time.GetEpochTime(2019, 7, 1, 0, 0, 0);
        public static readonly long LOCKTIME11 = Time.GetEpochTime(2019, 8, 1, 0, 0, 0);
        public static readonly long LOCKTIME12 = Time.GetEpochTime(2019, 9, 1, 0, 0, 0);
        public static readonly long LOCKTIME13 = Time.GetEpochTime(2019, 10, 1, 0, 0, 0);

        static async Task Main(string[] args)
        {
            //Thread.Sleep(50000);
            /* 设计思路
             * 1、根据总账户和投资人的信息确定要发放13笔交易的金额
             * 2、根据交易的金额，先生成13笔utxo，返回txid和vout
             * 3、等待utxo全部确认后执行发放，每一次使用一个utxo
             */
            //从Excel文件中获取投资人的信息
            ExcelOperation operation = new ExcelOperation();
            List<Investor> list = operation.ReadExcelFile();
            List<long> lockTimeList = new List<long>() { LOCKTIME1, LOCKTIME2, LOCKTIME3, LOCKTIME4, LOCKTIME5, LOCKTIME6, LOCKTIME7, LOCKTIME8, LOCKTIME9, LOCKTIME10, LOCKTIME11, LOCKTIME12, LOCKTIME13 };

            //循环遍历
            for (int i = 0; i < lockTimeList.Count; i++)
            {
                //组织参数
                SendRawTransactionInputsIM[] senders = new SendRawTransactionInputsIM[] {
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 0 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 1 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 2 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 3 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 4 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 5 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 6 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 7 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 8 },
                    new SendRawTransactionInputsIM() { TxId = "6FF3B2E8AF77A27672AD3A9D01F0B09E8840E170895D9928A0851178B6146856", Vout = 9 }};

                SendRawTransactionOutputsIM[] receivers = new SendRawTransactionOutputsIM[list.Count];
                LogHelper.Info($"************************begin write log for { i + 1 } transaction************************");
                for (int j = 0; j < list.Count; j++)
                {
                    if (i != 12)
                    {
                        receivers[j] = new SendRawTransactionOutputsIM();
                        receivers[j].Address = list[j].Address;
                        receivers[j].Amount = Convert.ToInt64(Math.Ceiling(list[j].Amount * 0.08));
                        LogHelper.Info($"Name: {list[j].Name}, Address: {list[j].Address}, TotalAmount: {list[j].Amount}, Amount: {receivers[j].Amount}, Phone: {list[j].Phone}");
                    }
                    else
                    {
                        receivers[j] = new SendRawTransactionOutputsIM();
                        receivers[j].Address = list[j].Address;
                        receivers[j].Amount = Convert.ToInt64(Math.Ceiling(list[j].Amount * 0.04));
                        LogHelper.Info($"Name: {list[j].Name}, Address: {list[j].Address}, TotalAmount: {list[j].Amount}, Amount: {receivers[j].Amount}, Phone: {list[j].Phone}");
                    }
                }
                string changeAddress = "fiiitQcsegnPV5BMcSiJwRxiU8VEoPw8wAFumC";
                long lockTime = lockTimeList[i];
                long feeRate = 2048;
                LogHelper.Info($"this time the lock time is: {lockTime}");
                /*
                //调用接口SendRawTransaction，发送交易
                AuthenticationHeaderValue authHeaderValue = null;
                RpcClient client = new RpcClient(new Uri("http://localhost:5006"), authHeaderValue, null, null, "application/json");
                RpcRequest request = RpcRequest.WithParameterList("SendRawTransaction", new List<object> { senders, receivers, changeAddress, lockTime, feeRate }, 1);
                RpcResponse response = await client.SendRequestAsync(request);
                if (response.HasError)
                {
                    LogHelper.Error(response.Error.Message);
                    throw new Exception(response.Error.Message);
                }
                string txHash = response.GetResult<string>();
                //记录一下发放日志：地址，金额，期数，锁定时间，交易Hash
                LogHelper.Info($"this transaction hash is: {txHash}");
                LogHelper.Info("************************end write log for the transaction************************");
                */
            }
        }
    }
}
