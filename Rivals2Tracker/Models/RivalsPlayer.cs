using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rivals2Tracker.Models
{
    class RivalsPlayer
    {
        public string Name { get; set; } = String.Empty;
        public string Character { get; set; } = String.Empty;
        public string Character2 { get; set; } = String.Empty;
        public string Character3 { get; set; } = String.Empty;
        public string EloString { get; set; } = String.Empty;
        public int Elo { get; set; } = -1;


        public RivalsPlayer(string playerText, string eloText)
        { 
            EloString = eloText.Trim();
            ParseOcrResult(playerText.Trim());

            try
            {
                Elo = Convert.ToInt32(EloString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting {EloString} to an int");
            }
        }

        public void ParseOcrResult(string text)
        {
            string[] split = text.Split(' ');

            Name = FormatName(split[0]);
            Character = FormatName(split[1]);
        }

        public bool IsKadecgos()
        {
            return Name == "Kadecgos";
        }

        public string FormatName(string name)
        {
            return char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }
    } 
}
