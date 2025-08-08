using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Rivals2Tracker.Resources.Converters
{
    class WinRatioToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is double d))
                return Brushes.Transparent;

            d = Math.Max(0.0, Math.Min(1.0, d));

            if (Math.Abs(d - 0.5) < 0.0001)
                return Brushes.Gray;

            if (d < 0.5)
            {
                double t = (d - 0.4) / (0.5 - 0.4);
                t = Math.Max(0, Math.Min(1, t));
                return LerpBrush(Colors.Red, Colors.Gray, t);
            }
            else
            {
                double t = (d - 0.5) / (0.6 - 0.5);
                t = Math.Max(0, Math.Min(1, t));
                return LerpBrush(Colors.Gray, Colors.Green, t);
            }
        }

        private SolidColorBrush LerpBrush(Color from, Color to, double t)
        {
            byte r = (byte)(from.R + (to.R - from.R) * t);
            byte g = (byte)(from.G + (to.G - from.G) * t);
            byte b = (byte)(from.B + (to.B - from.B) * t);
            byte a = (byte)(from.A + (to.A - from.A) * t);
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
