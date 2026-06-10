using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterTypeRepository : BaseRepository, IMeterTypeRepository
    {
        public List<MeterTypeDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<MeterTypeDto>> GetAllAsync()
        {
            var meterTypes = await Query<MeterType>().ToListAsync();

            var intervals = await Query<VerificationInterval>()
                .ToDictionaryAsync(vi => vi.MeterTypeId, vi => vi.Years);

            var result = new List<MeterTypeDto>();
            foreach (var mt in meterTypes)
            {
                result.Add(new MeterTypeDto
                {
                    Id = mt.Id,
                    Name = mt.Name,
                    Voltage = mt.Voltage,
                    MaxCurrent = mt.MaxCurrent,
                    AccuracyClass = mt.AccuracyClass,
                    DigitCount = mt.DigitCount,
                    DecimalPlaces = mt.DecimalPlaces,
                    ServiceLifeYears = mt.ServiceLifeYears,
                    VerificationIntervalYears = intervals.ContainsKey(mt.Id) ? intervals[mt.Id] : (int?)null
                });
            }

            return result;
        }

        public MeterTypeDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<MeterTypeDto> GetByIdAsync(int id)
        {
            var mt = await Query<MeterType>().FirstOrDefaultAsync(m => m.Id == id);
            if (mt == null) return null;

            var interval = await Query<VerificationInterval>()
                .FirstOrDefaultAsync(vi => vi.MeterTypeId == id);

            return new MeterTypeDto
            {
                Id = mt.Id,
                Name = mt.Name,
                Voltage = mt.Voltage,
                MaxCurrent = mt.MaxCurrent,
                AccuracyClass = mt.AccuracyClass,
                DigitCount = mt.DigitCount,
                DecimalPlaces = mt.DecimalPlaces,
                ServiceLifeYears = mt.ServiceLifeYears,
                VerificationIntervalYears = interval?.Years
            };
        }
    }
}