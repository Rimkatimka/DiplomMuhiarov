using EnergyMeteringSystem.App.ViewModels.Admin;
using EnergyMeteringSystem.Services;
using System.Windows;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для BackupView.xaml
    /// </summary>
    public partial class BackupView : UserControl
    {
        public BackupView()
        {
            InitializeComponent();
            DataContext = new BackupViewModel();
        }
    }
}
