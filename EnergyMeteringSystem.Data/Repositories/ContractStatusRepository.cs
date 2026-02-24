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
    public class ContractStatusRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ContractStatusRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            return _context.ContractStatus
                .Select(s => new DirectoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.AllowsBilling ? "Разрешены начисления" : "Начисления запрещены",
                    IsActive = true
                })
                .ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var entity = _context.ContractStatus.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.AllowsBilling ? "Разрешены начисления" : "Начисления запрещены",
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new ContractStatus
            {
                Name = dto.Name,
                AllowsBilling = true
            };
            _context.ContractStatus.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.ContractStatus.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.ContractStatus.Find(id);
            if (entity != null)
            {
                _context.ContractStatus.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
