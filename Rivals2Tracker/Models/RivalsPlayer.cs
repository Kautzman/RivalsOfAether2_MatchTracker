using Slipstream.Data;
using System;
using System.Diagnostics;
using System.Linq;
using Prism.Mvvm;

namespace Slipstream.Models
{
    public class RivalsPlayer : BindableBase
    {
        public string PlayerTag { get; set; } = String.Empty;
        public RivalsCharacter Character { get; set; }
        public RivalsCharacter Character2 { get; set; }
        public string EloString { get; set; } = String.Empty;

        private string _elo = String.Empty;
        public string Elo
        {
            get { return _elo; }
            set
            {
                SetProperty(ref _elo, value);
                EloString = value;
            }
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
                    PlayerTag = FormatName(string.Join(" ", split.Take(split.Length - 1)));
                }
                else
                {
                    PlayerTag = FormatName(split[0]);
                }
                
                Character.Name = FormatName(split[split.Length - 1]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to Parse Character and Name catastrophically.");
                PlayerTag = "";
                Character.Name = "";
            }
        }

        public bool IsLocalPlayer()
        {
            return PlayerTag.ToLower() == GlobalData.MyName.ToLower();
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
