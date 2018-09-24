// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System.Globalization;
using System.Windows.Controls;

namespace FiiiCoin.Wallet.Win.ValidationRules
{
    public class DoubleRangeRule : RuleBase
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double v = 0L;
            if(value == null || string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(true, null);
            if (double.TryParse(value.ToString(),out v))
            {
                var d = 0d;
                if(double.TryParse(Min,out d) && v < d)
                    return new ValidationResult(false, base.GetErrorMsg());
                if (double.TryParse(Max, out d) && v > d)
                    return new ValidationResult(false, base.GetErrorMsg());
            }
            return new ValidationResult(true, null);
        }

        public string Min { get; set; }

        public string Max { get; set; }
    }
}
