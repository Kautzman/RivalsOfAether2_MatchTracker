using Prism.Commands;
using Rivals2Tracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Rivals2Tracker.Data;
using System.Windows.Input;
using Rivals2Tracker.Services;
using System.Windows.Forms;

namespace Rivals2Tracker
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
                GlobalData.SaveCaptures = value;
                SetProperty(ref _saveCapturesIsChecked, value);
            }
        }

        public DelegateCommand SaveAndCloseCommand { get; private set; }

        public Settings_VM()
        {
            BoundKeyCode = HotKeyService.GetReadableShortcutFromUints(GlobalData.ModifierCode, GlobalData.HotKeyCode);

            SaveAndCloseCommand = new DelegateCommand(SaveAndClose);
            GetData();
        }

        private void GetData()
        {
            PlayerName = RivalsORM.GetPlayerName();
            Patch = RivalsORM.GetPatchValue();
        }

        private string GetCurrentKeybind()
        {
            Key key = KeyInterop.KeyFromVirtualKey((int)GlobalData.HotKeyCode);
            return new KeyConverter().ConvertToString(key);
        }

        private void SaveAndClose()
        {
            RivalsORM.SetMetaDataValue("Patch", Patch);
            RivalsORM.SetMetaDataValue("PlayerName", PlayerName);
            Close?.Invoke();
        }
    }
}
