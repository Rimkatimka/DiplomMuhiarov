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
            // Скрываем ненужные колонки
            if (e.PropertyName == "StartDate" || e.PropertyName == "EndDate" ||
                e.PropertyName == "PeriodStart" || e.PropertyName == "PeriodEnd" ||
                e.PropertyName == "StartValue" || e.PropertyName == "EndValue")
            {
                e.Cancel = true; // Скрываем колонку
                return;
            }
            var translations = new System.Collections.Generic.Dictionary<string, string>
    {
        {"Address", "Адрес"},
        {"Consumption", "Потребление, кВт·ч"},
        {"Percentage", "Доля, %"},
        {"Rank", "№"},
        {"ObjectType", "Тип объекта"},
        {"MeterSerial", "Серийный номер"},
        {"StartDate", "Дата начала"},
        {"EndDate", "Дата окончания"},
        {"ReadingDate", "Дата показания"},
        {"StartValue", "Нач. показание"},
        {"EndValue", "Кон. показание"},
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
        {"PeriodText", "Период"},
        {"UserName", "Пользователь"},
        {"FullName", "ФИО"},
        {"Role", "Роль"},
        {"EnteredCount", "Введено"},
        {"VerifiedCount", "Верифицировано"},
        {"RejectedCount", "Отклонено"},
        {"DebtAmount", "Сумма долга"},
        {"OverdueText", "Просрочка"}
        // {"Address", "Адрес"} ← ЭТУ СТРОКУ УБРАТЬ! Она уже есть в начале
    };

            if (translations.ContainsKey(e.PropertyName))
            {
                e.Column.Header = translations[e.PropertyName];
            }

            // Форматирование чисел
            if (e.Column is DataGridTextColumn textColumn &&
                (e.PropertyName == "Consumption" || e.PropertyName == "Value" ||
                 e.PropertyName == "StartValue" || e.PropertyName == "EndValue" ||
                 e.PropertyName == "DebtAmount"))
            {
                textColumn.Binding.StringFormat = "N2";
            }

            // Форматирование процентов
            if (e.Column is DataGridTextColumn percentColumn && e.PropertyName == "Percentage")
            {
                percentColumn.Binding.StringFormat = "F2";
            }

            // Форматирование дат
            if (e.Column is DataGridTextColumn dateColumn &&
                (e.PropertyName == "StartDate" || e.PropertyName == "EndDate" ||
                 e.PropertyName == "ReadingDate" || e.PropertyName == "VerificationDate" ||
                 e.PropertyName == "NextVerificationDate"))
            {
                dateColumn.Binding.StringFormat = "dd.MM.yyyy";
            }
        }

    }
}
