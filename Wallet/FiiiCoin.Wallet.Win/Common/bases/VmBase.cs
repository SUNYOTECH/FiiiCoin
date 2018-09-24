// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Biz;
using FiiiCoin.Wallet.Win.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FiiiCoin.Wallet.Win.Common
{
    public abstract class VmBase : ViewModelBase
    {
        public VmBase()
        {
            OnLoaded();
        }

        private string PageName
        {
            get
            {
                return GetPageName();
            }
        }

        protected void ShowMessage(string msg)
        {
            Initializer.Default.ShowMessage(msg);
        }

        protected void Send(SendMessageTopic msg, string pageName)
        {
            Messenger.Default.Send(msg, pageName);
        }

        public virtual void OnClosePopup()
        {
            Messenger.Default.Send(false, MessageTopic.ChangedPopupViewState);
        }

        private void GetSendMsg(SendMessageTopic msg)
        {
            switch (msg)
            {
                case SendMessageTopic.Refresh:
                    Refresh();
                    break;
                default:
                    break;
            }
        }

        protected void UpdatePage(string pageName, PageModel pageModel = PageModel.DialogPage)
        {
            switch (pageModel)
            {
                case PageModel.MainPage:
                    Messenger.Default.Send(pageName, MessageTopic.UpdateMainView);
                    break;
                case PageModel.DialogPage:
                    Messenger.Default.Send(pageName, MessageTopic.UpdatePopupView);
                    Messenger.Default.Send(true, MessageTopic.ChangedPopupViewState);
                    break;
                case PageModel.TabPage:
                    Messenger.Default.Send(pageName, Pages.MainPage);
                    break;
                case PageModel.MessagePage:
                    break;
                default:
                    break;
            }
        }


        protected virtual void Refresh()
        {

        }

        protected virtual string GetPageName() { return null; }

        protected virtual void OnLoaded()
        {
            RegeistMessenger<SendMessageTopic>(GetSendMsg);
        }

        protected void SendMessenger<T>(string pageName, T param)
        {
            Messenger.Default.Send(param, pageName);
        }

        protected void RegeistMessenger<T>(Action<T> action)
        {
            var pageName = GetPageName();
            if (string.IsNullOrEmpty(pageName))
                return;
            Messenger.Default.Register<T>(this, GetPageName(), action);
        }
    }

    public enum PageModel
    {
        MainPage,
        DialogPage,
        TabPage,
        MessagePage
    }

}
