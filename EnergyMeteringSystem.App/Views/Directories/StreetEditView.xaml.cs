using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Directories;
using EnergyMeteringSystem.Services;

namespace EnergyMeteringSystem.App.Views.Directories
{
    public partial class StreetEditView : Window
    {
        public StreetEditView()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput_StreetName(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем русские буквы, латиницу, пробелы, точки и дефисы
            string pattern = @"^[a-zA-Zа-яА-ЯёЁ\s.-]+$";
            if (!Regex.IsMatch(e.Text, pattern))
            {
                ToastNotificationService.ShowNear(sender as UIElement,
                    "Допустимы: буквы, пробел, точка, дефис", 1500);
                e.Handled = true;
            }
        }
    }
}