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
    public class ContractRepository : IContractRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ContractRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<ContractDto> GetAll()
        {
            return _context.Contract
                .Include("ConsumptionObject")
                .Include("ContractStatus")
                .Include("Tariff")
                .Select(c => new ContractDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    ConsumptionObjectId = c.ConsumptionObjectId,
                    Address = c.ConsumptionObject.Street.Name + ", д. " + c.ConsumptionObject.HouseNumber +
                             (c.ConsumptionObject.ApartmentNumber != null ? ", кв. " + c.ConsumptionObject.ApartmentNumber : ""),
                    ContractDate = c.ContractDate,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    ContractStatusId = c.ContractStatusId,
                    StatusName = c.ContractStatus.Name,
                    TariffId = c.TariffId,
                    TariffName = c.Tariff.TariffType.Name + " (" + c.Tariff.PricePerUnit + " руб/кВт)"
                })
                .ToList();
        }

        public List<ContractDto> GetByObjectId(int objectId)
        {
            return _context.Contract
                .Where(c => c.ConsumptionObjectId == objectId)
                .Select(c => new ContractDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    ContractDate = c.ContractDate,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    StatusName = c.ContractStatus.Name,
                    TariffName = c.Tariff.TariffType.Name
                })
                .ToList();
        }

        public ContractDto GetById(int id)
        {
            var c = _context.Contract
                .Include("ConsumptionObject")
                .Include("ContractStatus")
                .Include("Tariff")
                .FirstOrDefault(x => x.Id == id);

            if (c == null) return null;

            return new ContractDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                ConsumptionObjectId = c.ConsumptionObjectId,
                Address = c.ConsumptionObject.Street.Name + ", д. " + c.ConsumptionObject.HouseNumber,
                ContractDate = c.ContractDate,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ContractStatusId = c.ContractStatusId,
                StatusName = c.ContractStatus.Name,
                TariffId = c.TariffId,
                TariffName = c.Tariff.TariffType.Name
            };
        }

        public void Add(ContractDto dto)
        {
            var entity = new Contract
            {
                ContractNumber = dto.ContractNumber,
                ConsumptionObjectId = dto.ConsumptionObjectId,
                ContractDate = dto.ContractDate,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ContractStatusId = dto.ContractStatusId,
                TariffId = dto.TariffId
            };

            _context.Contract.Add(entity);
            _context.SaveChanges();
        }

        public void Update(ContractDto dto)
        {
            var entity = _context.Contract.Find(dto.Id);
            if (entity != null)
            {
                entity.ContractNumber = dto.ContractNumber;
                entity.StartDate = dto.StartDate;
                entity.EndDate = dto.EndDate;
                entity.ContractStatusId = dto.ContractStatusId;
                entity.TariffId = dto.TariffId;
                _context.SaveChanges();
            }
        }

        public void Terminate(int id, DateTime endDate)
        {
            var entity = _context.Contract.Find(id);
            if (entity != null)
            {
                entity.EndDate = endDate;
                entity.ContractStatusId = 2; // Расторгнут
                _context.SaveChanges();
            }
        }
    }
}
