using EnergyMeteringSystem.App.Services;
using EnergyMeteringSystem.Services.Export;
using System.Windows;

namespace EnergyMeteringSystem.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var exportDialogService = new ExportDialogService();
            var exportService = new ExportService(exportDialogService);

            var loginView = new Views.Auth.LoginView();
            loginView.Show();
        }
    }
}