using System;
using System.Linq;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Helpers
{
    public static class IdHelper
    {
        public static int GetNextId<T>(EnergyMeteringSystemEntities context, Func<T, int> getIdSelector) where T : class
        {
            System.Data.Entity.DbSet<T> dbSet = context.Set<T>();
            return !dbSet.Any() ? 1 : dbSet.Max(getIdSelector) + 1;
        }

        // Для каждой таблицы можно создать отдельный метод
        public static int GetNextUserId(EnergyMeteringSystemEntities context)
        {
            return !context.User.Any() ? 1 : context.User.Max(u => u.Id) + 1;
        }

        public static int GetNextMeterId(EnergyMeteringSystemEntities context)
        {
            return !context.Meter.Any() ? 1 : context.Meter.Max(m => m.Id) + 1;
        }

        public static int GetNextReadingId(EnergyMeteringSystemEntities context)
        {
            return !context.MeterReading.Any() ? 1 : context.MeterReading.Max(r => r.Id) + 1;
        }

        public static int GetNextAccrualId(EnergyMeteringSystemEntities context)
        {
            return !context.Accrual.Any() ? 1 : context.Accrual.Max(a => a.Id) + 1;
        }

        public static int GetNextPaymentId(EnergyMeteringSystemEntities context)
        {
            return !context.Payment.Any() ? 1 : context.Payment.Max(p => p.Id) + 1;
        }

        public static int GetNextContractId(EnergyMeteringSystemEntities context)
        {
            return !context.Contract.Any() ? 1 : context.Contract.Max(c => c.Id) + 1;
        }

        public static int GetNextTariffId(EnergyMeteringSystemEntities context)
        {
            return !context.Tariff.Any() ? 1 : context.Tariff.Max(t => t.Id) + 1;
        }
    }
}