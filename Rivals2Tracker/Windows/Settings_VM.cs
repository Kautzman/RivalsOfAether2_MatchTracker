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

namespace Rivals2Tracker
{
    class Settings_VM : BindableBase
    {
        public Action Close { get; set; }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                SetProperty(ref _playerName, value);
            }
        }

        private string _patch;
        public string Patch
        {
            get { return _patch; }
            set
            {
                SetProperty(ref _patch, value);
            }
        }
        public DelegateCommand SaveAndCloseCommand { get; private set; }

        public Settings_VM()
        {
            SaveAndCloseCommand = new DelegateCommand(SaveAndClose);
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
            Close?.Invoke();
        }
    }
}
