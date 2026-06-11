using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Auth;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Auth
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _viewModel;
        private bool _isPasswordVisible = false;

        public LoginView()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Привязка пароля из PasswordBox к ViewModel
            PasswordBox.PasswordChanged += (s, e) =>
            {
                _viewModel.Password = PasswordBox.Password;
                if (VisiblePasswordTextBox.Text != PasswordBox.Password)
                {
                    VisiblePasswordTextBox.Text = PasswordBox.Password;
                }
            };

            // Привязка для видимого поля
            VisiblePasswordTextBox.TextChanged += (s, e) =>
            {
                if (_isPasswordVisible && _viewModel.Password != VisiblePasswordTextBox.Text)
                {
                    _viewModel.Password = VisiblePasswordTextBox.Text;
                    PasswordBox.Password = VisiblePasswordTextBox.Text;
                }
            };
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            var button = sender as Button;
            var textBlock = button?.Content as TextBlock;

            if (_isPasswordVisible)
            {
                VisiblePasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordTextBox.Visibility = Visibility.Visible;
                if (textBlock != null) textBlock.Text = "👁‍🗨";
                button.ToolTip = "Скрыть пароль";
            }
            else
            {
                PasswordBox.Password = VisiblePasswordTextBox.Text;
                VisiblePasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                if (textBlock != null) textBlock.Text = "👁";
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

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _viewModel.LoginCommand.CanExecute(null))
            {
                _viewModel.LoginCommand.Execute(null);
            }
        }
    }
}