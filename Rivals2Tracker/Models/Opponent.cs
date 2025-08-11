using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Preview.Notes;

namespace Rivals2Tracker.Models
{
    class Opponent : BindableBase
    {
        private string _tag = String.Empty;
        public string Tag
        {
            get { return _tag; }
            set { SetProperty(ref _tag, value); }
        }

        private ObservableCollection<string> _notes = new();
        public ObservableCollection<string> Notes
        {
            get { return _notes; }
            set
            {
                SetProperty(ref _notes, value);
                RaisePropertyChanged(NotesLabel);
            }
        }
        public string NotesLabel => string.Join(Environment.NewLine, Notes);

        private int _winsTotal = 0;
        public int WinsTotal
        {
            get { return _winsTotal; }
            set { SetProperty(ref _winsTotal, value); }
        }

        private int _losesTotal = 0;
        public int LosesTotal
        {
            get { return _losesTotal; }
            set { SetProperty(ref _losesTotal, value); }
        }

        private int _winsSeasonal = 0;
        public int WinsSeasonal
        {
            get { return _winsSeasonal; }
            set { SetProperty(ref _winsSeasonal, value); }
        }

        private int _losesSeasonal = 0;
        public int LosesSeasonal
        {
            get { return _losesSeasonal; }
            set { SetProperty(ref _losesSeasonal, value); }
        }

        private void HookNotesChanged()
        {
            _notes.CollectionChanged += (s, e) =>
                RaisePropertyChanged(nameof(NotesLabel));
        }

        public Opponent()
        {
            HookNotesChanged();
        }

        public Opponent(string tag)
        {
            Tag = tag;
            HookNotesChanged();
        }
    }
}
