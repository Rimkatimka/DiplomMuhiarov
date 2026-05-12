using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Objects;
using System.Windows;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Objects
{
    /// <summary>
    /// Логика взаимодействия для ConsumptionObjectEditView.xaml
    /// </summary>
    public partial class ConsumptionObjectEditView : Window
    {
        public ConsumptionObjectEditView(ConsumptionObjectEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnObjectSaved += (s, e) => Close();
        }
        private void TextBox_PreviewTextInput_Alphanumeric(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictAlphaNumeric(sender, e);
        }

        // Номер квартиры, жильцы: только цифры
        private void TextBox_PreviewTextInput_NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictNumbersOnly(sender, e);
        }

        // Площадь: десятичное число
        private void TextBox_PreviewTextInput_Decimal(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictDecimalNumbers(sender, e);
        }

        // Блокировка пробела
        private void TextBox_PreviewKeyDown_BlockSpace(object sender, System.Windows.Input.KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }
        private void TextBox_PreviewTextInput_HouseNumber(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictHouseNumber(sender, e);
        }
    }
}
