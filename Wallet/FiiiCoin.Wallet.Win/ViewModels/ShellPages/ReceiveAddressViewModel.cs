// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Biz.Services;
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Common.Utils;
using FiiiCoin.Wallet.Win.Models;
using FiiiCoin.Wallet.Win.Models.UiModels;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace FiiiCoin.Wallet.Win.ViewModels.ShellPages
{
    public class ReceiveAddressViewModel : PopupShellBase
    {
        protected override string GetPageName()
        {
            return Pages.ReceiveAddressPage;
        }



        public ICommand BtnCommand { get; private set; }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            BtnCommand = new RelayCommand<string>(OnCommand);
            RegeistMessenger<UrlInfo>(OnRequestCreateUrl);

            LoadUrls();
            UrlInfos.CollectionChanged += (s, e) => { RaisePropertyChanged("UrlInfos"); };
        }

        void LoadUrls()
        {
            ReceiveAddressBookMonitor.Default.MonitorCallBack += accounts =>
            {
                ReceiveAddressBookMonitor.Default.Stop();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UrlInfos.Clear();
                    accounts.ForEach(x => UrlInfos.Add(new UrlInfo() { Address = x.Address, Tag = x.Tag }));
                });
            };
        }

        private ObservableCollection<UrlInfo> urlInfos;

        public ObservableCollection<UrlInfo> UrlInfos
        {
            get
            {
                if (urlInfos == null)
                    urlInfos = new ObservableCollection<UrlInfo>();
                return urlInfos;
            }
            set
            {
                urlInfos = value; RaisePropertyChanged("UrlInfos");
            }
        }

        private UrlInfo _selectedItem;

        public UrlInfo SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged("SelectedItem"); }
        }

        void OnRequestCreateUrl(UrlInfo urlInfo)
        {
            if (urlInfo == null)
                return;
            switch (urlInfo.Mode)
            {
                case UrlMode.CreateByReceive:
                    if (!UrlInfos.Any(x => x.Address == urlInfo.Address))
                    {
                        UrlInfos.Add(urlInfo);
                    }
                    break;
                case UrlMode.EditByReceive:
                    var result = AccountsService.Default.SetAccountTag(urlInfo.Address, urlInfo.Tag);
                    ReceiveAddressBookMonitor.Default.Start(3000);
                    break;
                default:
                    break;
            }
        }

        private void OnCommand(string msg)
        {
            ReceiveUrlPageMode mode;
            if (!Enum.TryParse(msg, out mode))
                return;

            switch (mode)
            {
                case ReceiveUrlPageMode.CreateUrl:
                    OnCreate();
                    break;
                case ReceiveUrlPageMode.CopyAddress:
                    if (SelectedItem != null)
                        ClipboardUtil.SetText(SelectedItem.Address);
                    break;
                case ReceiveUrlPageMode.CopyLabel:
                    if (SelectedItem == null)
                        return;
                    if (string.IsNullOrEmpty(SelectedItem.Tag))
                        ShowMessage(LanguageService.Default.GetLanguageValue("Error_EmptyTag"));
                    else
                        ClipboardUtil.SetText(SelectedItem.Tag);
                    break;
                case ReceiveUrlPageMode.Edit:
                    OnEdit();
                    break;
                case ReceiveUrlPageMode.Export:
                    Export();
                    break;
                default:
                    break;
            }
        }

        void OnCreate()
        {
            var txsetting = FiiiCoinService.Default.GetTxSettings();
            if (txsetting.IsFail)
                return;

            if (txsetting.Value.Encrypt)
            {
                UnlockWallet();
            }
            else
            {
                CreateUrl();
            }
        }

        void OnEdit()
        {
            if (SelectedItem == null) return;
            SelectedItem.Mode = UrlMode.EditByReceive;
            SendMessenger(Pages.CreatePayUrlPage, SelectedItem);
            UpdatePage(Pages.CreatePayUrlPage);
        }

        void UnlockWallet()
        {
            SendMsgData<InputWalletPwdPageTopic> sendMsgData = new SendMsgData<InputWalletPwdPageTopic>();
            sendMsgData.Token = InputWalletPwdPageTopic.UnLockWallet;
            sendMsgData.SetCallBack(CreateUrl);
            SendMessenger(Pages.InputWalletPwdPage, SendMessageTopic.Refresh);
            SendMessenger(Pages.InputWalletPwdPage, sendMsgData);
            UpdatePage(Pages.InputWalletPwdPage);
        }

        void CreateUrl()
        {
            UrlInfo newinfo = new UrlInfo();
            newinfo.Mode = UrlMode.CreateByReceive;
            SendMessenger(Pages.CreatePayUrlPage, newinfo);
            UpdatePage(Pages.CreatePayUrlPage);
        }

        protected override void Refresh()
        {
            base.Refresh();
            LoadUrls();
        }


        void Export()
        {
            if (UrlInfos == null || !UrlInfos.Any())
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV£¨*.csv£©|*.csv";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            var result = saveFileDialog.ShowDialog(BootStrapService.Default.Shell.GetWindow());
            if (result.HasValue && result.Value)
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    var header = string.Format("{0},{1}", LanguageService.Default.GetLanguageValue("Tag"), LanguageService.Default.GetLanguageValue("Address"));
                    stringBuilder.AppendLine(header);
                    UrlInfos.ToList().ForEach(x =>
                    {
                        var newline = string.Format("{0},{1}", x.Tag, x.Address);
                        stringBuilder.AppendLine(newline);
                    });


                    var file = saveFileDialog.FileName;
                    using (Stream stream = File.OpenWrite(file))
                    {
                        using (var writer = new StreamWriter(stream, Encoding.Unicode))
                        {
                            var data = stringBuilder.ToString().Replace(",", "\t");
                            writer.Write(data);
                            writer.Close();
                        }
                        stream.Close();
                    }
                    ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Export_Sucesses));
                }
                catch
                {
                    ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Export_Fail));
                }
            }
        }
    }


    enum ReceiveUrlPageMode
    {
        CreateUrl,
        CopyAddress,
        CopyLabel,
        Edit,
        Export
    }
}
