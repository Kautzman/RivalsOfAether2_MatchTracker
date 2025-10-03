using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Slipstream.Resources.Converters
{
    public class SelectedToStrokeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 4.0 : 4.0;
            }
            return 4.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }    
    }
}
