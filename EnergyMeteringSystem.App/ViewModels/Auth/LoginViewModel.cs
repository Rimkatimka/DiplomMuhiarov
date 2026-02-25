using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Services.Auth;
using EnergyMeteringSystem.App.Commands;

namespace EnergyMeteringSystem.App.ViewModels.Auth
{
    public class LoginViewModel : ViewModelBase
    {

        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                LoginCommand.RaiseCanExecuteChanged(); // ← обновляем команду
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                LoginCommand.RaiseCanExecuteChanged(); // ← обновляем команду
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public RelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object parameter) // ← с object
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object parameter) // ← с object
        {
            var user = AuthService.Login(Username, Password);

            if (user != null)
            {
                var shellView = new Views.Main.ShellView();
                shellView.Show();
                Application.Current.Windows[0]?.Close();
            }
            else
            {
                ErrorMessage = "Неверное имя пользователя или пароль";
            }
        }
    }
}
