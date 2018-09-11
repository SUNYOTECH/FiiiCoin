// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace FiiiChain.DataAgent
{
    public class Notify
    {
        public static Notify Current;
        public string CallbackApp { get; set; }

        static Notify()
        {
            Current = new Notify();
        }

        public void NewTxReceived(string txHash)
        {
            if(string.IsNullOrWhiteSpace(CallbackApp) || string.IsNullOrWhiteSpace(txHash))
            {
                return;
            }

            ProcessStartInfo info = new ProcessStartInfo(CallbackApp);
            info.Arguments = txHash;

            try
            {
                info.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(info);
            }
            catch
            {

            }
        }
    }
}
