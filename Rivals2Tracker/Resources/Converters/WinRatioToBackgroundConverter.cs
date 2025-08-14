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
            if (value is null)
            {
                return Brushes.Transparent;
            }

            string valueString = value?.ToString();
            valueString = new string(valueString.Take(5).ToArray());

            if (!Double.TryParse(valueString, out double doubleValue))
            {
                return Brushes.White;
            }

            doubleValue = doubleValue / 100;

            doubleValue = Math.Max(0.0, Math.Min(1.0, doubleValue));

            if (Math.Abs(doubleValue - 0.5) < 0.0001)
                return Brushes.LightGray;

            if (doubleValue < 0.5)
            {
                double t = (doubleValue - 0.39) / (0.5 - 0.37);
                t = Math.Max(0, Math.Min(1, t));
                return LerpBrush(Colors.OrangeRed, Colors.LightGray, t);
            }
            else
            {
                double t = (doubleValue - 0.5) / (0.61 - 0.5);
                t = Math.Max(0, Math.Min(1, t));
                return LerpBrush(Colors.LightGray, Colors.LightGreen, t);
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
