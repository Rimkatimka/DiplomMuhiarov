using EnergyMeteringSystem.App.ViewModels.Main;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Main
{
    /// <summary>
    /// Логика взаимодействия для DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}
