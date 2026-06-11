using System;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;

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

        // Конструктор для добавления
        public DirectoryEditViewModel() : base(null, null)
        {
            Title = "Добавление записи";
            Name = string.Empty;
            Description = string.Empty;
        }

        // Конструктор для редактирования
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

        protected override Task SaveToRepositoryAsync(DirectoryDto dto)
        {
            // Сохранение обрабатывается в DirectoryListViewModel через событие
            return Task.CompletedTask;
        }

        protected override bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        // Переопределяем SaveAsync, чтобы использовать событие OnDirectorySaved
        protected override async Task SaveAsync()
        {
            if (!CanSave()) return;

            // Вызываем событие OnDirectorySaved (собственное событие)
            OnDirectorySaved?.Invoke(this, EventArgs.Empty);
        }

        // Собственное событие
        public event EventHandler OnDirectorySaved;
    }
}