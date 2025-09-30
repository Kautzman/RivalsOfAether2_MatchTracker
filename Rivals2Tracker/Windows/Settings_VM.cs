using Prism.Commands;
using System;
using System.Linq;
using Prism.Mvvm;
using Slipstream.Data;
using Slipstream.Services;
using System.Windows.Forms;
using Slipstream.Windows;
using System.Windows;

namespace Slipstream
{
    class Settings_VM : BindableBase
    {
        public Action Close { get; set; }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set { SetProperty(ref _playerName, value); }
        }

        private string _patch;
        public string Patch
        {
            get { return _patch; }
            set { SetProperty(ref _patch, value); }
        }

        private string _SetKeybindText = "Set Keybind ...";
        public string SetKeybindText
        {
            get { return _SetKeybindText; }
            set { SetProperty(ref _SetKeybindText, value); }
        }

        private string _boundKeyCode;
        public string BoundKeyCode
        {
            get { return _boundKeyCode; }
            set { SetProperty(ref _boundKeyCode, value); }
        }

        private bool _saveCapturesIsChecked;
        public bool SaveCapturesIsChecked
        {
            get { return _saveCapturesIsChecked; }
            set
            {
                GlobalData.IsSaveCaptures = value;
                SetProperty(ref _saveCapturesIsChecked, value);
            }
        }

        private bool _EnableAudioIsChecked;
        public bool EnableAudioIsChecked
        {
            get { return _EnableAudioIsChecked; }
            set
            {
                GlobalData.IsPlayAudio = value;
                SetProperty(ref _EnableAudioIsChecked, value);
            }
        }

        public DelegateCommand SaveAndCloseCommand { get; private set; }
        public DelegateCommand ShowTutorialCommand { get; private set; }

        public Settings_VM()
        {
            EnableAudioIsChecked = GlobalData.IsPlayAudio;
            SaveCapturesIsChecked = GlobalData.IsSaveCaptures;
            BoundKeyCode = HotKeyService.GetReadableShortcutFromUints(GlobalData.ModifierCode, GlobalData.HotKeyCode);

            SaveAndCloseCommand = new DelegateCommand(SaveAndClose);
            ShowTutorialCommand = new DelegateCommand(ShowTutorial);
            GetData();
        }

        private void GetData()
        {
            PlayerName = RivalsORM.GetPlayerName();
            Patch = RivalsORM.GetPatchValue();
        }

        private void SaveAndClose()
        {
            RivalsORM.SetMetaDataValue("Patch", Patch);
            RivalsORM.SetMetaDataValue("PlayerName", PlayerName);
            RivalsORM.SetMetaDataValue("PlayAudio", EnableAudioIsChecked ? "1" : "0");
            RivalsORM.SetMetaDataValue("SaveCaptures", SaveCapturesIsChecked ? "1" : "0");
            Close?.Invoke();
        }

        private void ShowTutorial()
        {
            FirstStart firstStart = new FirstStart();

            firstStart.Owner = System.Windows.Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            firstStart.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            bool? result = firstStart.ShowDialog();
        }
    }
}
