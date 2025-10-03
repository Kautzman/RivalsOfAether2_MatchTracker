using Slipstream.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Slipstream.Resources.Converters
{
    public class MatchStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return Visibility.Collapsed;
            }

            if (value is RivalsMatch match)
            {
                return match.Status == MatchStatus.InProgress ? Visibility.Visible : Visibility.Collapsed;               
            }
            return Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
