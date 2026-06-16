using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.Models;
using EnergyMeteringSystem.App.Services.ExcelExport;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Reports
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly ReportRepository _reportRepository;
        private readonly ExcelExportService _excelExport;
        private readonly MeterRepository _meterRepository;
        private readonly ConsumptionObjectRepository _objectRepository;

        private DateTime _startDate;
        private DateTime _endDate;
        private string _dateError;
        private int _selectedReportType = 0;
        private bool _isTableMode = true;
        private bool _isChartMode = false;

        private int _startYear;
        private int _startMonth;
        private int _endYear;
        private int _endMonth;
        private string _startMonthName;
        private string _endMonthName;

        private ConsumptionReport _consumptionData;
        private TopObjectsReport _topObjectsData;
        private ConsumptionByTypeReport _typeDistributionData;
        private MonthlyDynamicsReport _monthlyDynamicsData;
        private ConsumptionByRegionReport _regionData;
        private AnomaliesReport _anomaliesData;
        private ExpiringMetersReport _expiringMetersData;
        private OperatorActivityReport _operatorActivityData;
        private ObjectAnalyticsReport _objectAnalyticsData;

        private SeriesCollection _topObjectsSeries;
        private string[] _topObjectsLabels;
        private SeriesCollection _monthlySeries;
        private string[] _monthLabels;
        private SeriesCollection _typeDistributionSeries = new SeriesCollection();

        public string TotalConsumptionDisplay => _consumptionData?.TotalConsumption.ToString("N0") ?? "0";
        public string AverageConsumptionDisplay => _consumptionData?.AverageConsumption.ToString("N2") ?? "0";
        public string MaxConsumptionDisplay => _consumptionData?.MaxConsumption.ToString("N0") ?? "0";
        public string MinConsumptionDisplay => _consumptionData?.MinConsumption.ToString("N0") ?? "0";
        public string AnomalyCountDisplay => _consumptionData?.AnomalyCount.ToString() ?? "0";
        public string TotalObjectsDisplay => _consumptionData?.TotalObjects.ToString() ?? "0";
        public string TotalRecordsDisplay => _consumptionData?.TotalRecords.ToString() ?? "0";

        public ReportViewModel()
        {
            _reportRepository = new ReportRepository();
            _meterRepository = new MeterRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _excelExport = new ExcelExportService();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();

            ExportCommand = new AsyncRelayCommand(async () => await ExportReportAsync());

            InitializeYearsAndMonths();

            _startYear = 2025;
            _startMonth = 1;
            _endYear = 2025;
            _endMonth = 12;

            _startDate = new DateTime(2025, 1, 1);
            _endDate = new DateTime(2025, 12, 31);
            _startMonthName = Months[0];  // Январь
            _endMonthName = Months[11];
            TopObjectsSeries = new SeriesCollection();
            MonthlySeries = new SeriesCollection();


            _ = LoadReportAsync();
        }

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }

        public int SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                if (SetProperty(ref _selectedReportType, value))
                {
                    _ = LoadReportAsync();
                    OnPropertyChanged(nameof(CurrentReportTitle));
                    OnPropertyChanged(nameof(ShowPeriodFilter));
                    OnPropertyChanged(nameof(ShowYearFilter));
                    OnPropertyChanged(nameof(ShowTopChart));
                    OnPropertyChanged(nameof(ShowMonthlyChart));
                    OnPropertyChanged(nameof(ShowTypeDistributionChart));
                    OnPropertyChanged(nameof(CanShowChart));
                }
            }
        }

        public string CurrentReportTitle
        {
            get
            {
                switch (SelectedReportType)
                {
                    case 0: return "📊 Отчет по потреблению за период";
                    case 1: return "🏆 ТОП-10 объектов по потреблению";
                    case 2: return "📈 Потребление по типам объектов";
                    case 3: return "📉 Динамика потребления по месяцам";
                    case 4: return "🗺️ Потребление по регионам";
                    case 5: return "⚠️ Аномалии потребления";
                    case 6: return "🔧 Счетчики с истекающей поверкой";
                    case 7: return "👥 Активность операторов";
                    case 8: return "📐 Аналитика по объектам";
                    default: return "Отчет";
                }
            }
        }

        public SeriesCollection TypeDistributionSeries
        {
            get => _typeDistributionSeries;
            set => SetProperty(ref _typeDistributionSeries, value);
        }

        public bool CanShowChart => SelectedReportType == 0 || SelectedReportType == 1 || SelectedReportType == 2 || SelectedReportType == 3;
        public bool ShowTopChart => SelectedReportType == 0 || SelectedReportType == 1;
        public bool ShowTypeDistributionChart => SelectedReportType == 0 || SelectedReportType == 2;
        public bool ShowMonthlyChart => SelectedReportType == 3;

        public bool IsTableMode
        {
            get => _isTableMode;
            set
            {
                if (SetProperty(ref _isTableMode, value))
                {
                    _isChartMode = !value;
                    OnPropertyChanged(nameof(IsChartMode));
                    _ = LoadReportAsync();
                }
            }
        }

        public bool IsChartMode
        {
            get => _isChartMode && CanShowChart;
            set
            {
                if (CanShowChart)
                {
                    if (SetProperty(ref _isChartMode, value))
                    {
                        _isTableMode = !value;
                        OnPropertyChanged(nameof(IsTableMode));
                        _ = LoadReportAsync();
                    }
                }
                else if (_isChartMode)
                {
                    _isChartMode = false;
                    _isTableMode = true;
                    OnPropertyChanged(nameof(IsTableMode));
                    OnPropertyChanged(nameof(IsChartMode));
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    ValidateDates();
                    _ = LoadReportAsync();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    ValidateDates();
                    _ = LoadReportAsync();
                }
            }
        }

        public string DateError
        {
            get => _dateError;
            set => SetProperty(ref _dateError, value);
        }

        public int StartYear
        {
            get => _startYear;
            set
            {
                if (SetProperty(ref _startYear, value))
                {
                    _startDate = new DateTime(_startYear, _startMonth, 1);
                    OnPropertyChanged(nameof(StartDate));
                    _ = LoadReportAsync();
                }
            }
        }

        public int StartMonth
        {
            get => _startMonth;
            set
            {
                if (SetProperty(ref _startMonth, value))
                {
                    _startDate = new DateTime(_startYear, _startMonth, 1);
                    OnPropertyChanged(nameof(StartDate));
                    _ = LoadReportAsync();
                }
            }
        }

        public int EndYear
        {
            get => _endYear;
            set
            {
                if (SetProperty(ref _endYear, value))
                {
                    int lastDay = DateTime.DaysInMonth(_endYear, _endMonth);
                    _endDate = new DateTime(_endYear, _endMonth, lastDay);
                    OnPropertyChanged(nameof(EndDate));
                    _ = LoadReportAsync();
                }
            }
        }

        public int EndMonth
        {
            get => _endMonth;
            set
            {
                if (SetProperty(ref _endMonth, value))
                {
                    int lastDay = DateTime.DaysInMonth(_endYear, _endMonth);
                    _endDate = new DateTime(_endYear, _endMonth, lastDay);
                    OnPropertyChanged(nameof(EndDate));
                    _ = LoadReportAsync();
                }
            }
        }

        public string StartMonthName
        {
            get => _startMonthName;
            set
            {
                if (SetProperty(ref _startMonthName, value))
                {
                    StartMonth = Months.IndexOf(value) + 1;
                }
            }
        }

        public string EndMonthName
        {
            get => _endMonthName;
            set
            {
                if (SetProperty(ref _endMonthName, value))
                {
                    EndMonth = Months.IndexOf(value) + 1;
                }
            }
        }

        public object CurrentData => GetCurrentData();

        public SeriesCollection TopObjectsSeries
        {
            get => _topObjectsSeries;
            set => SetProperty(ref _topObjectsSeries, value);
        }

        public string[] TopObjectsLabels
        {
            get => _topObjectsLabels;
            set => SetProperty(ref _topObjectsLabels, value);
        }

        public SeriesCollection MonthlySeries
        {
            get => _monthlySeries;
            set => SetProperty(ref _monthlySeries, value);
        }

        public string[] MonthLabels
        {
            get => _monthLabels;
            set => SetProperty(ref _monthLabels, value);
        }

        public Func<double, string> XFormatter => value => $"{value:N0}";

        public string TotalConsumption
        {
            get
            {
                switch (SelectedReportType)
                {
                    case 0: return _consumptionData?.TotalConsumption.ToString("N0") ?? "0";
                    case 1: return _topObjectsData?.TotalConsumption.ToString("N0") ?? "0";
                    case 2: return _typeDistributionData?.TotalConsumption.ToString("N0") ?? "0";
                    case 3: return _monthlyDynamicsData?.TotalConsumption.ToString("N0") ?? "0";
                    case 4: return _regionData?.TotalConsumption.ToString("N0") ?? "0";
                    default: return "—";
                }
            }
        }

        public string TotalObjects
        {
            get
            {
                switch (SelectedReportType)
                {
                    case 0: return _consumptionData?.TotalObjects.ToString() ?? "0";
                    case 4: return _regionData?.Records.Sum(r => r.ObjectsCount).ToString() ?? "0";
                    default: return "—";
                }
            }
        }

        public string TotalRecords
        {
            get
            {
                switch (SelectedReportType)
                {
                    case 0: return _consumptionData?.TotalRecords.ToString() ?? "0";
                    case 1: return _topObjectsData?.Records.Count.ToString() ?? "0";
                    case 2: return _typeDistributionData?.Records.Count.ToString() ?? "0";
                    case 3: return _monthlyDynamicsData?.Records.Count.ToString() ?? "0";
                    case 4: return _regionData?.Records.Count.ToString() ?? "0";
                    case 5: return _anomaliesData?.Records.Count.ToString() ?? "0";
                    case 6: return _expiringMetersData?.Records.Count.ToString() ?? "0";
                    case 7: return _operatorActivityData?.Records.Count.ToString() ?? "0";
                    case 8: return _objectAnalyticsData?.Records.Count.ToString() ?? "0";
                    default: return "0";
                }
            }
        }

        public bool ShowPeriodFilter => SelectedReportType == 0 || SelectedReportType == 1 || SelectedReportType == 2 || SelectedReportType == 3 || SelectedReportType == 4 || SelectedReportType == 5 || SelectedReportType == 7 || SelectedReportType == 8;
        public bool ShowYearFilter => false;
        public bool ShowMonthlyFilter => false;
        public bool ShowSummary => SelectedReportType == 0 || SelectedReportType == 1 || SelectedReportType == 2 || SelectedReportType == 3 || SelectedReportType == 4;

        public AsyncRelayCommand ExportCommand { get; }

        private void InitializeYearsAndMonths()
        {
            for (int i = 2020; i <= DateTime.Today.Year + 1; i++)
                Years.Add(i);

            Months.Clear();
            Months.Add("Январь");
            Months.Add("Февраль");
            Months.Add("Март");
            Months.Add("Апрель");
            Months.Add("Май");
            Months.Add("Июнь");
            Months.Add("Июль");
            Months.Add("Август");
            Months.Add("Сентябрь");
            Months.Add("Октябрь");
            Months.Add("Ноябрь");
            Months.Add("Декабрь");
        }

        private void ValidateDates()
        {
            if (StartDate > EndDate)
            {
                DateError = "Дата начала не может быть позже даты окончания";
            }
            else if (EndDate > DateTime.Today)
            {
                DateError = "Дата окончания не может быть позже сегодняшнего дня";
            }
            else
            {
                DateError = string.Empty;
            }
        }

        private async Task LoadReportAsync()
        {
            if (!string.IsNullOrEmpty(DateError)) return;

            await ExecuteAsync(async () =>
            {
                switch (SelectedReportType)
                {
                    case 0: await LoadConsumptionReportAsync(); break;
                    case 1: await LoadTopObjectsReportAsync(); break;
                    case 2: await LoadTypeDistributionReportAsync(); break;
                    case 3: await LoadMonthlyDynamicsReportAsync(); break;
                    case 4: await LoadRegionReportAsync(); break;
                    case 5: await LoadAnomaliesReportAsync(); break;
                    case 6: await LoadExpiringMetersReportAsync(); break;
                    case 7: LoadOperatorActivityReport(); break;
                    case 8: await LoadObjectAnalyticsReportAsync(); break;
                }

                OnPropertyChanged(nameof(CurrentData));
                OnPropertyChanged(nameof(TotalConsumption));
                OnPropertyChanged(nameof(TotalObjects));
                OnPropertyChanged(nameof(TotalRecords));
            }, "Ошибка загрузки отчета");
        }

        private async Task LoadConsumptionReportAsync()
        {
            var data = await _reportRepository.GetConsumptionReportOptimizedAsync(StartDate, EndDate);

            if (data == null || !data.Any())
            {
                _consumptionData = new ConsumptionReport
                {
                    Title = "Отчет по потреблению электроэнергии",
                    PeriodStart = StartDate,
                    PeriodEnd = EndDate,
                    Records = new List<ConsumptionRecord>(),
                    TotalConsumption = 0,
                    AverageConsumption = 0,
                    MaxConsumption = 0,
                    MinConsumption = 0,
                    AnomalyCount = 0
                };
                return;
            }

            var records = data.Select(d => new ConsumptionRecord
            {
                Address = d.Address,
                MeterSerial = d.MeterSerial,
                StartValue = d.StartValue,
                EndValue = d.EndValue,
                Consumption = d.Consumption,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                ObjectType = d.ObjectType ?? "Не указан"
            }).ToList();

            var totalConsumption = records.Sum(r => r.Consumption);

            _consumptionData = new ConsumptionReport
            {
                Title = "Отчет по потреблению электроэнергии",
                PeriodStart = StartDate,
                PeriodEnd = EndDate,
                Records = records,
                TotalConsumption = totalConsumption,
                TotalObjects = records.Select(r => r.Address).Distinct().Count(),
                TotalRecords = records.Count,

                // ✅ ДОПОЛНИТЕЛЬНЫЕ ИТОГИ
                AverageConsumption = records.Any() ? totalConsumption / records.Count : 0,
                MaxConsumption = records.Any() ? records.Max(r => r.Consumption) : 0,
                MinConsumption = records.Any() ? records.Min(r => r.Consumption) : 0,
                AnomalyCount = records.Count(r => r.Consumption > 500),
                ConsumptionByType = records
                    .GroupBy(r => r.ObjectType)
                    .ToDictionary(g => g.Key, g => g.Sum(r => r.Consumption))
            };
        }

        private async Task LoadTypeDistributionReportAsync()
        {
            var data = await _reportRepository.GetConsumptionReportOptimizedAsync(StartDate, EndDate);

            if (data == null || !data.Any())
            {
                TypeDistributionSeries = new SeriesCollection();
                _typeDistributionData = new ConsumptionByTypeReport
                {
                    Title = "Потребление по типам объектов",
                    PeriodStart = StartDate,
                    PeriodEnd = EndDate,
                    TotalConsumption = 0,
                    Records = new System.Collections.Generic.List<TypeConsumptionRecord>()
                };
                return;
            }

            var typeDistribution = data
                .Where(d => !string.IsNullOrEmpty(d.ObjectType))
                .GroupBy(d => d.ObjectType)
                .Select(g => new { TypeName = g.Key, Consumption = g.Sum(x => x.Consumption) })
                .ToList();

            var totalConsumption = typeDistribution.Sum(x => x.Consumption);

            _typeDistributionData = new ConsumptionByTypeReport
            {
                Title = "Потребление по типам объектов",
                PeriodStart = StartDate,
                PeriodEnd = EndDate,
                TotalConsumption = totalConsumption,
                Records = typeDistribution.Select(x => new TypeConsumptionRecord
                {
                    ObjectType = x.TypeName,
                    Consumption = x.Consumption,
                    Percentage = totalConsumption > 0 ? (x.Consumption / totalConsumption) * 100 : 0
                }).ToList()
            };

            TypeDistributionSeries = new SeriesCollection();
            foreach (var type in typeDistribution)
            {
                TypeDistributionSeries.Add(new PieSeries
                {
                    Title = type.TypeName,
                    Values = new ChartValues<decimal> { type.Consumption },
                    DataLabels = true,
                    LabelPoint = point => $"{type.TypeName}: {point.Y:F0} кВт·ч"
                });
            }
        }

        private async Task LoadTopObjectsReportAsync()
        {
            var data = await _reportRepository.GetConsumptionReportOptimizedAsync(StartDate, EndDate);

            if (data == null || !data.Any())
            {
                _topObjectsData = new TopObjectsReport
                {
                    Title = "ТОП-10 объектов по потреблению",
                    PeriodStart = StartDate,
                    PeriodEnd = EndDate,
                    TotalConsumption = 0,
                    Records = new System.Collections.Generic.List<TopObjectRecord>()
                };
                return;
            }

            var topByObject = data
                .GroupBy(d => new { d.ObjectId, d.Address })
                .Select(g => new { g.Key.ObjectId, g.Key.Address, Consumption = g.Sum(x => x.Consumption) })
                .Where(x => x.Consumption > 0)
                .OrderByDescending(x => x.Consumption)
                .Take(10)
                .ToList();

            var totalConsumption = topByObject.Sum(x => x.Consumption);

            _topObjectsData = new TopObjectsReport
            {
                Title = "ТОП-10 объектов по потреблению",
                PeriodStart = StartDate,
                PeriodEnd = EndDate,
                TotalConsumption = totalConsumption,
                Records = topByObject.Select((x, idx) => new TopObjectRecord
                {
                    Rank = idx + 1,
                    Address = x.Address,
                    ObjectType = "Объект",
                    Consumption = x.Consumption,
                    Percentage = totalConsumption > 0 ? (x.Consumption / totalConsumption) * 100 : 0
                }).ToList()
            };

            if (IsChartMode && _topObjectsData.Records.Any())
            {
                var top10 = _topObjectsData.Records.Take(10).ToList();
                TopObjectsLabels = top10.Select(o => o.Address).ToArray();
                TopObjectsSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Потребление",
                        Values = new ChartValues<decimal>(top10.Select(o => o.Consumption)),
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:F0}"
                    }
                };
            }
        }

        private async Task LoadMonthlyDynamicsReportAsync()
        {
            try
            {
                var monthlyData = new List<MonthlyRecord>();

                DateTime periodStart = new DateTime(_startYear, _startMonth, 1);
                DateTime periodEnd = new DateTime(_endYear, _endMonth, DateTime.DaysInMonth(_endYear, _endMonth));

                // +1 месяц до начала — чтобы было "предыдущее" показание для первого месяца периода
                DateTime fetchFrom = periodStart.AddMonths(-1);

                var allReadings = await _reportRepository.GetRawReadingsForPeriodAsync(fetchFrom, periodEnd);

                // Для каждого счётчика и месяца берём максимальное значение (последнее показание)
                var byMeterMonth = allReadings
                    .GroupBy(r => new { r.MeterId, r.ReadingDate.Year, r.ReadingDate.Month })
                    .Select(g => new {
                        g.Key.MeterId,
                        g.Key.Year,
                        g.Key.Month,
                        Value = g.Max(r => r.Value)
                    })
                    .OrderBy(x => x.MeterId).ThenBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList();

                // Словарь: (год, месяц) → суммарное потребление по всем счётчикам
                var consumptionByMonth = new Dictionary<(int Year, int Month), decimal>();
                DateTime cur = periodStart;
                while (cur <= periodEnd)
                {
                    consumptionByMonth[(cur.Year, cur.Month)] = 0;
                    cur = cur.AddMonths(1);
                }

                // По каждому счётчику считаем разницу между соседними месяцами
                foreach (var meterId in byMeterMonth.Select(x => x.MeterId).Distinct())
                {
                    var meterReadings = byMeterMonth
                        .Where(x => x.MeterId == meterId)
                        .OrderBy(x => x.Year).ThenBy(x => x.Month)
                        .ToList();

                    for (int i = 1; i < meterReadings.Count; i++)
                    {
                        var curr = meterReadings[i];
                        var prev = meterReadings[i - 1];

                        // Пропускаем если этот месяц вне нашего периода (это был fetchFrom)
                        if (!consumptionByMonth.ContainsKey((curr.Year, curr.Month)))
                            continue;

                        decimal consumption = curr.Value - prev.Value;
                        if (consumption > 0)
                            consumptionByMonth[(curr.Year, curr.Month)] += consumption;
                    }
                }

                // Строим список записей в порядке месяцев
                cur = periodStart;
                while (cur <= periodEnd)
                {
                    monthlyData.Add(new MonthlyRecord
                    {
                        Year = cur.Year,
                        Month = cur.Month,
                        MonthName = Months[cur.Month - 1],
                        Consumption = consumptionByMonth.ContainsKey((cur.Year, cur.Month))
                                        ? consumptionByMonth[(cur.Year, cur.Month)]
                                        : 0
                    });
                    cur = cur.AddMonths(1);
                }

                var maxMonth = monthlyData.OrderByDescending(x => x.Consumption).FirstOrDefault();
                // Прямо перед _monthlyDynamicsData = new MonthlyDynamicsReport{...}
                foreach (var kvp in consumptionByMonth.OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month))
                    System.Diagnostics.Debug.WriteLine($"DEBUG {kvp.Key.Month}/{kvp.Key.Year} = {kvp.Value}");
                _monthlyDynamicsData = new MonthlyDynamicsReport
                {
                    Title = "Динамика потребления по месяцам",
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Records = monthlyData,
                    TotalConsumption = monthlyData.Sum(x => x.Consumption),
                    AverageConsumption = monthlyData.Any() ? monthlyData.Average(x => x.Consumption) : 0,
                    MaxConsumption = maxMonth?.Consumption ?? 0,
                    MaxMonth = maxMonth?.MonthName ?? ""
                };

                if (_monthlyDynamicsData.Records.Any())
                {
                    MonthLabels = _monthlyDynamicsData.Records.Select(x => $"{x.MonthName}\n{x.Year}").ToArray();
                    MonthlySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title      = "Потребление, кВт·ч",
                    Values     = new ChartValues<decimal>(_monthlyDynamicsData.Records.Select(x => x.Consumption)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:F0}"
                }
            };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА в LoadMonthlyDynamicsReportAsync: {ex.Message}");
                _monthlyDynamicsData = new MonthlyDynamicsReport
                {
                    Title = "Динамика потребления по месяцам",
                    Records = new List<MonthlyRecord>()
                };
            }
        }

        private async Task LoadRegionReportAsync()
        {
            var data = await _reportRepository.GetConsumptionReportOptimizedAsync(StartDate, EndDate);
            var regionData = new System.Collections.Generic.Dictionary<string, RegionConsumptionRecord>();

            foreach (var item in data)
            {
                string region = ExtractRegionFromAddress(item.Address);
                if (!regionData.ContainsKey(region))
                {
                    regionData[region] = new RegionConsumptionRecord
                    {
                        Region = region,
                        Cities = new System.Collections.Generic.List<CityConsumptionRecord>()
                    };
                }
                regionData[region].Consumption += item.Consumption;
                regionData[region].ObjectsCount++;
                regionData[region].MetersCount++;
            }

            var totalConsumption = regionData.Values.Sum(x => x.Consumption);

            _regionData = new ConsumptionByRegionReport
            {
                Title = "Потребление по регионам",
                PeriodStart = StartDate,
                PeriodEnd = EndDate,
                TotalConsumption = totalConsumption,
                Records = regionData.Values.Select(r => {
                    r.Percentage = totalConsumption > 0 ? (r.Consumption / totalConsumption) * 100 : 0;
                    return r;
                }).OrderByDescending(r => r.Consumption).ToList()
            };
        }

        private async Task LoadAnomaliesReportAsync()
        {
            var data = await _reportRepository.GetConsumptionReportOptimizedAsync(StartDate, EndDate);
            var anomalies = new System.Collections.Generic.List<AnomalyRecord>();

            var byMeter = data.GroupBy(d => d.MeterSerial);

            foreach (var meter in byMeter)
            {
                var ordered = meter.OrderBy(d => d.StartDate).ToList();
                for (int i = 1; i < ordered.Count; i++)
                {
                    var prev = ordered[i - 1];
                    var curr = ordered[i];
                    var diff = curr.Consumption - prev.Consumption;
                    var diffPercent = prev.Consumption > 0 ? (diff / prev.Consumption) * 100 : 0;

                    if (Math.Abs(diffPercent) > 50)
                    {
                        anomalies.Add(new AnomalyRecord
                        {
                            Address = curr.Address,
                            MeterSerial = curr.MeterSerial,
                            PreviousConsumption = prev.Consumption,
                            CurrentConsumption = curr.Consumption,
                            Difference = diff,
                            DifferencePercent = diffPercent,
                            Status = diff > 0 ? "Скачок ↑" : "Падение ↓",
                            Comment = diff > 0 ? "Аномально высокое потребление" : "Резкое снижение потребления"
                        });
                    }
                }
            }

            _anomaliesData = new AnomaliesReport
            {
                Title = "Аномалии потребления",
                PeriodStart = StartDate,
                PeriodEnd = EndDate,
                Records = anomalies.OrderByDescending(a => Math.Abs(a.DifferencePercent)).Take(50).ToList(),
                AnomalyThreshold = 50
            };
        }

        private async Task LoadExpiringMetersReportAsync()
        {
            var allMeters = await _meterRepository.GetAllAsync();
            var allObjects = (await _objectRepository.GetAllAsync()).ToDictionary(o => o.Id, o => o.Address);

            _expiringMetersData = new ExpiringMetersReport
            {
                Title = "Счетчики с истекающей поверкой",
                PeriodStart = DateTime.Today,
                PeriodEnd = DateTime.Today.AddMonths(3),
                Records = allMeters.Select(m => new ExpiringMeterRecord
                {
                    SerialNumber = m.SerialNumber,
                    Address = allObjects.ContainsKey(m.ConsumptionObjectId) ? allObjects[m.ConsumptionObjectId] : "Неизвестно",
                    VerificationDate = m.LastVerificationDate ?? DateTime.MinValue,
                    NextVerificationDate = m.NextVerificationDate ?? DateTime.MaxValue,
                    DaysLeft = m.NextVerificationDate.HasValue ? (m.NextVerificationDate.Value - DateTime.Today).Days : 999,
                    Status = !m.NextVerificationDate.HasValue ? "Не указана" :
                             m.NextVerificationDate < DateTime.Today ? "Просрочена" :
                             m.NextVerificationDate < DateTime.Today.AddMonths(1) ? "Скоро" : "Норма"
                }).ToList()
            };

            _expiringMetersData.ExpiredCount = _expiringMetersData.Records.Count(r => r.Status == "Просрочена");
            _expiringMetersData.ExpiringSoonCount = _expiringMetersData.Records.Count(r => r.Status == "Скоро");
            _expiringMetersData.NormalCount = _expiringMetersData.Records.Count(r => r.Status == "Норма");
        }

        private void LoadOperatorActivityReport()
        {
            _operatorActivityData = new OperatorActivityReport
            {
                Title = "Активность операторов",
                PeriodStart = StartDate,
                PeriodEnd = EndDate,
                Records = new System.Collections.Generic.List<OperatorActivityRecord>()
            };
        }

        private async Task LoadObjectAnalyticsReportAsync()
        {
            var consumptionData = await _reportRepository.GetConsumptionReportOptimizedAsync(StartDate, EndDate);
            var objects = await _objectRepository.GetAllAsync();

            var analytics = new System.Collections.Generic.List<ObjectAnalyticsRecord>();

            foreach (var obj in objects)
            {
                var objConsumption = consumptionData.Where(c => c.ObjectId == obj.Id).Sum(c => c.Consumption);
                if (objConsumption > 0)
                {
                    analytics.Add(new ObjectAnalyticsRecord
                    {
                        Address = obj.Address,
                        ObjectType = obj.ObjectTypeName,
                        TotalArea = obj.TotalArea ?? 0,
                        ResidentCount = obj.ResidentCount ?? 0,
                        Consumption = objConsumption
                    });
                }
            }

            _objectAnalyticsData = new ObjectAnalyticsReport
            {
                Title = "Аналитика по объектам (эффективность)",
                PeriodStart = StartDate,
                PeriodEnd = EndDate,
                Records = analytics.OrderByDescending(a => a.ConsumptionPerArea).ToList()
            };
        }

        private object GetCurrentData()
        {
            switch (SelectedReportType)
            {
                case 0: return _consumptionData?.Records;
                case 1: return _topObjectsData?.Records;
                case 2: return _typeDistributionData?.Records;
                case 3: return _monthlyDynamicsData?.Records;
                case 4: return _regionData?.Records;
                case 5: return _anomaliesData?.Records;
                case 6: return _expiringMetersData?.Records;
                case 7: return _operatorActivityData?.Records;
                case 8: return _objectAnalyticsData?.Records;
                default: return null;
            }
        }

        private async Task ExportReportAsync()
        {
            await ExecuteAsync(async () =>
            {
                ReportBase reportToExport = null;
                System.Collections.Generic.List<(string Category, decimal Value)> chartData = null;
                string chartTitle = null;
                Microsoft.Office.Interop.Excel.XlChartType chartType = Microsoft.Office.Interop.Excel.XlChartType.xlPie;

                System.Collections.Generic.List<(string Category, decimal Value)> secondChartData = null;
                string secondChartTitle = null;
                Microsoft.Office.Interop.Excel.XlChartType secondChartType = Microsoft.Office.Interop.Excel.XlChartType.xlColumnClustered;

                switch (SelectedReportType)
                {
                    case 0:
                        reportToExport = _consumptionData;
                        if (TypeDistributionSeries != null && TypeDistributionSeries.Any())
                        {
                            chartData = new System.Collections.Generic.List<(string, decimal)>();
                            foreach (var series in TypeDistributionSeries)
                            {
                                var pieSeries = series as PieSeries;
                                if (pieSeries != null)
                                {
                                    var values = pieSeries.Values as LiveCharts.ChartValues<decimal>;
                                    if (values != null && values.Count > 0)
                                    {
                                        string category = pieSeries.Title;
                                        if (category.Length > 25) category = category.Substring(0, 22) + "...";
                                        chartData.Add((category, values[0]));
                                    }
                                }
                            }
                            chartTitle = "Распределение по типам объектов";
                            chartType = Microsoft.Office.Interop.Excel.XlChartType.xlPie;
                        }

                        if (TopObjectsSeries != null && TopObjectsSeries.Any())
                        {
                            secondChartData = new System.Collections.Generic.List<(string, decimal)>();
                            var columnSeries = TopObjectsSeries[0] as ColumnSeries;
                            if (columnSeries != null && columnSeries.Values != null && _consumptionData?.Records != null)
                            {
                                var topObjects = _consumptionData.Records
                                    .GroupBy(r => r.Address)
                                    .Select(g => new { Address = g.Key, Consumption = g.Sum(x => x.Consumption) })
                                    .OrderByDescending(x => x.Consumption)
                                    .Take(10)
                                    .ToList();

                                for (int i = 0; i < topObjects.Count; i++)
                                {
                                    string address = topObjects[i].Address;
                                    if (address.Length > 30) address = address.Substring(0, 27) + "...";
                                    secondChartData.Add((address, topObjects[i].Consumption));
                                }
                            }
                            secondChartTitle = "ТОП-10 объектов по потреблению";
                            secondChartType = Microsoft.Office.Interop.Excel.XlChartType.xlColumnClustered;
                        }
                        break;

                    case 1:
                        reportToExport = _topObjectsData;
                        if (TopObjectsSeries != null && TopObjectsSeries.Any())
                        {
                            chartData = new System.Collections.Generic.List<(string, decimal)>();
                            var columnSeries = TopObjectsSeries[0] as ColumnSeries;
                            if (columnSeries != null && columnSeries.Values != null && _topObjectsData?.Records != null)
                            {
                                foreach (var record in _topObjectsData.Records)
                                {
                                    string address = record.Address;
                                    if (address.Length > 30) address = address.Substring(0, 27) + "...";
                                    chartData.Add((address, record.Consumption));
                                }
                            }
                            chartTitle = "ТОП-10 объектов по потреблению";
                            chartType = Microsoft.Office.Interop.Excel.XlChartType.xlColumnClustered;
                        }
                        break;

                    case 2:
                        reportToExport = _typeDistributionData;
                        if (TypeDistributionSeries != null && TypeDistributionSeries.Any())
                        {
                            chartData = new System.Collections.Generic.List<(string, decimal)>();
                            foreach (var series in TypeDistributionSeries)
                            {
                                var pieSeries = series as PieSeries;
                                if (pieSeries != null)
                                {
                                    var values = pieSeries.Values as LiveCharts.ChartValues<decimal>;
                                    if (values != null && values.Count > 0)
                                    {
                                        string category = pieSeries.Title;
                                        if (category.Length > 25) category = category.Substring(0, 22) + "...";
                                        chartData.Add((category, values[0]));
                                    }
                                }
                            }
                            chartTitle = "Распределение по типам объектов";
                            chartType = Microsoft.Office.Interop.Excel.XlChartType.xlPie;
                        }
                        break;

                    case 3:
                        reportToExport = _monthlyDynamicsData;
                        if (MonthlySeries != null && MonthlySeries.Any())
                        {
                            chartData = new System.Collections.Generic.List<(string, decimal)>();
                            var columnSeries = MonthlySeries[0] as ColumnSeries;
                            if (columnSeries != null && columnSeries.Values != null && _monthlyDynamicsData?.Records != null)
                            {
                                foreach (var record in _monthlyDynamicsData.Records)
                                {
                                    chartData.Add(($"{record.MonthName} {record.Year}", record.Consumption));
                                }
                            }
                            chartTitle = "Динамика потребления по месяцам";
                            chartType = Microsoft.Office.Interop.Excel.XlChartType.xlLine;
                        }
                        break;

                    case 4: reportToExport = _regionData; break;
                    case 5: reportToExport = _anomaliesData; break;
                    case 6: reportToExport = _expiringMetersData; break;
                    case 7: reportToExport = _operatorActivityData; break;
                    case 8: reportToExport = _objectAnalyticsData; break;
                }

                if (reportToExport == null) return;

                string fileName = CurrentReportTitle.Replace("📊 ", "").Replace("🏆 ", "").Replace("📈 ", "").Replace("📉 ", "").Replace("🗺️ ", "").Replace("⚠️ ", "").Replace("🔧 ", "").Replace("👥 ", "").Replace("📐 ", "");

                if (IsChartMode)
                {
                    if (SelectedReportType == 0 && (chartData != null || secondChartData != null))
                    {
                        await Task.Run(() => _excelExport.ExportReportWithTwoCharts(reportToExport, fileName,
                            chartData, chartTitle, chartType,
                            secondChartData, secondChartTitle, secondChartType));
                    }
                    else if (chartData != null && chartData.Any())
                    {
                        await Task.Run(() => _excelExport.ExportReportWithNativeChart(reportToExport, fileName, chartData, chartTitle, chartType));
                    }
                    else
                    {
                        await Task.Run(() => _excelExport.ExportReport(reportToExport, fileName));
                    }
                }
                else
                {
                    await Task.Run(() => _excelExport.ExportReport(reportToExport, fileName));
                }
            }, "Ошибка экспорта отчета");
        }

        private string ExtractRegionFromAddress(string address)
        {
            if (address.Contains("Москва") || address.Contains("Московская")) return "Московская область";
            if (address.Contains("СПб") || address.Contains("Санкт-Петербург") || address.Contains("Ленинград")) return "Ленинградская область";
            if (address.Contains("Казань") || address.Contains("Татарстан")) return "Республика Татарстан";
            if (address.Contains("Уфа") || address.Contains("Башкортостан")) return "Республика Башкортостан";
            return "Другие регионы";
        }
    }
}