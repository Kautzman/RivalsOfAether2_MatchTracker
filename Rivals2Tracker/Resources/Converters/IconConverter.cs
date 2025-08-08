using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Rivals2Tracker.Resources.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            string charName = value.ToString();

            // Example: choose image based on string
            string imagePath = charName switch
            {
                "Forsburn" => "pack://application:,,,/Resources/CharacterIcons/yeen.png",
                "Loxodont" => "pack://application:,,,/Resources/CharacterIcons/loxodont.png",
                "Clairen" => "pack://application:,,,/Resources/CharacterIcons/clairen.png",
                "Zetterburn" => "pack://application:,,,/Resources/CharacterIcons/zetterburn.png",
                "Wrastor" => "pack://application:,,,/Resources/CharacterIcons/wrastor.png",
                "Fleet" => "pack://application:,,,/Resources/CharacterIcons/fleet.png",
                "Absa" => "pack://application:,,,/Resources/CharacterIcons/absa.png",
                "Olympia" => "pack://application:,,,/Resources/CharacterIcons/olympia.png",
                "Maypul" => "pack://application:,,,/Resources/CharacterIcons/maypul.png",
                "Kragg" => "pack://application:,,,/Resources/CharacterIcons/kragg.png",
                "Ranno" => "pack://application:,,,/Resources/CharacterIcons/ranno.png",
                "Orcane" => "pack://application:,,,/Resources/CharacterIcons/orcane.png",
                "Etalus" => "pack://application:,,,/Resources/CharacterIcons/etalus.png",
                _ => "pack://application:,,,/Resources/CharacterIcons/nothing.png"
            };

            return new BitmapImage(new Uri(imagePath));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Only needed if two-way binding
        }
    }
}
