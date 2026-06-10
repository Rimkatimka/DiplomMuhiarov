using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EnergyMeteringSystem.Core.Interfaces.Services;

namespace EnergyMeteringSystem.Services.Export
{
    public class ExportService
    {
        private readonly IExportDialogService _dialogService;

        public ExportService(IExportDialogService dialogService = null)
        {
            _dialogService = dialogService;
        }

        /// <summary>
        /// Экспорт с диалогом выбора формата (если диалог передан)
        /// </summary>
        public void ExportWithFormatDialog(DataGrid dataGrid, string fileName)
        {
            if (_dialogService != null)
            {
                var result = _dialogService.ShowFormatDialog();
                if (result.Success)
                {
                    ExportDataGrid(dataGrid, fileName, result.SelectedFormat);
                }
            }
            else
            {
                // Если диалог не передан - сразу в Excel
                ExportToExcel(dataGrid, fileName);
            }
        }

        public void ExportDataGrid(DataGrid dataGrid, string fileName, ExportFormat format)
        {
            if (dataGrid?.ItemsSource == null || !(dataGrid.ItemsSource as System.Collections.IEnumerable)?.GetEnumerator().MoveNext() == true)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (format == ExportFormat.Excel)
            {
                ExportToExcel(dataGrid, fileName);
            }
            else
            {
                ExportToPdf(dataGrid, fileName);
            }
        }

        private void ExportToExcel(DataGrid dataGrid, string fileName)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"{fileName}_{DateTime.Now:dd.MM.yyyy_HHmm}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Report");

                        // Заголовки
                        for (int i = 0; i < dataGrid.Columns.Count; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = dataGrid.Columns[i].Header.ToString();
                            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                        }

                        // Данные
                        var items = dataGrid.ItemsSource as System.Collections.IEnumerable;
                        int row = 2;
                        foreach (var item in items)
                        {
                            for (int col = 0; col < dataGrid.Columns.Count; col++)
                            {
                                if (dataGrid.Columns[col] is DataGridTextColumn textColumn && textColumn.Binding != null)
                                {
                                    var binding = textColumn.Binding as System.Windows.Data.Binding;
                                    var prop = item.GetType().GetProperty(binding?.Path.Path);
                                    var value = prop?.GetValue(item);
                                    worksheet.Cell(row, col + 1).Value = value?.ToString();
                                }
                            }
                            row++;
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show($"Экспорт завершен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToPdf(DataGrid dataGrid, string fileName)
        {
            MessageBox.Show("Экспорт в PDF будет доступен в следующей версии.\nРекомендуется использовать Excel формат.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}