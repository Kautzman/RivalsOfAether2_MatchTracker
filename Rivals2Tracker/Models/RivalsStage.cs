using Prism.Commands;
using Prism.Mvvm;
using Slipstream.Data;

namespace Slipstream.Models
{
    public class RivalsStage : BindableBase
    {
        public string StageName { get; set; }
        public string StagePicture { get; set; }
        public bool IsCounterpick { get; set; }

        private bool _isBanned = false;
        public bool IsBanned
        {
            get => _isBanned;
            set => SetProperty(ref _isBanned, value);
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public RivalsGame ParentGame { get; set; }

        public DelegateCommand ToggleBanStageCommand { get; set; }
        public DelegateCommand ToggleSelectStageCommand { get; set; }


        public RivalsStage(string stageName, string stagePicture, bool isCounterpick)
        {
            StageName = stageName;
            StagePicture = stagePicture;
            IsCounterpick = isCounterpick;

            ToggleBanStageCommand = new DelegateCommand(ToggleBanStage);
            ToggleSelectStageCommand = new DelegateCommand(ToggleSelectStage);
        }

        public void SetParentGame(RivalsGame parentGame)
        {
            ParentGame = parentGame;
        }

        private void ToggleBanStage()
        {
            IsBanned = !IsBanned;
            IsSelected = false;
            FlagGameAsActive();
        }

        private void ToggleSelectStage()
        {
            if (IsSelected)
            {
                IsSelected = false;
                return;
            }

            ParentGame.ClearStageSelection();
            IsSelected = !IsSelected;
            IsBanned = false;
            ParentGame.SelectedStage = this;
            FlagGameAsActive();

        }

        private void FlagGameAsActive()
        {
            if (ParentGame.Result == GameResult.Unplayed)
            {
                ParentGame.Result = GameResult.InProgress;
            }
        }
    }
}
