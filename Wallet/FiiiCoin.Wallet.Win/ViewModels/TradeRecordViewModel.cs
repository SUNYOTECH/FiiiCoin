// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz.Monitor;
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Common.Utils;
using FiiiCoin.Wallet.Win.Converters;
using FiiiCoin.Wallet.Win.Models;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FiiiCoin.Wallet.Win.ViewModels
{
    public class TradeRecordViewModel : VmBase
    {

        private int _timeSelectIndex = 0;
        private int _tradeSelectIndex = 0;
        private string _searchText;
        private string _searchAmount;
        private double _searchAmountValue;
        private bool _isShowTimeRange = false;
        private ObservableCollection<TradeRecordInfo> _tradeRecords;


        public int TimeSelectIndex
        {
            get { return _timeSelectIndex; }
            set
            {
                _timeSelectIndex = value;
                OnTimeSelectIndexChanged(value);
                Search();
                RaisePropertyChanged("TimeSelectIndex");
            }
        }

        public int TradeSelectIndex
        {
            get { return _tradeSelectIndex; }
            set
            {
                _tradeSelectIndex = value;
                Search();
                RaisePropertyChanged("TradeSelectIndex");
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                Search();
                RaisePropertyChanged("SearchText");
            }
        }

        public string SearchAmount
        {
            get { return _searchAmount; }
            set
            {
                if (_searchAmount == value)
                {
                    RaisePropertyChanged("SearchAmount");
                    return;
                }
                _searchAmount = value;
                if (!string.IsNullOrEmpty(value))
                    _searchAmountValue = double.Parse(value);
                else
                    _searchAmountValue = 0;
                Search();
                RaisePropertyChanged("SearchAmount");
            }
        }

        public bool IsShowTimeRange
        {
            get { return _isShowTimeRange; }
            set
            {
                _isShowTimeRange = value;
                RaisePropertyChanged("IsShowTimeRange");
            }
        }

        public ObservableCollection<TradeRecordInfo> TradeRecords
        {
            get
            {
                if (_tradeRecords == null)
                    _tradeRecords = new ObservableCollection<TradeRecordInfo>();
                return _tradeRecords;
            }
            set
            {
                _tradeRecords = value;
                RaisePropertyChanged("TradeRecords");
            }
        }

        private ObservableCollection<TradeRecordInfo> AllTradeRecords { get; set; } = new ObservableCollection<TradeRecordInfo>();


        public ICommand MouseDubleClickCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand CopyUriCommand { get; private set; }

        private DateTime _startDate;

        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; Search(); RaisePropertyChanged("StartDate"); }
        }

        private DateTime _endDate;

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; Search(); RaisePropertyChanged("EndDate"); }
        }

        private TradeRecordInfo _selectedItem;

        public TradeRecordInfo SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        private void OnTimeSelectIndexChanged(int index)
        {
            if (index == 6)
                IsShowTimeRange = true;
            else
                IsShowTimeRange = false;
        }

        private void OnMouseDubleClick(TradeRecordInfo tradeRecordInfo)
        {
            if (tradeRecordInfo == null)
                return;
            switch (tradeRecordInfo.Payment.Category)
            {
                case "self":
                case "send":
                    SendMessenger(Pages.TradeDetailSendPage, tradeRecordInfo);
                    UpdatePage(Pages.TradeDetailSendPage);
                    break;
                case "receive":
                    SendMessenger(Pages.TradeDetailReceivePage, tradeRecordInfo);
                    UpdatePage(Pages.TradeDetailReceivePage);
                    break;

                case "generate":
                    SendMessenger(Pages.TradeDetailMiningPage, tradeRecordInfo);
                    UpdatePage(Pages.TradeDetailMiningPage);
                    break;
                default:
                    break;
            }
        }

        protected override void OnLoaded()
        {
            StartDate = DateTime.Now.AddYears(-10);
            EndDate = DateTime.Now;
            base.OnLoaded();
            UpdateData();
            MouseDubleClickCommand = new RelayCommand<TradeRecordInfo>(OnMouseDubleClick);
            ExportCommand = new RelayCommand<DataGrid>(OnExport);
            CopyUriCommand = new RelayCommand<string>(OnCopyUri);
        }

        protected void OnCopyUri(string id)
        {
            if (string.IsNullOrEmpty(id) || SelectedItem == null)
                return;

            if (id == "address")
                ClipboardUtil.SetText(SelectedItem.Payment.Address);
            else if (id == "amount")
                ClipboardUtil.SetText((SelectedItem.Payment.Amount / Math.Pow(10, 8)).ToString("0.00000000"));
            else if (id == "label")
            {
                PaymentToMarkConverter converter = new PaymentToMarkConverter();
                var mark = converter.Convert(SelectedItem.Payment, typeof(object), null, new System.Globalization.CultureInfo(1033));
                if (mark != null)
                    ClipboardUtil.SetText(mark.ToString());
            }
            else if (id == "txid")
                ClipboardUtil.SetText(SelectedItem.Payment.TxId);
        }

        protected override string GetPageName()
        {
            return Pages.TradeRecordPage;
        }

        protected override void Refresh()
        {
            base.Refresh();

            TimeSelectIndex = 0;
            TradeSelectIndex = 0;
            SearchText = null;
            SearchAmount = "";
        }

        void OnExport(DataGrid dataGrid1)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV（*.csv）|*.csv";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            var result = saveFileDialog.ShowDialog(BootStrapService.Default.Shell.GetWindow());
            if (result.HasValue && result.Value)
            {
                var content = AllTradeRecords.GetCsvContent();
                var file = saveFileDialog.FileName;
                using (Stream stream = File.OpenWrite(file))
                {
                    using (var writer = new StreamWriter(stream, System.Text.Encoding.Unicode))
                    {
                        var data = content.Replace(",", "\t");
                        writer.Write(data);
                        writer.Close();
                    }
                    stream.Close();
                }
                ShowMessage(LanguageService.Default.GetLanguageValue(MessageKeys.Export_Sucesses));
            }
        }

        void UpdateData()
        {
            TradeRecodesMonitor.Default.MonitorCallBack += tradeRecords =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllTradeRecords.Clear();
                    tradeRecords.ToList().ForEach(x => AllTradeRecords.Add(x));
                    Search();
                });
            };
        }


        void Search()
        {
            var condition1 = GetCondtionByTime();
            var condition2 = GetCondtionByType();
            var condition3 = GetCondtionByAccount();
            var condition4 = GetCondtionByAmount();

            IEnumerable<TradeRecordInfo> result = AllTradeRecords;
            if (condition1 != null)
                result = condition1.Invoke(result);
            if (condition2 != null)
                result = condition2.Invoke(result);
            if (condition3 != null)
                result = condition3.Invoke(result);
            if (condition4 != null)
                result = condition4.Invoke(result);
            if (result == null)
                return;
            TradeRecords = new ObservableCollection<TradeRecordInfo>();
            result.ToList().ForEach(x =>
            {
                TradeRecords.Add(x);
            });
        }

        Func<IEnumerable<TradeRecordInfo>, IEnumerable<TradeRecordInfo>> GetCondtionByTime()
        {
            if (TimeSelectIndex == 0)
            {
                //all
                return (IEnumerable<TradeRecordInfo> x) => { return x; };
            }
            if (TimeSelectIndex == 1)
            {
                //today
                return (IEnumerable<TradeRecordInfo> x) => { return x.Where(p => p.TradeTime.ToString("yyyyMMdd").Equals(DateTime.Now.ToString("yyyyMMdd"))); };
            }
            if (TimeSelectIndex == 2)
            {
                //thisweek
                return (IEnumerable<TradeRecordInfo> x) =>
                {
                    return x.Where(p =>
                    {
                        DateTime temp1 = p.TradeTime.AddDays(-(int)p.TradeTime.DayOfWeek).Date;
                        DateTime temp2 = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).Date;
                        return temp1 == temp2;
                    });
                };
            }
            if (TimeSelectIndex == 3)
            {
                //thisMon
                return (IEnumerable<TradeRecordInfo> x) =>
                {
                    return x.Where(p => p.TradeTime.Year == DateTime.Now.Year && p.TradeTime.Month == DateTime.Now.Month);
                };
            }
            if (TimeSelectIndex == 4)
            {
                //prevMon
                return (IEnumerable<TradeRecordInfo> x) =>
                {
                    if (DateTime.Now.Month == 1)
                        return x.Where(p => p.TradeTime.Year == (DateTime.Now.Year - 1) && p.TradeTime.Month == 12);
                    else
                        return x.Where(p => p.TradeTime.Year == DateTime.Now.Year && (DateTime.Now.Month - p.TradeTime.Month) == -1);
                };
            }
            if (TimeSelectIndex == 5)
            {
                //thisYear
                return (IEnumerable<TradeRecordInfo> x) =>
                {
                    return x.Where(p => p.TradeTime.Year == DateTime.Now.Year);
                };
            }
            if (TimeSelectIndex == 6)
            {
                //other
                return (IEnumerable<TradeRecordInfo> x) =>
                {
                    return x.Where(p => p.TradeTime >= StartDate && p.TradeTime <= EndDate.AddDays(1));
                };
            }
            return null;
        }

        Func<IEnumerable<TradeRecordInfo>, IEnumerable<TradeRecordInfo>> GetCondtionByType()
        {
            if (TradeSelectIndex == 0)
            {
                //all
                return (IEnumerable<TradeRecordInfo> x) => { return x; };
            }
            if (TradeSelectIndex == 1)
            {
                //receive
                return (IEnumerable<TradeRecordInfo> x) => { return x.Where(p => p.Payment.Category == "receive"); };
            }
            if (TradeSelectIndex == 2)
            {
                //generate
                return (IEnumerable<TradeRecordInfo> x) => { return x.Where(p => p.Payment.Category == "send"); };
            }
            if (TradeSelectIndex == 3)
            {
                //pay
                return (IEnumerable<TradeRecordInfo> x) => { return x.Where(p => p.Payment.Category == "self"); };
            }
            if (TradeSelectIndex == 4)
            {
                //self
                return (IEnumerable<TradeRecordInfo> x) => { return x.Where(p => p.Payment.Category == "generate"); };
            }
            if (TradeSelectIndex == 5)
            {
                //send
                //return (IEnumerable<TradeRecordInfo> x) => { return x.Where(p => p.Payment.Category == "other"); };
            }
            return null;
        }

        Func<IEnumerable<TradeRecordInfo>, IEnumerable<TradeRecordInfo>> GetCondtionByAccount()
        {
            if (!string.IsNullOrEmpty(SearchText) && SearchText.Trim().Length != 0)
            {
                //all
                return (IEnumerable<TradeRecordInfo> x) => { return x.Where(p => p.Payment.Address != null && p.Payment.Address == SearchText.Trim()); };
            }
            return null;
        }

        Func<IEnumerable<TradeRecordInfo>, IEnumerable<TradeRecordInfo>> GetCondtionByAmount()
        {
            if (!string.IsNullOrEmpty(SearchAmount))
            {
                var amount = _searchAmountValue * Math.Pow(10, 8);
                //all
                return (IEnumerable<TradeRecordInfo> x) => { return x.Where(p => p.Payment.Amount >= amount); };
            }
            return null;
        }
    }
}
