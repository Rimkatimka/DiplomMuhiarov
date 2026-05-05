using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using EnergyMeteringSystem.Core.Models.DTO;
using Microsoft.Win32;

namespace EnergyMeteringSystem.Services.Export
{
    public class ExportService
    {
        /// <summary>
        /// Экспорт отчета по потреблению в Excel
        /// </summary>
        public void ExportConsumptionReport(List<ConsumptionReportDto> data, DateTime startDate, DateTime endDate)
        {
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"Отчет_потребление_{startDate:dd.MM.yyyy}_{endDate:dd.MM.yyyy}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Потребление");

                        // Заголовки
                        worksheet.Cell(1, 1).Value = "№";
                        worksheet.Cell(1, 2).Value = "Адрес";
                        worksheet.Cell(1, 3).Value = "Счетчик";
                        worksheet.Cell(1, 4).Value = "Период";
                        worksheet.Cell(1, 5).Value = "Нач. показание";
                        worksheet.Cell(1, 6).Value = "Кон. показание";
                        worksheet.Cell(1, 7).Value = "Потребление (кВт·ч)";

                        // Стиль заголовков
                        var headerRange = worksheet.Range(1, 1, 1, 7);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Данные
                        int row = 2;
                        decimal totalConsumption = 0;

                        foreach (var item in data)
                        {
                            worksheet.Cell(row, 1).Value = row - 1;
                            worksheet.Cell(row, 2).Value = item.Address;
                            worksheet.Cell(row, 3).Value = item.MeterSerial;
                            worksheet.Cell(row, 4).Value = $"{item.StartDate:dd.MM.yyyy} - {item.EndDate:dd.MM.yyyy}";
                            worksheet.Cell(row, 5).Value = item.StartValue;
                            worksheet.Cell(row, 6).Value = item.EndValue;
                            worksheet.Cell(row, 7).Value = item.Consumption;

                            // Формат чисел
                            worksheet.Cell(row, 5).Style.NumberFormat.Format = "# ##0.00";
                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "# ##0.00";
                            worksheet.Cell(row, 7).Style.NumberFormat.Format = "# ##0.00";

                            totalConsumption += item.Consumption;
                            row++;
                        }

                        // Итоговая строка
                        worksheet.Cell(row, 6).Value = "ИТОГО:";
                        worksheet.Cell(row, 6).Style.Font.Bold = true;
                        worksheet.Cell(row, 7).Value = totalConsumption;
                        worksheet.Cell(row, 7).Style.Font.Bold = true;
                        worksheet.Cell(row, 7).Style.NumberFormat.Format = "# ##0.00";

                        // Автоширина колонок
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show($"Отчет успешно экспортирован:\n{saveFileDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Экспорт отчета по начислениям в Excel
        /// </summary>
        public void ExportAccrualReport(List<AccrualReportDto> data, int year, int month)
        {
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string[] monthNames = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                                    "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"Отчет_начисления_{monthNames[month - 1]}_{year}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Начисления");

                        // Заголовки
                        worksheet.Cell(1, 1).Value = "№";
                        worksheet.Cell(1, 2).Value = "Адрес";
                        worksheet.Cell(1, 3).Value = "Период";
                        worksheet.Cell(1, 4).Value = "Начислено";
                        worksheet.Cell(1, 5).Value = "Оплачено";
                        worksheet.Cell(1, 6).Value = "Долг";

                        // Стиль заголовков
                        var headerRange = worksheet.Range(1, 1, 1, 6);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Данные
                        int row = 2;
                        decimal totalAccrual = 0;
                        decimal totalPaid = 0;
                        decimal totalDebt = 0;

                        foreach (var item in data)
                        {
                            worksheet.Cell(row, 1).Value = row - 1;
                            worksheet.Cell(row, 2).Value = item.Address;
                            worksheet.Cell(row, 3).Value = $"{monthNames[item.PeriodMonth - 1]} {item.PeriodYear}";
                            worksheet.Cell(row, 4).Value = item.AccrualAmount;
                            worksheet.Cell(row, 5).Value = item.PaidAmount;
                            worksheet.Cell(row, 6).Value = item.DebtAmount;

                            // Формат чисел
                            worksheet.Cell(row, 4).Style.NumberFormat.Format = "# ##0.00 ₽";
                            worksheet.Cell(row, 5).Style.NumberFormat.Format = "# ##0.00 ₽";
                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "# ##0.00 ₽";

                            // Цвет для долга
                            if (item.DebtAmount > 0)
                            {
                                worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Red;
                                worksheet.Cell(row, 6).Style.Font.Bold = true;
                            }

                            totalAccrual += item.AccrualAmount;
                            totalPaid += item.PaidAmount;
                            totalDebt += item.DebtAmount;
                            row++;
                        }

                        // Итоговая строка
                        worksheet.Cell(row, 3).Value = "ИТОГО:";
                        worksheet.Cell(row, 3).Style.Font.Bold = true;
                        worksheet.Cell(row, 4).Value = totalAccrual;
                        worksheet.Cell(row, 5).Value = totalPaid;
                        worksheet.Cell(row, 6).Value = totalDebt;

                        worksheet.Cell(row, 4).Style.Font.Bold = true;
                        worksheet.Cell(row, 5).Style.Font.Bold = true;
                        worksheet.Cell(row, 6).Style.Font.Bold = true;
                        worksheet.Cell(row, 6).Style.Font.FontColor = totalDebt > 0 ? XLColor.Red : XLColor.Black;

                        worksheet.Cell(row, 4).Style.NumberFormat.Format = "# ##0.00 ₽";
                        worksheet.Cell(row, 5).Style.NumberFormat.Format = "# ##0.00 ₽";
                        worksheet.Cell(row, 6).Style.NumberFormat.Format = "# ##0.00 ₽";

                        // Автоширина колонок
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show($"Отчет успешно экспортирован:\n{saveFileDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Экспорт реестра задолженности в Excel
        /// </summary>
        public void ExportDebtReport(List<DebtDto> data)
        {
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"Реестр_задолженности_{DateTime.Now:dd.MM.yyyy}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Задолженность");

                        // Заголовки
                        worksheet.Cell(1, 1).Value = "№";
                        worksheet.Cell(1, 2).Value = "Адрес";
                        worksheet.Cell(1, 3).Value = "Сумма долга";
                        worksheet.Cell(1, 4).Value = "Период";
                        worksheet.Cell(1, 5).Value = "Год";
                        worksheet.Cell(1, 6).Value = "Последний платеж";
                        worksheet.Cell(1, 7).Value = "Месяцев просрочки";

                        // Стиль заголовков
                        var headerRange = worksheet.Range(1, 1, 1, 7);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        string[] monthNames = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                                                "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

                        // Данные
                        int row = 2;
                        decimal totalDebt = 0;

                        foreach (var item in data)
                        {
                            worksheet.Cell(row, 1).Value = row - 1;
                            worksheet.Cell(row, 2).Value = item.Address;
                            worksheet.Cell(row, 3).Value = item.DebtAmount;
                            worksheet.Cell(row, 4).Value = monthNames[item.PeriodMonth - 1];
                            worksheet.Cell(row, 5).Value = item.PeriodYear;
                            worksheet.Cell(row, 6).Value = item.LastPaymentDate == DateTime.MinValue
                                ? "Нет платежей"
                                : item.LastPaymentDate.ToString("dd.MM.yyyy");
                            worksheet.Cell(row, 7).Value = item.MonthsOverdue;

                            // Формат чисел
                            worksheet.Cell(row, 3).Style.NumberFormat.Format = "# ##0.00 ₽";

                            // Цвет для долга (градация в зависимости от суммы)
                            if (item.DebtAmount > 10000)
                                worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.DarkRed;
                            else if (item.DebtAmount > 5000)
                                worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Red;
                            else if (item.DebtAmount > 1000)
                                worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Orange;

                            // Цвет для просрочки
                            if (item.MonthsOverdue > 6)
                                worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Red;
                            else if (item.MonthsOverdue > 3)
                                worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Orange;

                            totalDebt += item.DebtAmount;
                            row++;
                        }

                        // Итоговая строка
                        worksheet.Cell(row, 2).Value = "ИТОГО:";
                        worksheet.Cell(row, 2).Style.Font.Bold = true;
                        worksheet.Cell(row, 3).Value = totalDebt;
                        worksheet.Cell(row, 3).Style.Font.Bold = true;
                        worksheet.Cell(row, 3).Style.NumberFormat.Format = "# ##0.00 ₽";

                        // Автоширина колонок
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show($"Реестр задолженности успешно экспортирован:\n{saveFileDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Экспорт списка пользователей в Excel
        /// </summary>
        public void ExportUsersReport(List<UserDto> data)
        {
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"Пользователи_{DateTime.Now:dd.MM.yyyy}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Пользователи");

                        // Заголовки
                        worksheet.Cell(1, 1).Value = "№";
                        worksheet.Cell(1, 2).Value = "ФИО";
                        worksheet.Cell(1, 3).Value = "Логин";
                        worksheet.Cell(1, 4).Value = "Email";
                        worksheet.Cell(1, 5).Value = "Роль";
                        worksheet.Cell(1, 6).Value = "Статус";
                        worksheet.Cell(1, 7).Value = "Дата создания";

                        // Стиль заголовков
                        var headerRange = worksheet.Range(1, 1, 1, 7);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Данные
                        int row = 2;
                        foreach (var item in data)
                        {
                            worksheet.Cell(row, 1).Value = row - 1;
                            worksheet.Cell(row, 2).Value = item.FullName;
                            worksheet.Cell(row, 3).Value = item.Username;
                            worksheet.Cell(row, 4).Value = item.Email;
                            worksheet.Cell(row, 5).Value = item.RoleText;
                            worksheet.Cell(row, 6).Value = item.StatusText;
                            worksheet.Cell(row, 7).Value = item.CreatedText;

                            // Цвет статуса
                            if (item.IsActive)
                                worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Green;
                            else
                                worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Red;

                            row++;
                        }

                        // Автоширина колонок
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show($"Список пользователей успешно экспортирован:\n{saveFileDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

