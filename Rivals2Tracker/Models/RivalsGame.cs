using Prism.Commands;
using Prism.Mvvm;
using Slipstream.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Slipstream.Models
{
    public class RivalsGame : BindableBase
    {
        public int GameNumber { get; set; }

        private RivalsCharacter _myCharacter;
        public RivalsCharacter MyCharacter
        {
            get { return _myCharacter; }
            set { SetProperty(ref _myCharacter, value); }
        }

        private RivalsCharacter _oppCharacter;
        public RivalsCharacter OppCharacter
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

        public GameResult DefaultState;

        private GameResult _result;
        public GameResult Result
        {
            get { return _result; }
            set { SetProperty(ref _result, value); }
        }

        public ObservableCollection<RivalsCharacter> AvailableCharacters { get; set; }
        public RivalsCharacterEnum OpponentCharacter { get; set; }
        public ObservableCollection<RivalsStage> AllStages { get; set; } = new();
        public List<RivalsStage> BannedStages => AllStages.Where(s => s.IsBanned).ToList();
        public RivalsMatch ParentMatch { get; set; }
        public RivalsStage SelectedStage { get; set; }
        public DelegateCommand ShowMyFlyoutCommand { get; }
        public DelegateCommand ShowOppFlyoutCommand { get; }
        public DelegateCommand<RivalsCharacter> SetMyCharacterCommand { get; }
        public DelegateCommand<RivalsCharacter> SetOppCharacterCommand { get; }
        public DelegateCommand SetGameLostCommand { get; }
        public DelegateCommand SetGameWonCommand { get; }


        public RivalsGame(int gameNumber, RivalsCharacter myCharacter, RivalsCharacter oppCharacter, GameResult defaultGameState)
        {
            GameNumber = gameNumber;
            DefaultState = defaultGameState;
            Result = defaultGameState;
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
            SetMyCharacterCommand = new DelegateCommand<RivalsCharacter>(SelectMyCharacter);
            SetOppCharacterCommand = new DelegateCommand<RivalsCharacter>(SelectOppCharacter);
            AvailableCharacters = GlobalData.AllRivals;
        }

        private void SelectMyCharacter(RivalsCharacter character)
        {
            if (character is not null)
            {
                MySelectedImagePath = character.IconRef;
                MyCharacter = character;
                ParentMatch.CascadeCharacterSelection("Me", MyCharacter);
            }

            IsMyFlyoutOpen = false;
        }

        public void AutoSetMyCharacter(RivalsCharacter character)
        {
            MySelectedImagePath = character.IconRef;
            MyCharacter = character;
        }

        private void SelectOppCharacter(RivalsCharacter character)
        {
            if (character is not null)
            {
                OppSelectedImagePath = character.IconRef;
                OppCharacter = character;
                ParentMatch.CascadeCharacterSelection("Opponent", OppCharacter);
            }

            IsOppFlyoutOpen = false;
        }

        public void AutoSetOppCharacter(RivalsCharacter character)
        {
            OppSelectedImagePath = character.IconRef;
            OppCharacter = character;          
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

        public bool ResultIsValid()
        {
            return (Result == GameResult.Lose || Result == GameResult.Win);
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
            Result = Result == GameResult.Lose ? DefaultState : GameResult.Lose;
        }

        private void SetGameWon()
        {
            Result = Result == GameResult.Win ? DefaultState : GameResult.Win;
        }
    }
}
