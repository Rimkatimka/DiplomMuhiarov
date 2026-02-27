using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Admin;

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
