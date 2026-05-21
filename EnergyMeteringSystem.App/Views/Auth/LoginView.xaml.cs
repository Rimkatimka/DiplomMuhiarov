using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Auth;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private void LoginTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictLoginInput(sender, e);
        }
        // Перетаскивание окна
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // Закрытие окна
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
