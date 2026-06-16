using EnergyMeteringSystem.App.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Reports
{
    public partial class ReportView : UserControl
    {
        private Window _parentWindow;

        public ReportView()
        {
            InitializeComponent();
            DataContext = new ReportViewModel();

            // Подписываемся на загрузку контрола
            this.Loaded += ReportView_Loaded;
            this.Unloaded += ReportView_Unloaded;
        }

        private void ReportView_Loaded(object sender, RoutedEventArgs e)
        {
            // Находим родительское окно
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                // Подписываемся на изменение размера окна
                _parentWindow.SizeChanged += ParentWindow_SizeChanged;

                // Первоначальное обновление ширины
                RefreshColumnsWidth();
            }
        }

        private void ReportView_Unloaded(object sender, RoutedEventArgs e)
        {
            // Отписываемся от события при выгрузке
            if (_parentWindow != null)
            {
                _parentWindow.SizeChanged -= ParentWindow_SizeChanged;
                _parentWindow = null;
            }
        }

        private void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Обновляем ширину колонок при изменении размера окна
            RefreshColumnsWidth();
        }

        private void RefreshColumnsWidth()
        {
            if (MainDataGrid == null || MainDataGrid.Columns.Count == 0) return;

            // Получаем фактическую ширину DataGrid
            double totalWidth = MainDataGrid.ActualWidth;

            // Если ширина = 0, пробуем получить ширину родителя
            if (totalWidth <= 0)
            {
                var parent = MainDataGrid.Parent as FrameworkElement;
                if (parent != null)
                {
                    totalWidth = parent.ActualWidth - 20; // Отступы
                }
            }

            if (totalWidth <= 0) return;

            // Считаем количество видимых колонок
            int visibleColumns = 0;
            foreach (var col in MainDataGrid.Columns)
            {
                if (col.Visibility == Visibility.Visible)
                    visibleColumns++;
            }

            if (visibleColumns == 0) return;

            // Равномерно распределяем ширину
            double baseWidth = totalWidth / visibleColumns;

            foreach (var col in MainDataGrid.Columns)
            {
                if (col.Visibility == Visibility.Visible)
                {
                    // Устанавливаем ширину с небольшим запасом
                    col.Width = new DataGridLength(baseWidth, DataGridLengthUnitType.Star);
                }
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
                e.PropertyName == "StatusColor" || e.PropertyName == "VerificationStatusColor")
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
                {"StatusText", "Статус"},
                {"Amount", "Сумма"},
                {"Tariff", "Тариф"},
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
                // Десятичные числа (N2)
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

                // Целые числа (N0)
                if (e.PropertyName == "DaysLeft" || e.PropertyName == "ResidentCount" ||
                    e.PropertyName == "ServiceLifeYears" || e.PropertyName == "Rank" ||
                    e.PropertyName == "ObjectsCount" || e.PropertyName == "MetersCount" ||
                    e.PropertyName == "EnteredCount" || e.PropertyName == "VerifiedCount" ||
                    e.PropertyName == "RejectedCount" || e.PropertyName == "Year")
                {
                    textColumn.Binding.StringFormat = "N0";
                }

                // Проценты (F2)
                if (e.PropertyName == "Percentage" || e.PropertyName == "DifferencePercent")
                {
                    textColumn.Binding.StringFormat = "F2";
                }

                // Даты (dd.MM.yyyy)
                if (e.PropertyName == "ReadingDate" || e.PropertyName == "VerificationDate" ||
                    e.PropertyName == "NextVerificationDate" || e.PropertyName == "InstallationDate" ||
                    e.PropertyName == "LastVerificationDate" || e.PropertyName == "RemovalDate" ||
                    e.PropertyName == "ActionTime" || e.PropertyName == "CreatedAt" ||
                    e.PropertyName == "CreatedText")
                {
                    textColumn.Binding.StringFormat = "dd.MM.yyyy";
                }

                // Дата + Время (dd.MM.yyyy HH:mm)
                if (e.PropertyName == "EnteredAt" || e.PropertyName == "LastLoginText")
                {
                    textColumn.Binding.StringFormat = "dd.MM.yyyy HH:mm";
                }
            }

            // ============================================================
            // 4. ОБНОВЛЯЕМ ШИРИНУ ПОСЛЕ СОЗДАНИЯ ВСЕХ КОЛОНОК
            // ============================================================
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                RefreshColumnsWidth();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}