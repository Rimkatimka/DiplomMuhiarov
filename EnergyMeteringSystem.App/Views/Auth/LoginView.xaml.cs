using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

            var viewModel = new LoginViewModel();
            DataContext = viewModel;

            // Привязка пароля
            PasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.Password = PasswordBox.Password;
            };
        }
    }
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loginView = new LoginView();
            loginView.Show();
        }
    }
}
