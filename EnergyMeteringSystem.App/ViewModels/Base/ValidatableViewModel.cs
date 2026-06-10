using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.Helpers;

namespace EnergyMeteringSystem.App.ViewModels.Base
{
    public abstract class ValidatableViewModel : ViewModelBase
    {
        private Dictionary<string, bool> _fieldValidations = new();
        private Button _saveButton;
        private string _validationMessage;

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public bool CanSave => _fieldValidations.Values.All(v => v);

        public RelayCommand ValidateFieldsCommand { get; set; }

        protected ValidatableViewModel()
        {
            ValidateFieldsCommand = new RelayCommand(_ => ValidateAllFields());
        }

        public void RegisterField(string fieldName, FrameworkElement control, bool isValid)
        {
            _fieldValidations[fieldName] = isValid;
        }

        public void UpdateField(string fieldName, bool isValid)
        {
            if (_fieldValidations.ContainsKey(fieldName))
            {
                _fieldValidations[fieldName] = isValid;
                OnPropertyChanged(nameof(CanSave));

                var missingMessage = ValidationService.GetMissingFieldsMessage(_fieldValidations);
                ValidationMessage = missingMessage;

                if (_saveButton != null)
                {
                    ValidationService.UpdateButtonState(_saveButton, CanSave, missingMessage);
                }
            }
        }

        public void BindSaveButton(Button button)
        {
            _saveButton = button;
            ValidationService.UpdateButtonState(_saveButton, CanSave, ValidationMessage);
        }

        public virtual void ValidateAllFields()
        {
            // Переопределить в наследнике
            OnPropertyChanged(nameof(CanSave));
        }

        protected void HighlightFields(DependencyObject parent)
        {
            var validations = _fieldValidations.ToDictionary(
                kvp => FindControlByName(parent, kvp.Key),
                kvp => kvp.Value
            );
            ValidationService.HighlightRequiredFields(parent, validations);
        }

        private FrameworkElement FindControlByName(DependencyObject parent, string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement element && element.Name == name)
                    return element;

                var result = FindControlByName(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}