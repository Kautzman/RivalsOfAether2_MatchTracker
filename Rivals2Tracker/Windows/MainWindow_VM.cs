using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Slipstream.Models;
using System.Windows;
using Slipstream.Data;
using System.Diagnostics;
using Slipstream.Resources.Events;
using System.Media;
using Slipstream.Services;
using Windows.Networking.Vpn;
using System.Windows.Navigation;
using kWindows.Core;
using Slipstream.Windows;

namespace Slipstream
{
    class MainWindow_VM : BindableBase
    {

        #region Variables

        private ObservableCollection<RivalsSeason> _seasons = new();
        public ObservableCollection<RivalsSeason> Seasons
        {
            get { return _seasons; }
            set { SetProperty(ref _seasons, value); }
        }

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

        private MetaDataTable _allSeasonResults = new();
        public MetaDataTable AllSeasonResults
        {
            get { return _allSeasonResults; }
            set { SetProperty(ref _allSeasonResults, value); }
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
                RaisePropertyChanged(nameof(EnableSeasonSelectionComboBox));
            }
        }

        public bool EnableSeasonSelectionComboBox
        {
            get { return !ShowLifetimeResults; }
        }

        private MatchHistoryCollection _activeMatchHistory = new();
        public MatchHistoryCollection ActiveMatchHistory
        {
            get { return _activeMatchHistory; }
            set { SetProperty(ref _activeMatchHistory, value); }
        }

        private CharacterMetadata? _activeMatchSeason_CharacterData;
        public CharacterMetadata ActiveMatchSeason_CharacterData
        {
            get { return _activeMatchSeason_CharacterData; }
            set
            {
                ToggleRivalsMatchHistory();
                SetActiveMatchHistory(value, "season");
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

                SetMatchTextBoxReadOnly(value.Status);
                OpponentName = ActiveMatch.Opponent.Name;
                OpponentElo = ActiveMatch.Opponent.Elo;
                MyElo = ActiveMatch.Me.Elo;

                RaisePropertyChanged(nameof(CancelNewMatchButtonString));
                RaisePropertyChanged(nameof(CompletedLabelVisibility));
            }
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

        private Visibility _rivalsMatchHistoryViewVisibility = Visibility.Collapsed;
        public Visibility RivalsMatchHistoryViewVisibility
        {
            get { return _rivalsMatchHistoryViewVisibility; }
            set { SetProperty(ref _rivalsMatchHistoryViewVisibility, value); }
        }

        private Visibility _playerMatchHistoryViewVisibility = Visibility.Visible;
        public Visibility PlayerMatchHistoryViewVisibility
        {
            get { return _playerMatchHistoryViewVisibility; }
            set { SetProperty(ref _playerMatchHistoryViewVisibility, value); }
        }

        private Visibility _playerNotesVisibility = Visibility.Visible;
        public Visibility PlayerNotesVisibility
        {
            get { return _playerNotesVisibility; }
            set { SetProperty(ref _playerNotesVisibility, value); }
        }

        // Subwindow Matches
        private Visibility _playerMatchesVisibility = Visibility.Collapsed;
        public Visibility PlayerMatchesVisibility
        {
            get { return _playerMatchesVisibility; }
            set { SetProperty(ref _playerMatchesVisibility, value); }
        }

        public Visibility IsOpponentTagPhVisible
        {
            get => String.IsNullOrEmpty(OpponentName) && ActiveMatch?.Status == MatchStatus.InProgress ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility IsOpponentEloPhVisible
        {
            get => String.IsNullOrEmpty(OpponentElo) && ActiveMatch?.Status == MatchStatus.InProgress ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility IsMyTagPhVisible
        {
            get => String.IsNullOrEmpty(MyName) && ActiveMatch?.Status == MatchStatus.InProgress ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility IsMyEloPhVisible
        {
            get => String.IsNullOrEmpty(MyElo) && ActiveMatch?.Status == MatchStatus.InProgress ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility IsNotesPhVisible
        {
            get => String.IsNullOrEmpty(ActiveMatchNotes) && ActiveMatch?.Status == MatchStatus.InProgress ? Visibility.Visible : Visibility.Collapsed;
        }

        public string CancelNewMatchButtonString
        {
            get
            {
                if (ActiveMatch == null)
                    return " Start\nMatch";

                return ActiveMatch.Status == MatchStatus.InProgress ? "Cancel" : " Start\nMatch";
            }
        }

        private Opponent _activeOpponent;
        public Opponent ActiveOpponent
        {
            get { return _activeOpponent; }
            set
            {
                value.Tag = FilterEllipses(value.Tag);
                SetProperty(ref _activeOpponent, value);
            }
        }

        private Opponent _matchHistoryOpponent;
        public Opponent MatchHistoryOpponent
        {
            get { return _matchHistoryOpponent; }
            set
            {
                value.Tag = FilterEllipses(value.Tag);
                SetProperty(ref _matchHistoryOpponent, value);
            }
        }

        private string _matchHistoryOpponentTagSearch;
        public string MatchHistoryOpponentTagSearch
        {
            get { return _matchHistoryOpponentTagSearch; }
            set
            {
                SetProperty(ref _matchHistoryOpponentTagSearch, value);
            }
        }

        private MatchHistoryCollection _matchHistoryOpponentMatches = new();
        public MatchHistoryCollection MatchHistoryOpponentMatches
        {
            get { return _matchHistoryOpponentMatches; }
            set
            {
                SetProperty(ref _matchHistoryOpponentMatches, value);
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

        private string _myName = String.Empty;
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

        private string _myElo = String.Empty;
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

        private string _opponentElo = String.Empty;
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

        private string _opponentName = String.Empty;
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

        private string _activeMatchNotes = String.Empty;
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

        private string _activityText = String.Empty;
        public string ActivityText
        {
            get { return _activityText; }
            set
            {
                if (value == _activityText)
                {
                    // Ugh, this is a stupid hack to make the ActivtyTextFade Behavior actually redraw the label on duplicate errors
                    SetProperty(ref _activityText, String.Empty);
                }
                SetProperty(ref _activityText, value);
            }
        }

        private string _activeMatchStatusString = "Waiting For Match...";
        public string ActiveMatchStatusString
        {
            get
            {
                if (ActiveMatch is null)
                {
                    return "Waiting For Match...";
                }

                if (ActiveMatch.Status == MatchStatus.Finished)
                {
                    CompletedLabelVisibility = Visibility.Visible;
                }
                else
                {
                    CompletedLabelVisibility = Visibility.Collapsed;
                }

                if (ActiveMatch.Status == MatchStatus.InProgress)
                {
                    return "Match In Progress";
                }
                else
                {
                    return "Waiting For Match...";
                }
            }
            set
            {
                RaisePropertyChanged(nameof(CompletedLabelVisibility));
                SetProperty(ref _activeMatchStatusString, value);
            }
        }

        public string FilterEllipses(string rawInput)
        {
            rawInput = rawInput.Replace("...", ".");
            rawInput = rawInput.Replace("..,", ".");
            rawInput = rawInput.Replace(".,,", ".");
            rawInput = rawInput.Replace(".,", ".");
            rawInput = rawInput.Replace("..", ".");

            return rawInput;
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

        private bool _isShowingRivalsHistory = false; // Player Search is open by default
        public bool IsShowingRivalsHistory
        {
            get { return _isShowingRivalsHistory; }
            set { SetProperty(ref _isShowingRivalsHistory, value); }
        }

        private bool _isShowingPlayerNotes = true;
        public bool IsShowingPlayerNotes
        {
            get { return _isShowingPlayerNotes; }
            set { SetProperty(ref _isShowingPlayerNotes, value); }
        }

        private bool _isMatchTextBoxesReadOnly = true;
        public bool IsMatchTextBoxesReadOnly
        {
            get { return _isMatchTextBoxesReadOnly; }
            set
            {
                SetProperty(ref _isMatchTextBoxesReadOnly, value);
                RaisePropertyChanged(nameof(IsMatchTextBoxesHitTestable));
            }
        }

        private bool _isBestOf3 = true;
        public bool IsBestOf3
        {
            get { return _isBestOf3; }
            set
            {
                GlobalData.BestOf = value ? 3 : GlobalData.BestOf;
                SetProperty(ref _isBestOf3, value);
            }
        }

        private bool _isBestOf5 = false;
        public bool IsBestOf5
        {
            get { return _isBestOf5; }
            set
            {
                GlobalData.BestOf = value ? 5 : GlobalData.BestOf;
                SetProperty(ref _isBestOf5, value);
            }
        }

        public bool IsMatchTextBoxesHitTestable
        {
            get => !IsMatchTextBoxesReadOnly;
        }

        public ObservableCollection<string> AvailableCharacters { get; set; }
        #endregion

        #region Commands

        public DelegateCommand StartMatchCommand { get; private set; }
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
        public DelegateCommand ToggleRivalsMatchHistoryCommand { get; set; }
        public DelegateCommand TogglePlayersMatchHistoryCommand { get; set; }
        public DelegateCommand TogglePlayerNotesCommand { get; set; }
        public DelegateCommand TogglePlayerMatchesCommand { get; set; }
        public DelegateCommand UpdateOpponentMatchHistoryCommand { get; set; }

        #endregion

        public MainWindow_VM()
        {
            StartMatchCommand = new DelegateCommand(() => _ = StartMatch());
            SetMatchWinCommand = new DelegateCommand(() => _ = SetMatchWin());
            SetMatchLoseCommand = new DelegateCommand(() => _ = SetMatchLose());
            SetMatchDiscardCommand = new DelegateCommand(() => _ = SetMatchDiscard());
            TestOcrCommand = new DelegateCommand(() => _ = StartNewMatchFromOCR());
            ShowMyFlyoutCommand = new DelegateCommand(ShowMyFlyout);
            ShowFlyoutCommand = new DelegateCommand(ShowFlyout);
            ShowSecondaryFlyoutCommand = new DelegateCommand(ShowSecondaryFlyout);
            SelectMyCharacterCommand = new DelegateCommand<string>(SelectMyCharacter);
            SelectCharacterCommand = new DelegateCommand<string>(SelectOppCharacter);
            SelectSecondaryCharacterCommand = new DelegateCommand<string>(SelectSecondaryCharacter);
            ShowSettingsWindowCommand = new DelegateCommand(ShowSettingsWindow);
            UpdateOpponentMatchHistoryCommand = new DelegateCommand(GetOpponentSeasonInfo);
            ToggleRivalsMatchHistoryCommand = new DelegateCommand(ToggleRivalsMatchHistory);
            TogglePlayersMatchHistoryCommand = new DelegateCommand(TogglePlayersMatchHistory);
            TogglePlayerNotesCommand = new DelegateCommand(TogglePlayerNotes);
            TogglePlayerMatchesCommand = new DelegateCommand(TogglePlayerMatches);

            Seasons = RivalsORM.GetSeasons();

            GlobalData.BestOf = 3;

            MatchHistoryUpdateEvent.MatchSaved += UpdateMatchHistory;
            try
            {
                SetupImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The match database is either malformed or missing and operation cannot continue (In SetupImages). Please report this bug if possible!\n\n {ex.Message}");
                Application.Current.Shutdown();
            }

#if !DEBUGNODB
            try
            {
                GetMetadata();
                GetMatches(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The match database is either malformed or missing and operation cannot continue (In Metadata and Matches). Please report this bug if possible!\n\n {ex.Message}");
                Application.Current.Shutdown();
            }
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
            if (RivalsORM.GetIsFirstStart() == "1")
            {
                FirstStart firstStart = new FirstStart();

                firstStart.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                firstStart.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                bool? result = firstStart.ShowDialog();
                RivalsORM.SetMetaDataValue("IsFirstStart", "0");
                MyName = RivalsORM.GetPlayerName();
                CurrentPatch = RivalsORM.GetPatchValue();
            }
            else
            {
                MyName = RivalsORM.GetPlayerName();
                CurrentPatch = RivalsORM.GetPatchValue();
            }
        }

        private async Task StartMatch()
        {
            await StartNewMatchFromOCR();
        }

        private bool IsMatchDataValid()
        {
            if (!ActiveMatch.AreCharactersSet())
            {
                MessageBox.Show("You must set character for each valid game before marking a match as won or lost");
                return false;
            }

            if (!ActiveMatch.AreStagesPicked())
            {
                MessageBox.Show("You must set a picked stage for each valid game before marking a match as won or lost");
                return false;
            }

            return true;
        }

        private async Task SetMatchWin()
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                if (!IsMatchDataValid())
                {
                    return;
                }

                if (ActiveMatch.CanBeFlaggedWin())
                {
                    ActiveMatch.IsWon = true;
                    ActivityText = "Match has been recorded as a 'Win'.";
                    ActiveMatch.Status = MatchStatus.Finished;
                    RivalsORM.AddMatch(ActiveMatch);
                    ResetMatchStatus();
                    return;
                }
                else
                {
                    MessageBox.Show("Can't flag a Match as 'Lost' unless you've lost more games than you've won");
                }
            }

            ActivityText = "Match is not active or not valid";
        }

        private async Task SetMatchLose()
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                if (!IsMatchDataValid())
                {
                    return;
                }

                if (!ActiveMatch.CanBeFlaggedLoss())
                {
                    MessageBox.Show("Can't flag a Match as 'Lost' unless you've lost more games than you've won");
                    return;
                }

                try
                {
                    ActiveMatch.IsWon = false;
                    ActivityText = "Match has been recorded as a 'Loss'.";
                    ActiveMatch.Status = MatchStatus.Finished;
                    RivalsORM.AddMatch(ActiveMatch);
                    ResetMatchStatus();
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception: {ex.Message}\n\n {ex}");
                }
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
        }

        private void ResetMatchStatus()
        {
            OpponentName = String.Empty;
            OpponentElo = String.Empty;
            ActiveMatchNotes = "";
            RaisePropertyChanged(nameof(ActiveMatch));
            RaisePropertyChanged(nameof(CancelNewMatchButtonString));
            RaisePropertyChanged(nameof(IsInputLocked));
            RaisePropertyChanged(nameof(ActiveMatchStatusString));
            RefreshPlaceholderVisibility();
            GetMatches();
            return;
        }

        private async Task StartNewMatchFromOCR()
        {
            RivalsOcrResult result = await RivalsOcrEngine.Capture();

            if (result.Match is null)
            {
                MessageBox.Show("Failed to find the Rivals client - is Rivals running?");
                return;
            }

            if (!result.IsValid)
            {
                // MessageBox.Show(result.ErrorText);
            }

            if (!result.IsSalvagable)
            {
                //SystemSounds.Exclamation.Play();
                //ErrorText = "Unrecoverable Capture - Is Rivals running?";
                //return;
            }

            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                // SystemSounds.Exclamation.Play();
                MessageBox.Show("Conclude the active match before starting a new one!");
                return;
            }

            ActiveMatch = result.Match;
            ActiveMatch.Patch = CurrentPatch;
            ActiveMatch.Status = MatchStatus.InProgress;
            ActiveMatch.Me.Character = MyCharacter;

            if (ActiveMatch.Me.EloString == String.Empty || ActiveMatch.Opponent.EloString == String.Empty)
            {
                AudioService.PlayError();
            }

            if (GlobalData.CharacterImageDict.TryGetValue(ActiveMatch.Opponent.Character, out string imagePath))
            {
                SelectedImagePath = imagePath;
            }
            RaisePropertyChanged(nameof(CancelNewMatchButtonString));
            RaisePropertyChanged(nameof(IsInputLocked));
            RaisePropertyChanged(nameof(ActiveMatchStatusString));

            MatchHistoryOpponentTagSearch = ActiveMatch.Opponent.Name;
            GetOpponentSeasonInfo();
            ToggleMatchHistoryView(MatchHistoryView.Players);
            TogglePlayerInformationView(PlayerInformationView.Notes);
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

        private void SetMatchTextBoxReadOnly(MatchStatus activeMatchStatus)
        {
            IsMatchTextBoxesReadOnly = activeMatchStatus != MatchStatus.InProgress;
        }

        private void GetOpponentSeasonInfo()
        {
            MatchHistoryOpponent = new Opponent(MatchHistoryOpponentTagSearch);

            if (String.IsNullOrEmpty(MatchHistoryOpponent.Tag))
            {
                return;
            }

            MatchHistoryOpponentMatches.MatchResults = new ObservableCollection<MatchResult>(
                RivalsORM.AllMatches.Where(r => r.Opponent?.Equals(MatchHistoryOpponent.Tag, StringComparison.OrdinalIgnoreCase) == true).ToList());

            foreach (MatchResult match in MatchHistoryOpponentMatches.MatchResults)
            {
                if (match.Result == "Win")
                {
                    MatchHistoryOpponent.WinsTotal++;

                    if (IsPatchMatch(currentPatch.Substring(0, 3), match.Patch))
                    {
                        MatchHistoryOpponent.WinsSeasonal++;
                    }
                }
                else if (match.Result == "Lose")
                {
                    MatchHistoryOpponent.LosesTotal++;

                    if (IsPatchMatch(currentPatch.Substring(0, 3), match.Patch))
                    {
                        MatchHistoryOpponent.LosesSeasonal++;
                    }
                }

                if (!String.IsNullOrEmpty(match.Notes))
                {
                    MatchHistoryOpponent.Notes.Add(match.Notes);
                    RaisePropertyChanged(MatchHistoryOpponent.NotesLabel);
                }
            }
        }

        private void ToggleMatchHistoryView(MatchHistoryView viewToToggle)
        {
            if (viewToToggle == MatchHistoryView.Players)
            {
                IsShowingPlayerNotes = true;
                PlayerMatchHistoryViewVisibility = Visibility.Visible;
                RivalsMatchHistoryViewVisibility = Visibility.Collapsed;
            }
            else if(viewToToggle == MatchHistoryView.Rivals)
            {
                IsShowingPlayerNotes = false;
                PlayerMatchHistoryViewVisibility = Visibility.Collapsed;
                RivalsMatchHistoryViewVisibility = Visibility.Visible;
            }
        }

        private void TogglePlayerInformationView(PlayerInformationView viewToToggle)
        {
            if (viewToToggle == PlayerInformationView.Notes)
            {
                IsShowingPlayerNotes = true;
                PlayerNotesVisibility = Visibility.Visible;
                PlayerMatchesVisibility = Visibility.Collapsed;
            }
            else if (viewToToggle == PlayerInformationView.Matches)
            {
                IsShowingPlayerNotes = false;
                PlayerNotesVisibility = Visibility.Collapsed;
                PlayerMatchesVisibility = Visibility.Visible;
            }
        }

        public async Task OnCaptureMatchHotKey()
        {
            SecondarySelectedImagePath = "/Resources/CharacterIcons/unknown.png";
            SelectedImagePath = "/Resources/CharacterIcons/unknown.png";
            await StartNewMatchFromOCR();
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

        private void ToggleRivalsMatchHistory()
        {
            IsShowingRivalsHistory = true;
            ToggleMatchHistoryView(MatchHistoryView.Rivals);
        }
        
        private void TogglePlayersMatchHistory()
        {
            IsShowingRivalsHistory = false;
            ToggleMatchHistoryView(MatchHistoryView.Players);
        }

        private void TogglePlayerNotes()
        {
            IsShowingPlayerNotes = true;
            TogglePlayerInformationView(PlayerInformationView.Notes);

        }

        private void TogglePlayerMatches()
        {
            IsShowingPlayerNotes = false;
            TogglePlayerInformationView(PlayerInformationView.Matches);

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
                MessageBox.Show("Character seems to be null?  That's probably a bug");
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
                // SystemSounds.Exclamation.Play();
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
                // SystemSounds.Exclamation.Play();
                ActivityText = "No Active Match - You can't select a character without a match active";
            }

            IsSecondaryFlyoutOpen = false;
        }

        private void ShowSettingsWindow()
        {            
            Settings settingsWindow = new Settings();

            settingsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
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
