using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class DirectoryEditViewModel : EditViewModelBase<DirectoryDto, object>
    {
        private string _name;
        private string _description;

        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DirectoryEditViewModel() : base(null, null)
        {
            Title = "Добавление записи";
            Name = string.Empty;
            Description = string.Empty;
        }

        public DirectoryEditViewModel(DirectoryDto existingItem)
            : base(null, existingItem)
        {
            Title = "Редактирование записи";
        }

        protected override void LoadItem(DirectoryDto item)
        {
            Name = item.Name;
            Description = item.Description;
        }

        protected override DirectoryDto GetDto()
        {
            return new DirectoryDto
            {
                Id = _originalItem?.Id ?? 0,
                Name = Name,
                Description = Description,
                IsActive = true
            };
        }

        protected override Task<bool> SaveToRepositoryAsync(DirectoryDto dto)
        {
            return Task.FromResult(true);
        }

        protected override bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        protected override async Task SaveAsync()
        {
            if (!CanSave())
                return;

            RaiseOnSaved();
        }
    }
}