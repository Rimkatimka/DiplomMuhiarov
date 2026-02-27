using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class ContractStatusRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ContractStatusRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.ContractStatus
                .Select(s => new { s.Id, s.Name, s.AllowsBilling })
                .ToList();

            return data.Select(s => new DirectoryDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.AllowsBilling ? "Разрешены начисления" : "Начисления запрещены",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            ContractStatus entity = _context.ContractStatus.Find(id);
            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.AllowsBilling ? "Разрешены начисления" : "Начисления запрещены",
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            ContractStatus entity = new()
            {
                Name = dto.Name,
                AllowsBilling = true
            };
            _ = _context.ContractStatus.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            ContractStatus entity = _context.ContractStatus.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            ContractStatus entity = _context.ContractStatus.Find(id);
            if (entity != null)
            {
                _ = _context.ContractStatus.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
