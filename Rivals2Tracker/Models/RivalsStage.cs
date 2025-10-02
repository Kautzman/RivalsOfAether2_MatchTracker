using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private void ToggleBanStage()
        {
            IsBanned = !IsBanned;
            IsSelected = false;
        }

        private void ToggleSelectStage()
        {
            IsSelected = !IsSelected;
            IsBanned = false;
        }
    }
}
