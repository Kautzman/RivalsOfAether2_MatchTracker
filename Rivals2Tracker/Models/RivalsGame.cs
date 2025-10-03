using Prism.Commands;
using Prism.Mvvm;
using Slipstream.Data;
using System.Collections.ObjectModel;
using System.Media;

namespace Slipstream.Models
{
    public class RivalsGame : BindableBase
    {
        public int GameNumber { get; set; }
        // public RivalsCharacter MyCharacter { get; set; }

        private string _myCharacter;
        public string MyCharacter
        {
            get { return _myCharacter; }
            set { SetProperty(ref _myCharacter, value); }
        }

        private string _oppCharacter;
        public string OppCharacter
        {
            get { return _oppCharacter; }
            set { SetProperty(ref _oppCharacter, value); }
        }


        private string _mySelectedImagePath = "/Resources/CharacterIcons/unknown.png";
        public string MySelectedImagePath
        {
            get { return _mySelectedImagePath; }
            set { SetProperty(ref _mySelectedImagePath, value); }
        }

        private string _oppSelectedImagePath = "/Resources/CharacterIcons/unknown.png";
        public string OppSelectedImagePath
        {
            get { return _oppSelectedImagePath; }
            set { SetProperty(ref _oppSelectedImagePath, value); }
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

        private GameResult _result = GameResult.InProgress;
        public GameResult Result
        {
            get { return _result; }
            set { SetProperty(ref _result, value); }
        }

        public ObservableCollection<string> AvailableCharacters { get; set; }

        public RivalsCharacter OpponentCharacter { get; set; }
        public ObservableCollection<RivalsStage> AllStages { get; set; } = new();
        public ObservableCollection<RivalsStage> BannedStages { get; set; } = new ObservableCollection<RivalsStage>();
        public RivalsStage SelectedStage { get; set; }

        public DelegateCommand ShowMyFlyoutCommand { get; }
        public DelegateCommand ShowOppFlyoutCommand { get; }
        public DelegateCommand<string> SetMyCharacterCommand { get; }
        public DelegateCommand<string> SetOppCharacterCommand { get; }
        public DelegateCommand SetGameLostCommand { get; }
        public DelegateCommand SetGameWonCommand { get; }


        public RivalsGame(int gameNumber, string myCharacter, string oppCharacter)
        {
            GameNumber = gameNumber;
            SelectMyCharacter(myCharacter);
            SelectOppCharacter(oppCharacter);

            foreach (RivalsStage stage in GlobalData.AllStages)
            {
                RivalsStage newStage = new RivalsStage(stage.StageName, stage.StagePicture, stage.IsCounterpick);
                newStage.SetParentGame(this);

                if (GameNumber == 1)
                {
                    newStage.IsBanned = newStage.IsCounterpick;
                }

                AllStages.Add(newStage);
            }

            ShowMyFlyoutCommand = new DelegateCommand(ShowMyFlyout);
            ShowOppFlyoutCommand = new DelegateCommand(ShowOppFlyout);
            SetGameLostCommand = new DelegateCommand(SetGameLost);
            SetGameWonCommand = new DelegateCommand(SetGameWon);
            SetMyCharacterCommand = new DelegateCommand<string>(SelectMyCharacter);
            SetOppCharacterCommand = new DelegateCommand<string>(SelectOppCharacter);

            AvailableCharacters = new ObservableCollection<string>(GlobalData.AllCharacters);
        }


        private void SelectMyCharacter(string character)
        {
            if (!string.IsNullOrEmpty(character))
            {
                if (GlobalData.CharacterImageDict.TryGetValue(character, out string imagePath))
                {
                    MySelectedImagePath = imagePath;
                    MyCharacter = character;
                }
            }
            else
            {
                SystemSounds.Exclamation.Play();
            }

            IsMyFlyoutOpen = false;
        }
        private void SelectOppCharacter(string character)
        {
            if (!string.IsNullOrEmpty(character))
            {
                if (GlobalData.CharacterImageDict.TryGetValue(character, out string imagePath))
                {
                    OppSelectedImagePath = imagePath;
                    OppCharacter = character;
                }
            }
            else
            {
                SystemSounds.Exclamation.Play();
            }

            IsOppFlyoutOpen = false;
        }

        public void ClearStageSelection()
        {
            foreach (RivalsStage stage in AllStages)
            {
                if (stage.IsSelected)
                {
                    stage.IsSelected = false;                
                }
            }
        }


        private void ShowMyFlyout()
        {
            IsMyFlyoutOpen = true;
        }

        private void ShowOppFlyout()
        {
            IsOppFlyoutOpen = true;
        }

        private void SetGameLost()
        {
            Result = Result == GameResult.Lose ? GameResult.InProgress : GameResult.Lose;
        }

        private void SetGameWon()
        {
            Result = Result == GameResult.Win ? GameResult.InProgress : GameResult.Win;
        }
    }
}
