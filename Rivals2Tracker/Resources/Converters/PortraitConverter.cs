using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Slipstream.Resources.Converters
{
    public  class PortraitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            string charName = value.ToString();

            return charName switch
            {
                "Forsburn" => "pack://application:,,,/ImageResources/Portraits/forsburn_portrait.png",
                "Loxodont" => "pack://application:,,,/ImageResources/Portraits/loxodont_portrait.png",
                "Clairen" => "pack://application:,,,/ImageResources/Portraits/clairen_portrait.png",
                "Zetterburn" => "pack://application:,,,/ImageResources/Portraits/zetterburn_portrait.png",
                "Wrastor" => "pack://application:,,,/ImageResources/Portraits/wrastor_portrait.png",
                "Fleet" => "pack://application:,,,/ImageResources/Portraits/fleet_portrait.png",
                "Absa" => "pack://application:,,,/ImageResources/Portraits/absa_portrait.png",
                "Olympia" => "pack://application:,,,/ImageResources/Portraits/olympia_portrait.png",
                "Maypul" => "pack://application:,,,/ImageResources/Portraits/maypul_portrait.png",
                "Kragg" => "pack://application:,,,/ImageResources/Portraits/kragg_portrait.png",
                "Ranno" => "pack://application:,,,/ImageResources/Portraits/ranno_portrait.png",
                "Orcane" => "pack://application:,,,/ImageResources/Portraits/orcane_portrait.png",
                "Etalus" => "pack://application:,,,/ImageResources/Portraits/etalus_portrait.png",
                _ => "pack://application:,,,/Resources/CharacterIcons/unknown.png"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Only needed if two-way binding
        }
    }
}
