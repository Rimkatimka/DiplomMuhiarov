using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public CalculationService()
        {
            _objectRepository = new ConsumptionObjectRepository();
            _readingRepository = new MeterReadingRepository();
            _accrualRepository = new AccrualRepository();
        }

        public List<AccrualCalculationDto> CalculateForPeriod(int year, int month)
        {
            var result = new List<AccrualCalculationDto>();
            var objects = _objectRepository.GetAll();

            foreach (var obj in objects)
            {
                // Проверяем, есть ли уже начисление за этот период
                bool exists = _accrualRepository.Exists(obj.Id, year, month);

                // Получаем показания за период
                var readings = _readingRepository.GetReadingsForPeriod(obj.Id, year, month);

                if (!readings.Any())
                    continue;

                decimal totalConsumption = 0;

                // Группируем по счётчикам и суммируем потребление
                var meterReadings = readings.GroupBy(r => r.MeterId);
                foreach (var meter in meterReadings)
                {
                    var ordered = meter.OrderBy(r => r.ReadingDate).ToList();
                    if (ordered.Count >= 2)
                    {
                        // Берем первое и последнее показание за месяц
                        var first = ordered.First();
                        var last = ordered.Last();
                        totalConsumption += last.Value - first.Value;
                    }
                }

                // TODO: применить тариф
                decimal amount = totalConsumption * 5.0m; // временно 5 руб/кВт

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

            return result;
        }

        public decimal CalculateAmount(decimal consumption, int tariffId)
        {
            // TODO: брать тариф из БД
            return consumption * 5.0m;
        }
    }
}
