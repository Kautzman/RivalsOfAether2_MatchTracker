using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Rivals2Tracker.Models;
using System.Windows;
using System.Transactions;
using System.Runtime.CompilerServices;
using Rivals2Tracker.Data;
using Rivals2Tracker.HotkeyHandler;
using System.Windows.Forms;

namespace Rivals2Tracker
{
    class MainWindow_VM : BindableBase
    {
        private ObservableCollection<MatchResult> _matchResults = new();
        public ObservableCollection<MatchResult> MatchResults
        {
            get { return _matchResults; }
            set { SetProperty(ref _matchResults, value); }
        }

        private RivalsMatch? _activeMatch;
        public RivalsMatch? ActiveMatch
        {
            get { return _activeMatch; }
            set { SetProperty(ref _activeMatch, value); }
        }

        private string _activityText;
        public string ActivityText
        {
            get { return _activityText; }
            set { SetProperty(ref _activityText, value); }
        }

        private string _activeMatchStatusString;
        public string ActiveMatchStatusString
        {
            get
            {
                if (ActiveMatch is null)
                {
                    return "No Active Match";
                }

                return ActiveMatch.StatusString;
            }
            set { SetProperty(ref _activeMatchStatusString, value); }
        }

        public bool IsInputLocked
        {
            get
            {
                if (ActiveMatch is not null)
                {
                    if (ActiveMatch.Status == MatchStatus.InProgress)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private string currentPatch = "1.0.0";
        public string CurrentPatch
        {
            get { return currentPatch; }
            set { SetProperty(ref currentPatch, value); }
        }


        public DelegateCommand SetMatchWinCommand { get; private set; }
        public DelegateCommand SetMatchLoseCommand { get; private set; }
        public DelegateCommand SetMatchDiscardCommand { get; private set; }
        public DelegateCommand TestOcrCommand { get; private set; }

        public MainWindow_VM()
        {
            TestOcrCommand = new DelegateCommand(() => _ = SetMatchWin());
            TestOcrCommand = new DelegateCommand(() => _ = SetMatchLose());
            TestOcrCommand = new DelegateCommand(() => _ = SetMatchDiscard());
            TestOcrCommand = new DelegateCommand(() => _ = DoTheOcr());

            GetMatches();
        }

        private async Task SetMatchWin()
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                ActiveMatch.IsWon = true;
                ActivityText = "Match has been set to 'Win' and written to DB.";
                ActiveMatch.Status = MatchStatus.Finished;
                RivalsORM.AddMatch(ActiveMatch);
                GetMatches();
                return;
            }

            ActivityText = "Match is not active or not valid";
        }

        private async Task SetMatchLose()
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                ActiveMatch.IsWon = false;
                ActivityText = "Match has been set to 'Lose' and written to DB.";
                ActiveMatch.Status = MatchStatus.Finished;
                RivalsORM.AddMatch(ActiveMatch);
                GetMatches();
                return;
            }

            ActivityText = "Match is not active or not valid";
        }

        private async Task SetMatchDiscard()
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                ActiveMatch.IsWon = false;
                ActivityText = "Match has been set to 'Invalid' and written to DB.";
                ActiveMatch.Status = MatchStatus.Invalid;
                RivalsORM.AddMatch(ActiveMatch);
                GetMatches();
                return;
            }

            ActivityText = "You can't flag a match as Invalid if it's not In Progress.";
        }

        private async Task DoTheOcr()
        {
            RivalsOcrResult result = await RivalsOcrEngine.Capture();

            if (!result.IsValid)
            {
                ActivityText = result.ErrorText;
                return;
            }

            if (ActiveMatch is not null)
            {
                if (ActiveMatch.Status == MatchStatus.InProgress)
                {
                    ActivityText = "Conclude the active match before starting a new one!";
                    return;
                }
            }

            if (ActiveMatch is null || ActiveMatch.Status != MatchStatus.InProgress)
            {
                ActiveMatch = result.Match;
                ActiveMatch.Patch = CurrentPatch;
            }

            Console.WriteLine(result);
        }

        public async Task OnCaptureMatchHotKey()
        {
            await DoTheOcr();
        }

        private void GetMatches()
        {
            MatchResults = RivalsORM.GetAllMatches();
            Console.WriteLine();
        }
    }
}
