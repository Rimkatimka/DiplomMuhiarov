using System.Windows;

namespace EnergyMeteringSystem.App
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Views.Auth.LoginView loginView = new();
            loginView.Show();
        }
    }
}
