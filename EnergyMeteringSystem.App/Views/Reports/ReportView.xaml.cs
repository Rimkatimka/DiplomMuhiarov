using EnergyMeteringSystem.App.ViewModels.Reports;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Reports
{
    /// <summary>
    /// Логика взаимодействия для ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
            DataContext = new ReportViewModel();
        }

        public void RefreshColumns()
        {
            if (MainDataGrid != null && MainDataGrid.ItemsSource != null)
            {
                var columns = MainDataGrid.Columns.ToList();
                MainDataGrid.AutoGenerateColumns = false;
                MainDataGrid.Columns.Clear();
                MainDataGrid.AutoGenerateColumns = true;
                MainDataGrid.ItemsSource = null;
                MainDataGrid.ItemsSource = (System.Collections.IEnumerable)((DataContext as ViewModels.Reports.ReportViewModel)?.CurrentData);
            }
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // ============================================================
            // 1. СКРЫВАЕМ НЕНУЖНЫЕ КОЛОНКИ
            // ============================================================
            if (e.PropertyName == "StartDate" || e.PropertyName == "EndDate" ||
                e.PropertyName == "PeriodStart" || e.PropertyName == "PeriodEnd" ||
                e.PropertyName == "Id" || e.PropertyName == "ObjectId" ||
                e.PropertyName == "MeterId" || e.PropertyName == "StreetId" ||
                e.PropertyName == "CityId" || e.PropertyName == "RegionId" ||
                e.PropertyName == "PeriodText" || e.PropertyName == "ConsumptionText" ||
                e.PropertyName == "StatusColor" || e.PropertyName == "VerificationStatusColor" ||
                e.PropertyName == "PreviousDisplay" || e.PropertyName == "CurrentDisplay" ||
                e.PropertyName == "DifferenceDisplay" || e.PropertyName == "DifferencePercentDisplay" ||
                e.PropertyName == "ConsumptionPerAreaDisplay" || e.PropertyName == "ConsumptionPerPersonDisplay" ||
                e.PropertyName == "VerificationDateDisplay" || e.PropertyName == "NextVerificationDateDisplay")
            {
                e.Cancel = true;
                return;
            }

            // ============================================================
            // 2. ПЕРЕВОДЫ ЗАГОЛОВКОВ
            // ============================================================
            var translations = new Dictionary<string, string>
            {
                // Основные
                {"Address", "Адрес"},
                {"Consumption", "Потребление, кВт·ч"},
                {"Percentage", "Доля, %"},
                {"Rank", "№"},
                {"ObjectType", "Тип объекта"},
                {"MeterSerial", "Серийный номер"},
                {"ReadingDate", "Дата показания"},
                {"Value", "Показание"},
                {"Region", "Регион"},
                {"City", "Город"},
                {"Street", "Улица"},
                {"HouseNumber", "Дом"},
                {"ApartmentNumber", "Кв."},
                {"TotalArea", "Площадь, м²"},
                {"ResidentCount", "Жильцов"},
                {"SerialNumber", "Серийный номер"},
                {"Status", "Статус"},
                {"DaysLeft", "Дней осталось"},
                {"VerificationDate", "Дата поверки"},
                {"NextVerificationDate", "След. поверка"},
                {"PeriodDisplay", "Период"},
                {"UserName", "Пользователь"},
                {"FullName", "ФИО"},
                {"Role", "Роль"},
                {"EnteredCount", "Введено"},
                {"VerifiedCount", "Верифицировано"},
                {"RejectedCount", "Отклонено"},
                {"DebtAmount", "Сумма долга"},
                {"OverdueText", "Просрочка"},
                
                // Даты
                {"StartDate", "Дата начала"},
                {"EndDate", "Дата окончания"},
                {"StartValue", "Нач. показание"},
                {"EndValue", "Кон. показание"},
                {"PreviousConsumption", "Пред. потребление"},
                {"CurrentConsumption", "Тек. потребление"},
                {"Difference", "Разница"},
                {"DifferencePercent", "Разница, %"},
                {"MonthName", "Месяц"},
                {"Year", "Год"},
                {"ObjectsCount", "Кол-во объектов"},
                {"MetersCount", "Кол-во счетчиков"},
                {"ConsumptionPerArea", "кВт·ч/м²"},
                {"ConsumptionPerPerson", "кВт·ч/чел"},
                {"ServiceLifeYears", "Срок службы, лет"},
                {"RemovalDate", "Дата изъятия"},
                {"InitialReading", "Нач. показание"},
                {"InstallationDate", "Дата установки"},
                {"LastVerificationDate", "Последняя поверка"},
                {"VerificationStatusText", "Статус поверки"},
                {"StatusName", "Статус"},
                {"MeterTypeName", "Тип счетчика"},
                {"Username", "Логин"},
                {"Email", "Email"},
                {"CreatedText", "Дата создания"},
                {"LastLoginText", "Последний вход"},
                {"IsActive", "Активен"},
                {"RoleText", "Роль"},
                {"DisplayName", "Пользователь"},
                {"Comment", "Комментарий"},
                {"Reason", "Причина"},
                {"ActionType", "Действие"},
                {"TableName", "Таблица"},
                {"RecordId", "ID записи"},
                {"ActionTime", "Время"},
                {"UserDisplay", "Пользователь"},
                {"DisplayDetails", "Детали"},
                {"StatusColor", "Цвет статуса"},
                {"StatusText", "Статус"},
                {"Amount", "Сумма"},
                {"Tariff", "Тариф"},
                {"ConsumptionText", "Потребление"},
                {"ObjectTitle", "Объект"},
                {"RegionName", "Регион"},
                {"CityName", "Город"},
                {"StreetName", "Улица"},
                {"TotalConsumption", "Общее потребление"},
                {"AveragePerObject", "Среднее на объект"},
                {"TypeName", "Тип"},
                {"EnteredBy", "Кто ввёл"},
                {"EnteredAt", "Время ввода"},
                {"ReadingStatusId", "Статус"},
                {"TariffZone", "Тарифная зона"},
                {"RejectionReasonId", "Причина отклонения"},
                {"OldValuesJson", "Старые значения"},
                {"NewValuesJson", "Новые значения"},
                {"IpAddress", "IP-адрес"},
            };

            // Применяем перевод
            if (translations.ContainsKey(e.PropertyName))
            {
                e.Column.Header = translations[e.PropertyName];
            }

            // ============================================================
            // 3. ФОРМАТИРОВАНИЕ КОЛОНОК
            // ============================================================
            if (e.Column is DataGridTextColumn textColumn)
            {
                // ----- 3.1. Десятичные числа (N2) -----
                if (e.PropertyName == "Consumption" || e.PropertyName == "Value" ||
                    e.PropertyName == "StartValue" || e.PropertyName == "EndValue" ||
                    e.PropertyName == "DebtAmount" || e.PropertyName == "Amount" ||
                    e.PropertyName == "TotalArea" || e.PropertyName == "InitialReading" ||
                    e.PropertyName == "PreviousConsumption" || e.PropertyName == "CurrentConsumption" ||
                    e.PropertyName == "Difference" || e.PropertyName == "ConsumptionPerArea" ||
                    e.PropertyName == "ConsumptionPerPerson")
                {
                    textColumn.Binding.StringFormat = "N2";
                }

                // ----- 3.2. Целые числа (N0) -----
                if (e.PropertyName == "DaysLeft" || e.PropertyName == "ResidentCount" ||
                    e.PropertyName == "ServiceLifeYears" || e.PropertyName == "Rank" ||
                    e.PropertyName == "ObjectsCount" || e.PropertyName == "MetersCount" ||
                    e.PropertyName == "EnteredCount" || e.PropertyName == "VerifiedCount" ||
                    e.PropertyName == "RejectedCount" || e.PropertyName == "Year")
                {
                    textColumn.Binding.StringFormat = "N0";
                }

                // ----- 3.3. Проценты (F2) -----
                if (e.PropertyName == "Percentage" || e.PropertyName == "DifferencePercent")
                {
                    textColumn.Binding.StringFormat = "F2";
                }

                // ----- 3.4. Даты (dd.MM.yyyy) -----
                if (e.PropertyName == "ReadingDate" || e.PropertyName == "VerificationDate" ||
                    e.PropertyName == "NextVerificationDate" || e.PropertyName == "InstallationDate" ||
                    e.PropertyName == "LastVerificationDate" || e.PropertyName == "RemovalDate" ||
                    e.PropertyName == "StartDate" || e.PropertyName == "EndDate" ||
                    e.PropertyName == "ActionTime" || e.PropertyName == "CreatedAt" ||
                    e.PropertyName == "CreatedText")
                {
                    textColumn.Binding.StringFormat = "dd.MM.yyyy";
                }

                // ----- 3.5. Дата + Время (dd.MM.yyyy HH:mm) -----
                if (e.PropertyName == "EnteredAt" || e.PropertyName == "LastLoginText")
                {
                    textColumn.Binding.StringFormat = "dd.MM.yyyy HH:mm";
                }
            }

            // ============================================================
            // 4. ШИРИНА КОЛОНОК
            // ============================================================
            if (e.PropertyName == "Address" || e.PropertyName == "FullName" ||
                e.PropertyName == "DisplayDetails" || e.PropertyName == "Comment")
            {
                e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
            else if (e.PropertyName == "Consumption" || e.PropertyName == "Value")
            {
                e.Column.Width = new DataGridLength(120);
            }
            else if (e.PropertyName == "Percentage" || e.PropertyName == "DifferencePercent")
            {
                e.Column.Width = new DataGridLength(80);
            }
            else if (e.PropertyName == "PeriodDisplay" || e.PropertyName == "PeriodText")
            {
                e.Column.Width = new DataGridLength(130);
            }
            else if (e.PropertyName == "Rank" || e.PropertyName == "DaysLeft" ||
                     e.PropertyName == "ObjectsCount" || e.PropertyName == "MetersCount")
            {
                e.Column.Width = new DataGridLength(70);
            }
            else
            {
                e.Column.Width = new DataGridLength(100);
            }
        }
    }
}