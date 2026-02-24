using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IMeterReadingRepository
    {
        void Add(MeterReadingInputDto reading);
        List<MeterReadingVerificationDto> GetForVerification();
        void UpdateStatus(int readingId, int newStatusId, int? rejectionReasonId = null, string comment = null);
        List<MeterForReadingDto> GetMetersByObjectId(int objectId);
        decimal? GetLastReading(int meterId);
        List<MeterReadingHistoryDto> GetHistoryByMeterId(int meterId);
        List<MeterReadingHistoryDto> GetHistoryByObjectId(int objectId);
    }
}
