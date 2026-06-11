using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public static class DirectoryFactory
    {
        public static DirectoryListViewModel CreateReadingStatusViewModel()
        {
            var repo = new ReadingStatusRepository();
            return new DirectoryListViewModel(repo, "Статусы показаний");
        }

        public static DirectoryListViewModel CreateObjectTypeViewModel()
        {
            var repo = new ObjectTypeRepository();
            return new DirectoryListViewModel(repo, "Типы объектов");
        }

        public static DirectoryListViewModel CreateRejectionReasonViewModel()
        {
            var repo = new RejectionReasonRepository();
            return new DirectoryListViewModel(repo, "Причины отклонения показаний");
        }

        public static DirectoryListViewModel CreateMeterStatusViewModel()
        {
            var repo = new MeterStatusRepository();
            return new DirectoryListViewModel(repo, "Статусы счётчиков");
        }

        public static DirectoryListViewModel CreateEnergySourceViewModel()
        {
            var repo = new EnergySourceRepository();
            return new DirectoryListViewModel(repo, "Источники энергии");
        }

        public static DirectoryListViewModel CreateVerificationIntervalViewModel()
        {
            var repo = new VerificationIntervalRepository();
            return new DirectoryListViewModel(repo, "Интервалы поверки");
        }

        public static DirectoryListViewModel CreateMeterTypeViewModel()
        {
            var repo = new MeterTypeDirectoryRepository();
            return new DirectoryListViewModel(repo, "Типы счётчиков");
        }
    }
}