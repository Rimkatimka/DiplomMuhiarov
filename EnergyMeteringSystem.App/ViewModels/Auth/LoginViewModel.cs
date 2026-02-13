using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Services.Auth;
using GalaSoft.MvvmLight.Command;

namespace EnergyMeteringSystem.App.ViewModels.Auth
{
    internal class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;

        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public RelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin()
        {
            var user = _authService.Login(Username, Password);

            if (user != null)
            {
                // Открываем главное окно
                var shellView = new Views.Main.ShellView();
                shellView.Show();

                // Закрываем окно входа
                Application.Current.Windows[0]?.Close();
            }
            else
            {
                ErrorMessage = "Неверное имя пользователя или пароль";
                Password = string.Empty;
            }
        }
    }
}
