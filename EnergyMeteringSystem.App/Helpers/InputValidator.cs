using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EnergyMeteringSystem.Services;

namespace EnergyMeteringSystem.App.Helpers
{
    public static class InputValidator
    {
        /// <summary>
        /// Разрешает только латиницу, цифры, точку, подчёркивание, дефис. Без пробелов.
        /// </summary>
        public static void RestrictLoginInput(object sender, TextCompositionEventArgs e)
        {
            string pattern = @"^[a-zA-Z0-9._-]+$";
            if (!Regex.IsMatch(e.Text, pattern))
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Только латиница, цифры, . _ -", 1500);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает цифры, русские буквы, слеш, дефис (для номера дома)
        /// </summary>
        public static void RestrictHouseNumber(object sender, TextCompositionEventArgs e)
        {
            string pattern = @"^[0-9а-яА-ЯёЁ/-]+$";
            if (!Regex.IsMatch(e.Text, pattern))
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Только цифры, русские буквы, / и -", 1500);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Запрещает пробел.
        /// </summary>
        public static void BlockSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Пробел запрещён", 1500);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает только цифры (целые числа).
        /// </summary>
        public static void RestrictNumbersOnly(object sender, TextCompositionEventArgs e)
        {
            string pattern = @"^[0-9]+$";
            if (!Regex.IsMatch(e.Text, pattern))
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Только цифры", 1500);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает только цифры, одну точку или одну запятую (для дробных чисел).
        /// </summary>
        public static void RestrictDecimalNumbers(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string currentText = textBox.Text;
            string newText = currentText.Insert(textBox.CaretIndex, e.Text);

            if (!Regex.IsMatch(e.Text, @"^[0-9.,]$"))
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Только цифры, . или ,", 1500);
                e.Handled = true;
                return;
            }

            int dotCount = newText.Count(c => c == '.');
            int commaCount = newText.Count(c => c == ',');

            if (dotCount > 1 || commaCount > 1 || (dotCount > 0 && commaCount > 0))
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Только одна точка или одна запятая", 1500);
                e.Handled = true;
                return;
            }

            if ((e.Text == "." || e.Text == ",") && currentText.Length == 0)
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Число не может начинаться с точки или запятой", 1500);
                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        /// <summary>
        /// Разрешает только латиницу и цифры.
        /// </summary>
        public static void RestrictAlphaNumeric(object sender, TextCompositionEventArgs e)
        {
            string pattern = @"^[a-zA-Z0-9]+$";
            if (!Regex.IsMatch(e.Text, pattern))
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Только латиница и цифры", 1500);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает только кириллицу, пробелы и дефис (для ФИО).
        /// </summary>
        public static void RestrictCyrillic(object sender, TextCompositionEventArgs e)
        {
            string pattern = @"^[а-яА-ЯёЁ\s-]+$";
            if (!Regex.IsMatch(e.Text, pattern))
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Только русские буквы, пробел и дефис", 1500);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Запрещает пробел и ограничивает длину.
        /// </summary>
        public static void BlockSpaceAndLimitLength(object sender, KeyEventArgs e, int maxLength)
        {
            if (e.Key == Key.Space)
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Пробел запрещён", 1500);
                e.Handled = true;
                return;
            }

            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text.Length >= maxLength && e.Key != Key.Back && e.Key != Key.Delete)
            {
                ToastNotificationService.ShowNear(sender as UIElement, $"Максимум {maxLength} символов", 1500);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает только латинские буквы, цифры и спецсимволы для email (@ . _ % -)
        /// </summary>
        public static void RestrictEmailCharacters(object sender, TextCompositionEventArgs e)
        {
            string pattern = @"^[a-zA-Z0-9@._%-]+$";
            if (!Regex.IsMatch(e.Text, pattern))
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Недопустимый символ в email", 1500);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Проверяет формат email при потере фокуса
        /// </summary>
        public static void ValidateEmailOnLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text)) return;

            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            bool isValid = Regex.IsMatch(textBox.Text, pattern);

            if (!isValid)
            {
                ToastNotificationService.ShowNear(sender as UIElement, "Неверный формат email (пример: user@mail.ru)", 2000);
                textBox.BorderBrush = Brushes.Red;
                textBox.BorderThickness = new Thickness(2);
                textBox.ToolTip = "Неверный формат email";
            }
            else
            {
                textBox.BorderBrush = Brushes.Green;
                textBox.BorderThickness = new Thickness(1);
                textBox.ToolTip = "Email корректный";
            }
        }

        /// <summary>
        /// Сбрасывает подсветку при получении фокуса
        /// </summary>
        public static void ResetEmailBorderOnFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            textBox.BorderBrush = Brushes.LightGray;
            textBox.BorderThickness = new Thickness(1);
            textBox.ToolTip = "Формат: имя@домен.ру";
        }
    }
}