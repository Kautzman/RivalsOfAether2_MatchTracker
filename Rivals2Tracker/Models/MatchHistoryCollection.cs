using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Slipstream.Models
{
    public class MatchHistoryCollection : BindableBase
    {
        private RivalsCharacter _character;
        public RivalsCharacter Character
        {
            get => _character;
            set => SetProperty(ref _character, value);
        }

        private string _season;
        public string Season
        {
            get => _season;
            set => SetProperty(ref _season, value);
        }

        private ObservableCollection<MatchResult> _matchResults;
        public ObservableCollection<MatchResult> MatchResults
        {
            get => _matchResults;
            set => SetProperty(ref _matchResults, value);
        }
    }
}
