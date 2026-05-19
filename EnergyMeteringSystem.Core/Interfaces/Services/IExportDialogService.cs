namespace EnergyMeteringSystem.Core.Interfaces.Services
{
    public enum ExportFormat
    {
        Excel,
        Pdf
    }

    public interface IExportDialogService
    {
        ExportFormatResult ShowFormatDialog();
    }

    public class ExportFormatResult
    {
        public bool Success { get; set; }
        public ExportFormat SelectedFormat { get; set; }
    }
}