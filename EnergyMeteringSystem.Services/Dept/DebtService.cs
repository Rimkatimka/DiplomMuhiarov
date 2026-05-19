using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.Services.Debt
{
    public class DebtService
    {
        private readonly EnergyMeteringSystemEntities _context;
        private readonly PaymentRepository _paymentRepository;
        private readonly ConsumptionObjectRepository _objectRepository;

        public DebtService()
        {
            _context = new EnergyMeteringSystemEntities();
            _paymentRepository = new PaymentRepository();
            _objectRepository = new ConsumptionObjectRepository();
        }

        private decimal GetCurrentTariff()
        {
            try
            {
                var tariff = _context.Tariff
                    .FirstOrDefault(t => t.StartDate <= DateTime.Today &&
                                         (t.EndDate == null || t.EndDate >= DateTime.Today));

                if (tariff != null)
                    return tariff.PricePerUnit;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения тарифа: {ex.Message}");
            }

            return 5.8m; // значение по умолчанию
        }

        /// <summary>
        /// Рассчитать сумму двухмесячного норматива для объекта
        /// </summary>
        public decimal CalculateTwoMonthNorm(int objectId)
        {
            var obj = _objectRepository.GetById(objectId);
            if (obj == null) return 5000;

            // Норматив 150 кВт·ч на человека (пример для вашего региона)
            decimal normPerPerson = 150m;
            decimal tariff = GetCurrentTariff(); // 5.8 руб/кВт·ч
            int residentCount = obj.ResidentCount ?? 1;

            decimal monthlyNormCost = normPerPerson * residentCount * tariff;
            return monthlyNormCost * 2; // Двухмесячный размер
        }

        /// <summary>
        /// Проверка всех должников и обновление статусов
        /// </summary>
        public void ProcessAllDebtors()
        {
            var debtors = _paymentRepository.GetDebtors();

            foreach (var debtor in debtors)
            {
                ProcessDebtor(debtor);
            }
        }
        public void UpdateDebtStatuses()
        {
            var debtors = _paymentRepository.GetDebtors();

            foreach (var debtor in debtors)
            {
                var twoMonthNorm = CalculateTwoMonthNorm(debtor.ConsumptionObjectId);
                var log = _context.DebtProcessLog.FirstOrDefault(l => l.ConsumptionObjectId == debtor.ConsumptionObjectId);

                if (log != null)
                {
                    log.DebtAmount = debtor.DebtAmount;
                    log.TwoMonthNormAmount = twoMonthNorm;
                    log.LastUpdate = DateTime.Now;

                    if (debtor.DebtAmount >= twoMonthNorm && log.Status == "NoDebt")
                    {
                        log.Status = "Warning";
                        // Здесь можно добавить отправку уведомления
                    }
                    else if (debtor.DebtAmount < twoMonthNorm)
                    {
                        log.Status = "NoDebt";
                        log.NotificationSent = false;
                        log.RestrictionApplied = false;
                        log.DisconnectionApplied = false;
                    }
                }
                else
                {
                    // Создаем запись если нет
                    _context.DebtProcessLog.Add(new DebtProcessLog
                    {
                        ConsumptionObjectId = debtor.ConsumptionObjectId,
                        DebtAmount = debtor.DebtAmount,
                        TwoMonthNormAmount = twoMonthNorm,
                        Status = debtor.DebtAmount >= twoMonthNorm ? "Warning" : "NoDebt",
                        LastUpdate = DateTime.Now
                    });
                }
            }

            _context.SaveChanges();
        }
        /// <summary>
        /// Обработка конкретного должника по постановлению №354
        /// </summary>
        public DebtProcessResult ProcessDebtor(DebtDto debtor)
        {
            var result = new DebtProcessResult();
            var log = GetOrCreateLog(debtor.ConsumptionObjectId);

            var obj = _objectRepository.GetById(debtor.ConsumptionObjectId);
            var twoMonthNorm = CalculateTwoMonthNorm(debtor.ConsumptionObjectId);

            result.DebtAmount = debtor.DebtAmount;
            result.TwoMonthNorm = twoMonthNorm;

            // Шаг 1: Проверка превышения двухмесячного норматива
            if (debtor.DebtAmount < twoMonthNorm)
            {
                result.Status = DebtStatus.NoDebt;
                result.Message = "Долг не превышает двухмесячный норматив";
                UpdateLog(log, DebtStatus.NoDebt);
                return result;
            }

            // Шаг 2: Долг превышает норматив - нужно уведомление
            if (!log.NotificationSent)
            {
                result.Status = DebtStatus.Warning;
                result.Message = $"Долг {debtor.DebtAmount:F2} руб. превышает норматив {twoMonthNorm:F2} руб. Требуется уведомление.";
                result.RequiredAction = "Направить уведомление за 20 дней";
                result.NextActionDate = DateTime.Now.AddDays(20);
                return result;
            }

            // Шаг 3: Уведомление отправлено, проверяем прошло ли 20 дней
            if (log.NotificationDate.HasValue &&
                log.NotificationDate.Value.AddDays(20) <= DateTime.Now &&
                !log.RestrictionApplied)
            {
                result.Status = DebtStatus.Restricted;
                result.Message = "Введено частичное ограничение подачи электроэнергии";
                result.RequiredAction = "Снизить подачу (если технически возможно)";
                result.NextActionDate = DateTime.Now.AddDays(10);

                log.RestrictionDate = DateTime.Now;
                log.RestrictionApplied = true;
                log.Status = "Restricted";
                _context.SaveChanges();
                return result;
            }

            // Шаг 4: После 10 дней ограничения - полное отключение
            if (log.RestrictionApplied &&
                log.RestrictionDate.Value.AddDays(10) <= DateTime.Now &&
                !log.DisconnectionApplied)
            {
                result.Status = DebtStatus.Disconnected;
                result.Message = "Произведено полное отключение электроэнергии";
                result.RequiredAction = "Обратиться в энергосбыт для погашения долга";

                log.DisconnectionDate = DateTime.Now;
                log.DisconnectionApplied = true;
                log.Status = "Disconnected";
                _context.SaveChanges();
                return result;
            }

            return result;
        }

        private DebtProcessLog GetOrCreateLog(int objectId)
        {
            var log = _context.DebtProcessLog.FirstOrDefault(l => l.ConsumptionObjectId == objectId);
            if (log == null)
            {
                log = new DebtProcessLog
                {
                    ConsumptionObjectId = objectId,
                    NotificationSent = false,
                    RestrictionApplied = false,
                    DisconnectionApplied = false,
                    Status = "NoDebt"
                };
                _context.DebtProcessLog.Add(log);
                _context.SaveChanges();
            }
            return log;
        }

        private void UpdateLog(DebtProcessLog log, DebtStatus status)
        {
            log.Status = status.ToString();
            log.LastUpdate = DateTime.Now;
            _context.SaveChanges();
        }

        /// <summary>
        /// Отметить, что уведомление отправлено
        /// </summary>
        public void MarkNotificationSent(int objectId)
        {
            var log = GetOrCreateLog(objectId);
            log.NotificationSent = true;
            log.NotificationDate = DateTime.Now;
            log.Status = "Warning";
            _context.SaveChanges();
        }
    }

    public class DebtProcessResult
    {
        public decimal DebtAmount { get; set; }
        public decimal TwoMonthNorm { get; set; }
        public DebtStatus Status { get; set; }
        public string Message { get; set; }
        public string RequiredAction { get; set; }
        public DateTime? NextActionDate { get; set; }
    }

    public enum DebtStatus
    {
        NoDebt,      // Нет долга или ниже норматива
        Warning,     // Требуется уведомление
        Restricted,  // Частичное ограничение
        Disconnected // Полное отключение
    }
}