using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.App.ViewModels.Base
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _eventArgsCache = new();
        private bool _isBatching;
        private readonly HashSet<string> _pendingProperties = new();
        private readonly object _batchLock = new();
        private bool _isLoading;
        private string _errorMessage;
        private bool _disposed;

        public event PropertyChangedEventHandler PropertyChanged;

        // Индикатор загрузки
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Сообщение об ошибке
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_isBatching)
            {
                lock (_batchLock)
                {
                    if (propertyName != null)
                        _pendingProperties.Add(propertyName);
                }
                return;
            }

            if (propertyName != null)
            {
                var args = _eventArgsCache.GetOrAdd(propertyName, p => new PropertyChangedEventArgs(p));
                PropertyChanged?.Invoke(this, args);
            }
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void BeginBatchUpdate()
        {
            lock (_batchLock)
            {
                _isBatching = true;
                _pendingProperties.Clear();
            }
        }

        protected void EndBatchUpdate()
        {
            lock (_batchLock)
            {
                _isBatching = false;
                foreach (var prop in _pendingProperties)
                    OnPropertyChanged(prop);
                _pendingProperties.Clear();
            }
        }

        protected virtual void RaiseAllPropertiesChanged()
        {
            OnPropertyChanged(string.Empty);
        }

        // ✅ НОВЫЙ МЕТОД: безопасное выполнение асинхронных операций
        protected async Task ExecuteAsync(Func<Task> action, string errorPrefix = "Ошибка")
        {
            if (IsLoading) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                await action();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{errorPrefix}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[ViewModelBase] {errorPrefix}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ✅ НОВЫЙ МЕТОД: безопасное выполнение с возвратом результата
        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string errorPrefix = "Ошибка", T defaultValue = default)
        {
            if (IsLoading) return defaultValue;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{errorPrefix}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[ViewModelBase] {errorPrefix}: {ex.Message}");
                return defaultValue;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ✅ IDisposable реализация
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _pendingProperties.Clear();
            }
            _disposed = true;
        }
    }
}