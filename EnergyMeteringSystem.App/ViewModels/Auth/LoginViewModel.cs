using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Services.Auth;

namespace EnergyMeteringSystem.App.ViewModels.Auth
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;

        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set
            {
                _ = SetProperty(ref _username, value);
                LoginCommand?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _ = SetProperty(ref _password, value);
                LoginCommand?.RaiseCanExecuteChanged();
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
            _authService = new AuthService();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object parameter)
        {
            Core.Models.DTO.UserDto user = _authService.Login(Username, Password);

            if (user != null)
            {
                Views.Auth.LoginView loginWindow = Application.Current.Windows.OfType<Views.Auth.LoginView>().FirstOrDefault();

                Views.Main.ShellView shellView = new(user);  // ← передаем пользователя
                shellView.Show();
                Application.Current.MainWindow = shellView;

                loginWindow?.Close();
            }
            else
            {
                ErrorMessage = "Неверное имя пользователя или пароль";
            }
        }
    }
}