// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz;
using FiiiCoin.Wallet.Win.Common;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Windows.Input;

namespace FiiiCoin.Wallet.Win.ViewModels
{
    public class InitViewModel : VmBase
    {
        protected override void OnLoaded()
        {
            base.OnLoaded();
            OnPageLoadedCommand = new RelayCommand(OnPageLoaded);
        }

        public ICommand OnPageLoadedCommand { get; private set; }

        void OnPageLoaded()
        {
            InitWalletSataus();
        }

        void InitWalletSataus()
        {
            Initializer.Default.InitializedInvoke += OnInitialized;
            Initializer.Default.Start();
        }

        private InitMsgEvent _msg;

        public InitMsgEvent Msg
        {
            get {
                if (_msg == null)
                    _msg = new InitMsgEvent(false, LanguageService.Default.GetLanguageValue("WalletLoading"));
                return _msg; }
            set {
                _msg = value;
                RaisePropertyChanged("Msg"); }
        }

        void OnInitialized(InitMsgEvent msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Msg.IsSucesses = msg.IsSucesses;
                Msg.Message = msg.Message;
                if(msg.IsSucesses)
                    UpdatePage(Pages.MainPage, PageModel.MainPage);
            });
        }
    }
}
