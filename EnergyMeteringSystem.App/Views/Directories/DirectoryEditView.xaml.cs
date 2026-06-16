using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Directories;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Directories
{
    public partial class DirectoryEditView : Window
    {
        private readonly DirectoryEditViewModel _viewModel;

        public DirectoryEditView(DirectoryEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            viewModel.OnSaved += (s, e) => Close();
            viewModel.OnCancelled += (s, e) => Close();

            this.Closing += (s, e) =>
            {
                if (_viewModel.HasChanges && DialogService.ConfirmCancel())
                {
                    e.Cancel = false;
                }
            };
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