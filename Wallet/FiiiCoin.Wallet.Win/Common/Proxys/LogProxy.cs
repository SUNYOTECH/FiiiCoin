// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using FiiiCoin.Utility;
using System.Windows;
using FiiiCoin.Wallet.Win.Models;
using System.Threading.Tasks;
using System.Threading;

namespace FiiiCoin.Wallet.Win.Common.Proxys
{
    class LogProxy<T> : RealProxy
    {
        private T _target;
        public LogProxy(T target) : base(typeof(T))
        {
            this._target = target;
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMessage = (IMethodCallMessage)msg;
            PreProceede(callMessage);

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            object returnResult = null;
            var task = Task.Factory.StartNew(async () =>
            {
                returnResult = callMessage.MethodBase.Invoke(this._target, callMessage.Args);
                autoResetEvent.Set();
            });
            autoResetEvent.WaitOne();

            if (returnResult is Result)
            {
                var result = returnResult as Result;
                PostProceede(result);
            }

            return new ReturnMessage(returnResult, new object[0], 0, null, callMessage);
        }

        public void PreProceede(IMethodCallMessage msg)
        {
            var methodName = msg.MethodName;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Logger.Singleton.Info(methodName);
            });
        }

        public void PostProceede(Result result)
        {
            if (result == null) return;

            if (result.ApiResponse!=null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (result.ApiResponse.Result != null)
                        Logger.Singleton.Info(result.ApiResponse.Result.ToString().Replace("\r\n", ""));
                });
            }
        }
    }

    //TransparentProxy
    public static class TransparentProxy
    {
        public static T Create<T>()
        {
            T instance = Activator.CreateInstance<T>();
            LogProxy<T> realProxy = new LogProxy<T>(instance);
            T transparentProxy = (T)realProxy.GetTransparentProxy();
            return transparentProxy;
        }
    }
}