// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Common;

namespace FiiiCoin.Wallet.Win.ViewModels.ShellPages
{
    public class LanguageViewModel : PopupShellBase
    {
        protected override string GetPageName()
        {
            return Pages.LanguagePage;
        }

        private int _selectedIndex = 0;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; RaisePropertyChanged("SelectedIndex"); }
        }

        void OnSelectionChanged()
        {
            var langaugeType = (LanguageType)SelectedIndex;
            LanguageService.Default.SetLanguage(langaugeType);
        }

        public override void OnOkClick()
        {
            var langaugeType = (LanguageType)SelectedIndex;
            if (LanguageService.Default.LanguageType != langaugeType)
            {
                OnSelectionChanged();
                AppSettingConfig.Default.SwitchLanguage();
                AppSettingConfig.Default.SaveLanguageType(langaugeType);
            }
            base.OnOkClick();
        }

        protected override void OnLoaded()
        {
            SelectedIndex = (int)LanguageService.Default.LanguageType;
            base.OnLoaded();
        }
    }
}