using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Slipstream.Resources.Behaviors
{
    public static class TextboxWatermark
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached(
                "Watermark",
                typeof(string),
                typeof(TextboxWatermark),
                new PropertyMetadata(null, OnWatermarkChanged));

        public static readonly DependencyProperty WatermarkVisibilityProperty =
            DependencyProperty.RegisterAttached(
                "WatermarkVisibility",
                typeof(Visibility),
                typeof(TextboxWatermark),
                new PropertyMetadata(Visibility.Collapsed, OnWatermarkVisibilityChanged));

        public static string GetWatermark(DependencyObject obj)
        {
            return (string)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded += UpdateWatermark;
                textBox.TextChanged += UpdateWatermark;
                textBox.GotFocus += UpdateWatermark;
                textBox.LostFocus += UpdateWatermark;
            }
        }

        public static Visibility GetWatermarkVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(WatermarkVisibilityProperty);
        }

        public static void SetWatermarkVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(WatermarkVisibilityProperty, value);
        }

        private static void OnWatermarkVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                UpdateWatermark(textBox, null);
            }
        }

        private static void UpdateWatermark(object sender, RoutedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox == null) return;

            string watermark = GetWatermark(textBox);
            Visibility visibility = GetWatermarkVisibility(textBox);

            if (string.IsNullOrEmpty(textBox.Text) && !textBox.IsFocused && visibility == Visibility.Visible)
            {
                textBox.Background = CreateWatermarkBrush(watermark);
            }
            else
            {
                textBox.Background = Brushes.Transparent;
            }
        }

        private static VisualBrush CreateWatermarkBrush(string watermarkText)
        {
            return new VisualBrush
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                Visual = new TextBlock
                {
                    Text = watermarkText,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(15, 0, 0, 0),
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 22,
                    FontStyle = FontStyles.Italic
                }
            };
        }
    }
}
