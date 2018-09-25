// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Utility;
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Biz.Services;
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Common.Utils;
using FiiiCoin.Wallet.Win.Models;
using FiiiCoin.Wallet.Win.Models.CommonParams;
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
    public class PayUrlsViewModel : PopupShellBase
    {
        protected override string GetPageName()
        {
            return Pages.PayUrlsPage;
        }

        public ICommand BtnCommand { get; private set; }
        public ICommand MouseDubleClickCommand { get; private set; }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            BtnCommand = new RelayCommand<string>(OnCommand);
            MouseDubleClickCommand = new RelayCommand<UrlInfo>(OnMouseDubleClick);
            RegeistMessenger<UrlInfo>(OnRequestCreateUrl);
            RegeistMessenger<PayUrlPageType>(OnGetRequest);
            PayAddressBookMonitor.Default.MonitorCallBack += books => {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PayAddressBookMonitor.Default.Stop();
                    UrlInfos.Clear();
                    books.ForEach(x =>
                    {
                        UrlInfos.Add(new UrlInfo(x));
                    });
                });
            };
            LoadUrls();
            UrlInfos.CollectionChanged += (s, e) => { RaisePropertyChanged("UrlInfos"); };
        }
        
        void LoadUrls()
        {
            PayAddressBookMonitor.Default.Start(3000);
        }

        protected override void Refresh()
        {
            base.Refresh();
            LoadUrls();
        }
        
        private ObservableCollection<UrlInfo> _urlInfos;

        public ObservableCollection<UrlInfo> UrlInfos
        {
            get
            {
                if (_urlInfos == null)
                    _urlInfos = new ObservableCollection<UrlInfo>();
                return _urlInfos;
            }
            set
            {
                _urlInfos = value; RaisePropertyChanged("UrlInfos");
            }
        }

        private UrlInfo _selectedItem;

        public UrlInfo SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged("SelectedItem"); }
        }


        private PayUrlPageType _pageType;

        public PayUrlPageType PageType
        {
            get { return _pageType; }
            set { _pageType = value; RaisePropertyChanged("PageType"); }
        }


        void OnRequestCreateUrl(UrlInfo urlInfo)
        {
            if (urlInfo == null)
                return;
            var netstr = "mainnet";
            switch (NodeSetting.CurrentNetworkType)
            {
                case Biz.NetworkType.MainnetPort:
                    break;
                case Biz.NetworkType.TestNetPort:
                    netstr = "testnet";
                    break;
                default:
                    break;
            }

            if (!AddressTools.AddressVerfy(netstr, urlInfo.Address))
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Error_Address));
                return;
            }
            switch (urlInfo.Mode)
            {
                case UrlMode.CreatePay:
                    if (UrlInfos.Any(x => x.Address.Equals(urlInfo.Address)))
                    {
                        ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Address_Existed));
                        return;
                    }
                    var result = AddressBookService.Default.AddNewAddressBookItem(urlInfo.Address,urlInfo.Tag);
                    if (!result.IsFail)
                    {
                        //UrlInfos.Add(urlInfo);
                        LoadUrls();
                    }
                    else
                    {
                        ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Add_Fail));
                    }
                    break;
                case UrlMode.Edit:
                    if (UrlInfos.Any(x => x.Address.Equals(urlInfo.Address) && x.Id != urlInfo.Id))
                    {
                        ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Address_Existed));
                        return;
                    }
                    var updateResult = AddressBookService.Default.AddNewAddressBookItem(urlInfo.Address, urlInfo.Tag);
                    if (!updateResult.IsFail)
                    {
                        var urlinfo = UrlInfos.FirstOrDefault(x => x.Id == urlInfo.Id);
                        if (urlinfo == null)
                            UrlInfos.Add(urlInfo);
                        else
                        { 
                            urlinfo.Tag = urlInfo.Tag;
                            urlinfo.Address = urlInfo.Address;
                        }
                    }
                    else
                    {
                        ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Edit_Fail));
                    }
                    SelectedItem.Address = urlInfo.Address;
                    SelectedItem.Tag = urlInfo.Tag;
                    SelectedItem.Mode = urlInfo.Mode;
                    break;
                default:
                    break;
            }
        }

        private void OnGetRequest(PayUrlPageType pageType)
        {
            this.PageType = pageType;
        }

        private void OnCommand(string msg)
        {
            PayUrlPageMode mode;
            if (!Enum.TryParse(msg, out mode))
                return;

            switch (mode)
            {
                case PayUrlPageMode.CreateUrl:
                    OnCreate();
                    break;
                case PayUrlPageMode.CopyAddress:
                    if (SelectedItem != null)
                        ClipboardUtil.SetText(SelectedItem.Address);
                    break;
                case PayUrlPageMode.CopyLabel:
                    if (SelectedItem == null)
                        return;
                    if (string.IsNullOrEmpty(SelectedItem.Tag))
                        ShowMessage(LanguageService.Default.GetLanguageValue("Error_EmptyTag"));
                    else
                        ClipboardUtil.SetText(SelectedItem.Tag);
                    break;
                case PayUrlPageMode.Delete:
                    OnDelete();
                    break;
                case PayUrlPageMode.Edit:
                    OnEdit();
                    break;
                case PayUrlPageMode.Choose:
                    OnChoose();
                    break;
                case PayUrlPageMode.Export:
                    OnExport();
                    break;
                default:
                    break;
            }
        }

        private void OnMouseDubleClick(UrlInfo selectitem)
        {
            if (selectitem != null)
            {
                SendMsgData<TitleWithParams<string>> sendMsgData = new SendMsgData<TitleWithParams<string>>();
                sendMsgData.Token = new TitleWithParams<string> {  Params = selectitem.Address ,Title = "PayAddress" };
                sendMsgData.SetCallBack(() =>
                {
                    UpdatePage(Pages.PayUrlsPage);
                });
                SendMessenger(Pages.ImagePage, sendMsgData);
                UpdatePage(Pages.ImagePage);
            }
        }

        void OnExport()
        {
            if (UrlInfos == null || !UrlInfos.Any())
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV（*.csv）|*.csv";
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

        void OnCreate()
        {
            UrlInfo newinfo = new UrlInfo();
            newinfo.Mode = UrlMode.CreatePay;
            SendMessenger(Pages.CreatePayUrlPage, newinfo);
            UpdatePage(Pages.CreatePayUrlPage);
        }

        void OnDelete()
        {
            var removeItem = SelectedItem;
            if (SelectedItem == null) removeItem = UrlInfos.FirstOrDefault() ;

            var result = AddressBookService.Default.GetAddressBookItemByAddress(removeItem.Address);
            if (result.IsFail)
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Delete_Fail));
                return;
            }

            var deleteResult = AddressBookService.Default.DeleteAddressBookByIds(result.Value.Id);
            if (deleteResult.IsFail)
            {
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Delete_Fail));
                return;
            }

            UrlInfos.Remove(removeItem);
        }

        void OnEdit()
        {
            if (SelectedItem == null) return;
            SelectedItem.Mode = UrlMode.Edit;
            SendMessenger(Pages.CreatePayUrlPage, SelectedItem);
            UpdatePage(Pages.CreatePayUrlPage);
        }

        void OnChoose()
        {
            SendMessenger<UrlInfo>(Pages.SendPage, SelectedItem);
            base.OnClosePopup();
        }

        public override void OnClosePopup()
        {
            base.OnClosePopup();
            PageType = PayUrlPageType.Edit;
        }
    }


    public enum PayUrlPageType
    {
        Edit,
        Choose
    }

    enum PayUrlPageMode
    {
        CreateUrl,
        CopyAddress,
        CopyLabel,
        Delete,
        Edit,
        Choose,
        Export
    }

}