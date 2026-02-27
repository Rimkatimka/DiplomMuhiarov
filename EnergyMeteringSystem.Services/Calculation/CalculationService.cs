using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Services;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.Services.Calculation
{
    public class CalculationService : ICalculationService
    {
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly MeterReadingRepository _readingRepository;
        private readonly AccrualRepository _accrualRepository;
        private readonly MeterRepository _meterRepository;

        public CalculationService()
        {
            _objectRepository = new ConsumptionObjectRepository();
            _readingRepository = new MeterReadingRepository();
            _accrualRepository = new AccrualRepository();
            _meterRepository = new MeterRepository();
        }

        public List<AccrualCalculationDto> CalculateForPeriod(int year, int month)
        {
            List<AccrualCalculationDto> result = [];

            try
            {
                System.Diagnostics.Debug.WriteLine($"=================================");
                System.Diagnostics.Debug.WriteLine($"РАСЧЕТ НАЧИСЛЕНИЙ ЗА ПЕРИОД: {month}.{year}");
                System.Diagnostics.Debug.WriteLine($"=================================");

                List<ConsumptionObjectDto> objects = _objectRepository.GetAll();
                System.Diagnostics.Debug.WriteLine($"Всего объектов в БД: {objects.Count}");

                foreach (ConsumptionObjectDto obj in objects)
                {
                    System.Diagnostics.Debug.WriteLine($"\n--- Объект: {obj.Address} (ID={obj.Id}) ---");

                    // Проверяем, есть ли уже начисление за этот период
                    bool exists = _accrualRepository.Exists(obj.Id, year, month);
                    System.Diagnostics.Debug.WriteLine($"Начисление за период уже существует: {exists}");

                    // Получаем показания за период
                    List<MeterReadingDto> readings = _readingRepository.GetReadingsForPeriod(obj.Id, year, month);
                    System.Diagnostics.Debug.WriteLine($"Найдено показаний за период: {readings.Count}");

                    if (!readings.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"Нет показаний за {month}.{year}, объект пропущен");
                        continue;
                    }

                    decimal totalConsumption = 0;

                    // Группируем по счетчикам
                    IEnumerable<IGrouping<int, MeterReadingDto>> meterReadings = readings.GroupBy(r => r.MeterId);
                    System.Diagnostics.Debug.WriteLine($"Количество счетчиков с показаниями: {meterReadings.Count()}");

                    foreach (IGrouping<int, MeterReadingDto> meter in meterReadings)
                    {
                        List<MeterReadingDto> ordered = meter.OrderBy(r => r.ReadingDate).ToList();
                        System.Diagnostics.Debug.WriteLine($"  Счетчик ID={meter.Key}, показаний: {ordered.Count}");

                        if (ordered.Any())
                        {
                            MeterReadingDto last = ordered.Last();
                            System.Diagnostics.Debug.WriteLine($"    Последнее показание: {last.ReadingDate:dd.MM.yyyy} = {last.Value}");

                            // Получаем показания за предыдущий месяц
                            DateTime previousMonth = new DateTime(year, month, 1).AddMonths(-1);
                            List<MeterReadingDto> previousReadings = _readingRepository
                                .GetReadingsForPeriod(obj.Id, previousMonth.Year, previousMonth.Month)
                                .Where(r => r.MeterId == meter.Key)
                                .ToList();

                            decimal previousValue;
                            if (previousReadings.Any())
                            {
                                previousValue = previousReadings.OrderByDescending(r => r.ReadingDate).First().Value;
                                System.Diagnostics.Debug.WriteLine($"    Предыдущее показание: {previousReadings.OrderByDescending(r => r.ReadingDate).First().ReadingDate:dd.MM.yyyy} = {previousValue}");
                            }
                            else
                            {
                                // Если нет показаний за предыдущий месяц, берем начальные показания счетчика
                                MeterDto meterInfo = _meterRepository.GetById(meter.Key);
                                previousValue = meterInfo?.InitialReading ?? 0;
                                System.Diagnostics.Debug.WriteLine($"    Начальные показания счетчика: {previousValue}");
                            }

                            decimal consumption = last.Value - previousValue;
                            System.Diagnostics.Debug.WriteLine($"    Потребление за месяц: {consumption}");

                            totalConsumption += consumption;
                        }
                    }

                    // Применяем тариф (временно 5 руб/кВт)
                    decimal amount = totalConsumption * 5.0m;

                    System.Diagnostics.Debug.WriteLine($"ИТОГО по объекту: потребление={totalConsumption}, сумма={amount}");

                    result.Add(new AccrualCalculationDto
                    {
                        ConsumptionObjectId = obj.Id,
                        Address = obj.Address,
                        TotalConsumption = totalConsumption,
                        TotalAmount = amount,
                        ReadingsCount = readings.Count,
                        HasExistingAccrual = exists
                    });
                }

                System.Diagnostics.Debug.WriteLine($"=================================");
                System.Diagnostics.Debug.WriteLine($"ИТОГО рассчитано объектов: {result.Count}");
                System.Diagnostics.Debug.WriteLine($"=================================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА: {ex.Message}");
            }

            return result;
        }

        public decimal CalculateAmount(decimal consumption, int tariffId)
        {
            // TODO: брать тариф из БД
            return consumption * 5.0m;
        }
    }
}
