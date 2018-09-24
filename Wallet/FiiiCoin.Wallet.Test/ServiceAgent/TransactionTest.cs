// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using FiiiCoin.DTO;
using FiiiCoin.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace FiiiCoin.Wallet.Test.ServiceAgent
{
    [TestClass]
    public class TransactionTest
    {
        [TestMethod]
        public async Task SendToAddress()
        {
            Transaction tran = new Transaction();
            string address = "1No2SahjFuguswiSvHv1DqotTRdMNs4FH";
            long amount = 100000000000;
            string commentTo = "Join";
            string result = await tran.SendToAddress(address, amount, "", commentTo, false);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task SendMany()
        {
            Transaction tran = new Transaction();
            string fromAccount = "1B7CXXyT2KQSHprzbprQDXnmkFEUTJU7yS";
            SendManyOM[] om = new SendManyOM[] { new SendManyOM { Address = "1317nkscoSnkZnGdFMqVawjJv3xxU5vyfb", Tag = "John", Amount = 100000000000 }, new SendManyOM { Address = "1No2SahjFuguswiSvHv1DqotTRdMNs4FH", Tag = null, Amount = 100000000000 } };
            string[] subtractFeeFromAmount = new string[] { "1317nkscoSnkZnGdFMqVawjJv3xxU5vyfb", "1No2SahjFuguswiSvHv1DqotTRdMNs4FH" };
            string result = await tran.SendMany(fromAccount, om, subtractFeeFromAmount);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task SetTxFee()
        {
            Transaction tran = new Transaction();
            await tran.SetTxFee(100000);
        }

        [TestMethod]
        public async Task SetConfirmations()
        {
            Transaction tran = new Transaction();
            await tran.SetConfirmations(2);
        }

        [TestMethod]
        public async Task GetTxSettings()
        {
            Transaction tran = new Transaction();
            TransactionFeeSettingOM om = await tran.GetTxSettings();
            Assert.IsNotNull(om);
        }

        [TestMethod]
        public async Task EstimateTxFeeForSendToAddress()
        {
            Transaction tran = new Transaction();
            string address = "1No2SahjFuguswiSvHv1DqotTRdMNs4FH";
            long amount = 100000000000;
            string commentTo = "Join";
            TxFeeForSendOM om = await tran.EstimateTxFeeForSendToAddress(address, amount, "", commentTo, false);
            Assert.IsNotNull(om);
        }

        [TestMethod]
        public async Task EstimateTxFeeForSendMany()
        {
            Transaction tran = new Transaction();
            string fromAccount = "1B7CXXyT2KQSHprzbprQDXnmkFEUTJU7yS";
            SendManyOM[] om = new SendManyOM[] { new SendManyOM { Address = "1317nkscoSnkZnGdFMqVawjJv3xxU5vyfb", Tag = "John", Amount = 100000000000 }, new SendManyOM { Address = "1No2SahjFuguswiSvHv1DqotTRdMNs4FH", Tag = null, Amount = 100000000000 } };
            string[] subtractFeeFromAmount = new string[] { "1317nkscoSnkZnGdFMqVawjJv3xxU5vyfb", "1No2SahjFuguswiSvHv1DqotTRdMNs4FH" };
            TxFeeForSendOM result = await tran.EstimateTxFeeForSendMany(fromAccount, om, subtractFeeFromAmount);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ListTransactions()
        {
            Transaction tran = new Transaction();
            PaymentOM[] result = await tran.ListTransactions("*", 10, 0, true);
            Assert.IsNotNull(result);
        }
    }
}
