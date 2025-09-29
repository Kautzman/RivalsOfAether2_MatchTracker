using kWindows.Core;
using Rivals2Tracker.Data;
using Rivals2Tracker.Services;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Rivals2Tracker
{
    public partial class Settings : kWindow
    {
        Settings_VM thisVM;
        bool _isCapturingHotKey = false;

        public Settings()
        {
            InitializeComponent();

            if (DataContext is Settings_VM vm)
            {
                thisVM = vm;
                vm.Close = () => this.Close();
            }
        }

        private void SetHotKeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCapturingHotKey == true)
            {
                thisVM.BoundKeyCode = HotKeyService.GetReadableShortcutFromUints(GlobalData.ModifierCode, GlobalData.HotKeyCode);
                _isCapturingHotKey = false;
                thisVM.SetKeybindText = "Set Keybind ...";
                return;
            }

            _isCapturingHotKey = true;
            thisVM.SetKeybindText = "Cancel";
            thisVM.BoundKeyCode = "Press desired key combination...";
            this.PreviewKeyDown += CaptureHotKey_KeyDown;
        }

        private void CaptureHotKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isCapturingHotKey)
                return;

            ModifierKeys modifiers = e.KeyboardDevice.Modifiers;
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key is Key.LeftCtrl or Key.RightCtrl or
                Key.LeftAlt or Key.RightAlt or
                Key.LeftShift or Key.RightShift or
                Key.LWin or Key.RWin)
            {
                return;
            }

            _isCapturingHotKey = false;
            this.PreviewKeyDown -= CaptureHotKey_KeyDown;
            e.Handled = true;

            RivalsORM.SaveHotKeyToDatabase(modifiers, key);
            HotKeyService.RegisterNewHotKey(modifiers, key);
            thisVM.BoundKeyCode = HotKeyService.GetReadableShortcut(modifiers, key);
            thisVM.SetKeybindText = "Set Keybind ...";
        }
    }
}
