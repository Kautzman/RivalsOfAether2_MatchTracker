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
using System.Diagnostics;
using System.Windows.Media;

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

        private MetaDataTable _currentSeasonResults = new();
        public MetaDataTable CurrentSeasonResults
        {
            get { return _currentSeasonResults; }
            set { SetProperty(ref _currentSeasonResults, value); }
        }

        private MetaDataTable _lastSeasonResults = new();
        public MetaDataTable LastSeasonResults
        {
            get { return _lastSeasonResults; }
            set { SetProperty(ref _lastSeasonResults, value); }
        }

        private MetaDataTable _allSeasonResults = new();
        public MetaDataTable AllSeasonResults
        {
            get { return _allSeasonResults; }
            set { SetProperty(ref _allSeasonResults, value); }
        }

        private ObservableCollection<WeightedCharacterMetadata> _weightedMatchupData = new();
        public ObservableCollection<WeightedCharacterMetadata> WeightedMatchupData
        {
            get { return _weightedMatchupData; }
            set { SetProperty(ref _weightedMatchupData, value); }
        }

        private RivalsMatch? _activeMatch;
        public RivalsMatch? ActiveMatch
        {
            get { return _activeMatch; }
            set { SetProperty(ref _activeMatch, value); }
        }

        private Opponent _activeOpponent;
        public Opponent ActiveOpponent
        {
            get { return _activeOpponent; }
            set { SetProperty(ref _activeOpponent, value); }
        }

        private string _activityText;
        public string ActivityText
        {
            get { return _activityText; }
            set { SetProperty(ref _activityText, value); }
        }

        private string _errorText;
        public string ErrorText
        {
            get { return _errorText; }
            set { SetProperty(ref _errorText, value); }
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

        private string currentPatch = "1.3.2";
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
            SetMatchWinCommand = new DelegateCommand(() => _ = SetMatchWin());
            SetMatchLoseCommand = new DelegateCommand(() => _ = SetMatchLose());
            SetMatchDiscardCommand = new DelegateCommand(() => _ = SetMatchDiscard());
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
                RaisePropertyChanged("IsInputLocked");
                RaisePropertyChanged("ActiveMatchStatusString");
                RivalsORM.AddMatch(ActiveMatch);
                ErrorText = String.Empty;
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
                RaisePropertyChanged("IsInputLocked");
                RaisePropertyChanged("ActiveMatchStatusString");
                RivalsORM.AddMatch(ActiveMatch);
                ErrorText = String.Empty;
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
                ActivityText = "Discarded Match";
                ActiveMatch.Status = MatchStatus.Invalid;
                RaisePropertyChanged("IsInputLocked");
                RaisePropertyChanged("ActiveMatchStatusString");
                ErrorText = String.Empty;
                GetMatches();
                ActiveOpponent = new();
                return;
            }

            ActivityText = "You can't flag a match as Invalid if it's not In Progress.";
        }

        private async Task DoTheOcr()
        {
            RivalsOcrResult result = await RivalsOcrEngine.Capture(); 

            if (!result.IsValid)
            {
                ErrorText = result.ErrorText;
            }

            if (!result.IsSalvagable)
            {
                ErrorText = "Unrecoverable Capture - Try Again.";
                return;
            }

            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                ActivityText = "Conclude the active match before starting a new one!";
                return;
            }     

            ActiveMatch = result.Match;
            ActiveMatch.Patch = CurrentPatch;
            ActiveMatch.Status = MatchStatus.InProgress;
            RaisePropertyChanged("IsInputLocked");
            RaisePropertyChanged("ActiveMatchStatusString");

            GetPlayerInfo();
        }

        public void GetPlayerInfo()
        {
            if (ActiveMatch is null)
            {
                Debug.WriteLine("Active Match was null - exiting GetPlayerInfo()");
                return;
            }

            ActiveOpponent = new(ActiveMatch.Opponent.Name ?? "??");

            List<MatchResult> opponentResults = MatchResults.Where(r => r.Opponent == ActiveMatch.Opponent.Name).ToList();

            foreach (MatchResult match in opponentResults)
            {
                if (match.Result == "Win")
                {
                    ActiveOpponent.WinsTotal++;

                    if (IsPatchMatch(currentPatch.Substring(0, 3), match.Patch))
                    {
                        ActiveOpponent.WinsSeasonal++;
                    }
                }
                else if (match.Result == "Lose")
                {
                    ActiveOpponent.LosesTotal++;

                    if (IsPatchMatch(currentPatch.Substring(0, 3), match.Patch))
                    {
                        ActiveOpponent.LosesSeasonal++;
                    }
                }

                if (!String.IsNullOrEmpty(match.Notes))
                {
                    ActiveOpponent.Notes.Add(match.Notes);
                    RaisePropertyChanged(ActiveOpponent.NotesLabel);
                }
            }
        }

        public async Task OnCaptureMatchHotKey()
        {
            await DoTheOcr();
        }

        private void GetMatches()
        {
            MatchResults = RivalsORM.GetAllMatches();
            LastSeasonResults = GetSeasonData("1.2");
            CurrentSeasonResults = GetSeasonData("1.3");
            AllSeasonResults = GetSeasonData("all");
            BuildWeightedData();
            Console.WriteLine(WeightedMatchupData.Count);
        }

        private MetaDataTable GetSeasonData(string patch)
        {
            MetaDataTable metadatatable = new MetaDataTable();
            metadatatable.Patch = patch;

            metadatatable.TableTitle = patch == "all" ? $"Matchup Totals" : $"{patch} Matchups";

            foreach (MatchResult match in MatchResults)
            {
                if (IsPatchMatch(patch, match.Patch))
                {
                    metadatatable.AddResult(match);
                }
            }

            return metadatatable;
        }

        private void BuildWeightedData()
        {
            WeightedMatchupData.Clear();

            foreach (string character in GlobalData.AllCharacters)
            {
                WeightedMatchupData.Add(new WeightedCharacterMetadata(character));
            }

            foreach (MatchResult match in MatchResults)
            {
                try
                {
                    WeightedMatchupData.First(wd => wd.Character == match.OppChar1)
                        .AddResult(match.MyElo, match.OpponentElo, match.Result);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Failed to find opponent by character {match.OppChar1}");
                }
            }
        }

        private bool IsPatchMatch(string patchToMatch, string thisMatchPatch)
        {
            if (patchToMatch == "all")
            {
                return true;
            }

            if (thisMatchPatch.Substring(0, 3) == patchToMatch)
            {
                return true;
            }

            return false;
        }
    }
}
