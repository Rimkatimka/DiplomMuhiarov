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
    public class RejectionReasonRepository : IRejectionReasonRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public RejectionReasonRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<RejectionReasonDto> GetAll()
        {
            return _context.RejectionReason
                .Select(r => new RejectionReasonDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    RequiresComment = r.RequiresComment
                })
                .ToList();
        }
    }
}
