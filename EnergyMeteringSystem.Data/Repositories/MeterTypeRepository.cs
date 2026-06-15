using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterTypeRepository : BaseRepository, IMeterTypeRepository
    {
        public async Task<List<MeterTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var query = await (from mt in Query<MeterType>()
                               join vi in Query<VerificationInterval>() on mt.Id equals vi.MeterTypeId into joined
                               from vi in joined.DefaultIfEmpty()
                               select new MeterTypeDto
                               {
                                   Id = mt.Id,
                                   Name = mt.Name,
                                   Voltage = mt.Voltage,
                                   MaxCurrent = mt.MaxCurrent,
                                   AccuracyClass = mt.AccuracyClass,
                                   DigitCount = mt.DigitCount,
                                   DecimalPlaces = mt.DecimalPlaces,
                                   ServiceLifeYears = mt.ServiceLifeYears,
                                   VerificationIntervalYears = vi != null ? vi.Years : (int?)null
                               })
                               .OrderBy(mt => mt.Name)
                               .ToListAsync(cancellationToken);

            return query;
        }

        public List<MeterTypeDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<MeterTypeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await (from mt in Query<MeterType>()
                                join vi in Query<VerificationInterval>() on mt.Id equals vi.MeterTypeId into joined
                                from vi in joined.DefaultIfEmpty()
                                where mt.Id == id
                                select new MeterTypeDto
                                {
                                    Id = mt.Id,
                                    Name = mt.Name,
                                    Voltage = mt.Voltage,
                                    MaxCurrent = mt.MaxCurrent,
                                    AccuracyClass = mt.AccuracyClass,
                                    DigitCount = mt.DigitCount,
                                    DecimalPlaces = mt.DecimalPlaces,
                                    ServiceLifeYears = mt.ServiceLifeYears,
                                    VerificationIntervalYears = vi != null ? vi.Years : (int?)null
                                })
                                .FirstOrDefaultAsync(cancellationToken);

            return result;
        }

        public MeterTypeDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }
    }
}