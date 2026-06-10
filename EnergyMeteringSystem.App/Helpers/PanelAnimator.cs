using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace EnergyMeteringSystem.App.Helpers
{
    public static class PanelAnimator
    {
        private static Storyboard _currentAnimation;

        /// <summary>
        /// Плавное сворачивание/разворачивание панели
        /// </summary>
        public static void AnimatePanel(FrameworkElement panel, double fromWidth, double toWidth, int durationMs = 250)
        {
            var animation = new DoubleAnimation
            {
                From = fromWidth,
                To = toWidth,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animation, panel);
            Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.WidthProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);

            _currentAnimation?.Stop();
            _currentAnimation = storyboard;

            storyboard.Begin();
        }

        /// <summary>
        /// Плавное изменение отступа контента
        /// </summary>
        public static void AnimateMargin(FrameworkElement element, Thickness fromMargin, Thickness toMargin, int durationMs = 250)
        {
            var animation = new ThicknessAnimation
            {
                From = fromMargin,
                To = toMargin,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);

            storyboard.Begin();
        }

        /// <summary>
        /// Анимация поворота кнопки
        /// </summary>
        public static void AnimateButtonRotation(FrameworkElement button, double fromAngle, double toAngle, int durationMs = 250)
        {
            if (button.RenderTransform == null || !(button.RenderTransform is RotateTransform))
            {
                button.RenderTransform = new RotateTransform();
                button.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var animation = new DoubleAnimation
            {
                From = fromAngle,
                To = toAngle,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animation, button);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);

            storyboard.Begin();
        }

        /// <summary>
        /// Плавное изменение видимости с анимацией прозрачности
        /// </summary>
        public static void AnimateVisibility(FrameworkElement element, Visibility visibility, int durationMs = 200)
        {
            if (visibility == Visibility.Visible)
            {
                element.Visibility = Visibility.Visible;
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(durationMs)
                };
                element.BeginAnimation(UIElement.OpacityProperty, animation);
            }
            else
            {
                var animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(durationMs)
                };
                animation.Completed += (s, e) => element.Visibility = Visibility.Collapsed;
                element.BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }
    }
}