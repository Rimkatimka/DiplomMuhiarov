using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Auth;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Auth
{
    public partial class LoginView : Window
    {
        private bool _isPasswordVisible = false;

        public LoginView()
        {
            InitializeComponent();

            LoginViewModel viewModel = new();
            DataContext = viewModel;

            // Привязка пароля
            PasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.Password = PasswordBox.Password;
                if (VisiblePasswordTextBox.Text != PasswordBox.Password)
                {
                    VisiblePasswordTextBox.Text = PasswordBox.Password;
                }
            };

            // Привязка для видимого поля
            VisiblePasswordTextBox.TextChanged += (s, e) =>
            {
                if (_isPasswordVisible && viewModel.Password != VisiblePasswordTextBox.Text)
                {
                    viewModel.Password = VisiblePasswordTextBox.Text;
                    PasswordBox.Password = VisiblePasswordTextBox.Text;
                }
            };
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            var button = sender as Button;
            var textBlock = button.Content as TextBlock;

            if (_isPasswordVisible)
            {
                // Показываем пароль
                VisiblePasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordTextBox.Visibility = Visibility.Visible;

                // Меняем иконку на "глаз перечёркнутый" или меняем текст
                if (textBlock != null)
                {
                    textBlock.Text = "👁‍🗨";
                }
                button.ToolTip = "Скрыть пароль";
            }
            else
            {
                // Скрываем пароль
                PasswordBox.Password = VisiblePasswordTextBox.Text;
                VisiblePasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;

                // Возвращаем обычный глаз
                if (textBlock != null)
                {
                    textBlock.Text = "👁";
                }
                button.ToolTip = "Показать пароль";
            }
        }

        private void LoginTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictLoginInput(sender, e);
        }

        private void LoginTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }

        private void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }
    }
}