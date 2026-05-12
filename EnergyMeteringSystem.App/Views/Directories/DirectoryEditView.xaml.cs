using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Directories;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Directories
{
    /// <summary>
    /// Логика взаимодействия для DirectoryEditView.xaml
    /// </summary>
    public partial class DirectoryEditView : Window
    {
        public DirectoryEditView(DirectoryEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnDirectorySaved += (s, e) => Close();
        }
        private void TextBox_PreviewTextInput_General(object sender, TextCompositionEventArgs e)
        {
            string pattern = @"^[a-zA-Zа-яА-ЯёЁ0-9\s.-]+$";
            e.Handled = !Regex.IsMatch(e.Text, pattern);
        }

        private void TextBox_PreviewKeyDown_BlockSpace(object sender, KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }
    }
}
