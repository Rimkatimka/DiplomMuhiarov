using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        List<PaymentDto> GetByPeriod(int year, int month);
        List<PaymentDto> GetByObjectId(int objectId);
        PaymentDto GetById(int id);
        void Add(PaymentRegistrationDto dto);
        decimal GetTotalForPeriod(int year, int month);
        List<DebtDto> GetDebtors();  // ← исправили на DebtDto
    }
}
