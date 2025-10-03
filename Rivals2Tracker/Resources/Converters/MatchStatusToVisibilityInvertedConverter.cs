using Slipstream.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Slipstream.Resources.Converters
{
    public class MatchStatusToVisibilityInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return Visibility.Visible;
            }

            if (value is RivalsMatch match)
            {
                return match.Status == MatchStatus.InProgress ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
