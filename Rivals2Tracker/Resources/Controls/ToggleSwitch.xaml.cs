using System.Windows;
using System.Windows.Controls;

namespace Slipstream.Resources.Controls
{
    public partial class ToggleSwitch : UserControl
    {
        public ToggleSwitch()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsOnChanged));

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }

        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch toggleSwitch = (ToggleSwitch)d;
            toggleSwitch.OnToggled?.Invoke(toggleSwitch, new ToggleEventArgs((bool)e.NewValue));
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty OnContentProperty =
            DependencyProperty.Register("OnContent", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("On"));

        public string OnContent
        {
            get { return (string)GetValue(OnContentProperty); }
            set { SetValue(OnContentProperty, value); }
        }

        public static readonly DependencyProperty OffContentProperty =
            DependencyProperty.Register("OffContent", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("Off"));

        public string OffContent
        {
            get { return (string)GetValue(OffContentProperty); }
            set { SetValue(OffContentProperty, value); }
        }

        public event System.EventHandler<ToggleEventArgs> OnToggled;

        public class ToggleEventArgs : System.EventArgs
        {
            public bool IsOn { get; }

            public ToggleEventArgs(bool isOn)
            {
                IsOn = isOn;
            }
        }
    }
}
