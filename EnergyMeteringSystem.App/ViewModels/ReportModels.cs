using System;
using System.Collections.Generic;

namespace EnergyMeteringSystem.App.Models
{
    // Базовый класс для всех отчетов
    public abstract class ReportBase
    {
        public string Title { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    // 1. Отчет по потреблению за период
    public class ConsumptionReport : ReportBase
    {
        public List<ConsumptionRecord> Records { get; set; } = new();
        public decimal TotalConsumption { get; set; }
        public int TotalObjects { get; set; }
        public int TotalRecords { get; set; }
    }

    public class ConsumptionRecord
    {
        public string Address { get; set; }
        public string MeterSerial { get; set; }
        public decimal StartValue { get; set; }
        public decimal EndValue { get; set; }
        public decimal Consumption { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Старый период (скрыт)
        public string PeriodText => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";

        // Новый период (показывается вместо дат)
        public string PeriodDisplay
        {
            get
            {
                if (StartDate.Month == EndDate.Month && StartDate.Year == EndDate.Year)
                {
                    string[] months = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                                    "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
                    return months[StartDate.Month - 1] + " " + StartDate.Year;
                }
                else
                {
                    return $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
                }
            }
        }
    }

    // 2. ТОП-10 объектов
    public class TopObjectsReport : ReportBase
    {
        public List<TopObjectRecord> Records { get; set; } = new();
        public decimal TotalConsumption { get; set; }
    }

    public class TopObjectRecord
    {
        public int Rank { get; set; }
        public string Address { get; set; }
        public string ObjectType { get; set; }
        public decimal Consumption { get; set; }
        public decimal Percentage { get; set; }
    }

    // 3. Потребление по типам объектов
    public class ConsumptionByTypeReport : ReportBase
    {
        public List<TypeConsumptionRecord> Records { get; set; } = new();
        public decimal TotalConsumption { get; set; }
    }

    public class TypeConsumptionRecord
    {
        public string ObjectType { get; set; }
        public decimal Consumption { get; set; }
        public decimal Percentage { get; set; }
    }

    // 4. Динамика по месяцам
    public class MonthlyDynamicsReport : ReportBase
    {
        public List<MonthlyRecord> Records { get; set; } = new();
        public decimal TotalConsumption { get; set; }
        public decimal AverageConsumption { get; set; }
        public decimal MaxConsumption { get; set; }
        public string MaxMonth { get; set; }
    }

    public class MonthlyRecord
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal Consumption { get; set; }
    }

    // 5. Потребление по регионам
    public class ConsumptionByRegionReport : ReportBase
    {
        public List<RegionConsumptionRecord> Records { get; set; } = new();
        public decimal TotalConsumption { get; set; }
    }

    public class RegionConsumptionRecord
    {
        public string Region { get; set; }
        public decimal Consumption { get; set; }
        public decimal Percentage { get; set; }
        public int ObjectsCount { get; set; }
        public int MetersCount { get; set; }
        public List<CityConsumptionRecord> Cities { get; set; } = new();
    }

    public class CityConsumptionRecord
    {
        public string City { get; set; }
        public decimal Consumption { get; set; }
        public decimal Percentage { get; set; }
        public int ObjectsCount { get; set; }
    }

    // 6. Аналитика по объектам (на м² / чел)
    public class ObjectAnalyticsReport : ReportBase
    {
        public List<ObjectAnalyticsRecord> Records { get; set; } = new();
    }

    public class ObjectAnalyticsRecord
    {
        public string Address { get; set; }
        public string ObjectType { get; set; }
        public decimal TotalArea { get; set; }
        public int ResidentCount { get; set; }
        public decimal Consumption { get; set; }
        public decimal ConsumptionPerArea => TotalArea > 0 ? Consumption / TotalArea : 0;
        public decimal ConsumptionPerPerson => ResidentCount > 0 ? Consumption / ResidentCount : 0;
    }

    // 7. Аномалии потребления
    public class AnomaliesReport : ReportBase
    {
        public List<AnomalyRecord> Records { get; set; } = new();
        public decimal AnomalyThreshold { get; set; } = 50; // порог в %
    }

    public class AnomalyRecord
    {
        public string Address { get; set; }
        public string MeterSerial { get; set; }
        public decimal PreviousConsumption { get; set; }
        public decimal CurrentConsumption { get; set; }
        public decimal Difference { get; set; }
        public decimal DifferencePercent { get; set; }
        public string Status { get; set; } // "Скачок", "Падение"
        public string Comment { get; set; }
    }

    // 8. Счетчики с истекающей поверкой
    public class ExpiringMetersReport : ReportBase
    {
        public List<ExpiringMeterRecord> Records { get; set; } = new();
        public int ExpiredCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int NormalCount { get; set; }
    }

    public class ExpiringMeterRecord
    {
        public string SerialNumber { get; set; }
        public string Address { get; set; }
        public DateTime VerificationDate { get; set; }
        public DateTime NextVerificationDate { get; set; }
        public int DaysLeft { get; set; }
        public string Status { get; set; } // "Просрочена", "Скоро", "Норма"
        public string StatusColor { get; set; }
    }

    // 9. Активность операторов
    public class OperatorActivityReport : ReportBase
    {
        public List<OperatorActivityRecord> Records { get; set; } = new();
        public int TotalEntered { get; set; }
        public int TotalVerified { get; set; }
    }

    public class OperatorActivityRecord
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public int EnteredCount { get; set; }
        public int VerifiedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    // 10. История показаний счетчика
    public class MeterHistoryReport : ReportBase
    {
        public string MeterSerial { get; set; }
        public string Address { get; set; }
        public List<MeterHistoryRecord> Records { get; set; } = new();
    }

    public class MeterHistoryRecord
    {
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public decimal Consumption { get; set; }
        public string Status { get; set; }
        public string EnteredBy { get; set; }
    }

    // 11. Дашборд (KPI)
    public class DashboardReport : ReportBase
    {
        public int TotalObjects { get; set; }
        public int TotalMeters { get; set; }
        public int ReadingsToday { get; set; }
        public int ReadingsWeek { get; set; }
        public int ExpiredMeters { get; set; }
        public int ExpiringSoonMeters { get; set; }
        public List<MonthlyRecord> MonthlyDynamics { get; set; } = new();
        public List<TopObjectRecord> TopAnomalies { get; set; } = new();
    }
}