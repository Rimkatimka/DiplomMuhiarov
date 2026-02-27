using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Admin;

namespace EnergyMeteringSystem.App.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для UserManagementView.xaml
    /// </summary>
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
            DataContext = new UserManagementViewModel();
        }
    }
}
