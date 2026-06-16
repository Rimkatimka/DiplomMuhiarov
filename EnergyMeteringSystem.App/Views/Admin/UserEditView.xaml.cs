using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Admin;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Admin
{
    public partial class UserEditView : UserControl
    {
        private readonly UserEditViewModel _viewModel;

        public UserEditView(UserEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            viewModel.OnSaved += (s, e) =>
            {
                // Закрываем окно
                var window = Window.GetWindow(this);
                window?.Close();
            };

            viewModel.OnCancelled += (s, e) =>
            {
                var window = Window.GetWindow(this);
                window?.Close();
            };
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictEmailCharacters(sender, e);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputValidator.ValidateEmailOnLostFocus(sender, e);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputValidator.ResetEmailBorderOnFocus(sender, e);
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && _viewModel != null)
            {
                _viewModel.NewPassword = passwordBox.Password;
                ValidatePasswords();
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && _viewModel != null)
            {
                _viewModel.ConfirmPassword = passwordBox.Password;
                ValidatePasswords();
            }
        }

        private void ValidatePasswords()
        {
            if (_viewModel == null) return;

            string newPass = _viewModel.NewPassword;
            string confirmPass = _viewModel.ConfirmPassword;

            if (!string.IsNullOrEmpty(newPass) || !string.IsNullOrEmpty(confirmPass))
            {
                if (newPass != confirmPass)
                {
                    _viewModel.PasswordError = "Пароли не совпадают";
                    _viewModel.HasPasswordError = true;
                }
                else if (string.IsNullOrEmpty(newPass))
                {
                    _viewModel.PasswordError = "Введите новый пароль";
                    _viewModel.HasPasswordError = true;
                }
                else if (newPass.Length < 3)
                {
                    _viewModel.PasswordError = "Пароль должен содержать минимум 3 символа";
                    _viewModel.HasPasswordError = true;
                }
                else
                {
                    _viewModel.PasswordError = "";
                    _viewModel.HasPasswordError = false;
                }
            }
            else
            {
                _viewModel.PasswordError = "";
                _viewModel.HasPasswordError = false;
            }
        }
    }
}