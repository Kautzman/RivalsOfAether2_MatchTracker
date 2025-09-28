using System;
using System.Globalization;
using System.Windows.Data;

namespace Rivals2Tracker.Resources.Converters
{
    public class WidthToRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && parameter is string range)
            {
                switch (range)
                {
                    case "Small":
                        return width <= 2160;

                    case "Medium":
                        return width >= 2161 && width < 2880;

                    case "Large":
                        return width >= 2881;

                    default:
                        return false;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
