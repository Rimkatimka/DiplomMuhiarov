using ClosedXML.Excel;
using EnergyMeteringSystem.App.Models;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace EnergyMeteringSystem.App.Services.ExcelExport
{
    public class ExcelExportService
    {
        private readonly ReportStyles _styles;

        public ExcelExportService()
        {
            _styles = new ReportStyles();
        }

        // Старый метод - экспорт только данных
        public bool ExportReport(ReportBase report, string defaultFileName)
        {
            try
            {
                using var workbook = new XLWorkbook();
                IXLWorksheet ws = null;

                switch (report)
                {
                    case ConsumptionReport r:
                        ws = CreateConsumptionSheet(workbook, r);
                        break;
                    case TopObjectsReport r:
                        ws = CreateTopObjectsSheet(workbook, r);
                        break;
                    case ConsumptionByTypeReport r:
                        ws = CreateTypeDistributionSheet(workbook, r);
                        break;
                    case MonthlyDynamicsReport r:
                        ws = CreateMonthlyDynamicsSheet(workbook, r);
                        break;
                    case ConsumptionByRegionReport r:
                        ws = CreateRegionSheet(workbook, r);
                        break;
                    case ObjectAnalyticsReport r:
                        ws = CreateObjectAnalyticsSheet(workbook, r);
                        break;
                    case AnomaliesReport r:
                        ws = CreateAnomaliesSheet(workbook, r);
                        break;
                    case ExpiringMetersReport r:
                        ws = CreateExpiringMetersSheet(workbook, r);
                        break;
                    case OperatorActivityReport r:
                        ws = CreateOperatorActivitySheet(workbook, r);
                        break;
                    case MeterHistoryReport r:
                        ws = CreateMeterHistorySheet(workbook, r);
                        break;
                    case DashboardReport r:
                        ws = CreateDashboardSheet(workbook, r);
                        break;
                    default:
                        return false;
                }

                ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
                ws.PageSetup.Margins.Top = 0.5;
                ws.PageSetup.Margins.Bottom = 0.5;
                ws.PageSetup.Margins.Left = 0.5;
                ws.PageSetup.Margins.Right = 0.5;
                ws.PageSetup.FitToPages(1, 0);

                var dialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    workbook.SaveAs(dialog.FileName);
                    MessageBox.Show($"Отчет сохранен:\n{dialog.FileName}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        // НОВЫЙ МЕТОД: экспорт с нативным графиком Excel (с поддержкой всех типов графиков и стилей)
        public bool ExportReportWithNativeChart(ReportBase report, string defaultFileName,
                                         System.Collections.Generic.List<(string Category, decimal Value)> chartData,
                                         string chartTitle,
                                         Excel.XlChartType chartType)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"{defaultFileName}_с_графиком_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() != true) return false;

            Excel.Application excel = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excel = new Excel.Application();
                excel.Visible = false;
                excel.DisplayAlerts = false;
                workbook = excel.Workbooks.Add();
                worksheet = (Excel.Worksheet)workbook.Worksheets[1];
                worksheet.Name = "Отчет";

                int row = 1;
                int tableLastColumn = 3;

                // ЗАГОЛОВОК ОТЧЕТА
                var titleRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, tableLastColumn]];
                titleRange.Merge();
                titleRange.Value = report.Title;
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                titleRange.Interior.Color = System.Drawing.Color.FromArgb(45, 63, 94);
                titleRange.Font.Color = System.Drawing.Color.White;
                row += 2;

                // ЗАГОЛОВКИ ТАБЛИЦЫ
                string[] headers;
                if (report is ConsumptionReport)
                    headers = new[] { "Адрес", "Счетчик", "Потребление, кВт·ч" };
                else if (report is TopObjectsReport)
                    headers = new[] { "Ранг", "Адрес", "Потребление, кВт·ч" };
                else if (report is ConsumptionByTypeReport)
                    headers = new[] { "Тип объекта", "Потребление, кВт·ч", "Доля, %" };
                else if (report is MonthlyDynamicsReport)
                    headers = new[] { "Месяц", "Год", "Потребление, кВт·ч" };
                else
                    headers = new[] { "Категория", "Значение", "" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[row, i + 1] = headers[i];
                }

                var headerRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, tableLastColumn]];
                headerRange.Font.Bold = true;
                headerRange.Interior.Color = System.Drawing.Color.FromArgb(45, 63, 94);
                headerRange.Font.Color = System.Drawing.Color.White;
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                row++;

                // ДАННЫЕ ТАБЛИЦЫ
                int dataStartRow = row;

                if (report is ConsumptionReport consumptionReport)
                {
                    foreach (var record in consumptionReport.Records.OrderByDescending(x => x.Consumption))
                    {
                        worksheet.Cells[row, 1] = record.Address;
                        worksheet.Cells[row, 2] = record.MeterSerial;
                        worksheet.Cells[row, 3] = record.Consumption;
                        row++;
                    }
                }
                else if (report is TopObjectsReport topReport)
                {
                    foreach (var record in topReport.Records)
                    {
                        worksheet.Cells[row, 1] = record.Rank;
                        worksheet.Cells[row, 2] = record.Address;
                        worksheet.Cells[row, 3] = record.Consumption;
                        row++;
                    }
                }
                else if (report is ConsumptionByTypeReport typeReport)
                {
                    foreach (var record in typeReport.Records.OrderByDescending(x => x.Consumption))
                    {
                        worksheet.Cells[row, 1] = record.ObjectType;
                        worksheet.Cells[row, 2] = record.Consumption;
                        worksheet.Cells[row, 3] = record.Percentage;
                        row++;
                    }
                }
                else if (report is MonthlyDynamicsReport monthlyReport)
                {
                    foreach (var record in monthlyReport.Records)
                    {
                        worksheet.Cells[row, 1] = record.MonthName;
                        worksheet.Cells[row, 2] = record.Year;
                        worksheet.Cells[row, 3] = record.Consumption;
                        row++;
                    }
                }

                int dataEndRow = row - 1;

                // Чередование строк таблицы
                for (int r = dataStartRow; r <= dataEndRow; r++)
                {
                    var rowRange = worksheet.Range[worksheet.Cells[r, 1], worksheet.Cells[r, tableLastColumn]];
                    if (r % 2 == 0)
                    {
                        rowRange.Interior.Color = System.Drawing.Color.FromArgb(245, 245, 245);
                    }
                }

                // Границы таблицы
                var tableRange = worksheet.Range[worksheet.Cells[dataStartRow - 1, 1], worksheet.Cells[dataEndRow, tableLastColumn]];
                tableRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                tableRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

                // ГРАФИК
                if (chartData != null && chartData.Any())
                {
                    int chartStartCol = tableLastColumn + 2;
                    int chartDataRow = dataStartRow;

                    worksheet.Cells[chartDataRow, chartStartCol] = "Категория";
                    worksheet.Cells[chartDataRow, chartStartCol + 1] = "Значение, кВт·ч";
                    worksheet.Rows[chartDataRow].Font.Bold = true;
                    worksheet.Rows[chartDataRow].Interior.Color = System.Drawing.Color.FromArgb(45, 63, 94);
                    worksheet.Rows[chartDataRow].Font.Color = System.Drawing.Color.White;

                    int chartRow = chartDataRow + 1;
                    foreach (var item in chartData)
                    {
                        worksheet.Cells[chartRow, chartStartCol] = item.Category;
                        worksheet.Cells[chartRow, chartStartCol + 1] = item.Value;
                        chartRow++;
                    }

                    Excel.Range chartRange = worksheet.Range[
                        worksheet.Cells[chartDataRow, chartStartCol],
                        worksheet.Cells[chartRow - 1, chartStartCol + 1]];

                    Excel.ChartObjects chartObjects = (Excel.ChartObjects)worksheet.ChartObjects();

                    double leftPosition = (double)worksheet.Columns[chartStartCol].Left;
                    double topPosition = (double)worksheet.Rows[dataStartRow - 1].Top;
                    Excel.ChartObject chartObject = chartObjects.Add(leftPosition, topPosition, 450, 350);

                    Excel.Chart chart = chartObject.Chart;
                    chart.SetSourceData(chartRange);
                    chart.ChartType = chartType;

                    chart.HasTitle = true;
                    chart.ChartTitle.Text = chartTitle;
                    chart.ChartTitle.Font.Size = 12;
                    chart.ChartTitle.Font.Bold = true;

                    if (chartType == Excel.XlChartType.xlPie)
                    {
                        chart.ApplyDataLabels(Excel.XlDataLabelsType.xlDataLabelsShowPercent);
                        chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionRight;
                    }
                    else
                    {
                        chart.ApplyDataLabels(Excel.XlDataLabelsType.xlDataLabelsShowValue);
                    }
                }

                worksheet.Columns.AutoFit();
                workbook.SaveAs(dialog.FileName);

                // ✅ ЗАКРЫВАЕМ EXCEL БЕЗОПАСНО
                SafeCloseExcel(worksheet, workbook, excel);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });

                MessageBox.Show($"Отчет с графиком сохранен:\n{dialog.FileName}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                // Дополнительная очистка
                try
                {
                    if (worksheet != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                        worksheet = null;
                    }
                }
                catch { }

                try
                {
                    if (workbook != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                        workbook = null;
                    }
                }
                catch { }

                try
                {
                    if (excel != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                        excel = null;
                    }
                }
                catch { }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        /// <summary>
        /// Безопасное закрытие Excel
        /// </summary>
        private void SafeCloseExcel(Excel.Worksheet worksheet, Excel.Workbook workbook, Excel.Application excel)
        {
            try
            {
                if (workbook != null)
                {
                    try { workbook.Close(false); } catch { }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                    workbook = null;
                }
            }
            catch { }

            try
            {
                if (excel != null)
                {
                    try { excel.Quit(); } catch { }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                    excel = null;
                }
            }
            catch { }

            try
            {
                if (worksheet != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                    worksheet = null;
                }
            }
            catch { }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        // ========== ВСЕ СТАРЫЕ МЕТОДЫ СОЗДАНИЯ ЛИСТОВ (остаются без изменений) ==========

        private IXLWorksheet CreateConsumptionSheet(XLWorkbook workbook, ConsumptionReport report)
        {
            var ws = workbook.Worksheets.Add("Потребление");

            // ===== ЗАГОЛОВОК =====
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
            ws.Cell(3, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";

            // Заголовок - только нужные ячейки
            var titleRange = ws.Range(1, 1, 1, 8);
            titleRange.Merge();
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 16;
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromArgb(45, 63, 94);
            titleRange.Style.Font.FontColor = XLColor.White;

            // ===== KPI КАРТОЧКИ =====
            int kpiRow = 4;
            ws.Cell(kpiRow, 1).Value = "📊 ИТОГИ";
            ws.Cell(kpiRow, 1).Style.Font.Bold = true;
            ws.Cell(kpiRow, 1).Style.Font.FontSize = 14;
            ws.Range(kpiRow, 1, kpiRow, 2).Merge();
            kpiRow += 1;

            // Заголовки KPI
            ws.Cell(kpiRow, 1).Value = "Показатель";
            ws.Cell(kpiRow, 2).Value = "Значение";

            // Фон только на эти две ячейки, а не на всю строку
            ws.Cell(kpiRow, 1).Style.Font.Bold = true;
            ws.Cell(kpiRow, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(45, 63, 94);
            ws.Cell(kpiRow, 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(kpiRow, 2).Style.Font.Bold = true;
            ws.Cell(kpiRow, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(45, 63, 94);
            ws.Cell(kpiRow, 2).Style.Font.FontColor = XLColor.White;
            kpiRow++;

            int kpiStartRow = kpiRow;
            var kpiLabels = new string[]
            {
        "Общее потребление, кВт·ч:",
        "Среднее на запись:",
        "Максимум:",
        "Минимум:",
        "Аномалий (>500):",
        "Всего записей:"
            };

            foreach (var label in kpiLabels)
            {
                ws.Cell(kpiRow, 1).Value = label;
                kpiRow++;
            }

            // Фон для KPI (только ячейки, а не вся строка)
            var kpiRange = ws.Range(kpiStartRow, 1, kpiRow - 1, 2);
            kpiRange.Style.Fill.BackgroundColor = XLColor.FromArgb(227, 242, 253);
            kpiRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            kpiRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(45, 63, 94);

            // ===== ЗАГОЛОВКИ ТАБЛИЦЫ =====
            int headerRow = kpiRow + 2;

            // Заголовки таблицы - только ячейки, а не вся строка
            string[] headers = { "№", "Адрес", "Тип", "Счетчик", "Нач. показание", "Кон. показание", "Потребление, кВт·ч", "Статус" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
                ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
                ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(45, 63, 94);
                ws.Cell(headerRow, i + 1).Style.Font.FontColor = XLColor.White;
                ws.Cell(headerRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(headerRow, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            // ===== ДАННЫЕ =====
            int row = headerRow + 1;
            int rowIndex = 1;
            int dataStartRow = row;

            if (report.Records == null || !report.Records.Any())
            {
                ws.Cell(row, 1).Value = "Нет данных за выбранный период";
                return ws;
            }

            foreach (var record in report.Records.OrderByDescending(x => x.Consumption))
            {
                ws.Cell(row, 1).Value = rowIndex++;
                ws.Cell(row, 2).Value = record.Address ?? "Не указан";
                ws.Cell(row, 3).Value = record.ObjectType ?? "Не указан";
                ws.Cell(row, 4).Value = record.MeterSerial ?? "Нет номера";
                ws.Cell(row, 5).Value = Convert.ToDouble(record.StartValue);
                ws.Cell(row, 6).Value = Convert.ToDouble(record.EndValue);
                ws.Cell(row, 7).Value = Convert.ToDouble(record.Consumption);

                // Статус через формулу
                ws.Cell(row, 8).FormulaA1 = $"=IF(G{row}>500,\"⚠ Аномалия\",\"✅ Норма\")";

                // ✅ ФОН ТОЛЬКО НА ЯЧЕЙКУ С ПОТРЕБЛЕНИЕМ, А НЕ НА ВСЮ СТРОКУ
                if (record.Consumption > 500)
                {
                    ws.Cell(row, 7).Style.Font.FontColor = XLColor.Red;
                    ws.Cell(row, 7).Style.Font.Bold = true;
                    // ФОН ТОЛЬКО НА ЭТУ ЯЧЕЙКУ
                    ws.Cell(row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 200, 200);
                }

                // ✅ ЧЕРЕДОВАНИЕ СТРОК - ТОЛЬКО ЯЧЕЙКИ
                if (row % 2 == 0)
                {
                    for (int col = 1; col <= 8; col++)
                    {
                        ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);
                    }
                }

                row++;
            }

            int dataEndRow = row - 1;
            string gRange = $"G{dataStartRow}:G{dataEndRow}";
            string cRange = $"C{dataStartRow}:C{dataEndRow}";
            string gTotalCell = $"G{dataEndRow + 1}";

            // ===== ЗАПОЛНЯЕМ KPI ФОРМУЛАМИ =====
            int kpiFormulaRow = kpiStartRow;
            var kpiFormulas = new string[]
            {
        $"=SUM({gRange})",
        $"=AVERAGE({gRange})",
        $"=MAX({gRange})",
        $"=MIN({gRange})",
        $"=COUNTIF({gRange},\">500\")",
        $"=COUNTA({gRange})"
            };

            foreach (var formula in kpiFormulas)
            {
                ws.Cell(kpiFormulaRow, 2).FormulaA1 = formula;
                ws.Cell(kpiFormulaRow, 2).Style.NumberFormat.SetFormat("#,##0.00");
                kpiFormulaRow++;
            }

            // ===== ИТОГОВАЯ СТРОКА =====
            int totalRow = dataEndRow + 1;
            ws.Cell(totalRow, 6).Value = "ИТОГО:";
            ws.Cell(totalRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(totalRow, 7).FormulaA1 = $"=SUM({gRange})";
            ws.Cell(totalRow, 7).Style.NumberFormat.SetFormat("#,##0.00");
            ws.Cell(totalRow, 8).FormulaA1 = $"=COUNTA({gRange}) & \" записей\"";

            // ✅ ФОН ТОЛЬКО НА НУЖНЫЕ ЯЧЕЙКИ ИТОГОВОЙ СТРОКИ
            for (int col = 6; col <= 8; col++)
            {
                ws.Cell(totalRow, col).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 225, 242);
                ws.Cell(totalRow, col).Style.Font.Bold = true;
            }

            // ===== СВОДКА ПО ТИПАМ (СПРАВА) =====
            int pivotRow = headerRow + 1;
            int pivotCol = 10;

            ws.Cell(pivotRow, pivotCol).Value = "📊 СВОДКА ПО ТИПАМ";
            ws.Cell(pivotRow, pivotCol).Style.Font.Bold = true;
            ws.Cell(pivotRow, pivotCol).Style.Font.FontSize = 14;
            ws.Range(pivotRow, pivotCol, pivotRow, pivotCol + 2).Merge();
            pivotRow += 2;

            // Заголовки сводки
            string[] pivotHeaders = { "Тип объекта", "Потребление, кВт·ч", "Доля, %" };
            for (int i = 0; i < pivotHeaders.Length; i++)
            {
                ws.Cell(pivotRow, pivotCol + i).Value = pivotHeaders[i];
                ws.Cell(pivotRow, pivotCol + i).Style.Font.Bold = true;
                ws.Cell(pivotRow, pivotCol + i).Style.Fill.BackgroundColor = XLColor.FromArgb(45, 63, 94);
                ws.Cell(pivotRow, pivotCol + i).Style.Font.FontColor = XLColor.White;
                ws.Cell(pivotRow, pivotCol + i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            pivotRow++;

            int pivotStartRow = pivotRow;
            var types = report.Records.Select(r => r.ObjectType ?? "Не указан").Distinct().ToList();

            foreach (var type in types)
            {
                ws.Cell(pivotRow, pivotCol).Value = type;
                ws.Cell(pivotRow, pivotCol + 1).FormulaA1 = $"=SUMIF({cRange},\"{type}\",{gRange})";
                ws.Cell(pivotRow, pivotCol + 2).FormulaA1 = $"=IF({gTotalCell}>0, {GetColumnLetter(pivotCol + 1)}{pivotRow}/{gTotalCell}*100, 0)";
                ws.Cell(pivotRow, pivotCol + 2).Style.NumberFormat.SetFormat("#,##0.00");

                // Чередование строк в сводке
                if ((pivotRow - pivotStartRow) % 2 == 0)
                {
                    ws.Cell(pivotRow, pivotCol).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);
                    ws.Cell(pivotRow, pivotCol + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);
                    ws.Cell(pivotRow, pivotCol + 2).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);
                }

                pivotRow++;
            }

            // ИТОГО по сводной
            string hRange = $"{GetColumnLetter(pivotCol + 1)}{pivotStartRow}:{GetColumnLetter(pivotCol + 1)}{pivotRow - 1}";
            string iRange = $"{GetColumnLetter(pivotCol + 2)}{pivotStartRow}:{GetColumnLetter(pivotCol + 2)}{pivotRow - 1}";

            ws.Cell(pivotRow, pivotCol).Value = "ВСЕГО:";
            ws.Cell(pivotRow, pivotCol + 1).FormulaA1 = $"=SUM({hRange})";
            ws.Cell(pivotRow, pivotCol + 2).FormulaA1 = $"=SUM({iRange})";

            // Фон для итоговой строки сводки
            for (int i = 0; i <= 2; i++)
            {
                ws.Cell(pivotRow, pivotCol + i).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 225, 242);
                ws.Cell(pivotRow, pivotCol + i).Style.Font.Bold = true;
            }

            // ===== ФОРМАТИРОВАНИЕ =====
            ws.Column(7).Style.NumberFormat.SetFormat("#,##0.00");
            ws.Column(pivotCol + 1).Style.NumberFormat.SetFormat("#,##0.00");
            ws.Column(pivotCol + 2).Style.NumberFormat.SetFormat("#,##0.00");

            ws.Columns().AdjustToContents();
            ws.Column(7).Width = 18;
            ws.Column(pivotCol + 1).Width = 18;
            ws.Column(pivotCol + 2).Width = 14;

            return ws;
        }

        private string GetColumnLetter(int columnNumber)
        {
            string columnLetter = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnLetter;
        }

        private IXLWorksheet CreateTopObjectsSheet(XLWorkbook workbook, TopObjectsReport report)
        {
            var ws = workbook.Worksheets.Add("ТОП-10 объектов");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
            ws.Cell(3, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 5));
            _styles.ApplyHeaderStyle(ws.Row(5));
            ws.Cell(5, 1).Value = "Ранг";
            ws.Cell(5, 2).Value = "Адрес";
            ws.Cell(5, 3).Value = "Тип объекта";
            ws.Cell(5, 4).Value = "Потребление, кВт·ч";
            ws.Cell(5, 5).Value = "Доля, %";
            int row = 6;
            foreach (var record in report.Records)
            {
                ws.Cell(row, 1).Value = record.Rank;
                ws.Cell(row, 2).Value = record.Address;
                ws.Cell(row, 3).Value = record.ObjectType;
                ws.Cell(row, 4).Value = record.Consumption;
                ws.Cell(row, 5).Value = record.Percentage;
                row++;
            }
            ws.Cell(row, 3).Value = "ВСЕГО:";
            ws.Cell(row, 4).Value = report.TotalConsumption;
            _styles.ApplyTotalStyle(ws.Range(row, 3, row, 4));
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateMonthlyDynamicsSheet(XLWorkbook workbook, MonthlyDynamicsReport report)
        {
            var ws = workbook.Worksheets.Add("Динамика");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
            ws.Cell(3, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 4));
            ws.Cell(5, 1).Value = "Общее потребление:";
            ws.Cell(5, 2).Value = report.TotalConsumption;
            ws.Cell(6, 1).Value = "Среднее в месяц:";
            ws.Cell(6, 2).Value = report.AverageConsumption;
            ws.Cell(7, 1).Value = "Максимум:";
            ws.Cell(7, 2).Value = $"{report.MaxConsumption} ({report.MaxMonth})";
            _styles.ApplyKpiStyle(ws.Range(5, 1, 7, 2));
            _styles.ApplyHeaderStyle(ws.Row(9));
            ws.Cell(9, 1).Value = "Месяц";
            ws.Cell(9, 2).Value = "Год";
            ws.Cell(9, 3).Value = "Потребление, кВт·ч";
            int row = 10;
            foreach (var record in report.Records)
            {
                ws.Cell(row, 1).Value = record.MonthName;
                ws.Cell(row, 2).Value = record.Year;
                ws.Cell(row, 3).Value = record.Consumption;
                row++;
            }
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateTypeDistributionSheet(XLWorkbook workbook, ConsumptionByTypeReport report)
        {
            var ws = workbook.Worksheets.Add("По типам");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
            ws.Cell(3, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 3));
            _styles.ApplyHeaderStyle(ws.Row(5));
            ws.Cell(5, 1).Value = "Тип объекта";
            ws.Cell(5, 2).Value = "Потребление, кВт·ч";
            ws.Cell(5, 3).Value = "Доля, %";
            int row = 6;
            foreach (var record in report.Records.OrderByDescending(x => x.Consumption))
            {
                ws.Cell(row, 1).Value = record.ObjectType;
                ws.Cell(row, 2).Value = record.Consumption;
                ws.Cell(row, 3).Value = record.Percentage;
                row++;
            }
            ws.Cell(row, 1).Value = "ВСЕГО:";
            ws.Cell(row, 2).Value = report.TotalConsumption;
            ws.Cell(row, 3).Value = "100%";
            _styles.ApplyTotalStyle(ws.Range(row, 1, row, 3));
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateRegionSheet(XLWorkbook workbook, ConsumptionByRegionReport report)
        {
            var ws = workbook.Worksheets.Add("По регионам");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
            ws.Cell(3, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 5));
            _styles.ApplyHeaderStyle(ws.Row(5));
            ws.Cell(5, 1).Value = "Регион";
            ws.Cell(5, 2).Value = "Потребление, кВт·ч";
            ws.Cell(5, 3).Value = "Доля, %";
            ws.Cell(5, 4).Value = "Объектов";
            ws.Cell(5, 5).Value = "Счетчиков";
            int row = 6;
            foreach (var record in report.Records)
            {
                ws.Cell(row, 1).Value = record.Region;
                ws.Cell(row, 2).Value = record.Consumption;
                ws.Cell(row, 3).Value = record.Percentage;
                ws.Cell(row, 4).Value = record.ObjectsCount;
                ws.Cell(row, 5).Value = record.MetersCount;
                row++;
            }
            ws.Cell(row, 1).Value = "ВСЕГО:";
            ws.Cell(row, 2).Value = report.TotalConsumption;
            _styles.ApplyTotalStyle(ws.Range(row, 1, row, 2));
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateAnomaliesSheet(XLWorkbook workbook, AnomaliesReport report)
        {
            var ws = workbook.Worksheets.Add("Аномалии");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
            ws.Cell(3, 1).Value = $"Порог аномалии: >{report.AnomalyThreshold}%";
            ws.Cell(4, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 7));
            _styles.ApplyHeaderStyle(ws.Row(6));
            ws.Cell(6, 1).Value = "Адрес";
            ws.Cell(6, 2).Value = "Счетчик";
            ws.Cell(6, 3).Value = "Предыдущее, кВт·ч";
            ws.Cell(6, 4).Value = "Текущее, кВт·ч";
            ws.Cell(6, 5).Value = "Разница, кВт·ч";
            ws.Cell(6, 6).Value = "Разница, %";
            ws.Cell(6, 7).Value = "Статус";
            int row = 7;
            foreach (var record in report.Records)
            {
                ws.Cell(row, 1).Value = record.Address;
                ws.Cell(row, 2).Value = record.MeterSerial;
                ws.Cell(row, 3).Value = record.PreviousConsumption;
                ws.Cell(row, 4).Value = record.CurrentConsumption;
                ws.Cell(row, 5).Value = record.Difference;
                ws.Cell(row, 6).Value = record.DifferencePercent;
                ws.Cell(row, 7).Value = record.Status;
                row++;
            }
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateExpiringMetersSheet(XLWorkbook workbook, ExpiringMetersReport report)
        {
            var ws = workbook.Worksheets.Add("Поверка счетчиков");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 5));
            ws.Cell(4, 1).Value = "Просрочено:";
            ws.Cell(4, 2).Value = report.ExpiredCount;
            ws.Cell(5, 1).Value = "Истекает скоро:";
            ws.Cell(5, 2).Value = report.ExpiringSoonCount;
            ws.Cell(6, 1).Value = "В норме:";
            ws.Cell(6, 2).Value = report.NormalCount;
            _styles.ApplyHeaderStyle(ws.Row(8));
            ws.Cell(8, 1).Value = "Серийный номер";
            ws.Cell(8, 2).Value = "Адрес";
            ws.Cell(8, 3).Value = "Дата поверки";
            ws.Cell(8, 4).Value = "След. поверка";
            ws.Cell(8, 5).Value = "Дней осталось";
            int row = 9;
            foreach (var record in report.Records)
            {
                ws.Cell(row, 1).Value = record.SerialNumber;
                ws.Cell(row, 2).Value = record.Address;
                ws.Cell(row, 3).Value = record.VerificationDate;
                ws.Cell(row, 4).Value = record.NextVerificationDate;
                ws.Cell(row, 5).Value = record.DaysLeft;
                row++;
            }
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateOperatorActivitySheet(XLWorkbook workbook, OperatorActivityReport report)
        {
            var ws = workbook.Worksheets.Add("Активность операторов");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
            ws.Cell(3, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 5));
            _styles.ApplyHeaderStyle(ws.Row(5));
            ws.Cell(5, 1).Value = "Пользователь";
            ws.Cell(5, 2).Value = "ФИО";
            ws.Cell(5, 3).Value = "Роль";
            ws.Cell(5, 4).Value = "Введено";
            ws.Cell(5, 5).Value = "Верифицировано";
            ws.Cell(5, 6).Value = "Отклонено";
            int row = 6;
            foreach (var record in report.Records)
            {
                ws.Cell(row, 1).Value = record.UserName;
                ws.Cell(row, 2).Value = record.FullName;
                ws.Cell(row, 3).Value = record.Role;
                ws.Cell(row, 4).Value = record.EnteredCount;
                ws.Cell(row, 5).Value = record.VerifiedCount;
                ws.Cell(row, 6).Value = record.RejectedCount;
                row++;
            }
            ws.Cell(row, 3).Value = "ВСЕГО:";
            ws.Cell(row, 4).Value = report.TotalEntered;
            ws.Cell(row, 5).Value = report.TotalVerified;
            _styles.ApplyTotalStyle(ws.Range(row, 3, row, 5));
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateMeterHistorySheet(XLWorkbook workbook, MeterHistoryReport report)
        {
            var ws = workbook.Worksheets.Add("История счетчика");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Счетчик: {report.MeterSerial}";
            ws.Cell(3, 1).Value = $"Объект: {report.Address}";
            ws.Cell(4, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 5));
            _styles.ApplyHeaderStyle(ws.Row(6));
            ws.Cell(6, 1).Value = "Дата";
            ws.Cell(6, 2).Value = "Показание";
            ws.Cell(6, 3).Value = "Потребление";
            ws.Cell(6, 4).Value = "Статус";
            ws.Cell(6, 5).Value = "Кто ввел";
            int row = 7;
            foreach (var record in report.Records.OrderByDescending(r => r.ReadingDate))
            {
                ws.Cell(row, 1).Value = record.ReadingDate;
                ws.Cell(row, 2).Value = record.Value;
                ws.Cell(row, 3).Value = record.Consumption;
                ws.Cell(row, 4).Value = record.Status;
                ws.Cell(row, 5).Value = record.EnteredBy;
                row++;
            }
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateObjectAnalyticsSheet(XLWorkbook workbook, ObjectAnalyticsReport report)
        {
            var ws = workbook.Worksheets.Add("Аналитика объектов");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Период: {report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
            ws.Cell(3, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 7));
            _styles.ApplyHeaderStyle(ws.Row(5));
            ws.Cell(5, 1).Value = "Адрес";
            ws.Cell(5, 2).Value = "Тип";
            ws.Cell(5, 3).Value = "Площадь, м²";
            ws.Cell(5, 4).Value = "Жильцов";
            ws.Cell(5, 5).Value = "Потребление, кВт·ч";
            ws.Cell(5, 6).Value = "кВт·ч/м²";
            ws.Cell(5, 7).Value = "кВт·ч/чел";
            int row = 6;
            foreach (var record in report.Records)
            {
                ws.Cell(row, 1).Value = record.Address;
                ws.Cell(row, 2).Value = record.ObjectType;
                ws.Cell(row, 3).Value = record.TotalArea;
                ws.Cell(row, 4).Value = record.ResidentCount;
                ws.Cell(row, 5).Value = record.Consumption;
                ws.Cell(row, 6).Value = record.ConsumptionPerArea;
                ws.Cell(row, 7).Value = record.ConsumptionPerPerson;
                row++;
            }
            ws.Columns().AdjustToContents();
            return ws;
        }

        private IXLWorksheet CreateDashboardSheet(XLWorkbook workbook, DashboardReport report)
        {
            var ws = workbook.Worksheets.Add("Дашборд");
            ws.Cell(1, 1).Value = report.Title;
            ws.Cell(2, 1).Value = $"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm:ss}";
            _styles.ApplyTitleStyle(ws.Range(1, 1, 1, 2));
            var kpiStyle = ws.Style;
            kpiStyle.Font.Bold = true;
            kpiStyle.Font.FontSize = 14;
            kpiStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            kpiStyle.Fill.BackgroundColor = XLColor.FromArgb(45, 63, 94);
            kpiStyle.Font.FontColor = XLColor.White;
            ws.Cell(4, 1).Value = "Объектов";
            ws.Cell(4, 2).Value = report.TotalObjects;
            ws.Cell(5, 1).Value = "Счетчиков";
            ws.Cell(5, 2).Value = report.TotalMeters;
            ws.Cell(6, 1).Value = "Показаний сегодня";
            ws.Cell(6, 2).Value = report.ReadingsToday;
            ws.Cell(7, 1).Value = "За неделю";
            ws.Cell(7, 2).Value = report.ReadingsWeek;
            ws.Cell(8, 1).Value = "Просрочено поверок";
            ws.Cell(8, 2).Value = report.ExpiredMeters;
            ws.Cell(9, 1).Value = "Истекает скоро";
            ws.Cell(9, 2).Value = report.ExpiringSoonMeters;
            ws.Range(4, 1, 9, 2).Style = kpiStyle;
            _styles.ApplyHeaderStyle(ws.Row(11));
            ws.Cell(11, 4).Value = "Месяц";
            ws.Cell(11, 5).Value = "Потребление";
            int row = 12;
            foreach (var record in report.MonthlyDynamics)
            {
                ws.Cell(row, 4).Value = record.MonthName;
                ws.Cell(row, 5).Value = record.Consumption;
                row++;
            }
            _styles.ApplyHeaderStyle(ws.Row(11));
            ws.Cell(11, 7).Value = "ТОП аномалий";
            ws.Cell(12, 7).Value = "Адрес";
            ws.Cell(12, 8).Value = "Потребление";
            int anomalyRow = 13;
            foreach (var anomaly in report.TopAnomalies.Take(5))
            {
                ws.Cell(anomalyRow, 7).Value = anomaly.Address;
                ws.Cell(anomalyRow, 8).Value = anomaly.Consumption;
                anomalyRow++;
            }
            ws.Columns().AdjustToContents();
            return ws;
        }
        public bool ExportReportWithTwoCharts(ReportBase report, string defaultFileName,
                                         System.Collections.Generic.List<(string Category, decimal Value)> chart1Data,
                                         string chart1Title,
                                         Excel.XlChartType chart1Type,
                                         System.Collections.Generic.List<(string Category, decimal Value)> chart2Data,
                                         string chart2Title,
                                         Excel.XlChartType chart2Type)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"{defaultFileName}_с_графиками_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() != true) return false;

            Excel.Application excel = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excel = new Excel.Application();
                excel.Visible = false;
                excel.DisplayAlerts = false;
                workbook = excel.Workbooks.Add();
                worksheet = (Excel.Worksheet)workbook.Worksheets[1];
                worksheet.Name = "Отчет";

                int row = 1;
                int tableLastColumn = 3;

                // ЗАГОЛОВОК ОТЧЕТА
                var titleRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, tableLastColumn]];
                titleRange.Merge();
                titleRange.Value = report.Title;
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                titleRange.Interior.Color = System.Drawing.Color.FromArgb(45, 63, 94);
                titleRange.Font.Color = System.Drawing.Color.White;
                row += 2;

                // ЗАГОЛОВКИ ТАБЛИЦЫ
                string[] headers;
                if (report is ConsumptionReport)
                    headers = new[] { "Адрес", "Счетчик", "Потребление, кВт·ч" };
                else if (report is TopObjectsReport)
                    headers = new[] { "Ранг", "Адрес", "Потребление, кВт·ч" };
                else if (report is ConsumptionByTypeReport)
                    headers = new[] { "Тип объекта", "Потребление, кВт·ч", "Доля, %" };
                else
                    headers = new[] { "Категория", "Значение", "" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[row, i + 1] = headers[i];
                }

                var headerRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, tableLastColumn]];
                headerRange.Font.Bold = true;
                headerRange.Interior.Color = System.Drawing.Color.FromArgb(45, 63, 94);
                headerRange.Font.Color = System.Drawing.Color.White;
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                row++;

                // ДАННЫЕ ТАБЛИЦЫ
                int dataStartRow = row;

                if (report is ConsumptionReport consumptionReport)
                {
                    foreach (var record in consumptionReport.Records.OrderByDescending(x => x.Consumption))
                    {
                        worksheet.Cells[row, 1] = record.Address;
                        worksheet.Cells[row, 2] = record.MeterSerial;
                        worksheet.Cells[row, 3] = record.Consumption;
                        row++;
                    }
                }
                else if (report is TopObjectsReport topReport)
                {
                    foreach (var record in topReport.Records)
                    {
                        worksheet.Cells[row, 1] = record.Rank;
                        worksheet.Cells[row, 2] = record.Address;
                        worksheet.Cells[row, 3] = record.Consumption;
                        row++;
                    }
                }
                else if (report is ConsumptionByTypeReport typeReport)
                {
                    foreach (var record in typeReport.Records.OrderByDescending(x => x.Consumption))
                    {
                        worksheet.Cells[row, 1] = record.ObjectType;
                        worksheet.Cells[row, 2] = record.Consumption;
                        worksheet.Cells[row, 3] = record.Percentage;
                        row++;
                    }
                }

                int dataEndRow = row - 1;

                // Чередование строк таблицы
                for (int r = dataStartRow; r <= dataEndRow; r++)
                {
                    var rowRange = worksheet.Range[worksheet.Cells[r, 1], worksheet.Cells[r, tableLastColumn]];
                    if (r % 2 == 0)
                    {
                        rowRange.Interior.Color = System.Drawing.Color.FromArgb(245, 245, 245);
                    }
                }

                // Границы таблицы
                var tableRange = worksheet.Range[worksheet.Cells[dataStartRow - 1, 1], worksheet.Cells[dataEndRow, tableLastColumn]];
                tableRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                tableRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

                // ПЕРВЫЙ ГРАФИК
                if (chart1Data != null && chart1Data.Any())
                {
                    int chartStartCol = tableLastColumn + 2;
                    int chartDataRow = dataStartRow;

                    worksheet.Cells[chartDataRow, chartStartCol] = "Категория";
                    worksheet.Cells[chartDataRow, chartStartCol + 1] = "Значение, кВт·ч";
                    worksheet.Rows[chartDataRow].Font.Bold = true;
                    worksheet.Rows[chartDataRow].Interior.Color = System.Drawing.Color.FromArgb(45, 63, 94);
                    worksheet.Rows[chartDataRow].Font.Color = System.Drawing.Color.White;

                    int chartRow = chartDataRow + 1;
                    foreach (var item in chart1Data)
                    {
                        worksheet.Cells[chartRow, chartStartCol] = item.Category;
                        worksheet.Cells[chartRow, chartStartCol + 1] = item.Value;
                        chartRow++;
                    }

                    Excel.Range chartRange = worksheet.Range[
                        worksheet.Cells[chartDataRow, chartStartCol],
                        worksheet.Cells[chartRow - 1, chartStartCol + 1]];

                    Excel.ChartObjects chartObjects = (Excel.ChartObjects)worksheet.ChartObjects();

                    double leftPosition = (double)worksheet.Columns[chartStartCol].Left;
                    double topPosition = (double)worksheet.Rows[dataStartRow - 1].Top;
                    Excel.ChartObject chartObject = chartObjects.Add(leftPosition, topPosition, 450, 300);

                    Excel.Chart chart = chartObject.Chart;
                    chart.SetSourceData(chartRange);
                    chart.ChartType = chart1Type;

                    chart.HasTitle = true;
                    chart.ChartTitle.Text = chart1Title;
                    chart.ChartTitle.Font.Size = 12;
                    chart.ChartTitle.Font.Bold = true;

                    if (chart1Type == Excel.XlChartType.xlPie)
                    {
                        chart.ApplyDataLabels(Excel.XlDataLabelsType.xlDataLabelsShowPercent);
                        chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionRight;
                    }
                    else
                    {
                        chart.ApplyDataLabels(Excel.XlDataLabelsType.xlDataLabelsShowValue);
                    }
                }

                // ВТОРОЙ ГРАФИК
                if (chart2Data != null && chart2Data.Any())
                {
                    int chart2StartCol = tableLastColumn + 2;
                    int chart2DataRow = dataStartRow + 25;

                    worksheet.Cells[chart2DataRow, chart2StartCol] = "Категория";
                    worksheet.Cells[chart2DataRow, chart2StartCol + 1] = "Значение, кВт·ч";
                    worksheet.Rows[chart2DataRow].Font.Bold = true;
                    worksheet.Rows[chart2DataRow].Interior.Color = System.Drawing.Color.FromArgb(45, 63, 94);
                    worksheet.Rows[chart2DataRow].Font.Color = System.Drawing.Color.White;

                    int chart2Row = chart2DataRow + 1;
                    foreach (var item in chart2Data)
                    {
                        worksheet.Cells[chart2Row, chart2StartCol] = item.Category;
                        worksheet.Cells[chart2Row, chart2StartCol + 1] = item.Value;
                        chart2Row++;
                    }

                    Excel.Range chart2Range = worksheet.Range[
                        worksheet.Cells[chart2DataRow, chart2StartCol],
                        worksheet.Cells[chart2Row - 1, chart2StartCol + 1]];

                    Excel.ChartObjects chart2Objects = (Excel.ChartObjects)worksheet.ChartObjects();

                    double left2Position = (double)worksheet.Columns[chart2StartCol].Left;
                    double top2Position = (double)worksheet.Rows[chart2DataRow - 1].Top;
                    Excel.ChartObject chart2Object = chart2Objects.Add(left2Position, top2Position, 450, 300);

                    Excel.Chart chart2 = chart2Object.Chart;
                    chart2.SetSourceData(chart2Range);
                    chart2.ChartType = chart2Type;

                    chart2.HasTitle = true;
                    chart2.ChartTitle.Text = chart2Title;
                    chart2.ChartTitle.Font.Size = 12;
                    chart2.ChartTitle.Font.Bold = true;

                    chart2.ApplyDataLabels(Excel.XlDataLabelsType.xlDataLabelsShowValue);
                }

                worksheet.Columns.AutoFit();
                workbook.SaveAs(dialog.FileName);

                // ✅ ЗАКРЫВАЕМ EXCEL БЕЗОПАСНО
                SafeCloseExcel(worksheet, workbook, excel);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });

                MessageBox.Show($"Отчет с графиками сохранен:\n{dialog.FileName}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                try
                {
                    if (worksheet != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                        worksheet = null;
                    }
                }
                catch { }

                try
                {
                    if (workbook != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                        workbook = null;
                    }
                }
                catch { }

                try
                {
                    if (excel != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                        excel = null;
                    }
                }
                catch { }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
    }

    public class ReportStyles
    {
        public void ApplyTitleStyle(IXLRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Font.FontSize = 16;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Merge();
            range.Style.Fill.BackgroundColor = XLColor.FromArgb(45, 63, 94);
            range.Style.Font.FontColor = XLColor.White;
        }

        public void ApplyHeaderStyle(IXLRow row)
        {
            row.Style.Font.Bold = true;
            row.Style.Fill.BackgroundColor = XLColor.FromArgb(45, 63, 94);
            row.Style.Font.FontColor = XLColor.White;
            row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            row.Height = 25;
        }

        public void ApplyTotalStyle(IXLRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.FromArgb(217, 225, 242);
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            range.Style.Font.FontColor = XLColor.FromArgb(45, 63, 94);
        }

        public void ApplyKpiStyle(IXLRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.FromArgb(227, 242, 253);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            range.Style.Border.OutsideBorderColor = XLColor.FromArgb(45, 63, 94);
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            range.Style.Font.FontColor = XLColor.FromArgb(45, 63, 94);
        }

        // ✅ НОВЫЙ СТИЛЬ ДЛЯ АНОМАЛИЙ
        public void ApplyAnomalyStyle(IXLRange range)
        {
            range.Style.Font.FontColor = XLColor.Red;
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 235, 235);
        }

        // ✅ НОВЫЙ СТИЛЬ ДЛЯ НОРМЫ
        public void ApplyNormalStyle(IXLRange range)
        {
            range.Style.Font.FontColor = XLColor.Green;
            range.Style.Fill.BackgroundColor = XLColor.FromArgb(235, 255, 235);
        }
    }
}