using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Rivals2Tracker.Data
{
    public static class GlobalData
    {
        public static string CurrentSeason = "1.3";

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
