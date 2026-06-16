using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnergyMeteringSystem.App.ViewModels.Analytics;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.Views.Analytics
{
    public partial class HierarchyAnalyticsView : UserControl
    {
        public HierarchyAnalyticsView()
        {
            InitializeComponent();
        }

        private void RegionBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is RegionAnalyticsDto region)
            {
                var viewModel = DataContext as HierarchyAnalyticsViewModel;
                if (viewModel != null)
                {
                    viewModel.SelectRegionCommand?.Execute(region);
                }
            }
        }
    }
}