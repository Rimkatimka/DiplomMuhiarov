using System.Windows.Controls;
using System.Windows.Input;
using EnergyMeteringSystem.App.DTO;
using EnergyMeteringSystem.App.Services;
using EnergyMeteringSystem.App.ViewModels.Readings;

namespace EnergyMeteringSystem.App.Views.Readings
{
    public partial class MeterReadingVerificationView : UserControl
    {
        public MeterReadingVerificationView()
        {
            InitializeComponent();

            var databaseService = new DatabaseService();
            DataContext = new MeterReadingVerificationViewModel(databaseService);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var viewModel = DataContext as MeterReadingVerificationViewModel;

            // Только в пакетном режиме!
            if (viewModel == null || !viewModel.IsBatchMode) return;

            if (dataGrid?.SelectedItem is ReadingVerificationDto selectedReading)
            {
                // Переключаем галочку
                selectedReading.IsSelected = !selectedReading.IsSelected;

                // Обновляем отображение
                dataGrid.Items.Refresh();
            }
        }
    }
}