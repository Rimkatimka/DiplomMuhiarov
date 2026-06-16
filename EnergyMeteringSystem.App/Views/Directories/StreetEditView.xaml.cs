using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Directories
{
    public partial class StreetEditView : Window
    {
        private readonly ViewModels.Directories.StreetEditViewModel _viewModel;

        public StreetEditView()
        {
            InitializeComponent();
            _viewModel = DataContext as ViewModels.Directories.StreetEditViewModel;

            if (_viewModel != null)
            {
                _viewModel.OnStreetSaved += (s, e) => Close();

                this.Closing += (s, e) =>
                {
                    if (DialogService.ConfirmCancel())
                    {
                        e.Cancel = false;
                    }
                };
            }
        }

        private void TextBox_PreviewTextInput_StreetName(object sender, TextCompositionEventArgs e)
        {
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