using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterRepository : IMeterRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<MeterDto> GetByObjectId(int objectId)
        {
            // Загружаем Meter с навигационными свойствами через строки
            var query = from m in _context.Meter
                        where m.ConsumptionObjectId == objectId
                        select new MeterDto
                        {
                            Id = m.Id,
                            SerialNumber = m.SerialNumber,
                            MeterTypeName = m.MeterType.Name,        // EF сам подгрузит
                            StatusName = m.MeterStatus.Name,          // если связь есть
                            InstallationDate = m.InstallationDate,
                            VerificationDate = m.VerificationDate,
                            NextVerificationDate = m.NextVerificationDate,
                            InitialReading = m.InitialReading
                        };

            return query.ToList();
        }
    }
}
