using EnergyMeteringSystem.Core.Interfaces.Services;
using EnergyMeteringSystem.App.Views.Shared;  // ← правильный using
using System.Windows;

namespace EnergyMeteringSystem.App.Services
{
    public class ExportDialogService : IExportDialogService
    {
        public ExportFormatResult ShowFormatDialog()
        {
            var dialog = new ExportFormatDialog();
            dialog.Owner = Application.Current.MainWindow;

            var result = new ExportFormatResult { Success = false };

            if (dialog.ShowDialog() == true)
            {
                result.Success = true;
                result.SelectedFormat = dialog.SelectedFormat;
            }

            return result;
        }
    }
}