using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Rivals2Tracker.Resources.Behaviors
{
    public static class LabelFadeBehavior
    {
        public static readonly DependencyProperty EnableTextFadeProperty =
            DependencyProperty.RegisterAttached(
                "EnableTextFade",
                typeof(bool),
                typeof(LabelFadeBehavior),
                new PropertyMetadata(false, OnEnableTextFadeChanged));

        public static bool GetEnableTextFade(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableTextFadeProperty);
        }

        public static void SetEnableTextFade(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableTextFadeProperty, value);
        }

        private static void OnEnableTextFadeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Label label)
            {
                if ((bool)e.NewValue)
                {
                    DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(
                        ContentControl.ContentProperty, typeof(Label));
                    descriptor.AddValueChanged(label, OnContentChanged);
                }
                else
                {
                    DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(
                        ContentControl.ContentProperty, typeof(Label));
                    descriptor.RemoveValueChanged(label, OnContentChanged);
                }
            }
        }

        private static void OnContentChanged(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                AnimateLabel(label);
            }
        }

        public static void AnimateLabel(Label label)
        {
            var foregroundColorAnimation = new ColorAnimation
            {
                From = Color.FromRgb(255, 180, 180),
                To = Color.FromRgb(2, 16, 46),
                Duration = new Duration(TimeSpan.FromSeconds(30))
            };

            var backgroundColorAnimation = new ColorAnimation
            {
                From = Color.FromRgb(255, 0, 0),
                To = Color.FromRgb(2, 16, 46),
                Duration = new Duration(TimeSpan.FromSeconds(3))
            };

            SolidColorBrush fgBrush = new SolidColorBrush(Color.FromRgb(169, 0, 0));
            SolidColorBrush bgBrush = new SolidColorBrush(Color.FromRgb(2, 16, 46));

            label.Foreground = fgBrush;
            label.Background = bgBrush;
            fgBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundColorAnimation);
            bgBrush.BeginAnimation(SolidColorBrush.ColorProperty, backgroundColorAnimation);
        }
    }

    // TODO: Combine these into one behavior.
    public static class ActivityFadeBehavior
    {
        public static readonly DependencyProperty EnableTextFadeActivityProperty =
            DependencyProperty.RegisterAttached(
                "EnableTextFadeActivity",
                typeof(bool),
                typeof(ActivityFadeBehavior),
                new PropertyMetadata(false, OnEnableTextFadeChanged));

        public static bool GetEnableTextFadeActivity(DependencyObject obj)  // Changed from GetEnableTextFade
        {
            return (bool)obj.GetValue(EnableTextFadeActivityProperty);
        }

        public static void SetEnableTextFadeActivity(DependencyObject obj, bool value)  // Changed from SetEnableTextFade
        {
            obj.SetValue(EnableTextFadeActivityProperty, value);
        }

        private static void OnEnableTextFadeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Label label)
            {
                DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(
                    ContentControl.ContentProperty, typeof(Label));
                descriptor.AddValueChanged(label, OnContentChanged);
            }
        }

        private static void OnContentChanged(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                AnimateLabel(label);
            }
        }

        public static void AnimateLabel(Label label)
        {
            var foregroundColorAnimation = new ColorAnimation
            {
                From = Color.FromArgb(255, 255, 243, 129),
                To = Color.FromArgb(120, 255, 238, 119),
                Duration = new Duration(TimeSpan.FromSeconds(3))
            };

            var backgroundColorAnimation = new ColorAnimation
            {
                From = Color.FromRgb(255, 0, 0),
                To = Color.FromRgb(2, 16, 46),
                Duration = new Duration(TimeSpan.FromSeconds(3))
            };

            SolidColorBrush fgBrush = new SolidColorBrush(Color.FromRgb(169, 0, 0));

            label.Foreground = fgBrush;
            fgBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundColorAnimation);
        }
    }
}