using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnergyMeteringSystem.App.Helpers
{
    public static class ValidationService
    {
        public static void HighlightRequiredFields(DependencyObject parent, Dictionary<FrameworkElement, bool> validations)
        {
            foreach (var field in validations)
            {
                if (field.Key == null) continue;

                bool isValid = field.Value;
                var brush = isValid ? Brushes.White : new SolidColorBrush(Color.FromRgb(255, 240, 240));

                if (field.Key is TextBox textBox)
                {
                    textBox.Background = brush;
                    textBox.ToolTip = isValid ? null : "Обязательное поле";
                }
                else if (field.Key is ComboBox comboBox)
                {
                    comboBox.Background = brush;
                    comboBox.ToolTip = isValid ? null : "Обязательное поле";
                }
                else if (field.Key is PasswordBox passwordBox)
                {
                    passwordBox.Background = brush;
                    passwordBox.ToolTip = isValid ? null : "Обязательное поле";
                }
            }
        }

        public static void UpdateButtonState(Button button, bool canExecute, string reason = null)
        {
            if (button == null) return;

            button.IsEnabled = canExecute;
            button.ToolTip = canExecute ? null : reason ?? "Заполните все обязательные поля";
        }

        public static string GetMissingFieldsMessage(Dictionary<string, bool> fieldStatuses)
        {
            var missing = fieldStatuses.Where(f => !f.Value).Select(f => f.Key);
            return missing.Any() ? $"Не заполнено: {string.Join(", ", missing)}" : null;
        }
    }
}