using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Slipstream.Data;
using Slipstream.Models;
using Slipstream.Resources.Events;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace Slipstream
{
    class MatchDetails_VM : BindableBase
    {
        private IEventAggregator _eventAggregator;
        public Action Close { get; set; }

        private MatchResult _loadedMatch = new();
        public MatchResult LoadedMatch
        {
            get { return _loadedMatch; }
            set { SetProperty(ref _loadedMatch, value); }
        }

        private List<string> _selectableCharacters = new();
        public List<string> SelectableCharacters
        {
            get { return _selectableCharacters; }
            set { SetProperty(ref _selectableCharacters, value); }
        }

        public DelegateCommand SaveAndCloseCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand DeleteMatchCommand { get; set; }

        public MatchDetails_VM(MatchResult matchResult)
        {
            LoadedMatch = matchResult;
            SelectableCharacters = GlobalData.AllCharacters;
            SaveAndCloseCommand = new DelegateCommand(SaveAndClose);
            CancelCommand = new DelegateCommand(Cancel);
            DeleteMatchCommand = new DelegateCommand(DeleteMatch);
        }

        private void SaveAndClose()
        {
            RivalsORM.UpdateMatch(LoadedMatch);
            MatchHistoryUpdateEvent.PublishMatchSaved(LoadedMatch);
            Close?.Invoke();
        }
        private void Cancel()
        {
            Close?.Invoke();
        }

        private void DeleteMatch()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this match?", "Delete Match", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                RivalsORM.DeleteMatch(LoadedMatch);
                MessageBox.Show("Match Deleted!", "Don't be mad - it's just a game!");
                MatchHistoryUpdateEvent.PublishMatchSaved(LoadedMatch);
                Close?.Invoke();
            }
        }
    }
}
