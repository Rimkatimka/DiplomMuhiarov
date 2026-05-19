using EnergyMeteringSystem.Core.Interfaces.Services;
using System.Windows;

namespace EnergyMeteringSystem.App.Views.Shared
{
    public partial class ExportFormatDialog : Window
    {
        public ExportFormat SelectedFormat { get; private set; }

        public ExportFormatDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            SelectedFormat = ExcelRadio.IsChecked == true
                ? ExportFormat.Excel
                : ExportFormat.Pdf;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}