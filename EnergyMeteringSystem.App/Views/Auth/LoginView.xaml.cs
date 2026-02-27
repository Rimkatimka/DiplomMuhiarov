using System.Windows;
using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Auth;

namespace EnergyMeteringSystem.App.Views.Auth
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            LoginViewModel viewModel = new();
            DataContext = viewModel;

            // Привязка пароля
            PasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.Password = PasswordBox.Password;
                System.Diagnostics.Debug.WriteLine($"Password set to: '{viewModel.Password}'");
            };

        }
    }
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoginView loginView = new();
            loginView.Show();
        }
    }
}
