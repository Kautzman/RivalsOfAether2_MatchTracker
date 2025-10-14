using Prism.Commands;
using Prism.Mvvm;
using Slipstream.Data;
using System.Security.Policy;

namespace Slipstream.Models
{
    public class RivalsStage : BindableBase
    {
        public long ID { get; set; }
        public string StageName { get; set; }
        public string StageRefHorizontal { get; set; }
        public string StageRefVertical { get; set; }
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


        public RivalsStage(RivalsStageRecord record)
        {
            ID = record.ID;
            StageName = record.Name;
            StageRefHorizontal = record.ImageRefH;
            StageRefVertical = record.ImageRefV;
            IsCounterpick = record.IsCounterpick == 1;
        }

        public RivalsStage(long id, string name, string imageV, bool isCounterpick)
        {
            ID = id;
            StageName = name;
            StageRefVertical = imageV;
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
            if (ParentGame.Result == GameResultEnum.Unplayed)
            {
                ParentGame.Result = GameResultEnum.InProgress;
            }
        }
    }

    public record RivalsStageRecord
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string ImageRefV { get; set; }
        public string ImageRefH { get; set; }
        public long IsCounterpick { get; set; }
    }
}
