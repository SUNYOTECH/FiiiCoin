// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Utility;
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Biz.Services;
using FiiiCoin.Wallet.Win.Common;
using System;
using System.Threading;
using System.Windows;

namespace FiiiCoin.Wallet.Win
{
    /// <summary>
    /// App.xaml 
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Action startupAction = () => { base.OnStartup(e); };
            SinlgeWindowStart(startupAction);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Singleton.Info(e.Exception.Message);
            ServiceMonitor.StopAll();
            var result = EngineService.Default.AppClosed();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceMonitor.StopAll();
            var result = EngineService.Default.AppClosed();
            base.OnExit(e);
        }

        int retryCount = 0;
        static Mutex mutex;
        void SinlgeWindowStart(Action startupAction)
        {
            bool createNew;
            mutex = new Mutex(true, "FiiiCoin.Wallet", out createNew);

            if (createNew)
            {
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                LanguageService.Default.SetLanguage(AppSettingConfig.Default.AppConfig.LanguageType);
                startupAction();
                var shell = BootStrapService.Default.Shell.GetWindow();
                if (shell != null)
                    shell.ShowDialog();
            }
            else
            {
                if (retryCount < 5)
                {
                    Thread.Sleep(300);
                    retryCount++;
                    SinlgeWindowStart(startupAction);
                    return;
                }
                MessageBox.Show("Application is already running...");
                Thread.Sleep(1000);
                Environment.Exit(1);
            }
            CloseMutex();
        }

        public static void CloseMutex()
        {
            if (mutex != null)
            {
                mutex.Close();
                mutex.Dispose();
            }
        }
    }
}
