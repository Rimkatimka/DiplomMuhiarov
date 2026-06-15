using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EnergyMeteringSystem.App.ViewModels.Base
{
    public abstract class ValidatableViewModel : INotifyPropertyChanged
    {
        // Правильный тип - Dictionary<string, List<string>>
        protected Dictionary<string, List<string>> _fieldValidations = new();

        // Добавляет ошибку для поля
        protected void AddError(string propertyName, string error)
        {
            if (!_fieldValidations.ContainsKey(propertyName))
                _fieldValidations[propertyName] = new List<string>();

            if (!_fieldValidations[propertyName].Contains(error))
                _fieldValidations[propertyName].Add(error);

            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(HasErrors));
        }

        // Удаляет ошибку для поля
        protected void RemoveError(string propertyName, string error)
        {
            if (_fieldValidations.ContainsKey(propertyName))
            {
                _fieldValidations[propertyName].Remove(error);

                if (_fieldValidations[propertyName].Count == 0)
                    _fieldValidations.Remove(propertyName);
            }

            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(HasErrors));
        }

        // Очищает все ошибки для поля
        protected void ClearErrors(string propertyName)
        {
            if (_fieldValidations.ContainsKey(propertyName))
            {
                _fieldValidations.Remove(propertyName);
                OnPropertyChanged(propertyName);
                OnPropertyChanged(nameof(HasErrors));
            }
        }

        // Очищает все ошибки для всех полей
        protected void ClearAllErrors()
        {
            _fieldValidations.Clear();
            OnPropertyChanged(nameof(HasErrors));
        }

        // Проверяет, есть ли ошибки
        public bool HasErrors => _fieldValidations.Count > 0;

        // Получает все ошибки для поля (через индексатор)
        public string this[string propertyName]
        {
            get
            {
                if (_fieldValidations.ContainsKey(propertyName))
                    return string.Join(Environment.NewLine, _fieldValidations[propertyName]);
                return null;
            }
        }

        // Получает список ошибок для поля
        public List<string> GetErrors(string propertyName)
        {
            if (_fieldValidations.ContainsKey(propertyName))
                return _fieldValidations[propertyName];
            return new List<string>();
        }

        // Проверяет, есть ли ошибка у поля
        public bool HasError(string propertyName)
        {
            return _fieldValidations.ContainsKey(propertyName) && _fieldValidations[propertyName].Count > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Вспомогательный метод для установки значения с валидацией
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Вспомогательный метод с валидацией
        protected bool SetPropertyWithValidation<T>(ref T field, T value, Func<T, bool> validate, string errorMessage, [CallerMemberName] string propertyName = null)
        {
            // Сначала удаляем старые ошибки для этого поля
            ClearErrors(propertyName);

            // Валидация
            if (validate != null && !validate(value))
            {
                AddError(propertyName, errorMessage);
                return false;
            }

            // Если валидация пройдена, обновляем значение
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}