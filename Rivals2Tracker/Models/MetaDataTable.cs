using Prism.Mvvm;
using Rivals2Tracker.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace Rivals2Tracker.Models
{
    class MetaDataTable : BindableBase
    {
        private string _tableTitle = string.Empty;
        public string TableTitle
        {
            get { return _tableTitle; }
            set { SetProperty(ref _tableTitle, value); }
        }
        public string Patch { get; set; }

        public int WinsTotal { get; set; }
        public int LosesTotal { get; set; }
        public int MatchesTotal
        {
            get => (WinsTotal + LosesTotal);
        }

        public string RecordString
        {
            get
            {
                return $"{WinsTotal} - {LosesTotal}";
            }
        }

        public string WinPercent
        {
            get => (((float)WinsTotal / (float)MatchesTotal) * 100.0f).ToString("n2") + "%";
        }

        private ObservableCollection<CharacterMetadata> _characterMetadata = new();
        public ObservableCollection<CharacterMetadata> CharacterData
        {
            get { return _characterMetadata; }
            set { SetProperty(ref _characterMetadata, value); }
        }

        public MetaDataTable()
        {
            BuildCharacterSet();
        }

        public void BuildCharacterSet()
        {
            CharacterData.Clear();

            foreach (string character in GlobalData.AllCharacters)
            {
                CharacterMetadata newCharacter = new CharacterMetadata(character);
                CharacterData.Add(newCharacter);
            }
        }

        public void AddResult(MatchResult matchResult)
        {
            CharacterMetadata? character = CharacterData.FirstOrDefault(c => c.Character == matchResult.OppChar1);

            if (character is null)
            {
                CharacterMetadata? unknownChar = CharacterData.FirstOrDefault(c => c.Character == "Unknown");


                if (matchResult.Result == "Win")
                {
                    unknownChar.Wins++;
                    WinsTotal++;
                }

                if (matchResult.Result == "Lose")
                {
                    unknownChar.Loses++;
                    LosesTotal++;
                }
            }
            else
            {
                if (matchResult.Result == "Win")
                {
                    character.Wins++;
                    WinsTotal++;
                }

                if (matchResult.Result == "Lose")
                {
                    character.Loses++;
                    LosesTotal++;
                }
            }
        }
    }
}
