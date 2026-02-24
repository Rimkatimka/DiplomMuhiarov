using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public static class DirectoryFactory
    {
        public static DirectoryListViewModel CreateMeterTypeViewModel()
        {
            return new DirectoryListViewModel(new MeterTypeRepository());
        }

        /// <summary>
        /// Статусы показаний
        /// </summary>
        public static DirectoryListViewModel CreateReadingStatusViewModel()
        {
            return new DirectoryListViewModel(new ReadingStatusRepository());
        }

        /// <summary>
        /// Способы оплаты
        /// </summary>
        public static DirectoryListViewModel CreatePaymentMethodViewModel()
        {
            return new DirectoryListViewModel(new PaymentMethodRepository());
        }

        /// <summary>
        /// Типы объектов
        /// </summary>
        public static DirectoryListViewModel CreateObjectTypeViewModel()
        {
            return new DirectoryListViewModel(new ObjectTypeRepository());
        }

        /// <summary>
        /// Причины отклонения
        /// </summary>
        public static DirectoryListViewModel CreateRejectionReasonViewModel()
        {
            return new DirectoryListViewModel(new RejectionReasonRepository());
        }

        /// <summary>
        /// Статусы счетчиков
        /// </summary>
        public static DirectoryListViewModel CreateMeterStatusViewModel()
        {
            return new DirectoryListViewModel(new MeterStatusRepository());
        }

        /// <summary>
        /// Статусы договоров
        /// </summary>
        public static DirectoryListViewModel CreateContractStatusViewModel()
        {
            return new DirectoryListViewModel(new ContractStatusRepository());
        }

        /// <summary>
        /// Типы тарифов
        /// </summary>
        public static DirectoryListViewModel CreateTariffTypeViewModel()
        {
            return new DirectoryListViewModel(new TariffTypeRepository());
        }

        /// <summary>
        /// Единицы измерения
        /// </summary>
        public static DirectoryListViewModel CreateUnitOfMeasureViewModel()
        {
            return new DirectoryListViewModel(new UnitOfMeasureRepository());
        }

        /// <summary>
        /// Источники энергии
        /// </summary>
        public static DirectoryListViewModel CreateEnergySourceViewModel()
        {
            return new DirectoryListViewModel(new EnergySourceRepository());
        }

        /// <summary>
        /// Интервалы поверки
        /// </summary>
        public static DirectoryListViewModel CreateVerificationIntervalViewModel()
        {
            return new DirectoryListViewModel(new VerificationIntervalRepository());
        }
    }
}
