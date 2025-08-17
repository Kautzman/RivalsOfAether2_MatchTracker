using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Rivals2Tracker.Resources.Converters
{
    class WinLoseColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string stringValue = value.ToString();

            if (string.IsNullOrEmpty(stringValue))
            {
                return "#888888";
            }

            if (stringValue == "Win")
            {
                return "#85FFBC";
            }

            return "#FF858C";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
