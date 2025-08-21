using Rivals2Tracker.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Rivals2Tracker.Models
{
    class RivalsPlayer : BindableBase
    {
        public string Name { get; set; } = String.Empty;
        public string Character { get; set; } = String.Empty;
        public string Character2 { get; set; } = String.Empty;
        public string Character3 { get; set; } = String.Empty;
        public string EloString { get; set; } = String.Empty;

        private string _elo = String.Empty;
        public string Elo
        {
            get { return _elo; }
            set { SetProperty(ref _elo, value); }
        }


        public RivalsPlayer(string playerText, string eloText)
        { 
            EloString = eloText.Trim();
            ParseOcrResult(playerText.Trim());

            try
            {
                // The OCR area clips the rest of 'Unranked' so instead of trying to code around it... just get the 'anked' and call it a day! :P
                if (eloText == "UNRANKED" || eloText == "ANKED")
                {
                    Elo = "U";
                }
                else
                {
                    Elo = EloString;
                }
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

        public bool IsLocalPlayer()
        {
            return Name.ToLower() == GlobalData.MyName.ToLower();
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
