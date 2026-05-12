using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace EnergyMeteringSystem.Services
{
    public static class ToastNotificationService
    {
        private static bool _isShowing = false;
        private static object _lock = new object();

        /// <summary>
        /// Показывает всплывающее уведомление рядом с указанным элементом
        /// </summary>
        /// <param name="target">Элемент, рядом с которым показывать уведомление (если null — в правом нижнем углу)</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="durationMs">Длительность показа (мс)</param>
        public static void ShowNear(UIElement target, string message, int durationMs = 2000)
        {
            lock (_lock)
            {
                if (_isShowing) return;
                _isShowing = true;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var popup = new PopupWindow(target, message, durationMs);
                popup.Closed += (s, e) =>
                {
                    lock (_lock) { _isShowing = false; }
                };
                popup.Show();
            });
        }

        private class PopupWindow : Window
        {
            public PopupWindow(UIElement target, string message, int durationMs)
            {
                Width = 300;
                Height = 60;
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                Background = Brushes.Transparent;
                Topmost = true;
                ShowInTaskbar = false;
                ResizeMode = ResizeMode.NoResize;
                Focusable = false;

                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12, 8, 12, 8)
                };

                var textBlock = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap
                };

                border.Child = textBlock;
                Content = border;

                SizeToContent = SizeToContent.WidthAndHeight;

                if (target != null)
                {
                    // Позиционируем относительно target
                    var targetPoint = target.PointToScreen(new Point(0, 0));
                    Left = targetPoint.X + target.RenderSize.Width + 10;
                    Top = targetPoint.Y - Height / 2 + target.RenderSize.Height / 2;
                }
                else
                {
                    // Правый нижний угол экрана
                    Left = SystemParameters.WorkArea.Width - Width - 10;
                    Top = SystemParameters.WorkArea.Height - Height - 10;
                }

                // Автозакрытие
                var timer = new Timer(durationMs);
                timer.Elapsed += (s, e) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    Dispatcher.Invoke(() => Close());
                };
                timer.AutoReset = false;
                timer.Start();
            }
        }
    }
}