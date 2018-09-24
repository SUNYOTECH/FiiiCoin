// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using FiiiCoin.Wallet.Win.Common;
using System.Windows.Controls;

namespace FiiiCoin.Wallet.Win.ValidationRules
{
    public abstract class RuleBase : ValidationRule
    {
        public string ErrorKey { get; set; }

        public string GetErrorMsg()
        {
            return LanguageService.Default.GetLanguageValue(ErrorKey);
        }
    }
}
