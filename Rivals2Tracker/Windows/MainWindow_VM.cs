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
using Windows.Data.Xml.Dom;
using System.Windows.Controls;
using System.Windows.Input;
using System.Security.Permissions;

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

        public CharacterMetadata ActiveMatchSeason_CharacterData
        {
            set
            {
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

                if ((value as RivalsMatch).Notes.Length == 0)
                {
                    NotesVisibility = Visibility.Visible;
                }
                else
                {
                    NotesVisibility = Visibility.Collapsed;
                }

                SetProperty(ref _activeMatch, value);
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

        private string _myName = "Kadecgos";
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

        private Visibility _notesVisibility = Visibility.Visible;
        public Visibility NotesVisibility
        {
            get { return _notesVisibility; }
            set { SetProperty(ref _notesVisibility, value); }
        }

        private Visibility _completedLabelVisibility = Visibility.Collapsed;
        public Visibility CompletedLabelVisibility
        {
            get { return _completedLabelVisibility; }
            set { SetProperty(ref _completedLabelVisibility, value); }
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
            set
            {
                if (value == "Finished")
                {
                    CompletedLabelVisibility = Visibility.Visible;
                }
                else
                {
                    CompletedLabelVisibility = Visibility.Collapsed;
                }

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

        private string currentPatch = "1.3.2";
        public string CurrentPatch
        {
            get { return currentPatch; }
            set { SetProperty(ref currentPatch, value); }
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

        private bool _isFlyoutOpen = false;
        public bool IsFlyoutOpen
        {
            get { return _isFlyoutOpen; }
            set { SetProperty(ref _isFlyoutOpen, value); }
        }

        private bool _isSecondaryFlyoutOpen = false;
        public bool IsSecondaryFlyoutOpen
        {
            get { return _isSecondaryFlyoutOpen; }
            set { SetProperty(ref _isSecondaryFlyoutOpen, value); }
        }

        public ObservableCollection<string> AvailableCharacters { get; set; }

        public DelegateCommand SetMatchWinCommand { get; private set; }
        public DelegateCommand SetMatchLoseCommand { get; private set; }
        public DelegateCommand SetMatchDiscardCommand { get; private set; }
        public DelegateCommand TestOcrCommand { get; private set; }
        public DelegateCommand ShowFlyoutCommand { get; }
        public DelegateCommand ShowSecondaryFlyoutCommand { get; }

        public DelegateCommand<string> SelectCharacterCommand { get; set; }
        public DelegateCommand<string> SelectSecondaryCharacterCommand { get; set; }

        public MainWindow_VM()
        {
            SetMatchWinCommand = new DelegateCommand(() => _ = SetMatchWin());
            SetMatchLoseCommand = new DelegateCommand(() => _ = SetMatchLose());
            SetMatchDiscardCommand = new DelegateCommand(() => _ = SetMatchDiscard());
            TestOcrCommand = new DelegateCommand(() => _ = DoTheOcr());
            ShowFlyoutCommand = new DelegateCommand(ShowFlyout);
            ShowSecondaryFlyoutCommand = new DelegateCommand(ShowSecondaryFlyout);
            SelectCharacterCommand = new DelegateCommand<string>(SelectCharacter);
            SelectSecondaryCharacterCommand = new DelegateCommand<string>(SelectSecondaryCharacter);

            SetupImages();
            GetMatches(true);

            RaisePropertyChanged("ActiveMatch");
        }

        public void SetupImages()
        {
            AvailableCharacters = new ObservableCollection<string>
            {
                "Forsburn",
                "Loxodont",
                "Clairen",
                "Zetterburn",
                "Wrastor",
                "Fleet",
                "Absa",
                "Olympia",
                "Maypul",
                "Kragg",
                "Ranno",
                "Orcane",
                "Etalus"
            };
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

            if (GlobalData.CharacterImageDict.TryGetValue(ActiveMatch.Opponent.Character, out string imagePath))
            {
                SelectedImagePath = imagePath;
            }

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

        private void GetMatches(bool isOnStart = false)
        {

            MatchResults = RivalsORM.GetAllMatches();
            // LastSeasonResults = GetSeasonData("1.2");
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
                character.WeightedData.DoTheMath();
            }

            return metadatatable;
        }

        private void ShowFlyout()
        {
            IsFlyoutOpen = true;
        }

        private void ShowSecondaryFlyout()
        {
            IsSecondaryFlyoutOpen = true;
        }

        // TODO: This dual-split but almost identical operation on primary and secondary is dumb.  Combine this into a better flow.
        // There is also a duplicate DataTemplate to pull out
        private void SelectCharacter(string character)
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
            
            IsFlyoutOpen = false;
        }
        public void SelectSecondaryCharacter(string character)
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

        // TODO: This is utter nonsense.  This mapping of image directory to characters by String.  Please just fix this...
        // FUTURE ME:  Lmao no thats k - how about I twiddle my thumbs instead ???
        private void OverrideOpponentCharBaseOnImage(string imagePath)
        {
            if (ActiveMatch is not null && ActiveMatch.Status == MatchStatus.InProgress)
            {
                switch (imagePath)
                {
                    case "/Resources/CharacterIcons/absa.png": ActiveMatch.Opponent.Character = "Absa"; break;
                    case "/Resources/CharacterIcons/clairen.png": ActiveMatch.Opponent.Character = "Clairen"; break;
                    case "/Resources/CharacterIcons/etalus.png": ActiveMatch.Opponent.Character = "Etalus"; break;
                    case "/Resources/CharacterIcons/fleet.png": ActiveMatch.Opponent.Character = "Fleet"; break;
                    case "/Resources/CharacterIcons/kragg.png": ActiveMatch.Opponent.Character = "Kragg"; break;
                    case "/Resources/CharacterIcons/loxodont.png": ActiveMatch.Opponent.Character = "Loxodont"; break;
                    case "/Resources/CharacterIcons/maypul.png": ActiveMatch.Opponent.Character = "Maypul"; break;
                    case "/Resources/CharacterIcons/olympia.png": ActiveMatch.Opponent.Character = "Olympia"; break;
                    case "/Resources/CharacterIcons/orcane.png": ActiveMatch.Opponent.Character = "Orcane"; break;
                    case "/Resources/CharacterIcons/ranno.png": ActiveMatch.Opponent.Character = "Ranno"; break;
                    case "/Resources/CharacterIcons/wrastor.png": ActiveMatch.Opponent.Character = "Wrastor"; break;
                    case "/Resources/CharacterIcons/yeen.png": ActiveMatch.Opponent.Character = "Forsburn"; break;
                    case "/Resources/CharacterIcons/zetterburn.png": ActiveMatch.Opponent.Character = "Zetterburn"; break;
                }

                RaisePropertyChanged(nameof(ActiveMatch));
                RaisePropertyChanged(ActiveMatch.Opponent.Character);
            }
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
