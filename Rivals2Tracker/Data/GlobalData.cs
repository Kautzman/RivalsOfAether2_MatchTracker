using Rivals2Tracker.HotkeyHandler;
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
        public static IntPtr MainWindowHandle { get; set; }
        public static string CurrentSeason = "1.3";
        public static string MyName = "Kadecgos";
        public static bool IsSaveCaptures = false;
        public static bool IsPlayAudio = true;
        public static uint HotKeyCode = 0;
        public static uint ModifierCode = 0;

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

        // TODO:  Update resources to point to portraits, not icons
        public static Dictionary<string, string> CharacterPortraitDict = new Dictionary<string, string>
        {
            { "Absa", "/ImageResources/Portraits/absa_portrait.png" },
            { "Clairen", "/ImageResources/Portraits/clairen_portrait.png" },
            { "Etalus", "/ImageResources/Portraits/etalus_portrait.png" },
            { "Fleet", "/ImageResources/Portraits/fleet_portrait.png" },
            { "Kragg",  "/ImageResources/Portraits/kragg_portrait.png" },
            { "Loxodont", "/ImageResources/Portraits/loxodont_portrait.png" },
            { "Maypul", "/ImageResources/Portraits/maypul_portrait.png" },
            { "Olympia", "/ImageResources/Portraits/olympia_portrait.png" },
            { "Orcane", "/ImageResources/Portraits/orcane_portrait.png" },
            { "Ranno", "/ImageResources/Portraits/ranno_portrait.png" },
            { "Wrastor", "/ImageResources/Portraits/wrastor_portrait.png" },
            { "Forsburn", "/ImageResources/Portraits/forsburn_portrait.png" },
            { "Zetterburn", "/ImageResources/Portraits/zetterburn_portrait.png" }
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

    public enum MatchHistoryView
    {
        Rivals,
        Players
    }

    public enum PlayerInformationView
    {
        Notes,
        Matches
    }
}
