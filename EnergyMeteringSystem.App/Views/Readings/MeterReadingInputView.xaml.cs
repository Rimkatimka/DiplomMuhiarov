using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Readings;
using EnergyMeteringSystem.Core.Models.DTO;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Readings
{
    /// <summary>
    /// Логика взаимодействия для MeterReadingInputView.xaml
    /// </summary>
    public partial class MeterReadingInputView : UserControl
    {
        public MeterReadingInputView(UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new MeterReadingInputViewModel(currentUser);
        }
        private void TextBox_PreviewTextInput_Decimal(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            InputValidator.RestrictDecimalNumbers(sender, e);
        }

        private void TextBox_PreviewKeyDown_BlockSpace(object sender, System.Windows.Input.KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }
    }
}
