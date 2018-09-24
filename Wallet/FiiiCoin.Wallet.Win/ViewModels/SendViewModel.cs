// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz;
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Biz.Services;
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Common.Utils;
using FiiiCoin.Wallet.Win.Models;
using FiiiCoin.Wallet.Win.Models.UiModels;
using FiiiCoin.Wallet.Win.ValidationRules;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FiiiCoin.Wallet.Win.ViewModels
{
    public class SendViewModel : VmBase
    {
        public SendViewModel()
        {
            if (IsInDesignMode)
            {

            }
            else
            {
                Init();
            }
        }

        void Init()
        {
            Fee = 0.0104;
            _sendItems = new ObservableCollection<SendItemInfo>();
            _sendItems.Add(new SendItemInfo());

            ChoseFeeCommand = new RelayCommand(OnChoseFee);
            SendCommand = new RelayCommand<ItemsControl>(OnSend);
            ClearCommand = new RelayCommand(OnClear);
            AddCommand = new RelayCommand(OnAdd);
            ChooseCommand = new RelayCommand<SendItemInfo>(OnChoose);
            PasteCommand = new RelayCommand<SendItemInfo>(OnPaste);
            ClearAddressCommand = new RelayCommand<SendItemInfo>(OnClearAddress);
            AllCommand = new RelayCommand<SendItemInfo>(OnAllClick);

            LoadData();

            RegeistMessenger<double>(OnUpdateFees);
            RegeistMessenger<bool>(OnCheckPwd);
            RegeistMessenger<UrlInfo>(OnGetUrlInfoRequest);
            RegeistMessenger<SendMsgData<SendItemInfo>>(OnGetMsgDataRequest);

            _sendItems.CollectionChanged += (s, e) => { RaisePropertyChanged("SendItems"); };
        }

        private double _fee;

        public double Fee
        {
            get { return _fee; }
            set { _fee = value; RaisePropertyChanged("Fee"); }
        }

        private long _overMoney;

        public long OverMoney
        {
            get { return _overMoney; }
            set
            {
                _overMoney = value;
                RaisePropertyChanged("OverMoney");
            }
        }

        private ObservableCollection<SendItemInfo> _sendItems;

        public ObservableCollection<SendItemInfo> SendItems
        {
            get
            {
                if (_sendItems == null)
                    _sendItems = new ObservableCollection<SendItemInfo>();
                return _sendItems;
            }
            set
            {
                _sendItems = value;
                RaisePropertyChanged("SendItems");
            }
        }

        public ICommand ChoseFeeCommand { get; private set; }
        public ICommand SendCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand ChooseCommand { get; private set; }
        public ICommand PasteCommand { get; private set; }
        public ICommand ClearAddressCommand { get; private set; }
        public ICommand AllCommand { get; private set; }

        void OnChoseFee()
        {
            SendMessenger(Pages.FeesPage, Fee);
            UpdatePage(Pages.FeesPage);
        }

        SendItemInfo _currentSendItem;

        void OnGetUrlInfoRequest(UrlInfo urlInfo)
        {
            if (urlInfo == null || _currentSendItem == null)
                return;
            _currentSendItem.Address = urlInfo.Address;
            _currentSendItem.Tag = urlInfo.Tag;
        }

        void OnGetMsgDataRequest(SendMsgData<SendItemInfo> data)
        {
            if (data == null || data.Token == null)
                return;
            SendItems.Clear();
            SendItems.Add(data.Token);
        }

        void OnUpdateFees(double fee)
        {
            var oldfee = Fee;
            Fee = fee;
            var feeResult = SetFee(Fee);
            if (feeResult.IsFail)
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Msg_Sendfailure));
                Fee = oldfee;
                return;
            }
        }

        void OnAdd()
        {
            SendItems.Add(new SendItemInfo());
        }

        void OnClear()
        {
            SendItems.Clear();
            SendItems.Add(new SendItemInfo());
        }

        void OnSend(ItemsControl items)
        {
            var istrue = ValidateData(items);
            if (!istrue)
                return;

            var result = FiiiCoinService.Default.GetTxSettings();
            if (result.IsFail)
                return;
            if (result.Value.Encrypt)
            {
                CheckPwd();
            }
            else
            {
                JumpToConfirmPage();
            }
        }

        void OnCheckPwd(bool isTrue)
        {
            if (isTrue)
            {
                SendDataToService();
            }
            else
            {
                ShowMessage(LanguageService.Default.GetLanguageValue("Msg_Sendfailure"));
            }
        }

        void OnChoose(SendItemInfo sendItemInfo)
        {
            if (sendItemInfo == null)
                return;
            _currentSendItem = sendItemInfo;
            SendMessenger(Pages.PayUrlsPage, ShellPages.PayUrlPageType.Choose);
            UpdatePage(Pages.PayUrlsPage);
        }

        void OnPaste(SendItemInfo sendItemInfo)
        {
            if (sendItemInfo == null)
                return;
            sendItemInfo.Address = Clipboard.GetText();
        }

        void OnAllClick(SendItemInfo sendItemInfo)
        {
            if (sendItemInfo == null)
                return;
            var allamount = SendItems.Except(new List<SendItemInfo> { sendItemInfo }).Sum(x => x.Amount);
            var result = OverMoney - allamount;

            sendItemInfo.PayAmountStr = (result / Math.Pow(10, 8)).ToString("0.00000000");
        }

        void OnClearAddress(SendItemInfo sendItemInfo)
        {
            if (sendItemInfo == null)
                return;
            SendItems.Remove(sendItemInfo);
            if (!SendItems.Any())
                SendItems.Add(new SendItemInfo());
        }

        Result SetFee(double fee)
        {
            Result result = FiiiCoinService.Default.SetTxFee(fee);
            return result;
        }
        
        void LoadData()
        {
            AmountMonitor.Default.MonitorCallBack += data => {
                Application.Current.Dispatcher.Invoke(() => {
                    OverMoney = data.CanUseAmount;
                });
            };

            TxSettingMonitor.Default.MonitorCallBack += data => {
                Application.Current.Dispatcher.Invoke(() => {
                    Fee = data.FeePerKB / 100000000d;
                    TxSettingMonitor.Default.Stop();
                });
            };
        }

        protected override string GetPageName()
        {
            return Pages.SendPage;
        }
        
        void CheckPwd()
        {
            SendMsgData<InputWalletPwdPageTopic> data = new SendMsgData<InputWalletPwdPageTopic>
            {
                Token = InputWalletPwdPageTopic.UnLockWallet
            };
            data.SetCallBack(JumpToConfirmPage);

            SendMessenger(Pages.InputWalletPwdPage, SendMessageTopic.Refresh);
            SendMessenger(Pages.InputWalletPwdPage, data);
            UpdatePage(Pages.InputWalletPwdPage);
        }

        void JumpToConfirmPage()
        {
            var result = FiiiCoinService.Default.EstimateTxFeeForSendMany(Initializer.Default.DefaultAccount.Address, SendItems);
            if (result.IsFail)
            {
                ShowMessage(result.GetErrorMsg());
                return;
            }

            var feeValue = result.Value.TotalFee;

            SendMsgData<ConfirmSendData> data = new SendMsgData<ConfirmSendData>();
            var amountAll = SendItems.Sum(x => x.Amount);
            ConfirmSendData sendData = new ConfirmSendData
            {
                Amount = amountAll / Math.Pow(10, 8),
            };
            sendData.Fee = feeValue / Math.Pow(10, 8);

            var tags = SendItems.Select(x =>
            {
                if (string.IsNullOrEmpty(x.Tag.Trim()))
                    return x.Address;
                else
                    return x.Tag;
            });
            sendData.ToAddress = string.Join(";", SendItems.Select(x => x.Tag));

            if (!SendItems.Any(x => x.IsContainFee))
                sendData.ArrivalAmount = sendData.Amount;
            else
                sendData.ArrivalAmount = (amountAll - feeValue) / Math.Pow(10, 8);
            data.Token = sendData;
            data.SetCallBack(() =>
            {
                SendDataToService();
            });

            SendMessenger(Pages.ConfirmSendPage, data);
            UpdatePage(Pages.ConfirmSendPage);
        }


        void SendDataToService()
        {
            if (Initializer.Default.DefaultAccount == null)
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Msg_Sendfailure));
                return;
            }

            var address = Initializer.Default.DefaultAccount.Address;
            var result = FiiiCoinService.Default.SendMany(address, SendItems);
            
            if (!result.IsFail)
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Msg_Sendsuccess));
            }
            else
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Msg_Sendfailure));
            }

            LockWallet();

            SendMessenger(Pages.PayUrlsPage, SendMessageTopic.Refresh);
        }
        
        void LockWallet()
        {
            WalletService.Default.LockWallet();
        }

        RegexRule _regexRule = null;
        private const string _addressPattern = "(fiiim|fiiit)[0-9a-zA-Z]{33}";
        bool ValidateData(ItemsControl items)
        {
            if (!Validator.IsValid(items))
            {
                ShowMessage(LanguageService.Default.GetLanguageValue("Error_Amount"));
                return false;
            }

            if(_regexRule == null)
                _regexRule = new RegexRule();
            if (!SendItems.Any(x => _regexRule.MatchAll(x.Address, _addressPattern)))
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(ValidationType.Error_Address.ToString()));
                return false;
            }

            if (SendItems.Any(x => string.IsNullOrEmpty( x.PayAmountStr)))
            {
                ShowMessage(LanguageService.Default.GetLanguageValue("Error_Amount"));
                return false;
            }

            //if (SendItems.Any(x => x.PayAmount < Fee))
            //{
            //    ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Error_SendOverMinFee));
            //    return false;
            //}

            var allAmount = SendItems.Sum(x => x.Amount);
            if (allAmount > OverMoney)
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Error_SendOverMax));
                return false;
            }

            return true;
        }
    }
}