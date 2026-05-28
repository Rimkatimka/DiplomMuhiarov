using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Main;

namespace EnergyMeteringSystem.App.Views.Main
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}