// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Common;
using FiiiCoin.Wallet.Win.Models.UiModels;

namespace FiiiCoin.Wallet.Win.ViewModels.ShellPages
{
    public class MessageViewModel : PopupShellBase
    {
        protected override string GetPageName()
        {
            return Pages.MessagePage;
        }

        private MessagePageData _messagePageData;

        public MessagePageData MessagePageData
        {
            get { return _messagePageData; }
            set { _messagePageData = value; RaisePropertyChanged("MessagePageData"); }
        }

        protected override void OnLoaded()
        {
            MessagePageData = new MessagePageData();
            base.OnLoaded();
            RegeistMessenger<MessagePageData>(OnRequestMsg);
        }

        protected override void Refresh()
        {
            MessagePageData = new MessagePageData();
        }

        void OnRequestMsg(MessagePageData messagePageData)
        {
            MessagePageData = messagePageData;
        }

        public override void OnOkClick()
        {
            base.OnOkClick();
            MessagePageData.InvokeOkCallBack();
        }

        public override void OnCancelClick()
        {
            base.OnCancelClick();
            MessagePageData.InvokeCancelCallBack();
        }
    }
}