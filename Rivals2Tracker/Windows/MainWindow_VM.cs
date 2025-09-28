using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Rivals2Tracker.Models;
using System.Windows;
using Rivals2Tracker.Data;
using System.Diagnostics;
using Rivals2Tracker.Resources.Events;

namespace Rivals2Tracker
{
    class MainWindow_VM : BindableBase
    {

        #region Variables
        private ObservableCollection<MatchResult> _matchResults = new();
        public ObservableCollection<MatchResult> MatchResults
        {
            get { return _matchResults; }
            set { SetProperty(ref _matchResults, value); }
        }

        private MetaDataTable _displayedCharacterResults = new();
        public MetaDataTable DisplayedCharacterResults
        {
            get { return _displayedCharacterResults; }
            set { SetProperty(ref _displayedCharacterResults, value); }
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

        private bool _showLifetimeResults = false;
        public bool ShowLifetimeResults
        {
            get { return _showLifetimeResults; }
            set
            {
                if (value == false)
                {
                    DisplayedCharacterResults = CurrentSeasonResults;
                }
                else
                {
                    DisplayedCharacterResults = AllSeasonResults;
                }

                SetProperty(ref _showLifetimeResults, value);
            }
        }

        private MatchHistoryCollection _activeMatchHistory = new();
        public MatchHistoryCollection ActiveMatchHistory
        {
            get { return _activeMatchHistory; }
            set
            {
                SetProperty(ref _activeMatchHistory, value); 
            }
        }

        private CharacterMetadata? _activeMatchSeason_CharacterData;
        public CharacterMetadata ActiveMatchSeason_CharacterData
        {
            get { return _activeMatchSeason_CharacterData; }
            set
            {
                PlayerMatchHistoryViewVisibility = Visibility.Collapsed;
                CharacterMatchHistoryViewVisibility = Visibility.Visible;
                SetActiveMatchHistory(value, "season");
            }
        }

        public CharacterMetadata ActiveMatchAll_CharacterData
        {
            set
            {
                SetActiveMatchHistory(value, "all");
            }
        }

        private RivalsMatch? _activeMatch;
        public RivalsMatch? ActiveMatch
        {
            get { return _activeMatch; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _activeMatch, value);
                    return;
                }

                if ((value as RivalsMatch).Opponent.Name.Length == 0)
                {
                    LocalPlayerNameVisibility = Visibility.Visible;
                }
                else
                {
                    LocalPlayerNameVisibility = Visibility.Collapsed;
                }

                SetProperty(ref _activeMatch, value);

                OpponentName = ActiveMatch.Opponent.Name;
                OpponentElo = ActiveMatch.Opponent.Elo;
                MyElo = ActiveMatch.Me.Elo;

                RaisePropertyChanged(nameof(CompletedLabelVisibility));
            }
        }

        private Opponent _activeOpponent;
        public Opponent ActiveOpponent
        {
            get { return _activeOpponent; }
            set
            {
                SetProperty(ref _activeOpponent, value);
            }
        }

        private string _myCharacter = "Wrastor";
        public string MyCharacter
        {
            get { return _myCharacter; }
            set
            {
                if (!string.IsNullOrEmpty(value) && ActiveMatch is not null)
                {
                    ActiveMatch.Me.Character = value;
                }

                SetProperty(ref _myCharacter, value);
            }
        }

        private string _myName;
        public string MyName
        {
            get { return _myName; }
            set
            {
                GlobalData.MyName = value;

                if (value.Length == 0)
                {
                    LocalPlayerNameVisibility = Visibility.Visible;
                }
                else
                {
                    LocalPlayerNameVisibility = Visibility.Collapsed;
                }

                RaisePropertyChanged(nameof(IsMyTagPhVisible));
                SetProperty(ref _myName, value);
            }
        }

        private Visibility _awaitingDataVisibility = Visibility.Visible;
        public Visibility AwaitingDataVisibility
        {
            get { return _awaitingDataVisibility; }
            set { SetProperty(ref _awaitingDataVisibility, value); }
        }

        private Visibility _localPlayerNameVisibility = Visibility.Collapsed;
        public Visibility LocalPlayerNameVisibility
        {
            get { return _localPlayerNameVisibility; }
            set { SetProperty(ref _localPlayerNameVisibility, value); }
        }

        private Visibility _completedLabelVisibility = Visibility.Collapsed;
        public Visibility CompletedLabelVisibility
        {
            get { return _completedLabelVisibility; }
            set { SetProperty(ref _completedLabelVisibility, value); }
        }

        private Visibility _characterMatchHistoryViewVisibility = Visibility.Collapsed;
        public Visibility CharacterMatchHistoryViewVisibility
        {
            get { return _characterMatchHistoryViewVisibility; }
            set { SetProperty(ref _characterMatchHistoryViewVisibility, value); }
        }

        private Visibility _playerMatchHistoryViewVisibility = Visibility.Collapsed;
        public Visibility PlayerMatchHistoryViewVisibility
        {
            get { return _playerMatchHistoryViewVisibility; }
            set { SetProperty(ref _playerMatchHistoryViewVisibility, value); }
        }

        private Visibility _IsOpponentTagPhVisible = Visibility.Visible;
        public Visibility IsOpponentTagPhVisible
        {
            get
            {
                return String.IsNullOrEmpty(OpponentName) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Visibility _isOpponentEloPhVisible = Visibility.Visible;
        public Visibility IsOpponentEloPhVisible
        {
            get
            {
                return String.IsNullOrEmpty(OpponentElo) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Visibility _IsMyTagPhVisible = Visibility.Visible;
        public Visibility IsMyTagPhVisible
        {
            get
            {
                return String.IsNullOrEmpty(MyName) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Visibility _isMyEloPhVisible = Visibility.Visible;
        public Visibility IsMyEloPhVisible
        {
            get
            {
                return String.IsNullOrEmpty(MyElo) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Visibility _isNotesPhVisible = Visibility.Visible;
        public Visibility IsNotesPhVisible
        {
            get
            {
                return String.IsNullOrEmpty(ActiveMatchNotes) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string _myElo;
        public string MyElo
        {
            get { return _myElo; }
            set
            {
                if (ActiveMatch is not null)
                {
                    ActiveMatch.Me.Elo = value;
                }

                RaisePropertyChanged(nameof(IsMyEloPhVisible));
                SetProperty(ref _myElo, value);
            }
        }

        private string _opponentElo;
        public string OpponentElo
        {
            get { return _opponentElo; }
            set
            {
                if (ActiveMatch is not null)
                {
                    ActiveMatch.Opponent.Elo = value;
                }

                RaisePropertyChanged(nameof(IsOpponentEloPhVisible));
                SetProperty(ref _opponentElo, value);
            }
        }

        private string _opponentName;
        public string OpponentName
        {
            get { return _opponentName; }
            set
            {
                if (ActiveMatch is not null)
                {
                    ActiveMatch.Opponent.Name = value;
                }

                RaisePropertyChanged(nameof(IsOpponentTagPhVisible));
                SetProperty(ref _opponentName, value);
            }
        }

        private string _activeMatchNotes;
        public string ActiveMatchNotes
        {
            get { return _activeMatchNotes; }
            set
            {
                if (ActiveMatch is not null)
                {
                    ActiveMatch.Notes = value;
                }

                SetProperty(ref _activeMatchNotes, value);
                RaisePropertyChanged(nameof(IsNotesPhVisible));

            }
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

                if (ActiveMatch.Status == MatchStatus.Finished)
                {
                    CompletedLabelVisibility = Visibility.Visible;
                }
                else
                {
                    CompletedLabelVisibility = Visibility.Collapsed;
                }

                return ActiveMatch.StatusString;
            }
            set
            {
                RaisePropertyChanged(nameof(CompletedLabelVisibility));
                SetProperty(ref _activeMatchStatusString, value);
            }
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

        private string currentPatch;
        public string CurrentPatch
        {
            get { return currentPatch; }
            set { SetProperty(ref currentPatch, value); }
        }
        private string _mySelectedCharPortrait = "/Resources/CharacterIcons/unknown.png";
        public string MySelectedCharPortrait
        {
            get { return _mySelectedCharPortrait; }
            set { SetProperty(ref _mySelectedCharPortrait, value); }
        }

        private string _selectedImagePath = "/Resources/CharacterIcons/unknown.png";
        public string SelectedImagePath
        {
            get { return _selectedImagePath; }
            set { SetProperty(ref _selectedImagePath, value); }
        }

        private string _secondarySelectedImagePath = "/Resources/CharacterIcons/unknown.png";
        public string SecondarySelectedImagePath
        {
            get { return _secondarySelectedImagePath; }
            set { SetProperty(ref _secondarySelectedImagePath, value); }
        }

        private bool _isMyFlyoutOpen = false;
        public bool IsMyFlyoutOpen
        {
            get { return _isMyFlyoutOpen; }
            set { SetProperty(ref _isMyFlyoutOpen, value); }
        }

        private bool _isOppFlyoutOpen = false;
        public bool IsOppFlyoutOpen
        {
            get { return _isOppFlyoutOpen; }
            set { SetProperty(ref _isOppFlyoutOpen, value); }
        }

        private bool _isSecondaryFlyoutOpen = false;
        public bool IsSecondaryFlyoutOpen
        {
            get { return _isSecondaryFlyoutOpen; }
            set { SetProperty(ref _isSecondaryFlyoutOpen, value); }
        }

        public ObservableCollection<string> AvailableCharacters { get; set; }
        #endregion

        #region Commands

        public DelegateCommand SetMatchWinCommand { get; private set; }
        public DelegateCommand SetMatchLoseCommand { get; private set; }
        public DelegateCommand SetMatchDiscardCommand { get; private set; }
        public DelegateCommand TestOcrCommand { get; private set; }
        public DelegateCommand ShowMyFlyoutCommand { get; }

        public DelegateCommand ShowFlyoutCommand { get; }
        public DelegateCommand ShowSecondaryFlyoutCommand { get; }
        public DelegateCommand ShowSettingsWindowCommand { get; private set; }

        public DelegateCommand<string> SelectMyCharacterCommand { get; set; }
        public DelegateCommand<string> SelectCharacterCommand { get; set; }
        public DelegateCommand<string> SelectSecondaryCharacterCommand { get; set; }

        #endregion

        public MainWindow_VM()
        {
            SetMatchWinCommand = new DelegateCommand(() => _ = SetMatchWin());
            SetMatchLoseCommand = new DelegateCommand(() => _ = SetMatchLose());
            SetMatchDiscardCommand = new DelegateCommand(() => _ = SetMatchDiscard());
            TestOcrCommand = new DelegateCommand(() => _ = DoTheOcr());
            ShowMyFlyoutCommand = new DelegateCommand(ShowMyFlyout);
            ShowFlyoutCommand = new DelegateCommand(ShowFlyout);
            ShowSecondaryFlyoutCommand = new DelegateCommand(ShowSecondaryFlyout);
            SelectMyCharacterCommand = new DelegateCommand<string>(SelectMyCharacter);
            SelectCharacterCommand = new DelegateCommand<string>(SelectOppCharacter);
            SelectSecondaryCharacterCommand = new DelegateCommand<string>(SelectSecondaryCharacter);
            ShowSettingsWindowCommand = new DelegateCommand(ShowSettingsWindow);

            MatchHistoryUpdateEvent.MatchSaved += UpdateMatchHistory;

            SetupImages();

#if !DEBUGNODB
            GetMetadata();
            GetMatches(true);
#endif

            RaisePropertyChanged("ActiveMatch");
        }

        private void SetupImages()
        {
            AvailableCharacters = new ObservableCollection<string>(GlobalData.AllCharacters);

#if DEBUGNODB
            string defaultCharacter = "Wrastor";
#else
            string defaultCharacter = RivalsORM.GetPlayerCharacter();
#endif

            if (GlobalData.CharacterPortraitDict.TryGetValue(defaultCharacter, out string imagePath))
            {
                MySelectedCharPortrait = imagePath;
                MyCharacter = defaultCharacter;
            }
        }

        private void GetMetadata()
        {
            if (RivalsORM.GetIsFirstStart() == "0")
            {
                // TODO:  Do the first stuff or something
                // Do first start stuff
            }
            else
            {
                MyName = RivalsORM.GetPlayerName();
                CurrentPatch = RivalsORM.GetPatchValue();
            }

            RivalsORM.SetMetaDataValue("IsFirstStart", "1");
        }

        private async Task SetMatchWin()
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                ActiveMatch.IsWon = true;
                ActivityText = "Match has been recorded as a 'Win'.";
                ActiveMatch.Status = MatchStatus.Finished;
                RivalsORM.AddMatch(ActiveMatch);
                ResetMatchStatus();
                return;
            }

            ActivityText = "Match is not active or not valid";
        }

        private async Task SetMatchLose()
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                ActiveMatch.IsWon = false;
                ActivityText = "Match has been recorded as a 'Loss'.";
                ActiveMatch.Status = MatchStatus.Finished;
                RivalsORM.AddMatch(ActiveMatch);
                ResetMatchStatus();
                return;
            }

            ActivityText = "Match is not active or not valid";
        }

        private async Task SetMatchDiscard()
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                ActiveMatch.IsWon = false;
                ActivityText = "Match has been discarded and has not been recorded.";
                ActiveMatch.Status = MatchStatus.Invalid;
                ActiveOpponent = new();
                ResetMatchStatus();
                return;
            }

            ActivityText = "You can't flag a match as Invalid if it's not In Progress.";
        }

        private void ResetMatchStatus()
        {
            ActiveMatchNotes = "";
            RaisePropertyChanged("IsInputLocked");
            RaisePropertyChanged("ActiveMatchStatusString");
            ErrorText = String.Empty;
            GetMatches();
            return;
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
                ErrorText = "Unrecoverable Capture - Is Rivals running?";
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
            ActiveMatch.Me.Character = MyCharacter;

            if (GlobalData.CharacterImageDict.TryGetValue(ActiveMatch.Opponent.Character, out string imagePath))
            {
                SelectedImagePath = imagePath;
            }

            RaisePropertyChanged("IsInputLocked");
            RaisePropertyChanged("ActiveMatchStatusString");

            GetPlayerInfo();
            ShowPlayerMatchData();
            RefreshPlaceholderVisibility();
        }

        private void RefreshPlaceholderVisibility()
        {
            RaisePropertyChanged(nameof(IsOpponentTagPhVisible));
            RaisePropertyChanged(nameof(IsOpponentEloPhVisible));
            RaisePropertyChanged(nameof(IsMyTagPhVisible));
            RaisePropertyChanged(nameof(IsMyEloPhVisible));
            RaisePropertyChanged(nameof(IsNotesPhVisible));
        }

        private void GetPlayerInfo()
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

        private void ShowPlayerMatchData()
        {
            PlayerMatchHistoryViewVisibility = Visibility.Visible;
            CharacterMatchHistoryViewVisibility = Visibility.Collapsed;
        }

        public async Task OnCaptureMatchHotKey()
        {
            SecondarySelectedImagePath = "/Resources/CharacterIcons/unknown.png";
            SelectedImagePath = "/Resources/CharacterIcons/unknown.png";
            await DoTheOcr();
        }

        private void GetMatches(bool isOnStart = false)
        {

            MatchResults = RivalsORM.GetAllMatches();
            CurrentSeasonResults = GetSeasonData("1.3");
            AllSeasonResults = GetSeasonData("all");

            if (isOnStart)
            {
                DisplayedCharacterResults = CurrentSeasonResults;
            }
            else
            {
                DisplayedCharacterResults = ShowLifetimeResults ? AllSeasonResults : CurrentSeasonResults;
            }
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

                    try
                    {
                        metadatatable.CharacterData.First(wd => wd.Character == match.OppChar1)
                            .WeightedData.AddResult(match.MyElo, match.OpponentElo, match.Result, match.Patch);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to find opponent by character {match.OppChar1}");
                        continue;
                    }
                }
            }

            metadatatable.CharacterData.Remove(metadatatable.CharacterData.First(c => c.Character == "Unknown"));

            foreach (CharacterMetadata character in metadatatable.CharacterData)
            {
                character.WeightedData.CalculateWeightedElo();
            }

            return metadatatable;
        }

        private void UpdateMatchHistory(MatchResult matchResult)
        {
            GetMatches();
        }

        private void ShowMyFlyout()
        {
            IsMyFlyoutOpen = true;
        }

        private void ShowFlyout()
        {
            IsOppFlyoutOpen = true;
        }

        private void ShowSecondaryFlyout()
        {   
            IsSecondaryFlyoutOpen = true;
        }

        // TODO: This dual-split but almost identical operation on primary and secondary is dumb.  Combine this into a better flow.
        // There is also a duplicate DataTemplate to pull out
        private void SelectMyCharacter(string character)
        {
            if (!string.IsNullOrEmpty(character))
            {
                if (GlobalData.CharacterPortraitDict.TryGetValue(character, out string imagePath))
                {
                    MySelectedCharPortrait = imagePath;
                    MyCharacter = character;
                }
            }
            else
            {
                ErrorText = "Character seems to be null?  That's probably a bug";
            }

            IsMyFlyoutOpen = false;
        }


        private void SelectOppCharacter(string character)
        {
            if (!string.IsNullOrEmpty(character) && ActiveMatch is not null)
            {
                if (GlobalData.CharacterImageDict.TryGetValue(character, out string imagePath))
                {
                    SelectedImagePath = imagePath;
                    ActiveMatch.Opponent.Character = character;
                }
            }
            else
            {
                ActivityText = "No Active Match - You can't select a character without a match active";
            }
            
            IsOppFlyoutOpen = false;
        }
        private void SelectSecondaryCharacter(string character)
        {
            if (!string.IsNullOrEmpty(character) && ActiveMatch is not null)
            {
                if (GlobalData.CharacterImageDict.TryGetValue(character, out string imagePath))
                {
                    SecondarySelectedImagePath = imagePath;
                    ActiveMatch.Opponent.Character2 = character;
                }
            }
            else
            {
                ActivityText = "No Active Match - You can't select a character without a match active";
            }

            IsSecondaryFlyoutOpen = false;
        }

        private void ShowSettingsWindow()
        {            
            Settings settingsWindow = new Settings();

            settingsWindow.Owner = System.Windows.Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            bool? result = settingsWindow.ShowDialog();
            GetMetadata();

        }

        // Stringly typed nonsense...
        private void SetActiveMatchHistory(CharacterMetadata characterData, string season)
        {
            if (characterData is null)
            {
                return;
            }

            MatchHistoryCollection matchHistory = new();

            matchHistory.Season = season == "all" ? "All Seasons" : "Current Season";
            matchHistory.Character = characterData.Character;

            matchHistory.MatchResults = RivalsORM.GetAllMatches(matchHistory.Character);

            ActiveMatchHistory = matchHistory;
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
