using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            try
            {

                Debug.WriteLine("Parsed Text: " + text);
                string[] split = text.Split(' ');

                if (split.Length > 2)
                {
                    Name = FormatName(string.Join(" ", split.Take(split.Length - 1)));
                }
                else
                {
                    Name = FormatName(split[0]);
                }
                
                Character = FormatName(split[split.Length - 1]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to Parse Character and Name catastrophically.");
                Name = "";
                Character = "";
            }
        }

        public bool IsKadecgos()
        {
            return Name == "Kadecgos";
        }

        public string FormatName(string name)
        {
            return char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }

        public void FormatManualData()
        {
            EloString = Elo.ToString();
        }
    } 
}
