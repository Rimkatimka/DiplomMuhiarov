using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnergyMeteringSystem.App.ViewModels.Admin;

namespace EnergyMeteringSystem.App.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для UserEditView.xaml
    /// </summary>
    public partial class UserEditView : UserControl
    {
        public UserEditView(UserEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;  // ← устанавливаем DataContext
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только латинские буквы, цифры и спецсимволы для email
            string allowedChars = @"^[a-zA-Z0-9@._%-]+$";

            if (!Regex.IsMatch(e.Text, allowedChars))
            {
                e.Handled = true; // Запрещаем ввод
            }
        }

        /// <summary>
        /// Запрещаем ввод пробела
        /// </summary>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Запрещаем пробел
            }
        }

        /// <summary>
        /// При потере фокуса проверяем формат email
        /// </summary>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Простая проверка наличия @ и точки
                if (!textBox.Text.Contains("@") || !textBox.Text.Contains("."))
                {
                    textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                    textBox.BorderThickness = new Thickness(2);
                    textBox.ToolTip = "Email должен содержать @ и . (пример: user@mail.ru)";
                }
                else
                {
                    textBox.BorderBrush = System.Windows.Media.Brushes.Green;
                    textBox.BorderThickness = new Thickness(1);
                    textBox.ToolTip = "Формат корректный";
                }
            }
        }

        /// <summary>
        /// При получении фокуса сбрасываем подсветку
        /// </summary>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.BorderBrush = System.Windows.Media.Brushes.LightGray;
                textBox.BorderThickness = new Thickness(1);
            }
        }
    }
}

