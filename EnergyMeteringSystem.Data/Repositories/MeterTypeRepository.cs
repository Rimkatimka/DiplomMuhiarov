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
        private const string CACHE_KEY_ALL = "MeterTypes_All";
        private const string CACHE_KEY_BY_ID = "MeterType_{0}";
        private const int CACHE_MINUTES = 60;

        public List<MeterTypeDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<MeterTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                // Один запрос с LEFT JOIN
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
            }, CACHE_MINUTES);
        }

        public MeterTypeDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<MeterTypeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
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
            }, CACHE_MINUTES);
        }
    }
}