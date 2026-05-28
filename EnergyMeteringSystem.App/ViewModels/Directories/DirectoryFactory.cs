using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public static class DirectoryFactory
    {
        public static DirectoryListViewModel CreateReadingStatusViewModel()
        {
            var repo = new ReadingStatusRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreatePaymentMethodViewModel()
        {
            var repo = new PaymentMethodRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateObjectTypeViewModel()
        {
            var repo = new ObjectTypeRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateRejectionReasonViewModel()
        {
            var repo = new RejectionReasonRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateMeterStatusViewModel()
        {
            var repo = new MeterStatusRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateContractStatusViewModel()
        {
            var repo = new ContractStatusRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateTariffTypeViewModel()
        {
            var repo = new TariffTypeRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateUnitOfMeasureViewModel()
        {
            var repo = new UnitOfMeasureRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateEnergySourceViewModel()
        {
            var repo = new EnergySourceRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateVerificationIntervalViewModel()
        {
            var repo = new VerificationIntervalRepository();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateMeterTypeViewModel()
        {
            var repo = new MeterTypeDirectoryRepository();
            return new DirectoryListViewModel(repo);
        }
    }
}