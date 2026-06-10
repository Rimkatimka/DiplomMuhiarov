using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Analytics
{
    public partial class AnalyticsView : UserControl
    {
        public AnalyticsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Analytics.AnalyticsViewModel();
        }
    }
}