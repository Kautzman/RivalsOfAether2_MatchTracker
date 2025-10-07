using Prism.Commands;
using Prism.Mvvm;
using Slipstream.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Slipstream.Models
{
    public class MatchResult : BindableBase
    {
        public long ID { get; set; }
        public string Date { get; set; }
        public string Opponent { get; set; }
        public string OpponentElo { get; set; }
        public RivalsCharacter MyChar { get; set; }
        public string MyElo { get; set; }
        public ObservableCollection<GameResult> Games { get; set; }
        public RivalsCharacter OppChar1 { get; set; }

        private RivalsCharacter _oppChar2;
        public RivalsCharacter OppChar2
        {
            get
            {
                if (_oppChar2 is null)
                {
                    return GlobalData.UnknownCharacter;
                }

                return _oppChar2;
            }
            set { SetProperty(ref _oppChar2, value); }
        }

        public string Result { get; set; }
        public string Patch { get; set; }
        public string Notes { get; set; }

        public DelegateCommand ShowMatchDetailsCommand { get; set; }

        public MatchResult() { }

        public MatchResult(MatchResultRecord matchRecord)
        {
            ShowMatchDetailsCommand = new DelegateCommand(ShowMatchDetails);

            ID = matchRecord.ID;
            Date = matchRecord.Date;
            Opponent = matchRecord.Opponent;
            OpponentElo = matchRecord.OpponentElo;
            MyChar = GlobalData.GetCharacterByID(matchRecord.MyChar);
            MyElo = matchRecord.MyElo;
            OppChar1 = GlobalData.GetCharacterByID(matchRecord.OppChar1);
            OppChar2 = GlobalData.GetCharacterByID(matchRecord.OppChar2);
            Result = matchRecord.Result;
            Patch = matchRecord.Patch;
            Notes = matchRecord.Notes;
        }

        private void ShowMatchDetails()
        {
            MatchDetails matchdDetails = new MatchDetails(this);

            matchdDetails.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            matchdDetails.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            matchdDetails.ShowDialog();
        }
    }

    public class MatchResultRecord
    {
        public long ID { get; set; }
        public string Date { get; set; }
        public string Opponent { get; set; }
        public string OpponentElo { get; set; }
        public string MyChar { get; set; }
        public string MyElo { get; set; }
        public string OppChar1 { get; set; }
        public string OppChar2 { get; set; }
        public string Result { get; set; }
        public string Patch { get; set; }
        public string Notes { get; set; }
    }
}
