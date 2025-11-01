using Slipstream.Data;
using System;
using System.Diagnostics;
using System.Linq;
using Prism.Mvvm;
using System.Windows.Forms;

namespace Slipstream.Models
{
    public class RivalsPlayer : BindableBase
    {
        public string PlayerTag { get; set; } = String.Empty;
        public RivalsCharacter Character { get; set; } = new();
        public RivalsCharacter Character2 { get; set; } = new();
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

        public RivalsPlayer(string playerChar, string playerTag, string playerElo)
        {
            ParseElo(playerElo.Trim());
            ParseTag(playerTag.Trim());
            ParseChar(playerChar.Trim());
        }

        private void ParseElo(string text)
        {
            try
            {
                // The OCR area clips the rest of 'Unranked' so instead of trying to code around it... just get the 'anked' and call it a day! :P
                if (text == "UNRANKED" || text == "ANKED" || text == "ANKEO")
                {
                    Elo = "U";
                }
                else
                {
                    Elo = text;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting {EloString} to an int");
            }
        }

        private void ParseTag(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                PlayerTag = GlobalData.CapitalizeFirstLetter(text);
            }
            else
            {
                PlayerTag = "UNKNOWN";
            }
        }

        private void ParseChar(string text)
        {
            try
            {
                Debug.WriteLine("Parsed Char Text: " + text);
                Character = GlobalData.AllRivals.First(r => r.Name.ToLower() == text.ToLower());
            }
            catch (Exception ex)
            {
#if DEBUGPHOTO
                MessageBox.Show($"Failed To Parse Character from text: {text} - defaulting to 'unknown'");
#endif
                Debug.WriteLine($"Failed To Parse Character from text: {text} - defaulting to 'unknown'");
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
