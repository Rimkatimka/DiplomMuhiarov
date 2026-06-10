using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Analytics;

namespace EnergyMeteringSystem.App.Views.Analytics
{
    public partial class HierarchyAnalyticsView : UserControl
    {
        public HierarchyAnalyticsView()
        {
            InitializeComponent();
            DataContext = new HierarchyAnalyticsViewModel();
        }
    }
}