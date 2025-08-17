using System.Windows;
using System.Windows.Controls;

namespace Rivals2Tracker.Resources.Controls
{
    public partial class ToggleSwitch : UserControl
    {
        public ToggleSwitch()
        {
            InitializeComponent();
        }

        // IsOn Property
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
            var toggleSwitch = (ToggleSwitch)d;
            toggleSwitch.OnToggled?.Invoke(toggleSwitch, new ToggleEventArgs((bool)e.NewValue));
        }

        // Header Property
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // OnContent Property
        public static readonly DependencyProperty OnContentProperty =
            DependencyProperty.Register("OnContent", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("On"));

        public string OnContent
        {
            get { return (string)GetValue(OnContentProperty); }
            set { SetValue(OnContentProperty, value); }
        }

        // OffContent Property
        public static readonly DependencyProperty OffContentProperty =
            DependencyProperty.Register("OffContent", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("Off"));

        public string OffContent
        {
            get { return (string)GetValue(OffContentProperty); }
            set { SetValue(OffContentProperty, value); }
        }

        // Toggled Event
        public event System.EventHandler<ToggleEventArgs> OnToggled;

        // Custom EventArgs for toggle events
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
