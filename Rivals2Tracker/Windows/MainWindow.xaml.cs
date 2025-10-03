using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using kWindows.Core;
using Slipstream.Data;
using Slipstream.HotkeyHandler;
using Slipstream.Services;

namespace Slipstream
{
    public partial class MainWindow : kWindow
    {
        WindowInteropHelper helper;
        nint hwnd;

        public MainWindow()
        {
            try
            {
                GlobalData.HotKeyCode = RivalsORM.GetMatchHotKey();
                GlobalData.ModifierCode = RivalsORM.GetMatchHotKeyModifier();
                GlobalData.IsPlayAudio = RivalsORM.GetPlayAudioValue() == 1;
                GlobalData.IsSaveCaptures = RivalsORM.GetSaveCapturesValue() == 1;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("[ERROR] Failed to retrieve Hotkey or the Hotkey modifer code -- setting to default (Scroll Lock)");
                GlobalData.HotKeyCode = 145; // Scroll Lock
                GlobalData.ModifierCode = 0;
            }
            AudioService.Initialize();
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            helper = new WindowInteropHelper(this);
            hwnd = helper.Handle;

            GlobalData.MainWindowHandle = hwnd;

            HotKeyService.RegisterHotKey(GlobalData.ModifierCode, GlobalData.HotKeyCode);

            HwndSource source = HwndSource.FromHwnd(GlobalData.MainWindowHandle);
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
    }
}
