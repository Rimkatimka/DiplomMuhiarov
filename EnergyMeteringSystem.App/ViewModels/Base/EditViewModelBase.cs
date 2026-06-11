using EnergyMeteringSystem.App.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.ViewModels.Base
{
    /// <summary>
    /// Базовый класс для ViewModel редактирования
    /// </summary>
    /// <typeparam name="TModel">Тип модели (DTO)</typeparam>
    /// <typeparam name="TRepository">Тип репозитория</typeparam>
    public abstract class EditViewModelBase<TModel, TRepository> : ValidatableViewModel
        where TModel : class, new()
        where TRepository : class
    {
        protected readonly TRepository _repository;
        protected TModel _originalItem;
        protected bool _isEditMode;
        private string _title;

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);  // ✅ Добавлен setter
        }

        public ICommand SaveCommand { get; protected set; }
        public ICommand CancelCommand { get; protected set; }

        public event EventHandler OnSaved;
        public event EventHandler OnCancelled;

        protected EditViewModelBase(TRepository repository, TModel item = null)
        {
            _repository = repository;
            _originalItem = item;
            IsEditMode = item != null;
            Title = IsEditMode ? "Редактирование" : "Добавление";

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            if (IsEditMode)
                LoadItem(item);
        }

        protected abstract void LoadItem(TModel item);
        protected abstract TModel GetDto();
        protected abstract Task SaveToRepositoryAsync(TModel dto);
        protected abstract bool CanSave();

        protected virtual async Task SaveAsync()
        {
            if (!CanSave()) return;

            await ExecuteAsync(async () =>
            {
                var dto = GetDto();
                await SaveToRepositoryAsync(dto);
                OnSaved?.Invoke(this, EventArgs.Empty);
            }, "Ошибка при сохранении");
        }

        protected virtual void Cancel()
        {
            OnCancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}