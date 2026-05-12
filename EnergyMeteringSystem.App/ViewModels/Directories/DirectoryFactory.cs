using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public static class DirectoryFactory
    {    

        public static DirectoryListViewModel CreateReadingStatusViewModel()
        {
            ReadingStatusRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreatePaymentMethodViewModel()
        {
            PaymentMethodRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateObjectTypeViewModel()
        {
            ObjectTypeRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateRejectionReasonViewModel()
        {
            RejectionReasonRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateMeterStatusViewModel()
        {
            MeterStatusRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateContractStatusViewModel()
        {
            ContractStatusRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateTariffTypeViewModel()
        {
            TariffTypeRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateUnitOfMeasureViewModel()
        {
            UnitOfMeasureRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateEnergySourceViewModel()
        {
            EnergySourceRepository repo = new();
            return new DirectoryListViewModel(repo);
        }

        public static DirectoryListViewModel CreateVerificationIntervalViewModel()
        {
            VerificationIntervalRepository repo = new();
            return new DirectoryListViewModel(repo);
        }
    }
}
