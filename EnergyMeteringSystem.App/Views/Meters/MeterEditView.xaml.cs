using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Meters;
using System.Windows;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Meters
{
    public partial class MeterEditView : Window
    {
        private readonly MeterEditViewModel _viewModel;

        public MeterEditView(MeterEditViewModel viewModel)
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

        private void TextBox_PreviewTextInput_Alphanumeric(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictAlphaNumeric(sender, e);
        }

        private void TextBox_PreviewTextInput_Decimal(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictDecimalNumbers(sender, e);
        }

        private void TextBox_PreviewTextInput_NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictNumbersOnly(sender, e);
        }

        private void TextBox_PreviewKeyDown_BlockSpace(object sender, KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }
    }
}