// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Utility;
using FiiiCoin.Wallet.Win.Biz;
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Biz.Services;
using FiiiCoin.Wallet.Win.Common;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FiiiCoin.Wallet.Win
{
    /// <summary>
    /// App.xaml 
    /// </summary>
    public partial class App : Application
    {
        static Mutex CurrentAppMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!DetectEnviroment())
            {
                return;
            }

            var netType = GetNetType(e.Args);
            if (netType == NetworkType.MainnetPort)
            {
                MessageBox.Show("FiiiChain Mainnet is not support now");
                Thread.Sleep(1000);
                Environment.Exit(1);
                return;
            }

            NodeSetting.CurrentNetworkType = netType;

            bool isCreateNew;
            CurrentAppMutex = DetectSingleRun(out isCreateNew);

            if (!isCreateNew)
            {
                MessageBox.Show("Application is already running...");
                Thread.Sleep(1000);
                Environment.Exit(1);
                CloseMutex();
            }
            
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            LanguageService.Default.SetLanguage(AppSettingConfig.Default.AppConfig.LanguageType);
            base.OnStartup(e);
            var shell = BootStrapService.Default.Shell.GetWindow();
            if (shell != null)
                shell.ShowDialog();

            CloseMutex();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                Logger.Singleton.Error(((Exception)e.ExceptionObject).Message);
            }

            ServiceMonitor.StopAll();
            var result = EngineService.Default.AppClosed();
        }

        private NetworkType GetNetType(string[] s)
        {
            try
            {
                if (s[0].ToLower() == "-testnet")
                    return NetworkType.TestNetPort;
                return NetworkType.MainnetPort;
            }
            catch
            {
                return NetworkType.MainnetPort;
            }
        }


        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Singleton.Error(e.Exception.Message);
            ServiceMonitor.StopAll();
            var result = EngineService.Default.AppClosed();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceMonitor.StopAll();
            var result = EngineService.Default.AppClosed();
            base.OnExit(e);
        }

        private Mutex DetectSingleRun(out bool createNew)
        {
            var mutexStr = NodeSetting.CurrentNetworkType == NetworkType.MainnetPort ? "FiiiCoin.Wallet" : "FiiiCoin.Wallet(testnet)";
            var mutex = new Mutex(true, mutexStr, out createNew);
            return mutex;
        }

        public static void CloseMutex()
        {
            if (CurrentAppMutex != null)
            {
                CurrentAppMutex.Close();
                CurrentAppMutex.Dispose();
            }
        }
        
        private static bool DetectRunTime()
        {
            RegistryKey hkml = Registry.LocalMachine;
            RegistryKey software = hkml.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Updates\", false);
            var names = software.GetSubKeyNames();

            if (!names.Any(x => x.ToLower().Contains("visual c++")))
            {
                MessageBox.Show("you have to install vc++ runtime");
                Thread.Sleep(3000);
                return false;
            }

            return DetectNetCore();
        }

        private static bool DetectNetCore()
        {
            RegistryKey hkml = Registry.LocalMachine;
            RegistryKey software = hkml.OpenSubKey(@"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App", false);
            var names = software.GetValueNames();
            var result = names.Any(x => x.StartsWith("2.0"));
            if(!result)
            {
                MessageBox.Show("you have to install .net core 2.0 runtime");
                Thread.Sleep(3000);
            }
            return result;
        }

        private static bool DetectEnviroment()
        {
            return DetectNetCore() && DetectRunTime();
        }
    }
}
