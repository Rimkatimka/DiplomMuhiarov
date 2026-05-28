using System.Windows;
using System.Windows.Input;
using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Meters;

namespace EnergyMeteringSystem.App.Views.Meters
{
    public partial class MeterEditView : Window
    {
        public MeterEditView(MeterEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnMeterSaved += (s, e) => Close();
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