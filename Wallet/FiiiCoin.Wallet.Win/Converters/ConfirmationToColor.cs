// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FiiiCoin.Wallet.Win.Converters
{
    public class ConfirmationToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var i = 0;
            if (int.TryParse(value.ToString(),out i))
            {
                if (i <= 0)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B8EFF"));
                if (i >= 6)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B8EFF"));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
