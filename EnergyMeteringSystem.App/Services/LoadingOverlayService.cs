using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnergyMeteringSystem.App.Services
{
    public static class LoadingOverlayService
    {
        private static Grid _overlay;
        private static TextBlock _messageText;
        private static Border _overlayBorder;
        private static bool _isInitialized;

        public static void Initialize(Grid overlayGrid, TextBlock messageText)
        {
            _overlay = overlayGrid;
            _messageText = messageText;

            if (_overlay != null)
            {
                _overlay.Visibility = Visibility.Collapsed;
                _overlay.Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));
            }

            _isInitialized = true;
        }

        public static void Show(string message = "Загрузка данных...")
        {
            if (!_isInitialized || _overlay == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_messageText != null)
                    _messageText.Text = message;

                _overlay.Visibility = Visibility.Visible;
                _overlay.Focus();
            });
        }

        public static void Hide()
        {
            if (!_isInitialized || _overlay == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _overlay.Visibility = Visibility.Collapsed;
            });
        }

        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string loadingMessage = "Загрузка данных...")
        {
            Show(loadingMessage);
            try
            {
                return await action();
            }
            finally
            {
                Hide();
            }
        }

        public static async Task ExecuteAsync(Func<Task> action, string loadingMessage = "Загрузка данных...")
        {
            Show(loadingMessage);
            try
            {
                await action();
            }
            finally
            {
                Hide();
            }
        }
    }
}