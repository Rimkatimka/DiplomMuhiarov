using System.Windows.Controls;
using System.Windows.Input;
using EnergyMeteringSystem.App.ViewModels.Readings;
using EnergyMeteringSystem.Data.DTO;

namespace EnergyMeteringSystem.App.Views.Readings
{
    public partial class MeterReadingVerificationView : UserControl
    {
        public MeterReadingVerificationView()
        {
            InitializeComponent();
            DataContext = new MeterReadingVerificationViewModel();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var viewModel = DataContext as MeterReadingVerificationViewModel;

            if (viewModel == null || !viewModel.IsBatchMode) return;

            if (dataGrid?.SelectedItem is VerificationDto selectedReading)
            {
                selectedReading.IsSelected = !selectedReading.IsSelected;
                dataGrid.Items.Refresh();
            }
        }
    }
}