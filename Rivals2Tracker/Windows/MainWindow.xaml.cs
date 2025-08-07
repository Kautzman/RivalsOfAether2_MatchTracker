using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            var hwnd = helper.Handle;

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
    }
}
