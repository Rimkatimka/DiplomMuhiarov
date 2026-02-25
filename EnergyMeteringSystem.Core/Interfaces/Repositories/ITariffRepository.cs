using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface ITariffRepository
    {
        List<TariffDto> GetAll();
        List<TariffDto> GetActive();
        TariffDto GetById(int id);
        TariffDto GetCurrentByType(int tariffTypeId);
        void Add(TariffDto tariff);
        void Update(TariffDto tariff);
        void Deactivate(int id, DateTime endDate);
    }
}
