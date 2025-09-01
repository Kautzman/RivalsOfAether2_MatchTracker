using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using kWindows.Core;
using Rivals2Tracker.HotkeyHandler;

namespace Rivals2Tracker
{
    public partial class MainWindow : kWindow
    {

        WindowInteropHelper helper;
        nint hwnd;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            helper = new WindowInteropHelper(this);
            hwnd = helper.Handle;

            HotKeyManager.RegisterHotKey(hwnd, 1, HotKeyManager.MOD_NONE, (uint)KeyInterop.VirtualKeyFromKey(Key.Scroll));

            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == HotKeyManager.WM_HOTKEY)
            {
                int id = wParam.ToInt32();

                if (id == 1)
                {
                    if (DataContext is MainWindow_VM vm)
                    {
                        vm.OnCaptureMatchHotKey();
                    }
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        private void RegisterNewHotkey(uint keycode)
        {
            // There is an assumption here that '1' is the index of the hotkey being tracked.  I might not even need to 'unregister' it and instead just
            // Can overwrite it with RegsiterHotKey() again.

            HotKeyManager.UnregisterHotKey(hwnd, 1);

            // Somehow pass a captured key code from VM to here (uint keycode)
            // ?? Do I need to do this in codebehind?  Seems like HotKeyManager is a singleton that's globally available.

            HotKeyManager.RegisterHotKey(hwnd, 1, HotKeyManager.MOD_NONE, keycode);
        }

        private void OuterBorder_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
                    RadiusX = border.CornerRadius.TopLeft,
                    RadiusY = border.CornerRadius.TopLeft
                };
            }
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
                FocusDump.Focus();
            }
        }

        private void ImageSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            ImageFlyout.IsOpen = true;
        }
    }
}
