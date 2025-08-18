using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Streaming.Adaptive;

namespace Rivals2Tracker.Data
{
    public static class GlobalData
    {
        public static string CurrentSeason = "1.3";
        public static string MyName = "Kadecgos";

        public static List<string> AllCharacters = new List<string>
        {
            "Absa",
            "Clairen",
            "Etalus",
            "Fleet",
            "Kragg",
            "Loxodont",
            "Maypul",
            "Olympia",
            "Orcane",
            "Ranno",
            "Wrastor",
            "Forsburn",
            "Zetterburn",
            "Unknown"
        };

        public static Dictionary<string, string> CharacterImageDict = new Dictionary<string, string>
        {
            { "Absa", "/Resources/CharacterIcons/absa.png" },
            { "Clairen", "/Resources/CharacterIcons/clairen.png" },
            { "Etalus", "/Resources/CharacterIcons/etalus.png" },
            { "Fleet", "/Resources/CharacterIcons/fleet.png" },
            { "Kragg",  "/Resources/CharacterIcons/kragg.png" },
            { "Loxodont", "/Resources/CharacterIcons/loxodont.png" },
            { "Maypul", "/Resources/CharacterIcons/maypul.png" },
            { "Olympia", "/Resources/CharacterIcons/olympia.png" },
            { "Orcane", "/Resources/CharacterIcons/orcane.png" },
            { "Ranno", "/Resources/CharacterIcons/ranno.png" },
            { "Wrastor", "/Resources/CharacterIcons/wrastor.png" },
            { "Forsburn", "/Resources/CharacterIcons/yeen.png" },
            { "Zetterburn", "/Resources/CharacterIcons/zetterburn.png" }
        };

        public static bool IsCurrentSeason(string patch)
        {
            if (patch == "all")
            {
                return true;
            }

            if (patch.Substring(0, 3) == CurrentSeason)
            {
                return true;
            }

            return false;
        }
    }
}
