using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Objects;
using System.Windows;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Objects
{
    public partial class ConsumptionObjectEditView : Window
    {
        private readonly ConsumptionObjectEditViewModel _viewModel;

        public ConsumptionObjectEditView(ConsumptionObjectEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            // Подписываемся на событие закрытия
            viewModel.OnObjectSaved += (s, e) => Close();
        }

        // Валидация номера дома (разрешаем цифры, русские буквы, / и -)
        private void TextBox_PreviewTextInput_HouseNumber(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictHouseNumber(sender, e);
        }

        // Валидация номера квартиры, жильцов: только цифры
        private void TextBox_PreviewTextInput_NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictNumbersOnly(sender, e);
        }

        // Валидация площади: десятичное число
        private void TextBox_PreviewTextInput_Decimal(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictDecimalNumbers(sender, e);
        }
    }
}