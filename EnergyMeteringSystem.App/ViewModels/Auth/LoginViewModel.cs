using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
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
                SetProperty(ref _username, value);
                (LoginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                (LoginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }        

        public AsyncRelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, CanExecuteLogin);
        }

        private bool CanExecuteLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private async Task ExecuteLoginAsync()
        {
            await ExecuteAsync(async () =>
            {
                UserDto user = await Task.Run(() => _authService.Login(Username, Password));

                if (user != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var loginWindow = Application.Current.Windows.OfType<Views.Auth.LoginView>().FirstOrDefault();
                        var shellView = new Views.Main.ShellView(user);
                        shellView.Show();
                        Application.Current.MainWindow = shellView;
                        loginWindow?.Close();
                    });
                }
                else
                {
                    ErrorMessage = "Неверное имя пользователя или пароль";
                }
            }, "Ошибка входа");
        }
    }
}