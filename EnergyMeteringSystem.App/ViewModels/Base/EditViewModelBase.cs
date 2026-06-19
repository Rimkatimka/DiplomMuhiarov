using EnergyMeteringSystem.App.Commands;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
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
            set => SetProperty(ref _title, value);
        }
        protected void RaiseOnSaved()
        {
            OnSaved?.Invoke(this, EventArgs.Empty);
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

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => !IsLoading);
            CancelCommand = new RelayCommand(_ => Cancel());

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IsLoading))
                    RaiseCanExecuteChanged();
            };

            if (IsEditMode)
                LoadItem(item);
        }

        protected abstract void LoadItem(TModel item);
        protected abstract TModel GetDto();
        protected abstract Task<bool> SaveToRepositoryAsync(TModel dto);
        protected new abstract bool CanSave();

        protected virtual string GetSaveValidationMessage() => "Заполните все обязательные поля корректно.";

        protected virtual async Task SaveAsync()
        {
            if (!CanSave())
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(GetSaveValidationMessage(), "Невозможно сохранить",
                        MessageBoxButton.OK, MessageBoxImage.Warning));
                return;
            }

            await ExecuteAsync(async () =>
            {
                var dto = GetDto();
                var saved = await SaveToRepositoryAsync(dto);
                if (!saved) return;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    string message = IsEditMode ? "Данные успешно обновлены!" : "Новая запись успешно создана!";
                    MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                });

                OnSaved?.Invoke(this, EventArgs.Empty);
            }, "Ошибка при сохранении");

            if (HasError)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        protected virtual void Cancel()
        {
            OnCancelled?.Invoke(this, EventArgs.Empty);
        }

        private bool _hasChanges;

        public bool HasChanges
        {
            get => _hasChanges;
            set => SetProperty(ref _hasChanges, value);
        }

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            var changed = base.SetProperty(ref storage, value, propertyName);
            if (changed)
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            return changed;
        }

        // ✅ МЕТОД ДЛЯ ПРИНУДИТЕЛЬНОГО ОБНОВЛЕНИЯ КНОПКИ
        public void RaiseCanExecuteChanged()
        {
            (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}