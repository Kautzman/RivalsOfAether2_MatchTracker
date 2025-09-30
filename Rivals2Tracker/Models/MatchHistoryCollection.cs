using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Slipstream.Models
{
    public class MatchHistoryCollection : BindableBase
    {
        private string _character;
        public string Character
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
